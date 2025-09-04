using Bharuwa.Erp.API.FMS.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bharuwa.Erp.API.FMS.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigurationService> _logger;
        private AppSettings? _cachedAppSettings;

        public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public AppSettings GetAppSettings()
        {
            if (_cachedAppSettings == null)
            {
                _cachedAppSettings = new AppSettings();
                _configuration.Bind(_cachedAppSettings);
                
                ValidateAppSettings(_cachedAppSettings);
            }

            return _cachedAppSettings;
        }

        public DatabaseSettings GetDatabaseSettings()
        {
            return GetAppSettings().Database;
        }

        public JwtSettings GetJwtSettings()
        {
            return GetAppSettings().Jwt;
        }

        public CorsSettings GetCorsSettings()
        {
            return GetAppSettings().Cors;
        }

        public LoggingSettings GetLoggingSettings()
        {
            return GetAppSettings().Logging;
        }

        public SecuritySettings GetSecuritySettings()
        {
            return GetAppSettings().Security;
        }

        public string GetConnectionString(string name)
        {
            var connectionString = _configuration.GetConnectionString(name);
            
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Connection string '{ConnectionStringName}' not found", name);
                throw new InvalidOperationException($"Connection string '{name}' not found");
            }

            return connectionString;
        }

        public T GetSection<T>(string sectionName) where T : class, new()
        {
            var section = new T();
            _configuration.GetSection(sectionName).Bind(section);
            return section;
        }

        private void ValidateAppSettings(AppSettings settings)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(settings.Jwt.Key))
                errors.Add("JWT Key is required");

            if (string.IsNullOrEmpty(settings.Jwt.Issuer))
                errors.Add("JWT Issuer is required");

            if (string.IsNullOrEmpty(settings.Jwt.Audience))
                errors.Add("JWT Audience is required");

            if (settings.Jwt.ExpirationMinutes <= 0)
                errors.Add("JWT ExpirationMinutes must be greater than 0");

            if (settings.Database.CommandTimeout <= 0)
                errors.Add("Database CommandTimeout must be greater than 0");

            if (settings.Security.MaxLoginAttempts <= 0)
                errors.Add("Security MaxLoginAttempts must be greater than 0");

            if (errors.Any())
            {
                var errorMessage = $"Configuration validation failed: {string.Join(", ", errors)}";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            _logger.LogInformation("Configuration validation completed successfully");
        }
    }
}