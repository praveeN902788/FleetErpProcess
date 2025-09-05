using Bharuwa.Erp.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Bharuwa.Erp.API.FMS.Controllers.Base
{
    /// <summary>
    /// Enhanced base controller providing comprehensive error handling, logging, and API versioning
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public abstract class EnhancedBaseController : ControllerBase
    {
        protected readonly ILogger<EnhancedBaseController> _logger;
        protected readonly IConfiguration _configuration;

        protected EnhancedBaseController(ILogger<EnhancedBaseController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Creates a standardized API response with version information
        /// </summary>
        protected APIResponseDto CreateApiResponse<T>(T data, string message = "Success", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var apiVersion = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
            
            return new APIResponseDto
            {
                ApiVersion = apiVersion,
                Data = data,
                Message = message,
                StatusCode = (int)statusCode,
                Timestamp = DateTime.UtcNow,
                RequestId = HttpContext.TraceIdentifier
            };
        }

        /// <summary>
        /// Creates an error response with proper error handling
        /// </summary>
        protected APIResponseDto CreateErrorResponse(Exception ex, string customMessage = null)
        {
            var apiVersion = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
            
            _logger.LogError(ex, "Error occurred in {ControllerName} at {Timestamp}", 
                GetType().Name, DateTime.UtcNow);

            return new APIResponseDto
            {
                ApiVersion = apiVersion,
                Data = null,
                Message = customMessage ?? GetErrorMessage(ex),
                StatusCode = GetHttpStatusCode(ex),
                Timestamp = DateTime.UtcNow,
                RequestId = HttpContext.TraceIdentifier,
                Error = new ErrorDetails
                {
                    Type = ex.GetType().Name,
                    Details = _configuration.GetValue<bool>("IncludeDetailedErrors") ? ex.ToString() : null
                }
            };
        }

        /// <summary>
        /// Wrapper for async operations with comprehensive error handling
        /// </summary>
        protected async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> operation, string successMessage = "Operation completed successfully")
        {
            try
            {
                _logger.LogInformation("Starting operation in {ControllerName}", GetType().Name);
                
                var result = await operation();
                var response = CreateApiResponse(result, successMessage);
                
                _logger.LogInformation("Operation completed successfully in {ControllerName}", GetType().Name);
                
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                var errorResponse = CreateErrorResponse(ex);
                return StatusCode(errorResponse.StatusCode, errorResponse);
            }
        }

        /// <summary>
        /// Wrapper for file operations
        /// </summary>
        protected async Task<IActionResult> ExecuteFileAsync(Func<Task<IActionResult>> operation)
        {
            try
            {
                _logger.LogInformation("Starting file operation in {ControllerName}", GetType().Name);
                
                var result = await operation();
                
                _logger.LogInformation("File operation completed successfully in {ControllerName}", GetType().Name);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File operation failed in {ControllerName}", GetType().Name);
                var errorResponse = CreateErrorResponse(ex);
                return StatusCode(errorResponse.StatusCode, errorResponse);
            }
        }

        /// <summary>
        /// Validates model state and returns appropriate error response if invalid
        /// </summary>
        protected IActionResult ValidateModelState()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var errorResponse = CreateErrorResponse(
                    new ValidationException("Model validation failed"), 
                    "Invalid request data");
                
                errorResponse.Error = new ErrorDetails
                {
                    Type = "ValidationException",
                    Details = errors
                };

                return BadRequest(errorResponse);
            }

            return null;
        }

        /// <summary>
        /// Gets appropriate HTTP status code based on exception type
        /// </summary>
        private static HttpStatusCode GetHttpStatusCode(Exception ex)
        {
            return ex switch
            {
                ArgumentNullException or ArgumentException => HttpStatusCode.BadRequest,
                KeyNotFoundException => HttpStatusCode.NotFound,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                InvalidOperationException => HttpStatusCode.BadRequest,
                BposException => HttpStatusCode.BadRequest,
                SqlException => HttpStatusCode.BadRequest,
                TimeoutException => HttpStatusCode.GatewayTimeout,
                ValidationException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };
        }

        /// <summary>
        /// Gets user-friendly error message based on exception type
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
                BposException => ex.Message,
                SqlException => "A database error occurred. Please try again later.",
                TimeoutException => "The request timed out. Please try again.",
                ValidationException => "Validation failed for the provided data.",
                _ => "An unexpected error occurred. Please try again later."
            };
        }
    }

    /// <summary>
    /// Error details for API responses
    /// </summary>
    public class ErrorDetails
    {
        public string Type { get; set; }
        public object Details { get; set; }
    }
}