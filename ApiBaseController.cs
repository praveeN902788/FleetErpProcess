using Bharuwa.Erp.Common;
using ERPServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Serilog;
using System.Diagnostics;

namespace Bharuwa.Erp.API.FMS
{
    /// <summary>
    /// Base controller providing common functionality for all API controllers
    /// Enhanced with modern logging, performance monitoring, and improved error handling
    /// </summary>
    [BasicAuthentication]
    public class ApiBaseController : Controller
    {
        private readonly ILogger<ApiBaseController> _logger;
        private readonly Stopwatch _stopwatch;

        public ApiBaseController(ILogger<ApiBaseController> logger)
        {
            _logger = logger;
            _stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Enhanced action execution with performance monitoring and logging
        /// </summary>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _stopwatch.Start();
            var actionName = context.ActionDescriptor.DisplayName;
            var requestId = HttpContext.TraceIdentifier;

            _logger.LogInformation("Action {ActionName} started for request {RequestId}", actionName, requestId);

            try
            {
                await base.OnActionExecutionAsync(context, next);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Action {ActionName} failed for request {RequestId}", actionName, requestId);
                throw;
            }
            finally
            {
                _stopwatch.Stop();
                _logger.LogInformation("Action {ActionName} completed in {ElapsedMs}ms for request {RequestId}", 
                    actionName, _stopwatch.ElapsedMilliseconds, requestId);
            }
        }

        /// <summary>
        /// Enhanced response wrapper with improved error handling and logging
        /// </summary>
        public async Task<IActionResult> ResponseWrapperAsync<T>(Func<Task<T>> func, string operationName = "Operation")
        {
            var requestId = HttpContext.TraceIdentifier;
            _logger.LogDebug("Starting {OperationName} for request {RequestId}", operationName, requestId);

            try
            {
                var result = await func();
                
                _logger.LogDebug("{OperationName} completed successfully for request {RequestId}", operationName, requestId);
                
                return new OkObjectResult(new ApiResponse<T>
                {
                    Status = "success",
                    Message = $"{operationName} completed successfully",
                    Data = result,
                    TraceId = requestId,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{OperationName} failed for request {RequestId}", operationName, requestId);
                return HandleError(ex, requestId);
            }
        }

        /// <summary>
        /// Enhanced file response wrapper with improved error handling
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ResponseFileWrapperAsync(Func<Task<IActionResult>> func, string operationName = "File Operation")
        {
            var requestId = HttpContext.TraceIdentifier;
            _logger.LogDebug("Starting {OperationName} for request {RequestId}", operationName, requestId);

            try
            {
                var result = await func();
                
                _logger.LogDebug("{OperationName} completed successfully for request {RequestId}", operationName, requestId);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{OperationName} failed for request {RequestId}", operationName, requestId);
                return HandleError(ex, requestId);
            }
        }

        /// <summary>
        /// Enhanced error handling with structured logging and improved error responses
        /// </summary>
        private IActionResult HandleError(Exception ex, string requestId)
        {
            var errorResponse = new ApiErrorResponse
            {
                TraceId = requestId,
                Message = GetErrorMessage(ex),
                Details = _logger.IsEnabled(LogLevel.Debug) ? ex.ToString() : null,
                Timestamp = DateTime.UtcNow
            };

            return ex switch
            {
                ArgumentNullException => new BadRequestObjectResult(errorResponse),
                ArgumentException => new BadRequestObjectResult(errorResponse),
                KeyNotFoundException => new NotFoundObjectResult(errorResponse),
                UnauthorizedAccessException => new UnauthorizedObjectResult(errorResponse),
                InvalidOperationException => new BadRequestObjectResult(errorResponse),
                BposException => new BadRequestObjectResult(new ApiErrorResponse 
                { 
                    TraceId = requestId, 
                    Message = ex.Message, 
                    Timestamp = DateTime.UtcNow 
                }),
                SqlException => new BadRequestObjectResult(new ApiErrorResponse 
                { 
                    TraceId = requestId, 
                    Message = "Database operation failed", 
                    Details = _logger.IsEnabled(LogLevel.Debug) ? ex.Message : null,
                    Timestamp = DateTime.UtcNow 
                }),
                TimeoutException => new ObjectResult(errorResponse) { StatusCode = 504 },
                _ => StatusCode(500, errorResponse)
            };
        }

        /// <summary>
        /// Get user-friendly error message based on exception type
        /// </summary>
        private static string GetErrorMessage(Exception ex)
        {
            return ex switch
            {
                ArgumentNullException => "A required parameter was null.",
                ArgumentException => "Invalid argument provided.",
                KeyNotFoundException => "The requested resource was not found.",
                UnauthorizedAccessException => "You do not have permission to perform this action.",
                InvalidOperationException => "The request could not be processed due to an invalid operation.",
                TimeoutException => "The operation timed out. Please try again.",
                _ => "An unexpected error occurred. Please try again later."
            };
        }

        /// <summary>
        /// Enhanced validation helper method
        /// </summary>
        protected IActionResult ValidateModel()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new ValidationError
                    {
                        Field = x.Key,
                        Messages = x.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    })
                    .ToArray();

                var response = new ValidationErrorResponse
                {
                    Message = "Validation failed",
                    Errors = errors,
                    TraceId = HttpContext.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                };

                return BadRequest(response);
            }

            return Ok();
        }

        /// <summary>
        /// Helper method to get current user information
        /// </summary>
        protected string? GetCurrentUserId()
        {
            return User?.FindFirst("UserId")?.Value;
        }

        /// <summary>
        /// Helper method to get current user email
        /// </summary>
        protected string? GetCurrentUserEmail()
        {
            return User?.FindFirst("Email")?.Value;
        }
    }

    /// <summary>
    /// Enhanced API response model with additional metadata
    /// </summary>
    public class ApiResponse<T>
    {
        public string Status { get; set; } = "success";
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string TraceId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Enhanced error response model
    /// </summary>
    public class ApiErrorResponse
    {
        public string TraceId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Validation error response model
    /// </summary>
    public class ValidationErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public ValidationError[] Errors { get; set; } = Array.Empty<ValidationError>();
        public string TraceId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Individual validation error model
    /// </summary>
    public class ValidationError
    {
        public string Field { get; set; } = string.Empty;
        public string[] Messages { get; set; } = Array.Empty<string>();
    }
}
