using GiamSat.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GiamSat.API
{
    /// <summary>
    /// Đồng bộ thông số Part từ external DB ALD_MFG vào bảng master Oven.FT14.
    /// </summary>
    /// <remarks>
    /// File:    GiamSat.API/Services/SFT14_Sync.cs
    /// Author:  Cong.Nguyen
    /// Created: 2026-06-17
    /// Mapping (theo yêu cầu UI Cấu hình chung):
    ///   - PartName    = Part.Number
    ///   - Freq_LL     = Part.Freq_LL + 1
    ///   - Freq_UL     = Part.Freq_UL + 1
    ///   - FreqTarget  = trung bình (Freq_LL, Freq_UL) — tính trên giá trị ĐÃ +1
    ///   - OD_BOD      = trung bình (Diam_LL, Diam_UL) của dòng PartZM có ZMmeasType.Name chứa 'BOD',
    ///                   chọn dòng có số nhỏ nhất trong Name (vd "UPT BOD @35MM" → 35)
    ///   - TipOdLength_1..3 + Diam_LL/UL_1..3 = các dòng PartZM có Name chứa 'TOD',
    ///                   sort theo số trong Name tăng dần, lấy tối đa 3 điểm
    ///   - Length      = PartNewSetting.LStd
    ///   - A/B/C/D/Formula/Z_Stiffness: KHÔNG ghi đè (do người dùng tự tính)
    /// </remarks>
    public class SFT14_Sync : ISFT14_Sync
    {
        private const double Epsilon = 1e-6;

        private readonly FreMeasurementDbContext _extContext;
        private readonly ApplicationDbContext _mainContext;

        public SFT14_Sync(FreMeasurementDbContext extContext, ApplicationDbContext mainContext)
        {
            _extContext = extContext;
            _mainContext = mainContext;

            _extContext.Database.SetCommandTimeout((int)TimeSpan.FromMinutes(60).TotalMilliseconds);
            _mainContext.Database.SetCommandTimeout((int)TimeSpan.FromMinutes(60).TotalMilliseconds);
        }

        public async Task<Result<List<PartSyncSourceDto>>> GetSyncSourcesAsync()
        {
            try
            {
                var sources = await _extContext.Parts
                    .AsNoTracking()
                    .Where(p => p.Number != null && p.Number != "")
                    .OrderBy(p => p.Number)
                    .Select(p => new PartSyncSourceDto { PartId = p.Id, PartName = p.Number })
                    .ToListAsync();

                return await Result<List<PartSyncSourceDto>>.SuccessAsync(sources);
            }
            catch (Exception ex)
            {
                return await Result<List<PartSyncSourceDto>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT14SyncResultDto>> SyncPartsAsync(List<int> partIds)
        {
            var result = new FT14SyncResultDto();
            try
            {
                if (partIds == null || partIds.Count == 0)
                    return await Result<FT14SyncResultDto>.SuccessAsync(result);

                // ── Bulk load nguồn external cho cả batch ────────────────────
                // Dùng List<int?> cho cột nullable (PartID) để EF dịch sang IN(...) an toàn,
                // tránh ".Value" trong predicate (một số trường hợp không dịch được sang SQL).
                var partIdSet = partIds.Distinct().ToList();
                var partIdNullable = partIdSet.Select(x => (int?)x).ToList();

                var parts = await _extContext.Parts.AsNoTracking()
                    .Where(p => partIdSet.Contains(p.Id))
                    .ToListAsync();

                // PartZM (keyless) — load thẳng, lọc theo PartID (nullable)
                var pzRows = await _extContext.PartZMs.AsNoTracking()
                    .Where(pz => partIdNullable.Contains(pz.PartID))
                    .Select(pz => new { pz.PartID, pz.ZMID, pz.Diam_LL, pz.Diam_UL })
                    .ToListAsync();

                // ZMmeasType.Name cho các ZMID xuất hiện trong pzRows
                var zmTypeIds = pzRows.Where(r => r.ZMID.HasValue).Select(r => r.ZMID!.Value).Distinct().ToList();
                var zmTypeList = await _extContext.ZMmeasTypes.AsNoTracking()
                    .Where(zt => zmTypeIds.Contains(zt.Id))
                    .Select(zt => new { zt.Id, zt.Name })
                    .ToListAsync();
                var zmTypeName = zmTypeList.ToDictionary(zt => zt.Id, zt => zt.Name);

                // Ghép PartZM + Name trong bộ nhớ → ZmRow theo PartID
                var zmByPart = pzRows
                    .Where(r => r.PartID.HasValue)
                    .GroupBy(r => r.PartID!.Value)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(r => new ZmRow
                        {
                            PartID = r.PartID!.Value,
                            Name = (r.ZMID.HasValue && zmTypeName.TryGetValue(r.ZMID.Value, out var n)) ? n : null,
                            Diam_LL = r.Diam_LL,
                            Diam_UL = r.Diam_UL
                        }).ToList());

                // PartNewSetting — CHỈ project PartId + LStd (các cột khác như LLI/LUI có thể
                // không tồn tại trong DB thực tế → tránh "Invalid column name").
                var settings = await _extContext.PartNewSettings.AsNoTracking()
                    .Where(s => partIdSet.Contains(s.PartId))
                    .Select(s => new { s.PartId, s.LStd })
                    .ToListAsync();
                var settingByPart = settings.GroupBy(s => s.PartId)
                    .ToDictionary(g => g.Key, g => g.First().LStd);

                // Existing FT14 theo PartName (tracked để update)
                var partNames = parts.Where(p => !string.IsNullOrWhiteSpace(p.Number))
                    .Select(p => p.Number).Distinct().ToList();
                var existingList = await _mainContext.FT14_TipOdFreqs
                    .Where(f => f.PartName != null && partNames.Contains(f.PartName))
                    .ToListAsync();
                var existingByName = existingList
                    .GroupBy(f => f.PartName!)
                    .ToDictionary(g => g.Key, g => g.First());

                foreach (var part in parts)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(part.Number)) { result.Skipped++; continue; }

                        // ── Tính các thông số mapping ────────────────────────
                        double? freqLL = part.Freq_LL.HasValue ? part.Freq_LL.Value + 1 : (double?)null;
                        double? freqUL = part.Freq_UL.HasValue ? part.Freq_UL.Value + 1 : (double?)null;
                        // FreqTarget = trung bình của Freq_LL & Freq_UL ĐÃ +1
                        double? freqTarget = (freqLL.HasValue && freqUL.HasValue)
                            ? (freqLL.Value + freqUL.Value) / 2.0
                            : (double?)null;

                        zmByPart.TryGetValue(part.Id, out var zmList);
                        zmList ??= new List<ZmRow>();

                        // OD_BOD: dòng chứa 'BOD', chọn số nhỏ nhất trong Name, avg(Diam_LL, Diam_UL)
                        var bod = zmList
                            .Where(z => Contains(z.Name, "BOD"))
                            .OrderBy(z => ExtractNumber(z.Name) ?? double.MaxValue)
                            .FirstOrDefault();
                        double? odBod = bod != null ? AvgDiam(bod.Diam_LL, bod.Diam_UL) : null;

                        // TOD: dòng chứa 'TOD', sort số trong Name tăng dần, lấy tối đa 3 điểm
                        var todRows = zmList
                            .Where(z => Contains(z.Name, "TOD"))
                            .OrderBy(z => ExtractNumber(z.Name) ?? double.MaxValue)
                            .ToList();
                        var tod1 = todRows.ElementAtOrDefault(0);
                        var tod2 = todRows.ElementAtOrDefault(1);
                        var tod3 = todRows.ElementAtOrDefault(2);

                        double? length = settingByPart.TryGetValue(part.Id, out var lStd) ? ToDouble(lStd) : null;

                        // ── Insert / Update / Skip ───────────────────────────
                        if (existingByName.TryGetValue(part.Number!, out var existing))
                        {
                            bool changed =
                                !DEq(existing.Length, length) ||
                                !DEq(existing.OD_BOD, odBod) ||
                                !DEq(existing.FreqTarget, freqTarget) ||
                                !DEq(existing.Freq_LL, freqLL) ||
                                !DEq(existing.Freq_UL, freqUL) ||
                                !SEq(existing.TipOdLength_1, tod1?.Name) || !DEq(existing.Diam_LL_1, ToDouble(tod1?.Diam_LL)) || !DEq(existing.Diam_UL_1, ToDouble(tod1?.Diam_UL)) ||
                                !SEq(existing.TipOdLength_2, tod2?.Name) || !DEq(existing.Diam_LL_2, ToDouble(tod2?.Diam_LL)) || !DEq(existing.Diam_UL_2, ToDouble(tod2?.Diam_UL)) ||
                                !SEq(existing.TipOdLength_3, tod3?.Name) || !DEq(existing.Diam_LL_3, ToDouble(tod3?.Diam_LL)) || !DEq(existing.Diam_UL_3, ToDouble(tod3?.Diam_UL));

                            if (!changed) { result.Skipped++; continue; }

                            ApplyMappedFields(existing, part.Number!, length, odBod, freqTarget, freqLL, freqUL, tod1, tod2, tod3);
                            existing.UpdateddAt = DateTime.Now;
                            result.Updated++;
                        }
                        else
                        {
                            var entity = new FT14_TipOdFreq
                            {
                                Id = Guid.NewGuid(),
                                CreatedAt = DateTime.Now,
                                CreatedMachine = Environment.MachineName,
                                Actived = true,
                            };
                            ApplyMappedFields(entity, part.Number!, length, odBod, freqTarget, freqLL, freqUL, tod1, tod2, tod3);
                            await _mainContext.FT14_TipOdFreqs.AddAsync(entity);
                            // Tránh trùng nếu trong batch có nhiều Part cùng Number
                            existingByName[part.Number!] = entity;
                            result.Inserted++;
                        }
                    }
                    catch (Exception exItem)
                    {
                        result.Failed++;
                        result.Messages.Add($"Part '{part.Number}' (Id={part.Id}): {exItem.Message}");
                    }
                }

                if (result.Inserted > 0 || result.Updated > 0)
                    await _mainContext.SaveChangesAsync();

                return await Result<FT14SyncResultDto>.SuccessAsync(result);
            }
            catch (Exception ex)
            {
                return await Result<FT14SyncResultDto>.FailAsync(ex.Message);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static void ApplyMappedFields(
            FT14_TipOdFreq e, string partName,
            double? length, double? odBod, double? freqTarget, double? freqLL, double? freqUL,
            ZmRow tod1, ZmRow tod2, ZmRow tod3)
        {
            e.PartName = partName;
            e.Length = length;
            e.OD_BOD = odBod;
            e.FreqTarget = freqTarget;
            e.Freq_LL = freqLL;
            e.Freq_UL = freqUL;

            e.TipOdLength_1 = tod1?.Name; e.Diam_LL_1 = ToDouble(tod1?.Diam_LL); e.Diam_UL_1 = ToDouble(tod1?.Diam_UL);
            e.TipOdLength_2 = tod2?.Name; e.Diam_LL_2 = ToDouble(tod2?.Diam_LL); e.Diam_UL_2 = ToDouble(tod2?.Diam_UL);
            e.TipOdLength_3 = tod3?.Name; e.Diam_LL_3 = ToDouble(tod3?.Diam_LL); e.Diam_UL_3 = ToDouble(tod3?.Diam_UL);
        }

        private static bool Contains(string? name, string token) =>
            !string.IsNullOrEmpty(name) && name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;

        // Cắt chuỗi lấy số đầu tiên trong Name (vd "UPT BOD @35MM" → 35, "UPT BOD @ 100 MM" → 100)
        private static double? ExtractNumber(string? name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var m = Regex.Match(name, @"\d+(\.\d+)?");
            if (!m.Success) return null;
            return double.TryParse(m.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : (double?)null;
        }

        private static double? AvgDiam(float? a, float? b)
        {
            if (a.HasValue && b.HasValue) return (a.Value + b.Value) / 2.0;
            if (a.HasValue) return a.Value;
            if (b.HasValue) return b.Value;
            return null;
        }

        private static double? ToDouble(float? v) => v.HasValue ? (double?)v.Value : null;

        private static bool DEq(double? a, double? b)
        {
            if (!a.HasValue && !b.HasValue) return true;
            if (a.HasValue != b.HasValue) return false;
            return Math.Abs(a!.Value - b!.Value) < Epsilon;
        }

        private static bool SEq(string? a, string? b) =>
            string.Equals(a ?? string.Empty, b ?? string.Empty, StringComparison.Ordinal);

        /// <summary>Dòng PartZM đã join ZMmeasType.Name (dùng nội bộ service).</summary>
        private class ZmRow
        {
            public int PartID { get; set; }
            public string? Name { get; set; }
            public float? Diam_LL { get; set; }
            public float? Diam_UL { get; set; }
        }
    }
}
