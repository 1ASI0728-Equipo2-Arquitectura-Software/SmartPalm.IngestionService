using Microsoft.EntityFrameworkCore;
using SmartPalm.IngestionService.Domain.Readings;
using SmartPalm.IngestionService.Domain.Repositories;

namespace SmartPalm.IngestionService.Infrastructure.Persistence;

internal class SensorReadingRepository(IngestionDbContext context) : ISensorReadingRepository
{
    public async Task AddRangeAsync(IEnumerable<SensorReading> values, CancellationToken token) => await context.SensorReadings.AddRangeAsync(values, token);
    public async Task<IReadOnlyList<SensorReading>> ListByGatewayAsync(string gateway, DateTime from, DateTime to, string? device, int page, int size, CancellationToken token)
    {
        ValidatePage(page, size); var normalized = SensorReading.NormalizeIdentifier(gateway); var query = context.SensorReadings.AsNoTracking().Where(x => x.GatewayMacAddress == normalized && x.MeasuredAt >= Utc(from) && x.MeasuredAt <= Utc(to));
        if (!string.IsNullOrWhiteSpace(device)) { var d = SensorReading.NormalizeIdentifier(device); query = query.Where(x => x.IotDeviceMacAddress == d); }
        return await query.OrderByDescending(x => x.MeasuredAt).Skip((page - 1) * size).Take(size).ToArrayAsync(token);
    }
    public async Task<IReadOnlyList<SensorReading>> ListByDeviceAsync(string device, DateTime from, DateTime to, int page, int size, CancellationToken token)
    {
        ValidatePage(page, size); var normalized = SensorReading.NormalizeIdentifier(device);
        return await context.SensorReadings.AsNoTracking().Where(x => x.IotDeviceMacAddress == normalized && x.MeasuredAt >= Utc(from) && x.MeasuredAt <= Utc(to)).OrderByDescending(x => x.MeasuredAt).Skip((page - 1) * size).Take(size).ToArrayAsync(token);
    }
    public async Task<IReadOnlyList<SensorReading>> LatestByDeviceAsync(string device, CancellationToken token)
    {
        var normalized = SensorReading.NormalizeIdentifier(device);
        return await context.SensorReadings.AsNoTracking().Where(x => x.IotDeviceMacAddress == normalized)
            .GroupBy(x => x.Type).Select(g => g.OrderByDescending(x => x.MeasuredAt).First()).ToArrayAsync(token);
    }
    private static void ValidatePage(int page, int size) { if (page <= 0 || size is <= 0 or > 10000) throw new ArgumentOutOfRangeException(nameof(page), "Page and size must be positive; size cannot exceed 10000."); }
    private static DateTime Utc(DateTime value) => value == DateTime.MinValue || value == DateTime.MaxValue ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value.ToUniversalTime();
}
