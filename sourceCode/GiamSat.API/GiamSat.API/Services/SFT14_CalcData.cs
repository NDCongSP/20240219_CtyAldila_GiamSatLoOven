using GiamSat.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API
{
    /// <summary>
    /// Service lấy dữ liệu để tính A,B,C,D cho Auto Sanding.
    /// </summary>
    /// <remarks>
    /// File:    GiamSat.API/Services/SFT14_CalcData.cs
    /// Author:  Cong.Nguyen
    /// Created: 2026-05-28
    /// Modified: 2026-06-08 — Bỏ filter Station cho Fre1/Fre2; chỉ lọc theo Part+WorkOrder
    /// Nguồn dữ liệu:
    ///   - Fre1      : external DB, Part+WorkFre1, cột Reading (nvarchar), sort by ShaftNum
    ///   - Fre2      : external DB, Part+WorkFre2, cột Reading (nvarchar), sort by ShaftNum
    ///   - StiffnessY: main DB (FT16), Part+WorkSpine, SandingMode=Test, khớp ShaftNum
    ///   - RPM       : tính từ motorFrom/To/Step, 2 row/bước nhảy
    /// </remarks>
    public class SFT14_CalcData : ISFT14_CalcData
    {
        private readonly FreMeasurementDbContext _extContext;
        private readonly ApplicationDbContext _mainContext;

        public SFT14_CalcData(FreMeasurementDbContext extContext, ApplicationDbContext mainContext)
        {
            _extContext = extContext;
            _mainContext = mainContext;

            _extContext.Database.SetCommandTimeout((int)TimeSpan.FromMinutes(60).TotalMilliseconds);
            _mainContext.Database.SetCommandTimeout((int)TimeSpan.FromMinutes(60).TotalMilliseconds);
        }

        public async Task<Result<List<AutoSandingTestRow>>> GetCalcDataAsync(
            string part,
            string workFre1,
            string workFre2,
            string workSpine,
            double offsetFre1,
            double offsetFre2,
            double motorFrom,
            double motorTo,
            double motorStep)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(part))
                    return await Result<List<AutoSandingTestRow>>.FailAsync("Part không được để trống.");
                if (string.IsNullOrWhiteSpace(workFre1))
                    return await Result<List<AutoSandingTestRow>>.FailAsync("Work Order Fre1 không được để trống.");

                // ── Fre1 từ external DB ──────────────────────────────────────
                var fre1Records = await _extContext.FreMeasurements
                    .AsNoTracking()
                    .Where(r => r.Part == part && r.WorkOrder == workFre1
                             && r.Reading != null && r.ShaftNum != null)
                    .OrderBy(r => r.ShaftNum)
                    .ToListAsync();

                if (fre1Records.Count == 0)
                    return await Result<List<AutoSandingTestRow>>.FailAsync(
                        $"Không tìm thấy dữ liệu Fre1 cho Part='{part}', WorkFre1='{workFre1}'.");

                // ── Fre2 từ external DB (workFre2 có thể khác workFre1) ──────
                var effectiveWorkFre2 = string.IsNullOrWhiteSpace(workFre2) ? workFre1 : workFre2;
                var fre2Records = await _extContext.FreMeasurements
                    .AsNoTracking()
                    .Where(r => r.Part == part && r.WorkOrder == effectiveWorkFre2
                             && r.Reading != null && r.ShaftNum != null)
                    .OrderBy(r => r.ShaftNum)
                    .ToListAsync();

                // ShaftNum nullable → lọc HasValue rồi build dictionary
                var fre2ByShaft = fre2Records
                    .Where(r => r.ShaftNum.HasValue)
                    .ToDictionary(r => r.ShaftNum!.Value, r => ParseReading(r.Reading));

                // ── StiffnessY từ FT16 main DB (SandingMode=Test) ────────────
                var effectiveWorkSpine = string.IsNullOrWhiteSpace(workSpine) ? workFre1 : workSpine;
                var ft16Records = await _mainContext.FT16_SandingLogDatas
                    .AsNoTracking()
                    .Where(r => r.Part == part && r.Work == effectiveWorkSpine
                             && r.SandingMode == EnumSandingMode.Test
                             && r.ShaftNum != null)
                    .ToListAsync();

                var stiffnessYByShaft = ft16Records
                    .Where(r => r.ShaftNum.HasValue)
                    .GroupBy(r => r.ShaftNum!.Value)
                    .ToDictionary(g => g.Key, g => g.First().SpineB ?? 0);

                // ── Ghép thành bảng kết quả ──────────────────────────────────
                // Số rows = số bước RPM × 2 (mỗi bước nhảy 2 row)
                // Ví dụ: From=100, To=500, Step=100 → 5 bước × 2 = 10 rows
                var rpmValues = GenerateRpmValues(motorFrom, motorTo, motorStep);
                int totalRows = rpmValues.Count * 2;
                if (totalRows < 1) totalRows = fre1Records.Count;

                var rows = new List<AutoSandingTestRow>();
                int rowIdx = 0;
                for (int i = 0; i < fre1Records.Count && rowIdx < totalRows; i++)
                {
                    var rec1 = fre1Records[i];
                    if (!rec1.ShaftNum.HasValue) continue;

                    int shaftNum = rec1.ShaftNum.Value;
                    double fre1 = Math.Round(ParseReading(rec1.Reading) + offsetFre1, 3);
                    double fre2 = fre2ByShaft.TryGetValue(shaftNum, out double f2)
                        ? Math.Round(f2 + offsetFre2, 3)
                        : 0;
                    double stiffY = stiffnessYByShaft.TryGetValue(shaftNum, out double sy)
                        ? Math.Round(sy, 3)
                        : 0;

                    // Mỗi RPM level chiếm đúng 2 row: row 0-1 → rpmValues[0], row 2-3 → rpmValues[1], ...
                    int rpmIdx = Math.Min(rowIdx / 2, rpmValues.Count - 1);
                    double rpm = rpmValues.Count > 0 ? rpmValues[rpmIdx] : 0;

                    rows.Add(new AutoSandingTestRow
                    {
                        RowIndex        = rowIdx + 1,
                        Fre1            = fre1,
                        BeltRotationRpm = rpm,
                        Fre2            = fre2,
                        StiffnessY      = stiffY,
                    });
                    rowIdx++;
                }

                return await Result<List<AutoSandingTestRow>>.SuccessAsync(rows);
            }
            catch (Exception ex)
            {
                return await Result<List<AutoSandingTestRow>>.FailAsync(ex.Message);
            }
        }

        // Reading lưu dạng nvarchar — parse với InvariantCulture để xử lý dấu chấm thập phân
        private static double ParseReading(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : 0;
        }

        private static List<double> GenerateRpmValues(double from, double to, double step)
        {
            var values = new List<double>();
            if (step <= 0) step = 1;
            for (double rpm = from; rpm <= to + 1e-9; rpm += step)
                values.Add(Math.Round(rpm, 0));
            return values;
        }
    }
}
