using Serilog;
using System.Diagnostics;

namespace Bharuwa.Erp.API.FMS.Middleware
{
    public class PerformanceMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
        private const int SlowRequestThresholdMs = 1000; // 1 second

        public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;

                if (elapsedMs > SlowRequestThresholdMs)
                {
                    _logger.LogWarning(
                        "Slow request detected: {Method} {Path} took {ElapsedMs}ms",
                        context.Request.Method,
                        context.Request.Path,
                        elapsedMs);
                }

                // Log performance metrics for all requests
                _logger.LogDebug(
                    "Request performance: {Method} {Path} - {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    elapsedMs);
            }
        }
    }
}