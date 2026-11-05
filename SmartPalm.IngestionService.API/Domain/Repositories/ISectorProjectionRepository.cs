using SmartPalm.IngestionService.Domain.Readings;

namespace SmartPalm.IngestionService.Domain.Repositories;

public interface ISectorProjectionRepository
{
    Task<SectorDeviceProjection?> FindAsync(long sectorId, CancellationToken cancellationToken);
    Task AddAsync(SectorDeviceProjection projection, CancellationToken cancellationToken);
    void Remove(SectorDeviceProjection projection);
}
