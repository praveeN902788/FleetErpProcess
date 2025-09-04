# Migration Guide: Legacy to Modern Architecture

This guide helps you migrate from the legacy code structure to the new enterprise-grade architecture.

## 🔄 Overview of Changes

### Before (Legacy)
```csharp
// Old BasicController.cs
public class BasicController(IDbContext dbContext, LoginHelper login) : ApiBaseController
{
    [HttpGet]
    [Route("api/v1/authConfiguration")]
    public async Task<IActionResult> V1configureAuthentication()
    {
        try
        {
            if (_dbContext is null)
            {
                return Unauthorized();
            }
            return new OkObjectResult(new FunctionResponse { 
                status = "ok", 
                result = new { 
                    userProfile = _dbContext.UserProfile, 
                    systemsettings = _dbContext.Settings, 
                    userMenus = await _login.getuserMenus(_dbContext.UserProfile.UserType, _dbContext.UserProfile.UserLevel) 
                } 
            });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }
}
```

### After (Modern)
```csharp
// New AuthController.cs
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("configuration")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserConfigurationResponse>), 200)]
    public async Task<IActionResult> GetUserConfiguration()
    {
        try
        {
            var configuration = await _userService.GetUserConfigurationAsync();
            return Ok(new ApiResponse<UserConfigurationResponse>
            {
                Data = configuration,
                Message = "User configuration retrieved successfully",
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "User not authenticated");
            return Unauthorized(new ApiResponse<object>
            {
                Status = "error",
                Message = "User not authenticated",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }
}
```

## 📋 Step-by-Step Migration

### Step 1: Update Authentication

#### Old Authentication (BasicAuthentication.cs)
```csharp
// Remove this file - replaced with JWT authentication
public class BasicAuthentication : TypeFilterAttribute
{
    // Complex authentication logic mixed with business logic
}
```

#### New Authentication
```csharp
// Use standard JWT authentication in Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        // JWT configuration
    });

// Use AuthenticationService for custom logic
public class AuthenticationService : IAuthenticationService
{
    // Clean, testable authentication logic
}
```

### Step 2: Update Controllers

#### Remove Old Controllers
- Delete `BasicController.cs`
- Delete `ApiBaseController.cs` (functionality moved to filters)

#### Create New Controllers
- Create `Controllers/AuthController.cs`
- Create `Controllers/HealthController.cs`
- Follow REST conventions
- Use proper HTTP status codes
- Implement proper error handling

### Step 3: Update Error Handling

#### Old Error Handling
```csharp
// In ApiBaseController.cs
private IActionResult HandleError(Exception ex)
{
    return ex switch
    {
        ArgumentNullException _ => new BadRequestObjectResult(new { message = "A required parameter was null." }),
        // ... more cases
    };
}
```

#### New Error Handling
```csharp
// GlobalExceptionFilter.cs - centralized error handling
public class GlobalExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        // Consistent error response format
        // Proper logging
        // Environment-aware error details
    }
}
```

### Step 4: Update Configuration

#### Old Configuration
```csharp
// Hard-coded values scattered throughout code
string connectionstring = cress.Item1;
string MasterDBConnectionString = _initialDal.getMasterDbConnectionString();
```

#### New Configuration
```csharp
// Strongly-typed configuration
public class AppSettings
{
    public DatabaseSettings Database { get; set; } = new();
    public JwtSettings Jwt { get; set; } = new();
    // ... other settings
}

// Usage in services
public class ConfigurationService : IConfigurationService
{
    public DatabaseSettings GetDatabaseSettings() => GetAppSettings().Database;
}
```

### Step 5: Update Response Format

#### Old Response Format
```csharp
// Inconsistent response formats
return new OkObjectResult(new FunctionResponse { 
    status = "ok", 
    result = new { userProfile = _dbContext.UserProfile } 
});
```

#### New Response Format
```csharp
// Consistent API response format
public class ApiResponse<T>
{
    public string Status { get; set; } = "success";
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string TraceId { get; set; } = string.Empty;
}

// Usage
return Ok(new ApiResponse<UserConfigurationResponse>
{
    Data = configuration,
    Message = "User configuration retrieved successfully",
    TraceId = HttpContext.TraceIdentifier
});
```

## 🔧 Configuration Updates

### Update appsettings.json
```json
{
  "AppSettings": {
    "Database": {
      "CommandTimeout": 30,
      "MaxRetryAttempts": 3
    },
    "Jwt": {
      "Key": "your-secure-jwt-key",
      "Issuer": "BharuwaERP",
      "Audience": "BharuwaERPUsers",
      "ExpirationMinutes": 60
    },
    "Cors": {
      "AllowedOrigins": ["http://localhost:3000"],
      "AllowedMethods": ["GET", "POST", "PUT", "DELETE"]
    }
  }
}
```

### Update Program.cs
```csharp
// Old Program.cs
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }
}

// New Program.cs
var builder = WebApplication.CreateBuilder(args);
// Modern minimal hosting model with all services configured
```

## 🚀 API Endpoint Changes

### Old Endpoints
```
GET /api/v1/authConfiguration
```

### New Endpoints
```
GET /api/v1/auth/configuration
GET /api/v1/auth/validate-access?resource=resourceName
GET /api/v1/auth/menus?userType=admin&userLevel=1
GET /api/v1/health
GET /api/v1/health/live
GET /api/v1/health/ready
```

## 🔒 Authentication Changes

### Old Authentication
```http
Authorization: Bearer <token>
X-Browseridentity-Token: <browser-token>
```

### New Authentication
```http
Authorization: Bearer <jwt-token>
```

## 📊 Response Format Changes

### Old Response
```json
{
  "status": "ok",
  "result": {
    "userProfile": { ... },
    "systemsettings": { ... },
    "userMenus": [ ... ]
  }
}
```

### New Response
```json
{
  "status": "success",
  "message": "User configuration retrieved successfully",
  "data": {
    "userProfile": { ... },
    "systemSettings": { ... },
    "userMenus": [ ... ]
  },
  "timestamp": "2024-01-01T00:00:00Z",
  "traceId": "request-id"
}
```

## 🧪 Testing Migration

### Update Client Applications
1. **Update API calls** to use new endpoints
2. **Update authentication** to use JWT tokens
3. **Update response parsing** to handle new format
4. **Test thoroughly** before production deployment

### Example Client Update
```javascript
// Old client code
fetch('/api/v1/authConfiguration', {
  headers: {
    'Authorization': 'Bearer ' + token,
    'X-Browseridentity-Token': browserToken
  }
})
.then(response => response.json())
.then(data => {
  if (data.status === 'ok') {
    // Handle response
  }
});

// New client code
fetch('/api/v1/auth/configuration', {
  headers: {
    'Authorization': 'Bearer ' + jwtToken
  }
})
.then(response => response.json())
.then(data => {
  if (data.status === 'success') {
    // Handle response
  }
});
```

## ⚠️ Breaking Changes

### Authentication
- JWT token format changed
- Browser identity token no longer required
- Token validation logic moved to service layer

### API Endpoints
- Some endpoint paths changed
- Response format standardized
- Error responses now include trace ID

### Configuration
- Configuration structure completely changed
- Hard-coded values moved to configuration
- Environment-specific settings required

## ✅ Migration Checklist

- [ ] Update authentication to use JWT
- [ ] Replace BasicAuthentication with JWT authentication
- [ ] Update all API endpoints to new format
- [ ] Update response parsing in client applications
- [ ] Configure new appsettings.json
- [ ] Update Program.cs to use minimal hosting model
- [ ] Test all endpoints thoroughly
- [ ] Update documentation
- [ ] Deploy to staging environment
- [ ] Monitor logs and performance
- [ ] Deploy to production

## 🆘 Troubleshooting

### Common Issues

1. **Authentication fails**
   - Check JWT configuration in appsettings.json
   - Verify token format in client applications
   - Check logs for authentication errors

2. **API endpoints return 404**
   - Verify new endpoint paths
   - Check API versioning configuration
   - Ensure controllers are properly registered

3. **Configuration errors**
   - Validate appsettings.json structure
   - Check for missing required configuration values
   - Verify environment-specific settings

4. **Performance issues**
   - Monitor request logs
   - Check database connection pooling
   - Review health check endpoints

### Getting Help
- Check the application logs for detailed error messages
- Use the health check endpoints to diagnose issues
- Review the Swagger documentation for API details
- Contact the development team for support

---

**Note**: This migration represents a significant architectural improvement. Take time to understand the changes and test thoroughly before deploying to production.