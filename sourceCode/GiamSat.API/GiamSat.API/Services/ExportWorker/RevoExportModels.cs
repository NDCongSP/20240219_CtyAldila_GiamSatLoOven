using System;
using System.Collections.Generic;

namespace GiamSat.API.Services.ExportWorker
{
    public enum RevoReportMode
    {
        ByStep = 0,
        ByShaft = 1,
        ByHour = 2
    }

    public enum RevoShaftScopeKind
    {
        Total = 0,
        Finished = 1
    }

    public class ReportModeOption
    {
        public string Name { get; set; } = string.Empty;
        public RevoReportMode Value { get; set; }
    }

    public class RevoStepRow
    {
        public string? RevoName { get; set; }
        public DateTime Hour { get; set; }
        public string HourBucket { get; set; } = string.Empty;
        public int ShaftNo { get; set; }
        public string ShaftKey { get; set; } = string.Empty;
        public int Stt { get; set; }
        public Guid? ShaftNum { get; set; }
        public string? Part { get; set; }
        public string? Work { get; set; }
        public string? Rev { get; set; }
        public string? Mandrel { get; set; }
        public int? StepId { get; set; }
        public string StepDisplay { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string DurationText { get; set; } = string.Empty;
        public bool IsAutoRolling { get; set; }
        public bool HighlightIncomplete { get; set; }
    }

    public class RevoShaftRow
    {
        public string ShaftLabel { get; set; } = string.Empty;
        public string? RevoName { get; set; }
        public DateTime Hour { get; set; }
        public string HourBucket { get; set; } = string.Empty;
        public int ShaftNo { get; set; }
        public int Stt { get; set; }
        public Guid ShaftNum { get; set; }
        public string? Part { get; set; }
        public string? Work { get; set; }
        public string? Mandrel { get; set; }
        public int StepCount { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public TimeSpan TotalTime { get; set; }
        public string TotalTimeText { get; set; } = string.Empty;
        public bool IsShaftFinished { get; set; }
        public bool HighlightIncomplete { get; set; }
    }

    public class RevoHourStats
    {
        public int TotalShafts { get; set; }
        public int FinishedShafts { get; set; }
    }

    public class RevoHourRow
    {
        public DateTime Hour { get; set; }
        public string HourRange { get; set; } = string.Empty;
        public int ShaftCount { get; set; }
        public long ShaftCountFinishedInHour { get; set; }
        public long IncompleteShaftCountInHour { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public TimeSpan TotalTime { get; set; }
        public string TotalHoursText => $"{(int)TotalTime.TotalHours:D2}:{TotalTime.Minutes:D2}:{TotalTime.Seconds:D2}";
        public bool HighlightIncomplete { get; set; }
        public Dictionary<int, RevoHourStats> MachineStats { get; set; } = new();

        public RevoHourStats GetMachineStats(int revoId)
        {
            if (MachineStats != null && MachineStats.TryGetValue(revoId, out var stats))
            {
                return stats;
            }
            return new RevoHourStats { TotalShafts = 0, FinishedShafts = 0 };
        }
    }

    public class RevoDropdownModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
