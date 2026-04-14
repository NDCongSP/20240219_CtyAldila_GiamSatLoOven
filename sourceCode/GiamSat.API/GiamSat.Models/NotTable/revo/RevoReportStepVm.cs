using System;

namespace GiamSat.Models
{
    /// <summary>Kết quả dbo.fn_RevoReport_Step — map cột SQL.</summary>
    public class RevoReportStepVm
    {
        public Guid Id { get; set; }
        public int? RevoId { get; set; }
        public string? RevoName { get; set; }
        public DateTime Hour { get; set; }
        public string? HourBucketDisplay { get; set; }
        /* ROW_NUMBER() trong SQL Server là bigint */
        public long ShaftNo { get; set; }
        public string? ShaftKey { get; set; }
        public Guid? ShaftNum { get; set; }
        public string? Part { get; set; }
        public string? Work { get; set; }
        public string? Rev { get; set; }
        public string? Mandrel { get; set; }
        public string? StepDisplay { get; set; }
        public DateTime? DisplayStartedAt { get; set; }
        public DateTime? DisplayEndedAt { get; set; }
        public string? DurationText { get; set; }
        public int IsAutoRolling { get; set; }
        public DateTime Started { get; set; }
        public long Stt { get; set; }
        public bool? IsShaftFinished { get; set; }
        public bool HighlightIncomplete { get; set; }
    }
}
