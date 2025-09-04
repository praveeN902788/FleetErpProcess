using Bharuwa.Erp.API.FMS.Configuration;

namespace Bharuwa.Erp.API.FMS.Services
{
    public interface IConfigurationService
    {
        AppSettings GetAppSettings();
        DatabaseSettings GetDatabaseSettings();
        JwtSettings GetJwtSettings();
        CorsSettings GetCorsSettings();
        LoggingSettings GetLoggingSettings();
        SecuritySettings GetSecuritySettings();
        string GetConnectionString(string name);
        T GetSection<T>(string sectionName) where T : class, new();
    }
}