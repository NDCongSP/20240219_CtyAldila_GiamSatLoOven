using System;

namespace GiamSat.Models
{
    /// <summary>Kết quả dbo.fn_RevoReport_Shaft.</summary>
    public class RevoReportShaftVm
    {
        public string? ShaftLabel { get; set; }
        public int? RevoId { get; set; }
        public string? RevoName { get; set; }
        public DateTime Hour { get; set; }
        public string? HourBucket { get; set; }
        /* ROW_NUMBER() → bigint */
        public long ShaftNo { get; set; }
        public long Stt { get; set; }
        public Guid ShaftNum { get; set; }
        public string? Part { get; set; }
        public string? Work { get; set; }
        public string? Mandrel { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public double TotalTimeSeconds { get; set; }
        public string? TotalTimeText { get; set; }
        public long StepCount { get; set; }
        public int IsAutoRollingShaft { get; set; }
    }
}
