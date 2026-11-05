namespace SmartPalm.IngestionService.Domain.Events;

public record SensorReadingsIngestedIntegrationEvent(
    Guid BatchId, string GatewayMac, int ReadingCount, DateTime SyncedAt, DateTime IngestedAt);
