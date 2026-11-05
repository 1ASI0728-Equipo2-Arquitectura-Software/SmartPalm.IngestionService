using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartPalm.IngestionService.Infrastructure.Persistence;

namespace SmartPalm.IngestionService.Api;

public class IngestionDatabaseHealthCheck(IngestionDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token = default) =>
        await db.Database.CanConnectAsync(token) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Ingestion database is unavailable.");
}
