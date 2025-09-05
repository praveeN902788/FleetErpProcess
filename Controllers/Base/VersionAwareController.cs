using Bharuwa.Erp.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bharuwa.Erp.API.FMS.Controllers.Base
{
    /// <summary>
    /// Base controller with integrated API version management
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public abstract class VersionAwareController : ControllerBase
    {
        protected readonly IVersionManagementService _versionService;
        protected readonly ILogger<VersionAwareController> _logger;

        protected VersionAwareController(
            IVersionManagementService versionService, 
            ILogger<VersionAwareController> logger)
        {
            _versionService = versionService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the current API version from the request context
        /// </summary>
        protected string CurrentApiVersion => _versionService.GetCurrentApiVersion(HttpContext);

        /// <summary>
        /// Checks if the current version is deprecated and adds deprecation headers
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            
            var version = CurrentApiVersion;
            
            if (_versionService.IsDeprecatedVersion(version))
            {
                var deprecationMessage = _versionService.GetDeprecationMessage(version);
                
                Response.Headers.Add("Deprecation", "true");
                Response.Headers.Add("Sunset", DateTime.UtcNow.AddDays(90).ToString("R")); // 90 days from now
                Response.Headers.Add("Link", $"<{Request.Scheme}://{Request.Host}/api/v{_versionService.GetLatestVersion()}>; rel=\"successor-version\"");
                
                _logger.LogWarning("Deprecated API version {Version} accessed. Message: {Message}", 
                    version, deprecationMessage);
            }

            // Add version information to response headers
            Response.Headers.Add("API-Version", version);
            Response.Headers.Add("API-Supported-Versions", string.Join(", ", _versionService.GetSupportedVersions()));
        }

        /// <summary>
        /// Creates a version-aware API response
        /// </summary>
        protected APIResponseDto CreateVersionedResponse<T>(T data, string message = "Success")
        {
            var version = CurrentApiVersion;
            
            return new APIResponseDto
            {
                ApiVersion = version,
                Data = data,
                Message = message,
                Timestamp = DateTime.UtcNow,
                RequestId = HttpContext.TraceIdentifier,
                VersionInfo = new VersionResponseInfo
                {
                    CurrentVersion = version,
                    LatestVersion = _versionService.GetLatestVersion(),
                    IsDeprecated = _versionService.IsDeprecatedVersion(version),
                    DeprecationMessage = _versionService.GetDeprecationMessage(version),
                    SupportedVersions = _versionService.GetSupportedVersions().ToArray()
                }
            };
        }

        /// <summary>
        /// Creates a version-aware error response
        /// </summary>
        protected APIResponseDto CreateVersionedErrorResponse(Exception ex, string customMessage = null)
        {
            var version = CurrentApiVersion;
            
            _logger.LogError(ex, "Error in {ControllerName} v{Version}", GetType().Name, version);

            return new APIResponseDto
            {
                ApiVersion = version,
                Data = null,
                Message = customMessage ?? GetErrorMessage(ex),
                Timestamp = DateTime.UtcNow,
                RequestId = HttpContext.TraceIdentifier,
                VersionInfo = new VersionResponseInfo
                {
                    CurrentVersion = version,
                    LatestVersion = _versionService.GetLatestVersion(),
                    IsDeprecated = _versionService.IsDeprecatedVersion(version),
                    DeprecationMessage = _versionService.GetDeprecationMessage(version),
                    SupportedVersions = _versionService.GetSupportedVersions().ToArray()
                }
            };
        }

        /// <summary>
        /// Executes an operation with version-aware error handling
        /// </summary>
        protected async Task<IActionResult> ExecuteVersionedAsync<T>(
            Func<Task<T>> operation, 
            string successMessage = "Operation completed successfully")
        {
            try
            {
                var result = await operation();
                var response = CreateVersionedResponse(result, successMessage);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = CreateVersionedErrorResponse(ex);
                return StatusCode(GetHttpStatusCode(ex), errorResponse);
            }
        }

        /// <summary>
        /// Gets HTTP status code based on exception type
        /// </summary>
        private static int GetHttpStatusCode(Exception ex)
        {
            return ex switch
            {
                ArgumentNullException or ArgumentException => 400,
                KeyNotFoundException => 404,
                UnauthorizedAccessException => 401,
                InvalidOperationException => 400,
                BposException => 400,
                TimeoutException => 504,
                _ => 500
            };
        }

        /// <summary>
        /// Gets user-friendly error message
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
                TimeoutException => "The request timed out. Please try again.",
                _ => "An unexpected error occurred. Please try again later."
            };
        }
    }

    /// <summary>
    /// Version information included in API responses
    /// </summary>
    public class VersionResponseInfo
    {
        public string CurrentVersion { get; set; }
        public string LatestVersion { get; set; }
        public bool IsDeprecated { get; set; }
        public string DeprecationMessage { get; set; }
        public string[] SupportedVersions { get; set; }
    }
}