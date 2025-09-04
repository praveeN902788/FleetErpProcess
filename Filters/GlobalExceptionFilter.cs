using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System.Net;

namespace Bharuwa.Erp.API.FMS.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "An unhandled exception occurred: {Message}", context.Exception.Message);

            var errorResponse = new ApiErrorResponse
            {
                TraceId = context.HttpContext.TraceIdentifier,
                Message = _environment.IsDevelopment() 
                    ? context.Exception.Message 
                    : "An unexpected error occurred. Please try again later.",
                Details = _environment.IsDevelopment() ? context.Exception.ToString() : null
            };

            context.Result = context.Exception switch
            {
                ArgumentNullException => new BadRequestObjectResult(errorResponse),
                ArgumentException => new BadRequestObjectResult(errorResponse),
                KeyNotFoundException => new NotFoundObjectResult(errorResponse),
                UnauthorizedAccessException => new UnauthorizedObjectResult(errorResponse),
                InvalidOperationException => new BadRequestObjectResult(errorResponse),
                TimeoutException => new ObjectResult(errorResponse) { StatusCode = (int)HttpStatusCode.GatewayTimeout },
                _ => new ObjectResult(errorResponse) { StatusCode = (int)HttpStatusCode.InternalServerError }
            };

            context.ExceptionHandled = true;
        }
    }

    public class ApiErrorResponse
    {
        public string TraceId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}