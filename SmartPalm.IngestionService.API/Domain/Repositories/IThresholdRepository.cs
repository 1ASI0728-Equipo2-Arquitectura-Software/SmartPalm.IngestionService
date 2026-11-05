using SmartPalm.IngestionService.Domain.Readings;

namespace SmartPalm.IngestionService.Domain.Repositories;

public interface IThresholdRepository
{
    Task AddRangeAsync(IEnumerable<AgronomicThreshold> thresholds, CancellationToken cancellationToken);
    Task<AgronomicThreshold?> FindAsync(string deviceMac, SensorType type, CancellationToken cancellationToken);
    Task<IReadOnlyList<AgronomicThreshold>> ListByDeviceAsync(string deviceMac, CancellationToken cancellationToken);
    Task<IReadOnlyList<AgronomicThreshold>> ListByGatewayAsync(string gatewayMac, CancellationToken cancellationToken);
}
