using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GiamSat.API.Controllers
{
    /// <summary>
    /// Endpoint xem log file Serilog. Hỗ trợ all-*.log, error-*.log, structured-*.json.
    /// </summary>
    [Authorize(Policy = AppPermissions.System_Logs_View)]
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        // 2024-04-30 12:34:56.789 +07:00 [INF] [TRACE_ID] SourceContext Message...
        private static readonly Regex LineRegex = new Regex(
            @"^(?<ts>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+\-]\d{2}:\d{2})\s\[(?<lvl>[A-Z]{3})\](?:\s\[(?<corr>[^\]]*)\])?\s(?<rest>.*)$",
            RegexOptions.Compiled);

        private readonly string _logDir;
        private readonly int _maxLines;
        private readonly ILogger<LogsController> _logger;

        public LogsController(IConfiguration configuration, ILogger<LogsController> logger)
        {
            _logger = logger;
            var configuredDir = configuration["Logs:Directory"] ?? "Logs";
            _logDir = Path.IsPathRooted(configuredDir)
                ? configuredDir
                : Path.Combine(AppContext.BaseDirectory, configuredDir);
            _maxLines = int.TryParse(configuration["Logs:MaxLinesPerRequest"], out var n) ? n : 5000;
        }

        [HttpGet("files")]
        public ActionResult<Result<List<LogFileInfo>>> GetFiles()
        {
            try
            {
                if (!Directory.Exists(_logDir))
                    return Ok(Result<List<LogFileInfo>>.Success(new List<LogFileInfo>()));

                var files = new DirectoryInfo(_logDir)
                    .GetFiles("*")
                    .Where(f => f.Extension == ".log" || f.Extension == ".json")
                    .OrderByDescending(f => f.LastWriteTime)
                    .Select(f => new LogFileInfo
                    {
                        FileName = f.Name,
                        Kind = ClassifyFile(f.Name),
                        SizeBytes = f.Length,
                        LastModified = f.LastWriteTime
                    })
                    .ToList();
                return Ok(Result<List<LogFileInfo>>.Success(files));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed listing log files in {LogDir}", _logDir);
                return Ok(Result<List<LogFileInfo>>.Fail(ex.Message));
            }
        }

        [HttpGet("entries/{fileName}")]
        public async Task<ActionResult<Result<List<LogEntry>>>> GetEntries(
            string fileName,
            [FromQuery] string level = null,
            [FromQuery] string keyword = null,
            [FromQuery] int take = 1000)
        {
            try
            {
                if (!IsValidFileName(fileName))
                    return Ok(Result<List<LogEntry>>.Fail("Tên file không hợp lệ."));

                var path = Path.Combine(_logDir, fileName);
                if (!System.IO.File.Exists(path))
                    return Ok(Result<List<LogEntry>>.Fail($"Không tìm thấy file: {fileName}"));

                if (take <= 0) take = 1000;
                if (take > _maxLines) take = _maxLines;

                var entries = await ReadEntriesAsync(path);
                IEnumerable<LogEntry> q = entries;
                if (!string.IsNullOrWhiteSpace(level))
                    q = q.Where(e => string.Equals(e.Level, level, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(keyword))
                    q = q.Where(e => (e.Message ?? "").IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0
                                  || (e.Source ?? "").IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);

                return Ok(Result<List<LogEntry>>.Success(
                    q.OrderByDescending(e => e.Timestamp).Take(take).ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed reading log file {FileName}", fileName);
                return Ok(Result<List<LogEntry>>.Fail(ex.Message));
            }
        }

        [HttpGet("trace/{correlationId}")]
        public async Task<ActionResult<Result<List<LogEntry>>>> GetByTrace(string correlationId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(correlationId))
                    return Ok(Result<List<LogEntry>>.Fail("Trace ID rỗng."));
                if (!Directory.Exists(_logDir))
                    return Ok(Result<List<LogEntry>>.Success(new List<LogEntry>()));

                var candidates = new DirectoryInfo(_logDir)
                    .GetFiles("all-*.log")
                    .OrderByDescending(f => f.LastWriteTime)
                    .Take(7)
                    .ToList();

                var matched = new List<LogEntry>();
                foreach (var f in candidates)
                {
                    var entries = await ReadEntriesAsync(f.FullName);
                    matched.AddRange(entries.Where(e => string.Equals(e.CorrelationId, correlationId, StringComparison.OrdinalIgnoreCase)));
                    if (matched.Count >= _maxLines) break;
                }
                return Ok(Result<List<LogEntry>>.Success(
                    matched.OrderBy(e => e.Timestamp).Take(_maxLines).ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed tracing {CorrelationId}", correlationId);
                return Ok(Result<List<LogEntry>>.Fail(ex.Message));
            }
        }

        [HttpGet("summary/{fileName}")]
        public async Task<ActionResult<Result<LogSummary>>> GetSummary(string fileName)
        {
            try
            {
                if (!IsValidFileName(fileName))
                    return Ok(Result<LogSummary>.Fail("Tên file không hợp lệ."));
                var path = Path.Combine(_logDir, fileName);
                if (!System.IO.File.Exists(path))
                    return Ok(Result<LogSummary>.Fail($"Không tìm thấy file: {fileName}"));

                var entries = await ReadEntriesAsync(path);
                var summary = new LogSummary
                {
                    FileName = fileName,
                    Total = entries.Count,
                    ByLevel = entries
                        .GroupBy(e => e.Level ?? "Unknown")
                        .Select(g => new LogSummaryItem { Level = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .ToList()
                };
                return Ok(Result<LogSummary>.Success(summary));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed summarizing {FileName}", fileName);
                return Ok(Result<LogSummary>.Fail(ex.Message));
            }
        }

        [HttpDelete("files/{fileName}")]
        public ActionResult<Result> DeleteFile(string fileName)
        {
            try
            {
                if (!IsValidFileName(fileName))
                    return Ok(Result.Fail("Tên file không hợp lệ."));
                var path = Path.Combine(_logDir, fileName);
                if (!System.IO.File.Exists(path))
                    return Ok(Result.Fail($"Không tìm thấy: {fileName}"));
                // Mở stream để force unlock trước khi delete (Serilog có thể đang giữ file).
                System.IO.File.Delete(path);
                _logger.LogWarning("Log file {FileName} đã bị XÓA bởi {User}", fileName, User?.Identity?.Name);
                return Ok(Result.Success("Đã xóa file."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed deleting {FileName}", fileName);
                return Ok(Result.Fail(ex.Message));
            }
        }

        /// <summary>Truncate file (giữ file nhưng xóa nội dung) — không phá Serilog đang ghi.</summary>
        [HttpPost("clear/{fileName}")]
        public ActionResult<Result> ClearFile(string fileName)
        {
            try
            {
                if (!IsValidFileName(fileName))
                    return Ok(Result.Fail("Tên file không hợp lệ."));
                var path = Path.Combine(_logDir, fileName);
                if (!System.IO.File.Exists(path))
                    return Ok(Result.Fail($"Không tìm thấy: {fileName}"));

                // Mở với FileShare.ReadWrite để không xung đột với Serilog đang giữ file.
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete))
                {
                    fs.SetLength(0);
                }
                _logger.LogWarning("Log file {FileName} đã bị CLEAR bởi {User}", fileName, User?.Identity?.Name);
                return Ok(Result.Success("Đã xóa nội dung file."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed clearing {FileName}", fileName);
                return Ok(Result.Fail(ex.Message));
            }
        }

        private static bool IsValidFileName(string fileName) =>
            !string.IsNullOrWhiteSpace(fileName)
            && !fileName.Contains("..")
            && !fileName.Contains('/')
            && !fileName.Contains('\\');

        private static string ClassifyFile(string fileName)
        {
            if (fileName.StartsWith("error-", StringComparison.OrdinalIgnoreCase)) return "error";
            if (fileName.StartsWith("structured-", StringComparison.OrdinalIgnoreCase)) return "json";
            if (fileName.StartsWith("all-", StringComparison.OrdinalIgnoreCase)) return "all";
            return "other";
        }

        private static async Task<List<LogEntry>> ReadEntriesAsync(string path)
        {
            // Mở file với FileShare.ReadWrite để KHÔNG tranh chấp lock với Serilog đang ghi.
            var lines = new List<string>();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var reader = new StreamReader(fs, System.Text.Encoding.UTF8))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lines.Add(line);
                }
            }

            // File CLEF (JSON-per-line) parse khác với plain text.
            if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return ParseClefLines(lines);
            }

            var entries = new List<LogEntry>(capacity: lines.Count / 2);
            LogEntry current = null;

            foreach (var line in lines)
            {
                var match = LineRegex.Match(line);
                if (match.Success)
                {
                    if (current != null) entries.Add(current);

                    DateTime ts;
                    DateTime.TryParseExact(
                        match.Groups["ts"].Value,
                        "yyyy-MM-dd HH:mm:ss.fff zzz",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out ts);

                    var corr = match.Groups["corr"].Success ? match.Groups["corr"].Value : null;
                    var rest = match.Groups["rest"].Value;

                    string source = null;
                    var msg = rest;
                    var firstSpace = rest.IndexOf(' ');
                    if (firstSpace > 0)
                    {
                        var token = rest.Substring(0, firstSpace);
                        if (token.Contains('.') && !token.Contains(':'))
                        {
                            source = token;
                            msg = rest.Substring(firstSpace + 1);
                        }
                    }

                    current = new LogEntry
                    {
                        Timestamp = ts,
                        Level = MapLevel(match.Groups["lvl"].Value),
                        CorrelationId = string.IsNullOrEmpty(corr) ? null : corr,
                        Source = source,
                        Message = msg,
                        Raw = line
                    };
                }
                else if (current != null)
                {
                    current.Message += "\n" + line;
                    current.Raw += "\n" + line;
                }
            }
            if (current != null) entries.Add(current);
            return entries;
        }

        private static string MapLevel(string serilogShort) => serilogShort switch
        {
            "VRB" => "Verbose",
            "DBG" => "Debug",
            "INF" => "Information",
            "WRN" => "Warning",
            "ERR" => "Error",
            "FTL" => "Fatal",
            _ => serilogShort
        };

        /// <summary>
        /// Parse file CLEF (Compact Log Event Format) do CompactJsonFormatter sinh ra.
        /// Mỗi line là 1 JSON object với keys: @t (timestamp), @l (level - mặc định Information nếu thiếu),
        /// @m (rendered message), @mt (template), @x (exception), và các property bổ sung như CorrelationId, SourceContext.
        /// </summary>
        private static List<LogEntry> ParseClefLines(List<string> lines)
        {
            var entries = new List<LogEntry>(capacity: lines.Count);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    using var doc = JsonDocument.Parse(line);
                    var root = doc.RootElement;

                    DateTime ts = DateTime.MinValue;
                    if (root.TryGetProperty("@t", out var tsEl) && tsEl.ValueKind == JsonValueKind.String)
                    {
                        DateTime.TryParse(tsEl.GetString(), CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out ts);
                    }

                    // CLEF không ghi @l cho level Information mặc định → fallback "Information".
                    var level = "Information";
                    if (root.TryGetProperty("@l", out var lvlEl) && lvlEl.ValueKind == JsonValueKind.String)
                        level = lvlEl.GetString();

                    string message = null;
                    if (root.TryGetProperty("@m", out var mEl) && mEl.ValueKind == JsonValueKind.String)
                        message = mEl.GetString();
                    else if (root.TryGetProperty("@mt", out var mtEl) && mtEl.ValueKind == JsonValueKind.String)
                        message = mtEl.GetString();

                    string exception = null;
                    if (root.TryGetProperty("@x", out var xEl) && xEl.ValueKind == JsonValueKind.String)
                        exception = xEl.GetString();
                    if (!string.IsNullOrEmpty(exception))
                        message = (message ?? "") + "\n" + exception;

                    string corr = null;
                    if (root.TryGetProperty("CorrelationId", out var cEl) && cEl.ValueKind == JsonValueKind.String)
                        corr = cEl.GetString();

                    string source = null;
                    if (root.TryGetProperty("SourceContext", out var sEl) && sEl.ValueKind == JsonValueKind.String)
                        source = sEl.GetString();

                    entries.Add(new LogEntry
                    {
                        Timestamp = ts,
                        Level = level,
                        CorrelationId = corr,
                        Source = source,
                        Message = message,
                        Raw = line
                    });
                }
                catch
                {
                    // JSON corrupt — bỏ dòng, đừng phá toàn bộ.
                }
            }
            return entries;
        }
    }
}
