using M01.RepositoryPattern.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PaymentServiceApi.HealthChecks
{
    public class DbHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _db;

        public DbHealthCheck(AppDbContext db) => _db = db;

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken)
        {
            return await _db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("Database is reachable")
                : HealthCheckResult.Unhealthy("Cannot connect to database");
        }
    }
}
