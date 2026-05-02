using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System;
using System.Threading.Tasks;

namespace GiamSat.API.Middleware
{
    /// <summary>
    /// Đọc/sinh Correlation-Id cho mỗi request, push vào Serilog LogContext.
    /// Header in/out: X-Correlation-Id. Mọi log trong request có cùng TraceId.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        public const string HeaderName = "X-Correlation-Id";

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string id = null;
            if (context.Request.Headers.TryGetValue(HeaderName, out var values) && !string.IsNullOrWhiteSpace(values.ToString()))
            {
                id = values.ToString();
            }
            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpperInvariant();
            }

            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(HeaderName))
                {
                    context.Response.Headers[HeaderName] = id;
                }
                return Task.CompletedTask;
            });

            using (LogContext.PushProperty("CorrelationId", id))
            {
                await _next(context);
            }
        }
    }
}
