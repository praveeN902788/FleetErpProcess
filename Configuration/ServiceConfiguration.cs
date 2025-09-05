using Bharuwa.Erp.API.FMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bharuwa.Erp.API.FMS.Configuration
{
    /// <summary>
    /// Configuration for dependency injection and service registration
    /// </summary>
    public static class ServiceConfiguration
    {
        /// <summary>
        /// Registers all application services
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register version management service
            services.AddScoped<IVersionManagementService, VersionManagementService>();

            // Register logging services
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.AddEventSourceLogger();
                
                // Add structured logging
                builder.AddJsonConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
                });
            });

            // Register health checks
            services.AddHealthChecks()
                .AddCheck("api", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is healthy"));

            // Register CORS
            services.AddCors(options =>
            {
                options.AddPolicy("DefaultPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Register API behavior options
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
                options.SuppressMapClientErrors = true;
            });

            // Register response compression
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
            });

            // Register rate limiting
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = Microsoft.AspNetCore.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    return Microsoft.AspNetCore.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new Microsoft.AspNetCore.RateLimiting.FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = Microsoft.AspNetCore.RateLimiting.QueueProcessingOrder.OldestFirst,
                            QueueLimit = 10
                        });
                });
            });

            return services;
        }

        /// <summary>
        /// Configures application pipeline
        /// </summary>
        public static IApplicationBuilder UseApplicationPipeline(this IApplicationBuilder app, IWebHostEnvironment environment)
        {
            // Use HTTPS redirection
            app.UseHttpsRedirection();

            // Use response compression
            app.UseResponseCompression();

            // Use CORS
            app.UseCors("DefaultPolicy");

            // Use rate limiting
            app.UseRateLimiter();

            // Use API logging middleware
            app.UseApiLogging();

            // Use health checks
            app.UseHealthChecks("/health");

            // Use routing
            app.UseRouting();

            // Use authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Use endpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });

            return app;
        }
    }
}