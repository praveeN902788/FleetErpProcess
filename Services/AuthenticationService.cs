using Bharuwa.Erp.API.FMS.Models;
using Bharuwa.Erp.Common;
using Bharuwa.Erp.Data;
using ERPServer.DataAccess;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Dapper;
using System.Data;

namespace Bharuwa.Erp.API.FMS.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IInitialDal _initialDal;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IDbContext _dbContext;

        public AuthenticationService(
            IInitialDal initialDal,
            ILogger<AuthenticationService> logger,
            IDbContext dbContext)
        {
            _initialDal = initialDal;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<AuthenticationResult> AuthenticateAsync(string token, string browserIdentityToken)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return new AuthenticationResult 
                    { 
                        IsAuthenticated = false, 
                        ErrorMessage = "Token is required" 
                    };
                }

                var connectionDetails = await _initialDal.getFirmConnectionDetails();
                using var connection = _initialDal.GetConnection(connectionDetails.Item2, connectionDetails.Item1);

                var tokenDetails = await connection.QueryFirstOrDefaultAsync<JwtTokenDetails>(
                    "SELECT BrowserIdentityToken, Expires_In, userprofiles, settings FROM JwtTokenDetails WHERE token = @token",
                    new { token });

                if (tokenDetails == null)
                {
                    _logger.LogWarning("Token not found: {Token}", token);
                    return new AuthenticationResult 
                    { 
                        IsAuthenticated = false, 
                        ErrorMessage = "Invalid token" 
                    };
                }

                if (!string.Equals(browserIdentityToken, tokenDetails.BrowserIdentityToken, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Browser identity token mismatch for token: {Token}", token);
                    return new AuthenticationResult 
                    { 
                        IsAuthenticated = false, 
                        ErrorMessage = "Browser identity token mismatch" 
                    };
                }

                if (tokenDetails.Expires_In < DateTime.UtcNow)
                {
                    _logger.LogWarning("Token expired: {Token}", token);
                    return new AuthenticationResult 
                    { 
                        IsAuthenticated = false, 
                        ErrorMessage = "Token expired" 
                    };
                }

                var systemSettings = JsonConvert.DeserializeObject<SystemSettings>(tokenDetails.settings);
                var userProfile = JsonConvert.DeserializeObject<UserProfile>(tokenDetails.userprofiles);

                if (systemSettings == null || userProfile == null)
                {
                    _logger.LogError("Failed to deserialize token details for token: {Token}", token);
                    return new AuthenticationResult 
                    { 
                        IsAuthenticated = false, 
                        ErrorMessage = "Invalid token data" 
                    };
                }

                var storageLocation = await connection.ExecuteScalarAsync<string>(
                    FileManager.GetQuery(DataQuery.GetStorageLocation), 
                    new { CompanyId = userProfile.CompanyInfo.COMPANYID });

                // Set up database context
                var masterDbConnectionString = _initialDal.getMasterDbConnectionString();
                var userConnectionString = connectionDetails.userConnectionString;
                var logDbConnectionString = connectionDetails.logDbConnectionString;

                _dbContext.setDbContext(
                    masterDbConnectionString,
                    userConnectionString,
                    userProfile.username,
                    userProfile.email,
                    systemSettings,
                    userProfile,
                    userProfile.EmployeeId,
                    logDbConnectionString,
                    (DataClientEnum)connectionDetails.databaseClient,
                    userProfile.CompanyInfo.COMPANYID,
                    storageLocation,
                    token);

                return new AuthenticationResult
                {
                    IsAuthenticated = true,
                    UserProfile = userProfile,
                    SystemSettings = systemSettings,
                    StorageLocation = storageLocation
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed for token: {Token}", token);
                return new AuthenticationResult 
                { 
                    IsAuthenticated = false, 
                    ErrorMessage = "Authentication failed" 
                };
            }
        }

        public async Task<AuthenticationResult> AuthenticateAnonymousAsync()
        {
            try
            {
                var connectionDetails = await _initialDal.getFirmConnectionDetails();
                var masterDbConnectionString = _initialDal.getMasterDbConnectionString();
                var userConnectionString = connectionDetails.userConnectionString;
                var logDbConnectionString = connectionDetails.logDbConnectionString;

                _dbContext.setDbContextforallow(
                    masterDbConnectionString,
                    userConnectionString,
                    "admin",
                    "",
                    logDbConnectionString,
                    (DataClientEnum)connectionDetails.databaseClient);

                return new AuthenticationResult { IsAuthenticated = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Anonymous authentication failed");
                return new AuthenticationResult 
                { 
                    IsAuthenticated = false, 
                    ErrorMessage = "Anonymous authentication failed" 
                };
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var connectionDetails = await _initialDal.getFirmConnectionDetails();
                using var connection = _initialDal.GetConnection(connectionDetails.Item2, connectionDetails.Item1);

                var tokenDetails = await connection.QueryFirstOrDefaultAsync<JwtTokenDetails>(
                    "SELECT Expires_In FROM JwtTokenDetails WHERE token = @token",
                    new { token });

                return tokenDetails != null && tokenDetails.Expires_In > DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation failed for token: {Token}", token);
                return false;
            }
        }

        public async Task<UserProfile?> GetUserProfileAsync(string token)
        {
            try
            {
                var connectionDetails = await _initialDal.getFirmConnectionDetails();
                using var connection = _initialDal.GetConnection(connectionDetails.Item2, connectionDetails.Item1);

                var userProfiles = await connection.ExecuteScalarAsync<string>(
                    "SELECT userprofiles FROM JwtTokenDetails WHERE token = @token",
                    new { token });

                return !string.IsNullOrEmpty(userProfiles) 
                    ? JsonConvert.DeserializeObject<UserProfile>(userProfiles) 
                    : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user profile for token: {Token}", token);
                return null;
            }
        }

        public async Task<SystemSettings?> GetSystemSettingsAsync(string token)
        {
            try
            {
                var connectionDetails = await _initialDal.getFirmConnectionDetails();
                using var connection = _initialDal.GetConnection(connectionDetails.Item2, connectionDetails.Item1);

                var settings = await connection.ExecuteScalarAsync<string>(
                    "SELECT settings FROM JwtTokenDetails WHERE token = @token",
                    new { token });

                return !string.IsNullOrEmpty(settings) 
                    ? JsonConvert.DeserializeObject<SystemSettings>(settings) 
                    : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system settings for token: {Token}", token);
                return null;
            }
        }
    }
}