using SmartPalm.IngestionService.Domain.Readings;

namespace SmartPalm.IngestionService.Domain.Repositories;

public interface IAuthorizedDeviceProjectionRepository
{
    Task<AuthorizedDeviceProjection?> FindAsync(string deviceMac, CancellationToken cancellationToken);
    Task AddAsync(AuthorizedDeviceProjection projection, CancellationToken cancellationToken);
}
