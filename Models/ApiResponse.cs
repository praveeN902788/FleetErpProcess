using System.ComponentModel.DataAnnotations;

namespace Bharuwa.Erp.API.FMS.Models
{
    public class ApiResponse<T>
    {
        public string Status { get; set; } = "success";
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string TraceId { get; set; } = string.Empty;
    }

    public class PaginatedResponse<T> : ApiResponse<IEnumerable<T>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public string? BrowserIdentityToken { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserProfile? UserProfile { get; set; }
        public SystemSettings? SystemSettings { get; set; }
    }

    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        [Required]
        [MinLength(6)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UserProfile
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string UserLevel { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public CompanyInfo CompanyInfo { get; set; } = new();
    }

    public class CompanyInfo
    {
        public string COMPANYID { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
    }

    public class SystemSettings
    {
        public string Theme { get; set; } = "default";
        public string Language { get; set; } = "en";
        public bool EnableNotifications { get; set; } = true;
        public int SessionTimeout { get; set; } = 30;
    }

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

    public class JwtTokenDetails
    {
        public string Token { get; set; } = string.Empty;
        public string BrowserIdentityToken { get; set; } = string.Empty;
        public DateTime Expires_In { get; set; }
        public string userprofiles { get; set; } = string.Empty;
        public string settings { get; set; } = string.Empty;
    }
}