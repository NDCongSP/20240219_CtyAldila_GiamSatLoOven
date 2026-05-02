using RestEase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    /// <summary>
    /// Một dòng log đã được parse từ file Serilog.
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string CorrelationId { get; set; }
        public string Source { get; set; }
        public string Raw { get; set; }
    }

    public class LogFileInfo
    {
        public string FileName { get; set; }
        public string Kind { get; set; }     // all | error | json | other
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
        public List<LogSummaryItem> ByLevel { get; set; } = new List<LogSummaryItem>();
    }

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

    /// <summary>
    /// RestEase client cho LogsController.
    /// </summary>
    [BasePath("api/Logs")]
    public interface ILogsClient
    {
        [Get("files")]
        Task<Result<List<LogFileInfo>>> GetFiles();

        [Get("entries/{fileName}")]
        Task<Result<List<LogEntry>>> GetEntries(
            [Path] string fileName,
            [Query("level")] string level = null,
            [Query("keyword")] string keyword = null,
            [Query("take")] int take = 1000);

        [Get("trace/{correlationId}")]
        Task<Result<List<LogEntry>>> GetByTrace([Path] string correlationId);

        [Get("summary/{fileName}")]
        Task<Result<LogSummary>> GetSummary([Path] string fileName);

        [Delete("files/{fileName}")]
        Task<Result> DeleteFile([Path] string fileName);

        [Post("clear/{fileName}")]
        Task<Result> ClearFile([Path] string fileName);
    }
}
