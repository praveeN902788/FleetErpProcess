using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace Bharuwa.Erp.API.FMS.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class SystemStatusController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly ILogger<SystemStatusController> _logger;

        public SystemStatusController(
            HealthCheckService healthCheckService,
            ILogger<SystemStatusController> logger)
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
    }
}