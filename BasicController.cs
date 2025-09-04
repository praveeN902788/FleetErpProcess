using Bharuwa.Erp.Common;
using Bharuwa.Erp.Data;
using ERPServer.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.ComponentModel.DataAnnotations;

namespace Bharuwa.Erp.API.FMS
{
    /// <summary>
    /// Enhanced Basic Controller providing authentication configuration functionality
    /// Maintains original functionality while adding modern features and best practices
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class BasicController : ApiBaseController
    {
        private readonly IDbContext _dbContext;
        private readonly LoginHelper _loginHelper;
        private readonly ILogger<BasicController> _logger;

        public BasicController(
            IDbContext dbContext, 
            LoginHelper loginHelper, 
            ILogger<BasicController> logger,
            ILogger<ApiBaseController> baseLogger) : base(baseLogger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _loginHelper = loginHelper ?? throw new ArgumentNullException(nameof(loginHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get user authentication configuration including profile, settings, and menus
        /// Enhanced version of the original V1configureAuthentication method
        /// </summary>
        /// <returns>User configuration data with enhanced response format</returns>
        [HttpGet("authConfiguration")]
        [ProducesResponseType(typeof(ApiResponse<UserConfigurationResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 401)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> V1configureAuthentication()
        {
            return await ResponseWrapperAsync(async () =>
            {
                _logger.LogInformation("Getting authentication configuration for user");

                // Validate database context
                if (_dbContext?.UserProfile == null)
                {
                    _logger.LogWarning("Database context or user profile is null");
                    throw new UnauthorizedAccessException("User not authenticated or database context unavailable");
                }

                // Validate user profile data
                if (string.IsNullOrEmpty(_dbContext.UserProfile.UserType) || 
                    string.IsNullOrEmpty(_dbContext.UserProfile.UserLevel))
                {
                    _logger.LogWarning("User type or level is missing for user: {Username}", 
                        _dbContext.UserProfile.username);
                    throw new InvalidOperationException("User profile is incomplete");
                }

                // Get user menus with error handling
                var userMenus = await GetUserMenusSafelyAsync(_dbContext.UserProfile.UserType, _dbContext.UserProfile.UserLevel);

                // Create enhanced response
                var configuration = new UserConfigurationResponse
                {
                    UserProfile = _dbContext.UserProfile,
                    SystemSettings = _dbContext.Settings,
                    UserMenus = userMenus
                };

                _logger.LogInformation("Successfully retrieved configuration for user: {Username}, Type: {UserType}, Level: {UserLevel}", 
                    _dbContext.UserProfile.username, 
                    _dbContext.UserProfile.UserType, 
                    _dbContext.UserProfile.UserLevel);

                return configuration;
            }, "Authentication Configuration");
        }

        /// <summary>
        /// Enhanced method to get user menus with proper error handling and logging
        /// </summary>
        private async Task<UserMenu[]> GetUserMenusSafelyAsync(string userType, string userLevel)
        {
            try
            {
                _logger.LogDebug("Retrieving user menus for type: {UserType}, level: {UserLevel}", userType, userLevel);

                var userMenus = await _loginHelper.getuserMenus(userType, userLevel);

                _logger.LogDebug("Retrieved {MenuCount} menus for user type: {UserType}, level: {UserLevel}", 
                    userMenus?.Length ?? 0, userType, userLevel);

                return userMenus ?? Array.Empty<UserMenu>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user menus for type: {UserType}, level: {UserLevel}", userType, userLevel);
                
                // Return empty array instead of throwing to maintain backward compatibility
                return Array.Empty<UserMenu>();
            }
        }

        /// <summary>
        /// Get user profile information
        /// New method for better separation of concerns
        /// </summary>
        /// <returns>User profile data</returns>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ApiResponse<UserProfile>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 401)]
        public async Task<IActionResult> GetUserProfile()
        {
            return await ResponseWrapperAsync(async () =>
            {
                _logger.LogInformation("Getting user profile");

                if (_dbContext?.UserProfile == null)
                {
                    throw new UnauthorizedAccessException("User profile not available");
                }

                return _dbContext.UserProfile;
            }, "User Profile");
        }

        /// <summary>
        /// Get system settings
        /// New method for better separation of concerns
        /// </summary>
        /// <returns>System settings data</returns>
        [HttpGet("settings")]
        [ProducesResponseType(typeof(ApiResponse<SystemSettings>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 401)]
        public async Task<IActionResult> GetSystemSettings()
        {
            return await ResponseWrapperAsync(async () =>
            {
                _logger.LogInformation("Getting system settings");

                if (_dbContext?.Settings == null)
                {
                    throw new UnauthorizedAccessException("System settings not available");
                }

                return _dbContext.Settings;
            }, "System Settings");
        }

        /// <summary>
        /// Get user menus for specific user type and level
        /// New method for better separation of concerns
        /// </summary>
        /// <param name="userType">Type of user</param>
        /// <param name="userLevel">Level of user</param>
        /// <returns>User menus</returns>
        [HttpGet("menus")]
        [ProducesResponseType(typeof(ApiResponse<UserMenu[]>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> GetUserMenus(
            [Required] string userType, 
            [Required] string userLevel)
        {
            return await ResponseWrapperAsync(async () =>
            {
                _logger.LogInformation("Getting user menus for type: {UserType}, level: {UserLevel}", userType, userLevel);

                // Validate input parameters
                if (string.IsNullOrWhiteSpace(userType))
                {
                    throw new ArgumentException("User type is required", nameof(userType));
                }

                if (string.IsNullOrWhiteSpace(userLevel))
                {
                    throw new ArgumentException("User level is required", nameof(userLevel));
                }

                var userMenus = await GetUserMenusSafelyAsync(userType, userLevel);

                return userMenus;
            }, "User Menus");
        }

        /// <summary>
        /// Health check endpoint for this controller
        /// New method for monitoring purposes
        /// </summary>
        /// <returns>Controller health status</returns>
        [HttpGet("health")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> GetHealth()
        {
            return await ResponseWrapperAsync(async () =>
            {
                _logger.LogDebug("Health check requested for BasicController");

                var healthStatus = new
                {
                    Controller = "BasicController",
                    Status = "Healthy",
                    DatabaseContext = _dbContext != null,
                    LoginHelper = _loginHelper != null,
                    Timestamp = DateTime.UtcNow
                };

                return healthStatus;
            }, "Health Check");
        }
    }

    /// <summary>
    /// Enhanced user configuration response model
    /// </summary>
    public class UserConfigurationResponse
    {
        public UserProfile? UserProfile { get; set; }
        public SystemSettings? SystemSettings { get; set; }
        public UserMenu[] UserMenus { get; set; } = Array.Empty<UserMenu>();
    }

    /// <summary>
    /// Enhanced user profile model with validation
    /// </summary>
    public class UserProfile
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string UserLevel { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public CompanyInfo CompanyInfo { get; set; } = new();
    }

    /// <summary>
    /// Company information model
    /// </summary>
    public class CompanyInfo
    {
        public string COMPANYID { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// System settings model
    /// </summary>
    public class SystemSettings
    {
        public string Theme { get; set; } = "default";
        public string Language { get; set; } = "en";
        public bool EnableNotifications { get; set; } = true;
        public int SessionTimeout { get; set; } = 30;
    }

    /// <summary>
    /// User menu model
    /// </summary>
    public class UserMenu
    {
        public string MenuId { get; set; } = string.Empty;
        public string MenuName { get; set; } = string.Empty;
        public string MenuUrl { get; set; } = string.Empty;
        public string ParentMenuId { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public string Icon { get; set; } = string.Empty;
    }
}
