using SmartPalm.IngestionService.Domain.Readings;

namespace SmartPalm.IngestionService.Domain.Repositories;

public interface ISensorReadingRepository
{
    Task AddRangeAsync(IEnumerable<SensorReading> readings, CancellationToken cancellationToken);
    Task<IReadOnlyList<SensorReading>> ListByGatewayAsync(long ownerUserId, string gatewayMac, DateTime from, DateTime to, string? deviceMac, int page, int size, CancellationToken cancellationToken);
    Task<IReadOnlyList<SensorReading>> ListByDeviceAsync(long ownerUserId, string deviceMac, DateTime from, DateTime to, int page, int size, CancellationToken cancellationToken);
    Task<IReadOnlyList<SensorReading>> LatestByDeviceAsync(long ownerUserId, string deviceMac, CancellationToken cancellationToken);
}
