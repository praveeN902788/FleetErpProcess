using Bharuwa.Erp.API.FMS.Configuration;
using Bharuwa.Erp.API.FMS.Middleware;
using Bharuwa.Erp.API.FMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace Bharuwa.Erp.API.FMS.Startup
{
    /// <summary>
    /// Enhanced startup configuration for the FMS API
    /// </summary>
    public static class EnhancedStartup
    {
        /// <summary>
        /// Configures all services for the application
        /// </summary>
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add API versioning and Swagger
            services.AddApiVersioningConfiguration();

            // Add application services
            services.AddApplicationServices(configuration);

            // Add controllers
            services.AddControllers(options =>
            {
                options.Filters.Add(new ProducesAttribute("application/json"));
                options.Filters.Add(new ConsumesAttribute("application/json"));
            });

            // Add health checks
            services.AddHealthChecks()
                .AddCheck("api", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is healthy"));

            // Add CORS
            services.AddCors(options =>
            {
                options.AddPolicy("DefaultPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Add response compression
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
            });

            // Add rate limiting
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
        /// Configures the application pipeline
        /// </summary>
        public static IApplicationBuilder ConfigurePipeline(this IApplicationBuilder app, IWebHostEnvironment environment, IApiVersionDescriptionProvider provider)
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

            // Use Swagger with versioning
            app.UseSwaggerWithVersioning(provider);

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