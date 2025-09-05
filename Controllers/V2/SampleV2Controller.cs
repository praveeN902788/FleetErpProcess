using Microsoft.AspNetCore.Mvc;

namespace Bharuwa.Erp.API.FMS.Controllers.V2
{
    /// <summary>
    /// Sample controller demonstrating the enhanced architecture features
    /// This controller showcases all the capabilities of the new API versioning and error handling system
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/sample")]
    [ApiVersion("2.0")]
    [Produces("application/json")]
    public class SampleV2Controller : VersionAwareController
    {
        public SampleV2Controller(
            IVersionManagementService versionService,
            ILogger<SampleV2Controller> logger) 
            : base(versionService, logger)
        {
        }

        /// <summary>
        /// Demonstrates successful operation with enhanced response
        /// </summary>
        /// <param name="request">Sample request data</param>
        /// <returns>Enhanced response with version information</returns>
        [HttpPost("success")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        [ProducesResponseType(typeof(APIResponseDto), 400)]
        [ProducesResponseType(typeof(APIResponseDto), 500)]
        public async Task<IActionResult> DemonstrateSuccess([FromBody] SampleRequest request)
        {
            if (request == null)
            {
                return BadRequest(CreateVersionedErrorResponse(
                    new ArgumentNullException(nameof(request)), 
                    "Sample request is required"));
            }

            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return validationResult;
            }

            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Processing sample request: {Request}", 
                    System.Text.Json.JsonSerializer.Serialize(request));
                
                // Simulate some processing
                await Task.Delay(100);
                
                var result = new
                {
                    Message = "Operation completed successfully",
                    RequestData = request,
                    ProcessedAt = DateTime.UtcNow,
                    Version = CurrentApiVersion,
                    Features = new[]
                    {
                        "Enhanced error handling",
                        "Version-aware responses",
                        "Comprehensive logging",
                        "Structured responses"
                    }
                };
                
                return result;
            }, "Sample operation completed successfully");
        }

        /// <summary>
        /// Demonstrates error handling with different exception types
        /// </summary>
        /// <param name="errorType">Type of error to simulate</param>
        /// <returns>Error response demonstrating error handling</returns>
        [HttpGet("error/{errorType}")]
        [ProducesResponseType(typeof(APIResponseDto), 400)]
        [ProducesResponseType(typeof(APIResponseDto), 404)]
        [ProducesResponseType(typeof(APIResponseDto), 500)]
        public async Task<IActionResult> DemonstrateErrorHandling(string errorType)
        {
            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Demonstrating error handling for type: {ErrorType}", errorType);
                
                // Simulate different types of errors
                switch (errorType.ToLowerInvariant())
                {
                    case "validation":
                        throw new ArgumentException("This is a validation error");
                    case "notfound":
                        throw new KeyNotFoundException("The requested resource was not found");
                    case "unauthorized":
                        throw new UnauthorizedAccessException("You do not have permission to access this resource");
                    case "timeout":
                        throw new TimeoutException("The operation timed out");
                    case "database":
                        throw new InvalidOperationException("Database operation failed");
                    default:
                        throw new Exception("This is a generic error");
                }
            }, "Error demonstration completed");
        }

        /// <summary>
        /// Demonstrates version information and deprecation handling
        /// </summary>
        /// <returns>Version information and metadata</returns>
        [HttpGet("version-info")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        public async Task<IActionResult> GetVersionInformation()
        {
            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Retrieving version information");
                
                var result = new
                {
                    CurrentVersion = CurrentApiVersion,
                    LatestVersion = _versionService.GetLatestVersion(),
                    SupportedVersions = _versionService.GetSupportedVersions(),
                    IsDeprecated = _versionService.IsDeprecatedVersion(CurrentApiVersion),
                    DeprecationMessage = _versionService.GetDeprecationMessage(CurrentApiVersion),
                    VersionFeatures = new
                    {
                        V1 = new[] { "Basic CRUD operations", "Simple authentication", "Basic reporting" },
                        V2 = new[] { "Enhanced filtering", "Advanced reporting", "Real-time notifications", "Analytics" }
                    },
                    MigrationGuide = new
                    {
                        FromV1ToV2 = new[]
                        {
                            "Update endpoint methods from GET to POST for complex queries",
                            "Use structured request objects instead of query parameters",
                            "Handle new response format with version information",
                            "Implement new analytics endpoints"
                        }
                    }
                };
                
                return result;
            }, "Version information retrieved successfully");
        }

        /// <summary>
        /// Demonstrates performance monitoring and logging
        /// </summary>
        /// <param name="delayMs">Delay in milliseconds to simulate processing time</param>
        /// <returns>Performance metrics</returns>
        [HttpGet("performance/{delayMs:int}")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        [ProducesResponseType(typeof(APIResponseDto), 400)]
        public async Task<IActionResult> DemonstratePerformanceMonitoring(int delayMs)
        {
            if (delayMs < 0 || delayMs > 10000)
            {
                return BadRequest(CreateVersionedErrorResponse(
                    new ArgumentException("Delay must be between 0 and 10000 milliseconds"), 
                    "Invalid delay parameter"));
            }

            return await ExecuteVersionedAsync(async () =>
            {
                var startTime = DateTime.UtcNow;
                _logger.LogInformation("Starting performance demonstration with delay: {DelayMs}ms", delayMs);
                
                // Simulate processing time
                await Task.Delay(delayMs);
                
                var endTime = DateTime.UtcNow;
                var actualDelay = (endTime - startTime).TotalMilliseconds;
                
                var result = new
                {
                    RequestedDelay = delayMs,
                    ActualDelay = actualDelay,
                    StartTime = startTime,
                    EndTime = endTime,
                    ProcessingTime = actualDelay,
                    PerformanceMetrics = new
                    {
                        IsWithinExpectedRange = actualDelay <= delayMs + 50, // Allow 50ms tolerance
                        Efficiency = delayMs > 0 ? (delayMs / actualDelay) * 100 : 100,
                        Status = actualDelay < 1000 ? "Fast" : actualDelay < 5000 ? "Normal" : "Slow"
                    }
                };
                
                _logger.LogInformation("Performance demonstration completed. Actual delay: {ActualDelay}ms", actualDelay);
                
                return result;
            }, "Performance monitoring demonstration completed");
        }

        /// <summary>
        /// Demonstrates comprehensive request/response logging
        /// </summary>
        /// <param name="request">Complex request with various data types</param>
        /// <returns>Response demonstrating logging capabilities</returns>
        [HttpPost("logging-demo")]
        [ProducesResponseType(typeof(APIResponseDto), 200)]
        [ProducesResponseType(typeof(APIResponseDto), 400)]
        public async Task<IActionResult> DemonstrateLogging([FromBody] ComplexSampleRequest request)
        {
            if (request == null)
            {
                return BadRequest(CreateVersionedErrorResponse(
                    new ArgumentNullException(nameof(request)), 
                    "Complex sample request is required"));
            }

            var validationResult = ValidateModelState();
            if (validationResult != null)
            {
                return validationResult;
            }

            return await ExecuteVersionedAsync(async () =>
            {
                _logger.LogInformation("Starting comprehensive logging demonstration");
                _logger.LogDebug("Request details: {Request}", System.Text.Json.JsonSerializer.Serialize(request));
                
                // Simulate various operations that would generate different log levels
                _logger.LogTrace("Trace level logging - detailed execution flow");
                _logger.LogDebug("Debug level logging - variable values and flow control");
                _logger.LogInformation("Information level logging - general application flow");
                _logger.LogWarning("Warning level logging - potentially harmful situations");
                
                // Simulate some processing
                await Task.Delay(200);
                
                var result = new
                {
                    Message = "Logging demonstration completed",
                    RequestId = HttpContext.TraceIdentifier,
                    LoggedEvents = new[]
                    {
                        "Request received and validated",
                        "Processing started",
                        "Various log levels demonstrated",
                        "Processing completed",
                        "Response prepared"
                    },
                    LogLevelsUsed = new[]
                    {
                        "Trace - Detailed execution flow",
                        "Debug - Variable values and flow control",
                        "Information - General application flow",
                        "Warning - Potentially harmful situations"
                    },
                    RequestData = request,
                    ProcessedAt = DateTime.UtcNow
                };
                
                _logger.LogInformation("Comprehensive logging demonstration completed successfully");
                
                return result;
            }, "Logging demonstration completed successfully");
        }
    }

    #region Request Models

    public class SampleRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Range(1, 100)]
        public int Age { get; set; }
        
        public string[] Tags { get; set; } = Array.Empty<string>();
        
        public bool IncludeMetadata { get; set; } = false;
    }

    public class ComplexSampleRequest
    {
        [Required]
        public string Operation { get; set; }
        
        public Dictionary<string, object> Parameters { get; set; } = new();
        
        public SampleRequest[] Items { get; set; } = Array.Empty<SampleRequest>();
        
        public DateTime? ScheduledTime { get; set; }
        
        public bool EnableLogging { get; set; } = true;
        
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }

    #endregion
}