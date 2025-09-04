using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace Bharuwa.Erp.API.FMS.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            HealthCheckService healthCheckService,
            ILogger<HealthController> logger)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
        }

        /// <summary>
        /// Get detailed health status of all system components
        /// </summary>
        /// <returns>Detailed health report</returns>
        [HttpGet]
        [ProducesResponseType(typeof(HealthReport), 200)]
        [ProducesResponseType(typeof(HealthReport), 503)]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GetHealth()
        {
            try
            {
                _logger.LogDebug("Performing health check");

                var report = await _healthCheckService.CheckHealthAsync();

                var response = new
                {
                    Status = report.Status.ToString(),
                    TotalDuration = report.TotalDuration,
                    Entries = report.Entries.Select(entry => new
                    {
                        Name = entry.Key,
                        Status = entry.Value.Status.ToString(),
                        Description = entry.Value.Description,
                        Duration = entry.Value.Duration,
                        Tags = entry.Value.Tags
                    }),
                    Timestamp = DateTime.UtcNow
                };

                return report.Status == HealthStatus.Healthy ? Ok(response) : StatusCode(503, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Message = "Health check failed",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get basic health status (liveness probe)
        /// </summary>
        /// <returns>Basic health status</returns>
        [HttpGet("live")]
        [ProducesResponseType(200)]
        [MapToApiVersion("1.0")]
        public IActionResult GetLiveness()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get readiness status (readiness probe)
        /// </summary>
        /// <returns>Readiness status</returns>
        [HttpGet("ready")]
        [ProducesResponseType(200)]
        [ProducesResponseType(503)]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GetReadiness()
        {
            try
            {
                var report = await _healthCheckService.CheckHealthAsync(registration => 
                    registration.Tags.Contains("ready"));

                return report.Status == HealthStatus.Healthy ? Ok(new
                {
                    Status = "Ready",
                    Timestamp = DateTime.UtcNow
                }) : StatusCode(503, new
                {
                    Status = "Not Ready",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Readiness check failed");
                return StatusCode(503, new
                {
                    Status = "Not Ready",
                    Message = "Readiness check failed",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get enhanced detailed health status with additional metrics (v2.0)
        /// </summary>
        /// <returns>Enhanced detailed health report</returns>
        [HttpGet]
        [ProducesResponseType(typeof(HealthReport), 200)]
        [ProducesResponseType(typeof(HealthReport), 503)]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> GetHealthV2()
        {
            try
            {
                _logger.LogDebug("Performing enhanced health check (v2.0)");

                var report = await _healthCheckService.CheckHealthAsync();

                var response = new
                {
                    Status = report.Status.ToString(),
                    TotalDuration = report.TotalDuration,
                    Entries = report.Entries.Select(entry => new
                    {
                        Name = entry.Key,
                        Status = entry.Value.Status.ToString(),
                        Description = entry.Value.Description,
                        Duration = entry.Value.Duration,
                        Tags = entry.Value.Tags,
                        Exception = entry.Value.Exception?.Message
                    }),
                    Timestamp = DateTime.UtcNow,
                    Version = "2.0",
                    Summary = new
                    {
                        TotalChecks = report.Entries.Count,
                        HealthyChecks = report.Entries.Count(e => e.Value.Status == HealthStatus.Healthy),
                        UnhealthyChecks = report.Entries.Count(e => e.Value.Status == HealthStatus.Unhealthy),
                        DegradedChecks = report.Entries.Count(e => e.Value.Status == HealthStatus.Degraded)
                    }
                };

                return report.Status == HealthStatus.Healthy ? Ok(response) : StatusCode(503, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Enhanced health check failed (v2.0)");
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Message = "Enhanced health check failed",
                    Timestamp = DateTime.UtcNow,
                    Version = "2.0"
                });
            }
        }

        /// <summary>
        /// Get enhanced basic health status with additional info (v2.0)
        /// </summary>
        /// <returns>Enhanced basic health status</returns>
        [HttpGet("live")]
        [ProducesResponseType(200)]
        [MapToApiVersion("2.0")]
        public IActionResult GetLivenessV2()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "2.0",
                Uptime = Environment.TickCount64,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            });
        }

        /// <summary>
        /// Get enhanced readiness status with dependency checks (v2.0)
        /// </summary>
        /// <returns>Enhanced readiness status</returns>
        [HttpGet("ready")]
        [ProducesResponseType(200)]
        [ProducesResponseType(503)]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> GetReadinessV2()
        {
            try
            {
                var report = await _healthCheckService.CheckHealthAsync(registration => 
                    registration.Tags.Contains("ready"));

                var readyChecks = report.Entries.Where(e => e.Value.Status == HealthStatus.Healthy).Count();
                var totalReadyChecks = report.Entries.Count;

                return report.Status == HealthStatus.Healthy ? Ok(new
                {
                    Status = "Ready",
                    Timestamp = DateTime.UtcNow,
                    Version = "2.0",
                    ReadyChecks = readyChecks,
                    TotalReadyChecks = totalReadyChecks,
                    ReadinessPercentage = totalReadyChecks > 0 ? (readyChecks * 100.0 / totalReadyChecks) : 0
                }) : StatusCode(503, new
                {
                    Status = "Not Ready",
                    Timestamp = DateTime.UtcNow,
                    Version = "2.0",
                    ReadyChecks = readyChecks,
                    TotalReadyChecks = totalReadyChecks,
                    ReadinessPercentage = totalReadyChecks > 0 ? (readyChecks * 100.0 / totalReadyChecks) : 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Enhanced readiness check failed (v2.0)");
                return StatusCode(503, new
                {
                    Status = "Not Ready",
                    Message = "Enhanced readiness check failed",
                    Timestamp = DateTime.UtcNow,
                    Version = "2.0"
                });
            }
        }
    }
}