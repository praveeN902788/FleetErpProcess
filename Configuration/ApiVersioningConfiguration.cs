using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bharuwa.Erp.API.FMS.Configuration
{
    /// <summary>
    /// Configuration for API versioning and Swagger documentation
    /// </summary>
    public static class ApiVersioningConfiguration
    {
        /// <summary>
        /// Configures API versioning services
        /// </summary>
        public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
        {
            // Add API versioning
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

            // Add API versioning explorer
            services.AddVersionedApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });

            // Add Swagger generation
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(options =>
            {
                options.OperationFilter<SwaggerDefaultValues>();
                options.IncludeXmlComments(GetXmlCommentsPath());
            });

            return services;
        }

        /// <summary>
        /// Configures Swagger UI with versioning support
        /// </summary>
        public static IApplicationBuilder UseSwaggerWithVersioning(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        $"FMS API {description.GroupName.ToUpperInvariant()}"
                    );
                }
                
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "FMS API Documentation";
                options.DefaultModelsExpandDepth(-1);
                options.DisplayRequestDuration();
            });

            return app;
        }

        private static string GetXmlCommentsPath()
        {
            var basePath = AppContext.BaseDirectory;
            var fileName = typeof(Program).Assembly.GetName().Name + ".xml";
            return Path.Combine(basePath, fileName);
        }
    }

    /// <summary>
    /// Configures Swagger options for each API version
    /// </summary>
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }
        }

        private static OpenApi.Models.OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApi.Models.OpenApiInfo()
            {
                Title = "FMS API",
                Version = description.ApiVersion.ToString(),
                Description = GetVersionDescription(description.ApiVersion.ToString()),
                Contact = new OpenApi.Models.OpenApiContact()
                {
                    Name = "FMS Development Team",
                    Email = "fms-dev@bharuwa.com"
                },
                License = new OpenApi.Models.OpenApiLicense()
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }

        private static string GetVersionDescription(string version)
        {
            return version switch
            {
                "1.0" => "Initial version of the FMS API with basic CRUD operations, authentication, and basic reporting functionality.",
                "2.0" => "Enhanced version with improved filtering, advanced reporting, real-time notifications, and analytics capabilities.",
                "1.5" => "Intermediate version with bug fixes and minor enhancements. This version is deprecated.",
                _ => $"API version {version} with various features and improvements."
            };
        }
    }

    /// <summary>
    /// Swagger operation filter for default values
    /// </summary>
    public class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(OpenApi.Models.OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;

            operation.Deprecated |= apiDescription.IsDeprecated();

            if (operation.Parameters == null)
            {
                return;
            }

            foreach (var parameter in operation.Parameters)
            {
                var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }

                if (parameter.Schema.Default == null && description.DefaultValue != null)
                {
                    parameter.Schema.Default = OpenApiAnyFactory.CreateFromJson(description.DefaultValue.ToString());
                }

                parameter.Required |= description.IsRequired;
            }
        }
    }
}