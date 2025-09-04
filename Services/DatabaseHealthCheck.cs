using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Data;
using ERPServer.DataAccess;

namespace Bharuwa.Erp.API.FMS.Services
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IInitialDal _initialDal;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        public DatabaseHealthCheck(IInitialDal initialDal, ILogger<DatabaseHealthCheck> logger)
        {
            _initialDal = initialDal;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Starting database health check");

                var connectionDetails = await _initialDal.getFirmConnectionDetails();
                using var connection = _initialDal.GetConnection(connectionDetails.Item2, connectionDetails.Item1);

                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync(cancellationToken);
                }

                // Simple query to test connectivity
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                command.CommandTimeout = 5;

                var result = await command.ExecuteScalarAsync(cancellationToken);

                if (result != null && result.ToString() == "1")
                {
                    _logger.LogDebug("Database health check completed successfully");
                    return HealthCheckResult.Healthy("Database is accessible");
                }

                return HealthCheckResult.Unhealthy("Database query failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                return HealthCheckResult.Unhealthy("Database health check failed", ex);
            }
        }
    }
}