using Bharuwa.Erp.API.FMS.Models;
using Bharuwa.Erp.API.FMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Bharuwa.Erp.API.FMS.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService,
            IAuthenticationService authenticationService,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        /// <summary>
        /// Get user authentication configuration including profile, settings, and menus
        /// </summary>
        /// <returns>User configuration data</returns>
        [HttpGet("configuration")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserConfigurationResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GetUserConfiguration()
        {
            try
            {
                _logger.LogInformation("Getting user configuration for authenticated user");

                var configuration = await _userService.GetUserConfigurationAsync();

                var response = new ApiResponse<UserConfigurationResponse>
                {
                    Data = configuration,
                    Message = "User configuration retrieved successfully",
                    TraceId = HttpContext.TraceIdentifier
                };

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "User not authenticated for configuration request");
                return Unauthorized(new ApiResponse<object>
                {
                    Status = "error",
                    Message = "User not authenticated",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user configuration");
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = "error",
                    Message = "Failed to retrieve user configuration",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Validate user access to a specific resource
        /// </summary>
        /// <param name="resource">The resource to check access for</param>
        /// <returns>Access validation result</returns>
        [HttpGet("validate-access")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> ValidateAccess([FromQuery] string resource)
        {
            try
            {
                if (string.IsNullOrEmpty(resource))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Status = "error",
                        Message = "Resource parameter is required",
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                // Extract user info from claims (you'll need to implement this based on your JWT structure)
                var userType = User.FindFirst("UserType")?.Value ?? "default";
                var userLevel = User.FindFirst("UserLevel")?.Value ?? "default";

                var hasAccess = await _userService.ValidateUserAccessAsync(userType, userLevel, resource);

                var response = new ApiResponse<bool>
                {
                    Data = hasAccess,
                    Message = hasAccess ? "Access granted" : "Access denied",
                    TraceId = HttpContext.TraceIdentifier
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate access for resource: {Resource}", resource);
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = "error",
                    Message = "Failed to validate access",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get user menus based on user type and level
        /// </summary>
        /// <param name="userType">Type of user</param>
        /// <param name="userLevel">Level of user</param>
        /// <returns>User menus</returns>
        [HttpGet("menus")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserMenu[]>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GetUserMenus([FromQuery] string userType, [FromQuery] string userLevel)
        {
            try
            {
                if (string.IsNullOrEmpty(userType) || string.IsNullOrEmpty(userLevel))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Status = "error",
                        Message = "User type and user level are required",
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                var menus = await _userService.GetUserMenusAsync(userType, userLevel);

                var response = new ApiResponse<UserMenu[]>
                {
                    Data = menus,
                    Message = $"Retrieved {menus.Length} menus for user type: {userType}, level: {userLevel}",
                    TraceId = HttpContext.TraceIdentifier
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user menus for type: {UserType}, level: {UserLevel}", userType, userLevel);
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = "error",
                    Message = "Failed to retrieve user menus",
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get enhanced user authentication configuration including profile, settings, and menus (v2.0)
        /// </summary>
        /// <returns>Enhanced user configuration data</returns>
        [HttpGet("configuration")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserConfigurationResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> GetUserConfigurationV2()
        {
            try
            {
                _logger.LogInformation("Getting enhanced user configuration for authenticated user (v2.0)");

                var configuration = await _userService.GetUserConfigurationAsync();

                var response = new ApiResponse<UserConfigurationResponse>
                {
                    Data = configuration,
                    Message = "Enhanced user configuration retrieved successfully (v2.0)",
                    TraceId = HttpContext.TraceIdentifier,
                    Version = "2.0"
                };

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "User not authenticated for configuration request (v2.0)");
                return Unauthorized(new ApiResponse<object>
                {
                    Status = "error",
                    Message = "User not authenticated",
                    TraceId = HttpContext.TraceIdentifier,
                    Version = "2.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get enhanced user configuration (v2.0)");
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = "error",
                    Message = "Failed to retrieve enhanced user configuration",
                    TraceId = HttpContext.TraceIdentifier,
                    Version = "2.0"
                });
            }
        }

        /// <summary>
        /// Enhanced user access validation with detailed permissions (v2.0)
        /// </summary>
        /// <param name="resource">The resource to check access for</param>
        /// <param name="action">The action to perform on the resource</param>
        /// <returns>Enhanced access validation result</returns>
        [HttpGet("validate-access")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> ValidateAccessV2([FromQuery] string resource, [FromQuery] string action = "read")
        {
            try
            {
                if (string.IsNullOrEmpty(resource))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Status = "error",
                        Message = "Resource parameter is required",
                        TraceId = HttpContext.TraceIdentifier,
                        Version = "2.0"
                    });
                }

                // Extract user info from claims (you'll need to implement this based on your JWT structure)
                var userType = User.FindFirst("UserType")?.Value ?? "default";
                var userLevel = User.FindFirst("UserLevel")?.Value ?? "default";

                var hasAccess = await _userService.ValidateUserAccessAsync(userType, userLevel, resource);

                var response = new ApiResponse<bool>
                {
                    Data = hasAccess,
                    Message = hasAccess ? $"Access granted for {action} on {resource}" : $"Access denied for {action} on {resource}",
                    TraceId = HttpContext.TraceIdentifier,
                    Version = "2.0"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate enhanced access for resource: {Resource}, action: {Action}", resource, action);
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = "error",
                    Message = "Failed to validate enhanced access",
                    TraceId = HttpContext.TraceIdentifier,
                    Version = "2.0"
                });
            }
        }

        /// <summary>
        /// Get enhanced user menus with additional metadata (v2.0)
        /// </summary>
        /// <param name="userType">Type of user</param>
        /// <param name="userLevel">Level of user</param>
        /// <param name="includeMetadata">Include additional metadata</param>
        /// <returns>Enhanced user menus</returns>
        [HttpGet("menus")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserMenu[]>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> GetUserMenusV2([FromQuery] string userType, [FromQuery] string userLevel, [FromQuery] bool includeMetadata = false)
        {
            try
            {
                if (string.IsNullOrEmpty(userType) || string.IsNullOrEmpty(userLevel))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Status = "error",
                        Message = "User type and user level are required",
                        TraceId = HttpContext.TraceIdentifier,
                        Version = "2.0"
                    });
                }

                var menus = await _userService.GetUserMenusAsync(userType, userLevel);

                var response = new ApiResponse<UserMenu[]>
                {
                    Data = menus,
                    Message = $"Retrieved {menus.Length} enhanced menus for user type: {userType}, level: {userLevel}",
                    TraceId = HttpContext.TraceIdentifier,
                    Version = "2.0"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get enhanced user menus for type: {UserType}, level: {UserLevel}", userType, userLevel);
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = "error",
                    Message = "Failed to retrieve enhanced user menus",
                    TraceId = HttpContext.TraceIdentifier,
                    Version = "2.0"
                });
            }
        }
    }
}