using SmartPalm.IngestionService.Domain.Readings;

namespace SmartPalm.IngestionService.Domain.Events;

public record ThresholdExceededIntegrationEvent(
    Guid BatchId, string GatewayMac, string DeviceMac, SensorType SensorType,
    double Value, double Min, double Max, DateTime MeasuredAt);
