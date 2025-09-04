using Bharuwa.Erp.API.FMS.Models;

namespace Bharuwa.Erp.API.FMS.Services
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateAsync(string token, string browserIdentityToken);
        Task<AuthenticationResult> AuthenticateAnonymousAsync();
        Task<bool> ValidateTokenAsync(string token);
        Task<UserProfile?> GetUserProfileAsync(string token);
        Task<SystemSettings?> GetSystemSettingsAsync(string token);
    }

    public class AuthenticationResult
    {
        public bool IsAuthenticated { get; set; }
        public string? ErrorMessage { get; set; }
        public UserProfile? UserProfile { get; set; }
        public SystemSettings? SystemSettings { get; set; }
        public string? StorageLocation { get; set; }
    }
}