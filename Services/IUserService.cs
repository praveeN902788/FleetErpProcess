using Bharuwa.Erp.API.FMS.Models;

namespace Bharuwa.Erp.API.FMS.Services
{
    public interface IUserService
    {
        Task<UserConfigurationResponse> GetUserConfigurationAsync();
        Task<UserMenu[]> GetUserMenusAsync(string userType, string userLevel);
        Task<bool> ValidateUserAccessAsync(string userType, string userLevel, string resource);
    }

    public class UserConfigurationResponse
    {
        public UserProfile? UserProfile { get; set; }
        public SystemSettings? SystemSettings { get; set; }
        public UserMenu[] UserMenus { get; set; } = Array.Empty<UserMenu>();
    }
}