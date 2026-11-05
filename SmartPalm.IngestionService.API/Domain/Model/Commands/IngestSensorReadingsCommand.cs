using SmartPalm.IngestionService.Domain.Readings;

namespace SmartPalm.IngestionService.Domain.Model.Commands;

public record IngestSensorReadingsCommand(
    string GatewayMac,
    string GatewayCredential,
    IReadOnlyList<SensorReadingInput> Readings,
    DateTime SyncedAt);
