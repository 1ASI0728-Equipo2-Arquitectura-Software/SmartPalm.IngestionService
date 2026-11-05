using Microsoft.EntityFrameworkCore;
using SmartPalm.IngestionService.Domain.Readings;
using SmartPalm.IngestionService.Domain.Repositories;

namespace SmartPalm.IngestionService.Infrastructure.Persistence;

internal class GatewayAuthorizationProjectionRepository(IngestionDbContext context)
    : IGatewayAuthorizationProjectionRepository
{
    public Task<GatewayAuthorizationProjection?> FindAsync(string gatewayMac, CancellationToken token) =>
        context.GatewayAuthorizationProjections.SingleOrDefaultAsync(
            x => x.GatewayMac == SensorReading.NormalizeIdentifier(gatewayMac), token);

    public async Task AddAsync(GatewayAuthorizationProjection projection, CancellationToken token) =>
        await context.GatewayAuthorizationProjections.AddAsync(projection, token);

    public async Task SetPlanActiveForOwnerAsync(long ownerUserId, bool active, CancellationToken token)
    {
        var gateways = await context.GatewayAuthorizationProjections
            .Where(x => x.OwnerUserId == ownerUserId)
            .ToListAsync(token);
        foreach (var gateway in gateways) gateway.SetPlanActive(active);
    }
}
