using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Bharuwa.Erp.API.FMS.Services
{
    /// <summary>
    /// Service for managing API versions and providing version-specific functionality
    /// </summary>
    public interface IVersionManagementService
    {
        string GetCurrentApiVersion(HttpContext context);
        bool IsVersionSupported(string version);
        string GetLatestVersion();
        IEnumerable<string> GetSupportedVersions();
        bool IsDeprecatedVersion(string version);
        string GetDeprecationMessage(string version);
    }

    public class VersionManagementService : IVersionManagementService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VersionManagementService> _logger;
        private readonly Dictionary<string, VersionInfo> _versionInfo;

        public VersionManagementService(IConfiguration configuration, ILogger<VersionManagementService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _versionInfo = InitializeVersionInfo();
        }

        public string GetCurrentApiVersion(HttpContext context)
        {
            var version = context.GetRequestedApiVersion()?.ToString() ?? "1.0";
            _logger.LogDebug("Current API version: {Version}", version);
            return version;
        }

        public bool IsVersionSupported(string version)
        {
            return _versionInfo.ContainsKey(version);
        }

        public string GetLatestVersion()
        {
            return _versionInfo.Values
                .Where(v => !v.IsDeprecated)
                .OrderByDescending(v => Version.Parse(v.Version))
                .FirstOrDefault()?.Version ?? "1.0";
        }

        public IEnumerable<string> GetSupportedVersions()
        {
            return _versionInfo.Keys;
        }

        public bool IsDeprecatedVersion(string version)
        {
            return _versionInfo.TryGetValue(version, out var info) && info.IsDeprecated;
        }

        public string GetDeprecationMessage(string version)
        {
            if (_versionInfo.TryGetValue(version, out var info) && info.IsDeprecated)
            {
                return info.DeprecationMessage;
            }
            return null;
        }

        private Dictionary<string, VersionInfo> InitializeVersionInfo()
        {
            return new Dictionary<string, VersionInfo>
            {
                ["1.0"] = new VersionInfo
                {
                    Version = "1.0",
                    ReleaseDate = new DateTime(2024, 1, 1),
                    IsDeprecated = false,
                    DeprecationMessage = null,
                    Features = new[] { "Basic CRUD operations", "Authentication", "Basic reporting" }
                },
                ["2.0"] = new VersionInfo
                {
                    Version = "2.0",
                    ReleaseDate = new DateTime(2024, 6, 1),
                    IsDeprecated = false,
                    DeprecationMessage = null,
                    Features = new[] { "Enhanced filtering", "Advanced reporting", "Real-time notifications", "Improved error handling" }
                },
                ["1.5"] = new VersionInfo
                {
                    Version = "1.5",
                    ReleaseDate = new DateTime(2024, 3, 1),
                    IsDeprecated = true,
                    DeprecationMessage = "Version 1.5 is deprecated. Please upgrade to version 2.0 for continued support.",
                    Features = new[] { "Intermediate features", "Bug fixes" }
                }
            };
        }
    }

    /// <summary>
    /// Information about a specific API version
    /// </summary>
    public class VersionInfo
    {
        public string Version { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool IsDeprecated { get; set; }
        public string DeprecationMessage { get; set; }
        public string[] Features { get; set; }
    }
}