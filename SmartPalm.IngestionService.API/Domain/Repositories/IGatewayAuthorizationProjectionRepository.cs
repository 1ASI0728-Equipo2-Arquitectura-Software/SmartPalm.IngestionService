using SmartPalm.IngestionService.Domain.Readings;

namespace SmartPalm.IngestionService.Domain.Repositories;

public interface IGatewayAuthorizationProjectionRepository
{
    Task<GatewayAuthorizationProjection?> FindAsync(string gatewayMac, CancellationToken cancellationToken);
    Task AddAsync(GatewayAuthorizationProjection projection, CancellationToken cancellationToken);
    Task SetPlanActiveForOwnerAsync(long ownerUserId, bool active, CancellationToken cancellationToken);
}
