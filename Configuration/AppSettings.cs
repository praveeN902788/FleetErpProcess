namespace Bharuwa.Erp.API.FMS.Configuration
{
    public class AppSettings
    {
        public DatabaseSettings Database { get; set; } = new();
        public JwtSettings Jwt { get; set; } = new();
        public CorsSettings Cors { get; set; } = new();
        public LoggingSettings Logging { get; set; } = new();
        public SecuritySettings Security { get; set; } = new();
    }

    public class DatabaseSettings
    {
        public string MasterConnectionString { get; set; } = string.Empty;
        public string UserConnectionString { get; set; } = string.Empty;
        public string LogConnectionString { get; set; } = string.Empty;
        public int CommandTimeout { get; set; } = 30;
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 2;
    }

    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; } = 60;
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }

    public class CorsSettings
    {
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
        public string[] AllowedMethods { get; set; } = new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
        public string[] AllowedHeaders { get; set; } = new[] { "Authorization", "Content-Type", "X-Requested-With" };
        public bool AllowCredentials { get; set; } = true;
    }

    public class LoggingSettings
    {
        public string LogLevel { get; set; } = "Information";
        public string LogFilePath { get; set; } = "logs/bharuwa-erp-.txt";
        public int RetentionDays { get; set; } = 30;
        public bool EnableConsoleLogging { get; set; } = true;
        public bool EnableFileLogging { get; set; } = true;
    }

    public class SecuritySettings
    {
        public bool RequireHttps { get; set; } = true;
        public int MaxLoginAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 15;
        public bool EnableRateLimiting { get; set; } = true;
        public int RateLimitRequestsPerMinute { get; set; } = 100;
    }
}