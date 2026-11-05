using Microsoft.EntityFrameworkCore;
using SmartPalm.IngestionService.Domain.Readings;
using SmartPalm.IngestionService.Domain.Repositories;

namespace SmartPalm.IngestionService.Infrastructure.Persistence;

internal class SectorProjectionRepository(IngestionDbContext context) : ISectorProjectionRepository
{
    public Task<SectorDeviceProjection?> FindAsync(long sectorId, CancellationToken token) => context.SectorDeviceProjections.SingleOrDefaultAsync(x => x.SectorId == sectorId, token);
    public async Task AddAsync(SectorDeviceProjection projection, CancellationToken token) => await context.SectorDeviceProjections.AddAsync(projection, token);
    public void Remove(SectorDeviceProjection projection) => context.SectorDeviceProjections.Remove(projection);
}
