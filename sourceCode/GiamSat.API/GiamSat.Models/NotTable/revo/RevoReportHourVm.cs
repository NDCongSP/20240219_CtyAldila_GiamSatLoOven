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
        /* COUNT_BIG trong SQL trả bigint — dùng long tránh lỗi cast Int64 → Int32 */
        public long ShaftCountFinishedInHour { get; set; }
        public long IncompleteShaftCountInHour { get; set; }
        public bool HighlightIncomplete { get; set; }
    }
}
