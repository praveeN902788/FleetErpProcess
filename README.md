# Bharuwa ERP FMS API

A modern, enterprise-grade Financial Management System API built with .NET 9.0, featuring clean architecture, comprehensive logging, and robust error handling.

## 🚀 Key Improvements Made

### Architecture & Design Patterns
- **Clean Architecture**: Implemented proper separation of concerns with distinct layers
- **Dependency Injection**: Full DI container setup with service registration
- **Repository Pattern**: Abstracted data access through interfaces
- **Service Layer**: Business logic separated into dedicated services
- **DTO Pattern**: Clean data transfer objects for API responses

### Security Enhancements
- **JWT Authentication**: Modern JWT-based authentication with proper token validation
- **Authorization**: Role-based access control with claims-based authorization
- **CORS Configuration**: Properly configured CORS policies
- **Input Validation**: Comprehensive request validation with data annotations
- **Rate Limiting**: Built-in rate limiting capabilities

### Error Handling & Logging
- **Global Exception Filter**: Centralized error handling with consistent response format
- **Structured Logging**: Serilog integration with structured logging
- **Request Logging**: Middleware for tracking all incoming requests
- **Performance Monitoring**: Built-in performance tracking and slow request detection
- **Health Checks**: Comprehensive health monitoring for all system components

### API Design
- **API Versioning**: Proper API versioning support
- **Swagger Documentation**: Auto-generated API documentation
- **Consistent Response Format**: Standardized API response structure
- **HTTP Status Codes**: Proper HTTP status code usage
- **Async/Await**: Full async/await pattern implementation

### Configuration Management
- **Strongly-Typed Configuration**: Type-safe configuration classes
- **Environment-Specific Settings**: Separate configurations for different environments
- **Configuration Validation**: Runtime configuration validation
- **Secrets Management**: Proper handling of sensitive configuration

## 📁 Project Structure

```
Bharuwa.Erp.API.FMS/
├── Configuration/           # Configuration classes
│   └── AppSettings.cs
├── Controllers/             # API controllers
│   ├── AuthController.cs
│   └── HealthController.cs
├── Filters/                # Action filters
│   ├── GlobalExceptionFilter.cs
│   └── ValidationFilter.cs
├── Middleware/             # Custom middleware
│   ├── RequestLoggingMiddleware.cs
│   └── PerformanceMonitoringMiddleware.cs
├── Models/                 # Data models and DTOs
│   └── ApiResponse.cs
├── Services/               # Business logic services
│   ├── AuthenticationService.cs
│   ├── UserService.cs
│   ├── ConfigurationService.cs
│   └── DatabaseHealthCheck.cs
├── Validators/             # Custom validators
├── Program.cs              # Application entry point
├── appsettings.json        # Configuration file
└── README.md              # This file
```

## 🛠️ Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server (or your preferred database)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Bharuwa.Erp.API.FMS
   ```

2. **Configure the application**
   - Update `appsettings.json` with your database connection strings
   - Configure JWT settings with a secure key
   - Set up CORS origins for your frontend applications

3. **Install dependencies**
   ```bash
   dotnet restore
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the API**
   - API: `https://localhost:5001`
   - Swagger UI: `https://localhost:5001/swagger`
   - Health Check: `https://localhost:5001/health`

## 🔧 Configuration

### JWT Settings
```json
{
  "Jwt": {
    "Key": "your-super-secret-jwt-key-with-at-least-256-bits",
    "Issuer": "BharuwaERP",
    "Audience": "BharuwaERPUsers",
    "ExpirationMinutes": 60
  }
}
```

### Database Settings
```json
{
  "Database": {
    "CommandTimeout": 30,
    "MaxRetryAttempts": 3,
    "RetryDelaySeconds": 2
  }
}
```

### CORS Settings
```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "https://yourdomain.com"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
    "AllowCredentials": true
  }
}
```

## 📊 API Endpoints

### Authentication
- `GET /api/v1/auth/configuration` - Get user configuration
- `GET /api/v1/auth/validate-access` - Validate user access to resource
- `GET /api/v1/auth/menus` - Get user menus

### Health Monitoring
- `GET /api/v1/health` - Detailed health status
- `GET /api/v1/health/live` - Liveness probe
- `GET /api/v1/health/ready` - Readiness probe

## 🔒 Security Features

### Authentication
- JWT-based authentication
- Token refresh mechanism
- Browser identity token validation
- Anonymous access support for public endpoints

### Authorization
- Role-based access control
- Resource-level permissions
- Claims-based authorization

### Data Protection
- Input validation and sanitization
- SQL injection prevention
- XSS protection
- CSRF protection

## 📈 Monitoring & Logging

### Logging
- Structured logging with Serilog
- Request/response logging
- Performance metrics
- Error tracking with stack traces

### Health Checks
- Database connectivity
- External service dependencies
- Application health status
- Kubernetes-ready health endpoints

### Performance Monitoring
- Request duration tracking
- Slow request detection
- Memory usage monitoring
- Database query performance

## 🧪 Testing

### Unit Testing
```bash
dotnet test
```

### Integration Testing
```bash
dotnet test --filter Category=Integration
```

### API Testing
Use the Swagger UI or tools like Postman to test the API endpoints.

## 🚀 Deployment

### Docker
```bash
docker build -t bharuwa-erp-fms .
docker run -p 5001:5001 bharuwa-erp-fms
```

### Kubernetes
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: bharuwa-erp-fms
spec:
  replicas: 3
  selector:
    matchLabels:
      app: bharuwa-erp-fms
  template:
    metadata:
      labels:
        app: bharuwa-erp-fms
    spec:
      containers:
      - name: bharuwa-erp-fms
        image: bharuwa-erp-fms:latest
        ports:
        - containerPort: 5001
        livenessProbe:
          httpGet:
            path: /health/live
            port: 5001
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 5001
```

## 🔧 Development Guidelines

### Code Style
- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods small and focused

### Error Handling
- Use global exception filter for consistent error responses
- Log all exceptions with appropriate log levels
- Return appropriate HTTP status codes
- Provide meaningful error messages

### Performance
- Use async/await for I/O operations
- Implement proper connection pooling
- Cache frequently accessed data
- Monitor and optimize database queries

### Security
- Validate all inputs
- Use parameterized queries
- Implement proper authentication and authorization
- Keep dependencies updated

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the documentation

## 🔄 Migration from Legacy Code

### Key Changes
1. **Authentication**: Replaced `BasicAuthentication` with modern JWT authentication
2. **Error Handling**: Centralized error handling with `GlobalExceptionFilter`
3. **Logging**: Replaced basic logging with structured logging using Serilog
4. **Configuration**: Moved from hard-coded values to strongly-typed configuration
5. **Architecture**: Implemented proper separation of concerns with services

### Breaking Changes
- Authentication header format changed from custom format to standard Bearer token
- API response format standardized
- Some endpoint paths may have changed
- Configuration structure completely restructured

### Migration Steps
1. Update client applications to use new authentication format
2. Update API calls to use new response format
3. Update configuration files
4. Test thoroughly before deploying to production

---

**Note**: This is a significant architectural improvement that follows enterprise-grade patterns and best practices. The code is now more maintainable, testable, and scalable.
