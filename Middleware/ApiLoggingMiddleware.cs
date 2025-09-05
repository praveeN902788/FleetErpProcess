using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Bharuwa.Erp.API.FMS.Middleware
{
    /// <summary>
    /// Middleware for comprehensive API request/response logging and monitoring
    /// </summary>
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiLoggingMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = context.TraceIdentifier;
            
            // Log request
            await LogRequestAsync(context, requestId);
            
            // Capture response
            var originalResponseBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in API request {RequestId}", requestId);
                await HandleExceptionAsync(context, ex, requestId);
            }
            finally
            {
                stopwatch.Stop();
                
                // Log response
                await LogResponseAsync(context, requestId, stopwatch.ElapsedMilliseconds);
                
                // Restore response body
                await responseBodyStream.CopyToAsync(originalResponseBodyStream);
                context.Response.Body = originalResponseBodyStream;
            }
        }

        private async Task LogRequestAsync(HttpContext context, string requestId)
        {
            var request = context.Request;
            var logData = new
            {
                RequestId = requestId,
                Method = request.Method,
                Path = request.Path.Value,
                QueryString = request.QueryString.Value,
                Headers = GetSafeHeaders(request.Headers),
                ContentType = request.ContentType,
                ContentLength = request.ContentLength,
                UserAgent = request.Headers.UserAgent.ToString(),
                RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("API Request: {RequestData}", JsonSerializer.Serialize(logData));

            // Log request body for POST/PUT requests (if enabled)
            if (_configuration.GetValue<bool>("Logging:LogRequestBody") && 
                (request.Method == "POST" || request.Method == "PUT" || request.Method == "PATCH"))
            {
                request.EnableBuffering();
                var body = await ReadRequestBodyAsync(request);
                if (!string.IsNullOrEmpty(body))
                {
                    _logger.LogDebug("Request Body: {RequestBody}", body);
                }
            }
        }

        private async Task LogResponseAsync(HttpContext context, string requestId, long elapsedMilliseconds)
        {
            var response = context.Response;
            var responseBody = await ReadResponseBodyAsync(response);

            var logData = new
            {
                RequestId = requestId,
                StatusCode = response.StatusCode,
                ContentType = response.ContentType,
                ContentLength = response.ContentLength,
                Headers = GetSafeHeaders(response.Headers),
                ElapsedMilliseconds = elapsedMilliseconds,
                ResponseBody = _configuration.GetValue<bool>("Logging:LogResponseBody") ? responseBody : null,
                Timestamp = DateTime.UtcNow
            };

            var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
            _logger.Log(logLevel, "API Response: {ResponseData}", JsonSerializer.Serialize(logData));
        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            try
            {
                request.Body.Position = 0;
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
                return body;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read request body");
                return string.Empty;
            }
        }

        private async Task<string> ReadResponseBodyAsync(HttpResponse response)
        {
            try
            {
                response.Body.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                response.Body.Seek(0, SeekOrigin.Begin);
                return body;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read response body");
                return string.Empty;
            }
        }

        private Dictionary<string, string> GetSafeHeaders(IHeaderDictionary headers)
        {
            var safeHeaders = new Dictionary<string, string>();
            var sensitiveHeaders = new[] { "authorization", "cookie", "x-api-key" };

            foreach (var header in headers)
            {
                if (sensitiveHeaders.Contains(header.Key.ToLowerInvariant()))
                {
                    safeHeaders[header.Key] = "[REDACTED]";
                }
                else
                {
                    safeHeaders[header.Key] = string.Join(", ", header.Value);
                }
            }

            return safeHeaders;
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, string requestId)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                RequestId = requestId,
                Message = "An unexpected error occurred",
                Timestamp = DateTime.UtcNow,
                Error = _configuration.GetValue<bool>("IncludeDetailedErrors") ? exception.ToString() : null
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// Extension method to register the API logging middleware
    /// </summary>
    public static class ApiLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiLoggingMiddleware>();
        }
    }
}