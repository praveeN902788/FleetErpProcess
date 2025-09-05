using Microsoft.AspNetCore.Mvc;

namespace Bharuwa.Erp.API.FMS.Controllers
{
    /// <summary>
    /// Controller for API version management and information
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class VersionController : ControllerBase
    {
        private readonly IVersionManagementService _versionService;
        private readonly ILogger<VersionController> _logger;

        public VersionController(
            IVersionManagementService versionService,
            ILogger<VersionController> logger)
        {
            _versionService = versionService;
            _logger = logger;
        }

        /// <summary>
        /// Gets information about all supported API versions
        /// </summary>
        /// <returns>Version information</returns>
        [HttpGet]
        [ProducesResponseType(typeof(VersionInfoResponse), 200)]
        public IActionResult GetVersionInfo()
        {
            _logger.LogInformation("Retrieving API version information");

            var response = new VersionInfoResponse
            {
                CurrentVersion = _versionService.GetCurrentApiVersion(HttpContext),
                LatestVersion = _versionService.GetLatestVersion(),
                SupportedVersions = _versionService.GetSupportedVersions().ToArray(),
                VersionDetails = GetVersionDetails(),
                Timestamp = DateTime.UtcNow,
                RequestId = HttpContext.TraceIdentifier
            };

            return Ok(response);
        }

        /// <summary>
        /// Gets detailed information about a specific API version
        /// </summary>
        /// <param name="version">API version to get information for</param>
        /// <returns>Detailed version information</returns>
        [HttpGet("{version}")]
        [ProducesResponseType(typeof(VersionDetailResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public IActionResult GetVersionDetail(string version)
        {
            _logger.LogInformation("Retrieving detailed information for API version {Version}", version);

            if (!_versionService.IsVersionSupported(version))
            {
                return NotFound(new ErrorResponse
                {
                    Message = $"API version {version} is not supported",
                    SupportedVersions = _versionService.GetSupportedVersions().ToArray(),
                    Timestamp = DateTime.UtcNow,
                    RequestId = HttpContext.TraceIdentifier
                });
            }

            var response = new VersionDetailResponse
            {
                Version = version,
                IsDeprecated = _versionService.IsDeprecatedVersion(version),
                DeprecationMessage = _versionService.GetDeprecationMessage(version),
                VersionDetails = GetVersionDetails().FirstOrDefault(v => v.Version == version),
                Timestamp = DateTime.UtcNow,
                RequestId = HttpContext.TraceIdentifier
            };

            return Ok(response);
        }

        /// <summary>
        /// Gets the latest API version information
        /// </summary>
        /// <returns>Latest version information</returns>
        [HttpGet("latest")]
        [ProducesResponseType(typeof(VersionDetailResponse), 200)]
        public IActionResult GetLatestVersion()
        {
            var latestVersion = _versionService.GetLatestVersion();
            _logger.LogInformation("Retrieving latest API version: {Version}", latestVersion);

            return GetVersionDetail(latestVersion);
        }

        /// <summary>
        /// Gets deprecation information for deprecated versions
        /// </summary>
        /// <returns>Deprecation information</returns>
        [HttpGet("deprecated")]
        [ProducesResponseType(typeof(DeprecationInfoResponse), 200)]
        public IActionResult GetDeprecationInfo()
        {
            _logger.LogInformation("Retrieving deprecation information");

            var deprecatedVersions = _versionService.GetSupportedVersions()
                .Where(v => _versionService.IsDeprecatedVersion(v))
                .Select(v => new DeprecatedVersionInfo
                {
                    Version = v,
                    DeprecationMessage = _versionService.GetDeprecationMessage(v),
                    SunsetDate = DateTime.UtcNow.AddDays(90) // Example sunset date
                })
                .ToArray();

            var response = new DeprecationInfoResponse
            {
                DeprecatedVersions = deprecatedVersions,
                LatestVersion = _versionService.GetLatestVersion(),
                MigrationGuide = GetMigrationGuide(),
                Timestamp = DateTime.UtcNow,
                RequestId = HttpContext.TraceIdentifier
            };

            return Ok(response);
        }

        private VersionInfo[] GetVersionDetails()
        {
            return new[]
            {
                new VersionInfo
                {
                    Version = "1.0",
                    ReleaseDate = new DateTime(2024, 1, 1),
                    IsDeprecated = false,
                    DeprecationMessage = null,
                    Features = new[] { "Basic CRUD operations", "Authentication", "Basic reporting" },
                    Endpoints = new[] { "GET /api/v1/customer/dashboard/counts", "GET /api/v1/customer/dashboard/details", "POST /api/v1/customer/tracking", "GET /api/v1/customer/vehicle-categories" }
                },
                new VersionInfo
                {
                    Version = "2.0",
                    ReleaseDate = new DateTime(2024, 6, 1),
                    IsDeprecated = false,
                    DeprecationMessage = null,
                    Features = new[] { "Enhanced filtering", "Advanced reporting", "Real-time notifications", "Improved error handling", "Analytics" },
                    Endpoints = new[] { "POST /api/v2/customer/dashboard/counts", "POST /api/v2/customer/dashboard/details", "POST /api/v2/customer/tracking", "GET /api/v2/customer/vehicle-categories", "POST /api/v2/customer/analytics" }
                },
                new VersionInfo
                {
                    Version = "1.5",
                    ReleaseDate = new DateTime(2024, 3, 1),
                    IsDeprecated = true,
                    DeprecationMessage = "Version 1.5 is deprecated. Please upgrade to version 2.0 for continued support.",
                    Features = new[] { "Intermediate features", "Bug fixes" },
                    Endpoints = new[] { "Legacy endpoints" }
                }
            };
        }

        private MigrationGuide GetMigrationGuide()
        {
            return new MigrationGuide
            {
                FromVersion = "1.0",
                ToVersion = "2.0",
                BreakingChanges = new[]
                {
                    "Dashboard endpoints now use POST instead of GET for complex filtering",
                    "Vehicle categories endpoint now supports type parameter",
                    "Enhanced error responses with additional metadata"
                },
                NewFeatures = new[]
                {
                    "Real-time analytics endpoint",
                    "Enhanced filtering capabilities",
                    "Improved error handling",
                    "Version-aware responses"
                },
                MigrationSteps = new[]
                {
                    "Update client code to use POST for dashboard endpoints",
                    "Add type parameter to vehicle categories requests",
                    "Handle new response structure with version information",
                    "Implement new analytics endpoints for enhanced functionality"
                }
            };
        }
    }

    #region Response Models

    public class VersionInfoResponse
    {
        public string CurrentVersion { get; set; }
        public string LatestVersion { get; set; }
        public string[] SupportedVersions { get; set; }
        public VersionInfo[] VersionDetails { get; set; }
        public DateTime Timestamp { get; set; }
        public string RequestId { get; set; }
    }

    public class VersionDetailResponse
    {
        public string Version { get; set; }
        public bool IsDeprecated { get; set; }
        public string DeprecationMessage { get; set; }
        public VersionInfo VersionDetails { get; set; }
        public DateTime Timestamp { get; set; }
        public string RequestId { get; set; }
    }

    public class DeprecationInfoResponse
    {
        public DeprecatedVersionInfo[] DeprecatedVersions { get; set; }
        public string LatestVersion { get; set; }
        public MigrationGuide MigrationGuide { get; set; }
        public DateTime Timestamp { get; set; }
        public string RequestId { get; set; }
    }

    public class DeprecatedVersionInfo
    {
        public string Version { get; set; }
        public string DeprecationMessage { get; set; }
        public DateTime SunsetDate { get; set; }
    }

    public class MigrationGuide
    {
        public string FromVersion { get; set; }
        public string ToVersion { get; set; }
        public string[] BreakingChanges { get; set; }
        public string[] NewFeatures { get; set; }
        public string[] MigrationSteps { get; set; }
    }

    public class ErrorResponse
    {
        public string Message { get; set; }
        public string[] SupportedVersions { get; set; }
        public DateTime Timestamp { get; set; }
        public string RequestId { get; set; }
    }

    #endregion
}