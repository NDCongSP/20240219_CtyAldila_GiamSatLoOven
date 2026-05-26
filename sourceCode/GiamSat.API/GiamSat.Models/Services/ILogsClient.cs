using System;
using System.Collections.Generic;

namespace GiamSat.Models
{
    /// <summary>
    /// DTO push qua SignalR cho UI realtime. Phải khớp method "LogEntry" trên hub.
    /// </summary>
    public class LogStreamEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string CorrelationId { get; set; }
        public string Source { get; set; }
    }

    /// <summary>Một dòng log đã parse — dùng bởi LogsController (server) và GiamSatApi.cs (client NSwag).</summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string CorrelationId { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public string Raw { get; set; }
    }

    public class LogFileInfo
    {
        public string FileName { get; set; }
        public string Kind { get; set; }
        public long SizeBytes { get; set; }
        public DateTime LastModified { get; set; }
    }

    public class LogSummaryItem
    {
        public string Level { get; set; }
        public int Count { get; set; }
    }

    public class LogSummary
    {
        public string FileName { get; set; }
        public int Total { get; set; }
        public List<LogSummaryItem> ByLevel { get; set; }
    }
}
