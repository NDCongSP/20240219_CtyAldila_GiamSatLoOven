using System;

namespace GiamSat.Models
{
    /// <summary>Kết quả dbo.fn_RevoReport_Hour.</summary>
    public class RevoReportHourVm
    {
        public DateTime Hour { get; set; }
        public string? HourRange { get; set; }
        public int ShaftCount { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public double TotalTimeSeconds { get; set; }
        public string? TotalTimeText { get; set; }
    }
}
