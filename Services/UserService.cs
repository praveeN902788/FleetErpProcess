using Bharuwa.Erp.API.FMS.Models;
using Bharuwa.Erp.Common;
using ERPServer.DataAccess;
using Microsoft.Extensions.Logging;

namespace Bharuwa.Erp.API.FMS.Services
{
    public class UserService : IUserService
    {
        private readonly IDbContext _dbContext;
        private readonly LoginHelper _loginHelper;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IDbContext dbContext,
            LoginHelper loginHelper,
            ILogger<UserService> logger)
        {
            _dbContext = dbContext;
            _loginHelper = loginHelper;
            _logger = logger;
        }

        public async Task<UserConfigurationResponse> GetUserConfigurationAsync()
        {
            try
            {
                if (_dbContext?.UserProfile == null)
                {
                    _logger.LogWarning("User profile not available in database context");
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                var userMenus = await _loginHelper.getuserMenus(
                    _dbContext.UserProfile.UserType, 
                    _dbContext.UserProfile.UserLevel);

                return new UserConfigurationResponse
                {
                    UserProfile = _dbContext.UserProfile,
                    SystemSettings = _dbContext.Settings,
                    UserMenus = userMenus
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user configuration for user: {Username}", 
                    _dbContext?.UserProfile?.username ?? "unknown");
                throw;
            }
        }

        public async Task<UserMenu[]> GetUserMenusAsync(string userType, string userLevel)
        {
            try
            {
                if (string.IsNullOrEmpty(userType) || string.IsNullOrEmpty(userLevel))
                {
                    _logger.LogWarning("Invalid user type or level provided: Type={UserType}, Level={UserLevel}", 
                        userType, userLevel);
                    return Array.Empty<UserMenu>();
                }

                var menus = await _loginHelper.getuserMenus(userType, userLevel);
                _logger.LogDebug("Retrieved {MenuCount} menus for user type: {UserType}, level: {UserLevel}", 
                    menus.Length, userType, userLevel);

                return menus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user menus for type: {UserType}, level: {UserLevel}", 
                    userType, userLevel);
                return Array.Empty<UserMenu>();
            }
        }

        public async Task<bool> ValidateUserAccessAsync(string userType, string userLevel, string resource)
        {
            try
            {
                if (string.IsNullOrEmpty(userType) || string.IsNullOrEmpty(userLevel) || string.IsNullOrEmpty(resource))
                {
                    _logger.LogWarning("Invalid parameters for access validation: Type={UserType}, Level={UserLevel}, Resource={Resource}", 
                        userType, userLevel, resource);
                    return false;
                }

                var userMenus = await GetUserMenusAsync(userType, userLevel);
                var hasAccess = userMenus.Any(menu => 
                    menu.MenuName?.Equals(resource, StringComparison.OrdinalIgnoreCase) == true ||
                    menu.MenuUrl?.Contains(resource, StringComparison.OrdinalIgnoreCase) == true);

                _logger.LogDebug("Access validation result: {HasAccess} for user type: {UserType}, level: {UserLevel}, resource: {Resource}", 
                    hasAccess, userType, userLevel, resource);

                return hasAccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate user access for type: {UserType}, level: {UserLevel}, resource: {Resource}", 
                    userType, userLevel, resource);
                return false;
            }
        }
    }
}