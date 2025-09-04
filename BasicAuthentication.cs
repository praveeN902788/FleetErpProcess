using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using ERPServer.Models;
using Bharuwa.Erp.Common;
using ImsPosLibraryCore.Models;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System;
using System.Linq;
using Dapper;
using DocumentFormat.OpenXml.InkML;
using System.Data;
using Bharuwa.Erp.Data;
using Bharuwa.Erp.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Bharuwa.Erp.Auth.Services;
using Bharuwa.Erp.Queries;
using Serilog;
using System.Diagnostics;

namespace ERPServer
{
    /// <summary>
    /// Enhanced Basic Authentication filter with improved logging, error handling, and performance monitoring
    /// Maintains original functionality while adding modern features
    /// </summary>
    public class BasicAuthentication : TypeFilterAttribute
    {
        public BasicAuthentication() : base(typeof(BasicAuthenticationFilter))
        {
            Order = -1;
        }

        /// <summary>
        /// Enhanced authentication filter with comprehensive logging and error handling
        /// </summary>
        private class BasicAuthenticationFilter : IAsyncAuthorizationFilter
        {
            private readonly ILogger<BasicAuthenticationFilter> _logger;
            private readonly Stopwatch _stopwatch;

            public BasicAuthenticationFilter(ILogger<BasicAuthenticationFilter> logger)
            {
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                _stopwatch = new Stopwatch();
            }

            /// <summary>
            /// Enhanced authorization process with comprehensive logging and error handling
            /// </summary>
            public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
            {
                _stopwatch.Start();
                var requestId = context.HttpContext.TraceIdentifier;
                var endpoint = context.HttpContext.GetEndpoint();
                var allowAnonymous = endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null;

                _logger.LogDebug("Starting authorization for request {RequestId}, Anonymous: {AllowAnonymous}", 
                    requestId, allowAnonymous);

                try
                {
                    // Get required services
                    var dbContext = GetRequiredService<IDbContext>(context.HttpContext);
                    var initialDal = GetRequiredService<IInitialDal>(context.HttpContext);

                    if (allowAnonymous)
                    {
                        await HandleAnonymousAccessAsync(context, dbContext, initialDal, requestId);
                    }
                    else
                    {
                        await HandleAuthenticatedAccessAsync(context, dbContext, initialDal, requestId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Authorization failed for request {RequestId}", requestId);
                    context.Result = new UnauthorizedResult();
                }
                finally
                {
                    _stopwatch.Stop();
                    _logger.LogDebug("Authorization completed for request {RequestId} in {ElapsedMs}ms", 
                        requestId, _stopwatch.ElapsedMilliseconds);
                }
            }

            /// <summary>
            /// Handle anonymous access with enhanced error handling
            /// </summary>
            private async Task HandleAnonymousAccessAsync(
                AuthorizationFilterContext context, 
                IDbContext dbContext, 
                IInitialDal initialDal, 
                string requestId)
            {
                _logger.LogInformation("Processing anonymous access for request {RequestId}", requestId);

                try
                {
                    var connectionDetails = await GetConnectionDetailsSafelyAsync(initialDal, requestId);
                    
                    var masterDbConnectionString = initialDal.getMasterDbConnectionString();
                    var userConnectionString = connectionDetails.userConnectionString;
                    var logDbConnectionString = connectionDetails.logDbConnectionString;

                    dbContext.setDbContextforallow(
                        masterDbConnectionString, 
                        userConnectionString, 
                        "admin", 
                        "", 
                        logDbConnectionString, 
                        (DataClientEnum)connectionDetails.databaseClient);

                    _logger.LogInformation("Anonymous access configured successfully for request {RequestId}", requestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to configure anonymous access for request {RequestId}", requestId);
                    throw;
                }
            }

            /// <summary>
            /// Handle authenticated access with enhanced token validation
            /// </summary>
            private async Task HandleAuthenticatedAccessAsync(
                AuthorizationFilterContext context, 
                IDbContext dbContext, 
                IInitialDal initialDal, 
                string requestId)
            {
                _logger.LogInformation("Processing authenticated access for request {RequestId}", requestId);

                try
                {
                    // Extract tokens from headers
                    var (token, browserIdentityToken) = ExtractTokensFromHeaders(context.HttpContext);

                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("No authorization token found for request {RequestId}", requestId);
                        context.Result = new UnauthorizedResult();
                        return;
                    }

                    // Get connection details
                    var connectionDetails = await GetConnectionDetailsSafelyAsync(initialDal, requestId);
                    
                    // Validate token and set up context
                    await ValidateTokenAndSetupContextAsync(
                        context, 
                        dbContext, 
                        initialDal, 
                        token, 
                        browserIdentityToken, 
                        connectionDetails, 
                        requestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process authenticated access for request {RequestId}", requestId);
                    context.Result = new UnauthorizedResult();
                }
            }

            /// <summary>
            /// Extract tokens from request headers with validation
            /// </summary>
            private (string token, string browserIdentityToken) ExtractTokensFromHeaders(HttpContext httpContext)
            {
                // Extract authorization token
                httpContext.Request.Headers.TryGetValue("Authorization", out var authToken);
                var token = authToken.ToString().Replace("Bearer ", string.Empty);

                // Extract browser identity token
                httpContext.Request.Headers.TryGetValue("X-Browseridentity-Token", out var browserToken);
                var browserIdentityToken = browserToken.ToString();

                _logger.LogDebug("Extracted tokens - Auth: {AuthTokenLength}, Browser: {BrowserTokenLength}", 
                    token.Length, browserIdentityToken.Length);

                return (token, browserIdentityToken);
            }

            /// <summary>
            /// Get connection details with error handling
            /// </summary>
            private async Task<dynamic> GetConnectionDetailsSafelyAsync(IInitialDal initialDal, string requestId)
            {
                try
                {
                    var connectionDetails = await initialDal.getFirmConnectionDetails();
                    
                    if (connectionDetails == null)
                    {
                        throw new InvalidOperationException("Failed to retrieve connection details");
                    }

                    _logger.LogDebug("Retrieved connection details for request {RequestId}", requestId);
                    return connectionDetails;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get connection details for request {RequestId}", requestId);
                    throw;
                }
            }

            /// <summary>
            /// Validate token and set up database context
            /// </summary>
            private async Task ValidateTokenAndSetupContextAsync(
                AuthorizationFilterContext context,
                IDbContext dbContext,
                IInitialDal initialDal,
                string token,
                string browserIdentityToken,
                dynamic connectionDetails,
                string requestId)
            {
                using var connection = initialDal.GetConnection(connectionDetails.Item2, connectionDetails.Item1);
                
                // Query token details
                var tokenDetails = await QueryTokenDetailsAsync(connection, token, requestId);
                
                if (tokenDetails == null)
                {
                    _logger.LogWarning("Token not found for request {RequestId}", requestId);
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // Validate browser identity token
                if (!ValidateBrowserIdentityToken(browserIdentityToken, tokenDetails.BrowserIdentityToken, requestId))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // Validate token expiration
                if (!ValidateTokenExpiration(tokenDetails.Expires_In, requestId))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // Set up database context
                await SetupDatabaseContextAsync(
                    dbContext, 
                    initialDal, 
                    connection, 
                    tokenDetails, 
                    connectionDetails, 
                    requestId);
            }

            /// <summary>
            /// Query token details from database
            /// </summary>
            private async Task<JwtTokenDetails?> QueryTokenDetailsAsync(IDbConnection connection, string token, string requestId)
            {
                try
                {
                    var tokenDetails = await connection.QueryFirstOrDefaultAsync<JwtTokenDetails>(
                        "SELECT BrowserIdentityToken, Expires_In, userprofiles, settings FROM JwtTokenDetails WHERE token = @token",
                        new { token });

                    _logger.LogDebug("Token query completed for request {RequestId}, Found: {TokenFound}", 
                        requestId, tokenDetails != null);

                    return tokenDetails;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to query token details for request {RequestId}", requestId);
                    return null;
                }
            }

            /// <summary>
            /// Validate browser identity token
            /// </summary>
            private bool ValidateBrowserIdentityToken(string providedToken, string storedToken, string requestId)
            {
                var isValid = string.Compare(providedToken, storedToken, StringComparison.OrdinalIgnoreCase) == 0;
                
                if (!isValid)
                {
                    _logger.LogWarning("Browser identity token mismatch for request {RequestId}", requestId);
                }

                return isValid;
            }

            /// <summary>
            /// Validate token expiration
            /// </summary>
            private bool ValidateTokenExpiration(DateTime expiresIn, string requestId)
            {
                var isValid = DateTime.Compare(expiresIn, DateTime.UtcNow) > 0;
                
                if (!isValid)
                {
                    _logger.LogWarning("Token expired for request {RequestId}, Expires: {ExpiresIn}", 
                        requestId, expiresIn);
                }

                return isValid;
            }

            /// <summary>
            /// Set up database context with user information
            /// </summary>
            private async Task SetupDatabaseContextAsync(
                IDbContext dbContext,
                IInitialDal initialDal,
                IDbConnection connection,
                JwtTokenDetails tokenDetails,
                dynamic connectionDetails,
                string requestId)
            {
                try
                {
                    // Deserialize user data
                    var systemSettings = DeserializeSystemSettings(tokenDetails.settings, requestId);
                    var userProfile = DeserializeUserProfile(tokenDetails.userprofiles, requestId);

                    if (systemSettings == null || userProfile == null)
                    {
                        throw new InvalidOperationException("Failed to deserialize user data");
                    }

                    // Get storage location
                    var storageLocation = await GetStorageLocationAsync(connection, userProfile.CompanyInfo.COMPANYID, requestId);

                    // Set up database context
                    var masterDbConnectionString = initialDal.getMasterDbConnectionString();
                    var userConnectionString = connectionDetails.userConnectionString;
                    var logDbConnectionString = connectionDetails.logDbConnectionString;

                    dbContext.setDbContext(
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
                        tokenDetails.Token);

                    _logger.LogInformation("Database context set up successfully for user {Username} in request {RequestId}", 
                        userProfile.username, requestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set up database context for request {RequestId}", requestId);
                    throw;
                }
            }

            /// <summary>
            /// Deserialize system settings with error handling
            /// </summary>
            private SystemSettings? DeserializeSystemSettings(string settings, string requestId)
            {
                try
                {
                    return JsonConvert.DeserializeObject<SystemSettings>(settings);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize system settings for request {RequestId}", requestId);
                    return null;
                }
            }

            /// <summary>
            /// Deserialize user profile with error handling
            /// </summary>
            private UserProfile? DeserializeUserProfile(string userProfiles, string requestId)
            {
                try
                {
                    return JsonConvert.DeserializeObject<UserProfile>(userProfiles);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize user profile for request {RequestId}", requestId);
                    return null;
                }
            }

            /// <summary>
            /// Get storage location with error handling
            /// </summary>
            private async Task<string> GetStorageLocationAsync(IDbConnection connection, string companyId, string requestId)
            {
                try
                {
                    var storageLocation = await connection.ExecuteScalarAsync<string>(
                        FileManager.GetQuery(DataQuery.GetStorageLocation), 
                        new { CompanyId = companyId });

                    return storageLocation ?? string.Empty;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get storage location for company {CompanyId} in request {RequestId}", 
                        companyId, requestId);
                    return string.Empty;
                }
            }

            /// <summary>
            /// Get required service with validation
            /// </summary>
            private T GetRequiredService<T>(HttpContext httpContext) where T : class
            {
                var service = httpContext.RequestServices.GetService(typeof(T)) as T;
                
                if (service == null)
                {
                    throw new InvalidOperationException($"Required service {typeof(T).Name} not found");
                }

                return service;
            }
        }
    }
}