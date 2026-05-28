using GiamSat.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API
{
    /// <summary>
    /// Service lấy dữ liệu đo tần số từ external DB để tính A,B,C,D cho Auto Sanding.
    /// </summary>
    /// <remarks>
    /// File:    GiamSat.API/Services/SFT14_CalcData.cs
    /// Author:  Cong.Nguyen
    /// Created: 2026-05-28
    /// Cách ghép dữ liệu:
    ///   - Fre1: Station = "Auto Fre No.1", Reading column, sort by ShaftNum
    ///   - Fre2: Station = "Auto Fre No.2", Reading column, sort by ShaftNum
    ///   - Ghép theo ShaftNum chung (inner join), hoặc theo index nếu ShaftNum không khớp
    ///   - BeltRotationRpm: tính từ motorFrom/To/Step, mỗi mức RPM gán cho nhóm 2 shaft
    /// </remarks>
    public class SFT14_CalcData : ISFT14_CalcData
    {
        private const string StationFre1 = "Auto Fre No.1";
        private const string StationFre2 = "Auto Fre No.2";

        private readonly FreMeasurementDbContext _extContext;

        public SFT14_CalcData(FreMeasurementDbContext extContext)
        {
            _extContext = extContext;
            _extContext.Database.SetCommandTimeout((int)TimeSpan.FromMinutes(60).TotalMilliseconds);
        }

        public async Task<Result<List<AutoSandingTestRow>>> GetCalcDataAsync(
            string part,
            string work,
            double offsetFre1,
            double offsetFre2,
            double motorFrom,
            double motorTo,
            double motorStep)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(part) || string.IsNullOrWhiteSpace(work))
                    return await Result<List<AutoSandingTestRow>>.FailAsync("Part và Work Order không được để trống.");

                var fre1Records = await _extContext.FreMeasurements
                    .AsNoTracking()
                    .Where(r => r.Part == part && r.WorkOrder == work && r.Station == StationFre1 && r.Reading != null)
                    .OrderBy(r => r.ShaftNum)
                    .ToListAsync();

                var fre2Records = await _extContext.FreMeasurements
                    .AsNoTracking()
                    .Where(r => r.Part == part && r.WorkOrder == work && r.Station == StationFre2 && r.Reading != null)
                    .OrderBy(r => r.ShaftNum)
                    .ToListAsync();

                if (fre1Records.Count == 0)
                    return await Result<List<AutoSandingTestRow>>.FailAsync(
                        $"Không tìm thấy dữ liệu Fre1 (Station='{StationFre1}') cho Part='{part}', Work='{work}'.");

                // Ghép Fre1 & Fre2 theo ShaftNum; nếu không có Fre2 thì để 0
                var fre2ByShaft = fre2Records.ToDictionary(r => r.ShaftNum, r => r.Reading ?? 0);

                var rpmValues = GenerateRpmValues(motorFrom, motorTo, motorStep);
                int count = fre1Records.Count;
                int sharftsPerRpm = rpmValues.Count > 0
                    ? (int)Math.Ceiling((double)count / rpmValues.Count)
                    : 1;
                if (sharftsPerRpm < 1) sharftsPerRpm = 1;

                var rows = new List<AutoSandingTestRow>();
                for (int i = 0; i < count; i++)
                {
                    var rec1 = fre1Records[i];
                    double fre1 = Math.Round((rec1.Reading ?? 0) + offsetFre1, 3);
                    double fre2 = fre2ByShaft.TryGetValue(rec1.ShaftNum, out double f2)
                        ? Math.Round(f2 + offsetFre2, 3)
                        : 0;

                    int rpmIdx = Math.Min(i / sharftsPerRpm, rpmValues.Count - 1);
                    double rpm = rpmValues.Count > 0 ? rpmValues[rpmIdx] : 0;

                    rows.Add(new AutoSandingTestRow
                    {
                        RowIndex        = i + 1,
                        Fre1            = fre1,
                        StiffnessZ      = 0,  // TODO: nhập tay hoặc bổ sung từ nguồn khác
                        BeltRotationRpm = rpm,
                        Fre2            = fre2,
                        StiffnessY      = 0,  // TODO: nhập tay hoặc bổ sung từ nguồn khác
                    });
                }

                return await Result<List<AutoSandingTestRow>>.SuccessAsync(rows);
            }
            catch (Exception ex)
            {
                return await Result<List<AutoSandingTestRow>>.FailAsync(ex.Message);
            }
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
