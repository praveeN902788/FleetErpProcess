# FMS API Architecture Documentation

## Overview

This document describes the enhanced architecture of the Fleet Management System (FMS) API, focusing on high-level code organization and comprehensive API version management.

## Architecture Principles

### 1. High-Level Code Organization

The API follows a clean, layered architecture with clear separation of concerns:

```
Controllers/
├── Base/
│   ├── EnhancedBaseController.cs      # Enhanced base controller with comprehensive error handling
│   └── VersionAwareController.cs       # Version-aware base controller
├── V1/
│   └── CustomerV1Controller.cs        # Version 1.0 controllers
├── V2/
│   └── CustomerV2Controller.cs         # Version 2.0 controllers
└── VersionController.cs               # API version management

Services/
└── VersionManagementService.cs         # Centralized version management

Configuration/
├── ApiVersioningConfiguration.cs      # API versioning setup
└── ServiceConfiguration.cs            # Service registration

Middleware/
└── ApiLoggingMiddleware.cs            # Request/response logging
```

### 2. API Version Management

The API implements a comprehensive versioning strategy:

- **URL-based versioning**: `/api/v{version}/controller`
- **Header-based versioning**: `X-Version` header support
- **Media type versioning**: `application/json;ver=2.0`
- **Deprecation management**: Automatic deprecation headers
- **Version information**: Centralized version metadata

## Key Components

### Enhanced Base Controller

The `EnhancedBaseController` provides:

- **Comprehensive error handling** with specific exception mapping
- **Structured logging** with request/response tracking
- **Standardized responses** with version information
- **Model validation** with detailed error messages
- **Performance monitoring** with execution time tracking

### Version-Aware Controller

The `VersionAwareController` extends the base controller with:

- **Automatic version detection** from request context
- **Deprecation warnings** for outdated versions
- **Version-specific responses** with metadata
- **Migration guidance** in response headers

### Version Management Service

The `VersionManagementService` provides:

- **Version validation** and support checking
- **Deprecation status** management
- **Latest version** identification
- **Migration information** and guidance

## API Versioning Strategy

### Version 1.0 (Stable)
- Basic CRUD operations
- Simple authentication
- Basic reporting functionality
- GET-based endpoints for simple queries

### Version 2.0 (Current)
- Enhanced filtering capabilities
- Advanced reporting with analytics
- Real-time notifications
- POST-based endpoints for complex queries
- Improved error handling and responses

### Version 1.5 (Deprecated)
- Intermediate features and bug fixes
- Scheduled for sunset in 90 days
- Migration path to version 2.0 provided

## Request/Response Patterns

### Standard Response Format

```json
{
  "apiVersion": "2.0",
  "data": { /* response data */ },
  "message": "Operation completed successfully",
  "statusCode": 200,
  "timestamp": "2024-01-15T10:30:00Z",
  "requestId": "req_123456789",
  "versionInfo": {
    "currentVersion": "2.0",
    "latestVersion": "2.0",
    "isDeprecated": false,
    "deprecationMessage": null,
    "supportedVersions": ["1.0", "2.0"]
  }
}
```

### Error Response Format

```json
{
  "apiVersion": "2.0",
  "data": null,
  "message": "Validation failed for the provided data",
  "statusCode": 400,
  "timestamp": "2024-01-15T10:30:00Z",
  "requestId": "req_123456789",
  "error": {
    "type": "ValidationException",
    "details": {
      "category": ["Category parameter is required"]
    }
  }
}
```

## Configuration

### API Versioning Setup

```csharp
services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Version"),
        new MediaTypeApiVersionReader("ver")
    );
    opt.ReportApiVersions = true;
});
```

### Logging Configuration

```json
{
  "Logging": {
    "LogRequestBody": true,
    "LogResponseBody": false,
    "IncludeDetailedErrors": false
  }
}
```

## Usage Examples

### Version 1.0 - Simple Dashboard Counts

```http
GET /api/v1/customer/dashboard/counts?fromDate=2024-01-01&toDate=2024-01-31
```

### Version 2.0 - Enhanced Dashboard with Filtering

```http
POST /api/v2/customer/dashboard/counts
Content-Type: application/json

{
  "dateRange": {
    "fromDate": "2024-01-01",
    "toDate": "2024-01-31"
  },
  "categories": ["fleet", "maintenance"],
  "includeTrends": true,
  "includeComparisons": true
}
```

### Version Information

```http
GET /api/version
```

Response includes all supported versions, deprecation status, and migration guidance.

## Migration Guide

### From Version 1.0 to 2.0

1. **Update endpoint methods**: Some GET endpoints become POST for complex filtering
2. **Enhance request structure**: Use structured request objects instead of query parameters
3. **Handle new response format**: Include version information in responses
4. **Implement new features**: Analytics endpoints and real-time capabilities

### Deprecation Handling

- Deprecated versions return warning headers
- Sunset dates are communicated via headers
- Migration guides are available via `/api/version/deprecated`

## Best Practices

### Controller Development

1. **Inherit from appropriate base controller**:
   - Use `VersionAwareController` for version-specific functionality
   - Use `EnhancedBaseController` for general API endpoints

2. **Implement proper error handling**:
   - Use `ExecuteVersionedAsync` for automatic error handling
   - Provide meaningful error messages
   - Include request IDs for tracking

3. **Add comprehensive logging**:
   - Log operation start/completion
   - Include relevant context information
   - Use structured logging format

### Version Management

1. **Plan version lifecycle**:
   - Define deprecation timelines
   - Provide migration paths
   - Communicate breaking changes

2. **Maintain backward compatibility**:
   - Support multiple versions simultaneously
   - Provide clear upgrade paths
   - Document version differences

3. **Monitor usage**:
   - Track version adoption
   - Identify deprecated version usage
   - Plan sunset timelines

## Security Considerations

- **Authentication**: All endpoints require authentication via `[BasicAuthentication]`
- **Authorization**: Role-based access control for sensitive operations
- **Rate Limiting**: Implemented to prevent abuse
- **Input Validation**: Comprehensive model validation
- **Error Information**: Controlled error detail exposure

## Performance Considerations

- **Response Compression**: Enabled for all responses
- **Caching**: Implemented where appropriate
- **Async Operations**: All operations are asynchronous
- **Connection Pooling**: Database connections are pooled
- **Monitoring**: Comprehensive performance tracking

## Monitoring and Observability

- **Health Checks**: Available at `/health`
- **Request/Response Logging**: Comprehensive API call tracking
- **Performance Metrics**: Execution time monitoring
- **Error Tracking**: Detailed error logging and reporting
- **Version Usage**: Tracking of API version adoption

## Future Enhancements

1. **GraphQL Support**: Consider GraphQL for complex queries
2. **WebSocket Integration**: Real-time updates and notifications
3. **Advanced Analytics**: Machine learning-based insights
4. **Microservices**: Break down into smaller, focused services
5. **Event Sourcing**: Implement event-driven architecture

## Conclusion

This architecture provides a robust foundation for the FMS API with:

- **Scalable version management** supporting multiple versions simultaneously
- **Comprehensive error handling** with detailed logging and monitoring
- **High-level code organization** with clear separation of concerns
- **Future-proof design** supporting evolution and enhancement
- **Developer-friendly** with clear patterns and documentation

The implementation follows industry best practices and provides a solid foundation for continued development and maintenance.