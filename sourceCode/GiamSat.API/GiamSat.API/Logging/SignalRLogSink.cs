using GiamSat.API.Hubs;
using GiamSat.Models;
using Microsoft.AspNetCore.SignalR;
using Serilog.Core;
using Serilog.Events;
using System;

namespace GiamSat.API.Logging
{
    /// <summary>
    /// Serilog sink đẩy log event qua SignalR cho UI realtime.
    /// Mặc định push level >= Warning (cấu hình qua Logs:SignalRMinLevel).
    /// Fire-and-forget để không block log pipeline.
    /// </summary>
    public class SignalRLogSink : ILogEventSink
    {
        private readonly IHubContext<LogsHub> _hub;
        private readonly LogEventLevel _minLevel;
        private readonly IFormatProvider _formatProvider;

        public SignalRLogSink(IHubContext<LogsHub> hub,
            LogEventLevel minLevel = LogEventLevel.Warning,
            IFormatProvider formatProvider = null)
        {
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
            _minLevel = minLevel;
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null) return;
            if (logEvent.Level < _minLevel) return;

            string corrId = null;
            string source = null;
            if (logEvent.Properties.TryGetValue("CorrelationId", out var cv) && cv is ScalarValue sv)
                corrId = sv.Value?.ToString();
            if (logEvent.Properties.TryGetValue("SourceContext", out var sc) && sc is ScalarValue scsv)
                source = scsv.Value?.ToString();

            var dto = new LogStreamEntry
            {
                Timestamp = logEvent.Timestamp.UtcDateTime,
                Level = logEvent.Level.ToString(),
                Message = logEvent.RenderMessage(_formatProvider),
                Exception = logEvent.Exception?.ToString(),
                CorrelationId = corrId,
                Source = source
            };

            try
            {
                _ = _hub.Clients.All.SendAsync("LogEntry", dto);
            }
            catch
            {
                // Nuốt lỗi SignalR — sink không được phá log pipeline.
            }
        }
    }
}
