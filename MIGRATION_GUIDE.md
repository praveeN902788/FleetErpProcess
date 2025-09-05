# Migration Guide: Existing Controllers to New Architecture

## Overview

This guide explains how to migrate existing controllers to the new enhanced architecture with proper API versioning and high-level code organization.

## Migration Steps

### 1. Update Existing Controller

The existing `CustomerController.cs` should be migrated to use the new architecture. Here's how:

#### Before (Current Implementation)
```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class CustomerController(ICustomerManagementDal customerManagementDal) : ApiBaseController
{
    // Direct method implementations
}
```

#### After (New Architecture)
```csharp
[ApiController]
[Route("api/v{version:apiVersion}/customer")]
[ApiVersion("1.0")]
public class CustomerV1Controller : VersionAwareController
{
    // Enhanced implementation with proper error handling
}
```

### 2. Key Changes Required

#### A. Base Controller Change
- **From**: `ApiBaseController`
- **To**: `VersionAwareController` or `EnhancedBaseController`

#### B. Error Handling
- **From**: `ResponseWrapperAsync`
- **To**: `ExecuteVersionedAsync`

#### C. Response Creation
- **From**: Direct `APIResponseDto` creation
- **To**: `CreateVersionedResponse` or `CreateApiResponse`

#### D. Logging
- **From**: Basic logging
- **To**: Structured logging with context

### 3. Example Migration

#### Original Method
```csharp
[HttpGet("GetDashboardCountsForCustomer")]
[ApiVersion("1.0")]
public async Task<IActionResult> GetDashboardCountsForCustomer([FromQuery] string Fromdate = null, string Todate = null)
{
    return await ResponseWrapperAsync(async () =>
    {
        APIResponseDto result = await _customerManagement.GetDashboardCountsForCustomer(Fromdate, Todate);
        return result;
    });
}
```

#### Migrated Method
```csharp
[HttpGet("dashboard/counts")]
[ApiVersion("1.0")]
[ProducesResponseType(typeof(APIResponseDto), 200)]
[ProducesResponseType(typeof(APIResponseDto), 400)]
[ProducesResponseType(typeof(APIResponseDto), 500)]
public async Task<IActionResult> GetDashboardCounts(
    [FromQuery] string fromDate = null, 
    [FromQuery] string toDate = null)
{
    return await ExecuteVersionedAsync(async () =>
    {
        _logger.LogInformation("Getting dashboard counts for customer from {FromDate} to {ToDate}", 
            fromDate, toDate);
        
        var result = await _customerManagement.GetDashboardCountsForCustomer(fromDate, toDate);
        return result;
    }, "Dashboard counts retrieved successfully");
}
```

### 4. Benefits of Migration

#### A. Enhanced Error Handling
- Automatic exception mapping to HTTP status codes
- Structured error responses with request IDs
- Comprehensive logging of errors

#### B. Version Management
- Automatic version detection and validation
- Deprecation warnings for outdated versions
- Version-specific response metadata

#### C. Improved Logging
- Structured logging with context
- Request/response tracking
- Performance monitoring

#### D. Better Documentation
- Swagger integration with version-specific documentation
- Comprehensive API documentation
- Migration guides and examples

### 5. Migration Checklist

- [ ] Update base controller inheritance
- [ ] Replace `ResponseWrapperAsync` with `ExecuteVersionedAsync`
- [ ] Update response creation methods
- [ ] Add comprehensive logging
- [ ] Update route patterns for consistency
- [ ] Add proper HTTP status code attributes
- [ ] Update parameter naming conventions
- [ ] Add input validation
- [ ] Update error handling
- [ ] Test all endpoints

### 6. Testing After Migration

#### A. Functional Testing
- Verify all endpoints work correctly
- Test error scenarios
- Validate response formats

#### B. Version Testing
- Test version-specific endpoints
- Verify deprecation warnings
- Check version information endpoints

#### C. Performance Testing
- Monitor response times
- Check memory usage
- Validate logging performance

### 7. Rollback Plan

If issues arise during migration:

1. **Immediate Rollback**: Revert to original controller
2. **Gradual Migration**: Migrate one endpoint at a time
3. **A/B Testing**: Run both versions simultaneously
4. **Monitoring**: Watch for errors and performance issues

### 8. Common Issues and Solutions

#### Issue: Version Not Recognized
**Solution**: Ensure proper API versioning configuration in startup

#### Issue: Error Responses Not Formatted Correctly
**Solution**: Use `CreateVersionedErrorResponse` instead of direct error creation

#### Issue: Logging Not Working
**Solution**: Ensure logger is properly injected and configured

#### Issue: Performance Degradation
**Solution**: Check middleware order and optimize logging configuration

## Conclusion

The migration to the new architecture provides significant benefits in terms of:

- **Maintainability**: Cleaner, more organized code
- **Reliability**: Better error handling and logging
- **Scalability**: Proper version management
- **Developer Experience**: Better documentation and debugging

Follow this guide carefully to ensure a smooth migration with minimal disruption to existing functionality.