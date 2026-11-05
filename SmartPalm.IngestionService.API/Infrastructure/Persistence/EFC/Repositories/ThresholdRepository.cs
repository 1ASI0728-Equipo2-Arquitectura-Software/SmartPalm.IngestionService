using Microsoft.EntityFrameworkCore;
using SmartPalm.IngestionService.Domain.Readings;
using SmartPalm.IngestionService.Domain.Repositories;

namespace SmartPalm.IngestionService.Infrastructure.Persistence;

internal class ThresholdRepository(IngestionDbContext context) : IThresholdRepository
{
    public async Task AddRangeAsync(IEnumerable<AgronomicThreshold> values, CancellationToken token) => await context.Thresholds.AddRangeAsync(values, token);
    public Task<AgronomicThreshold?> FindAsync(string device, SensorType type, CancellationToken token) { var d = SensorReading.NormalizeIdentifier(device); return context.Thresholds.SingleOrDefaultAsync(x => x.IotDeviceMacAddress == d && x.Type == type, token); }
    public async Task<IReadOnlyList<AgronomicThreshold>> ListByDeviceAsync(string device, CancellationToken token) { var d = SensorReading.NormalizeIdentifier(device); return await context.Thresholds.AsNoTracking().Where(x => x.IotDeviceMacAddress == d).OrderBy(x => x.Type).ToArrayAsync(token); }
    public async Task<IReadOnlyList<AgronomicThreshold>> ListByGatewayAsync(string gateway, CancellationToken token) { var g = SensorReading.NormalizeIdentifier(gateway); return await context.Thresholds.AsNoTracking().Where(x => x.GatewayMacAddress == g).ToArrayAsync(token); }
}
