using Microsoft.EntityFrameworkCore;
using SmartPalm.IngestionService.Domain.Readings;
using SmartPalm.IngestionService.Domain.Repositories;

namespace SmartPalm.IngestionService.Infrastructure.Persistence;

internal class AuthorizedDeviceProjectionRepository(IngestionDbContext context)
    : IAuthorizedDeviceProjectionRepository
{
    public Task<AuthorizedDeviceProjection?> FindAsync(string deviceMac, CancellationToken token) =>
        context.AuthorizedDeviceProjections.SingleOrDefaultAsync(
            x => x.DeviceMac == SensorReading.NormalizeIdentifier(deviceMac), token);

    public async Task AddAsync(AuthorizedDeviceProjection projection, CancellationToken token) =>
        await context.AuthorizedDeviceProjections.AddAsync(projection, token);
}
