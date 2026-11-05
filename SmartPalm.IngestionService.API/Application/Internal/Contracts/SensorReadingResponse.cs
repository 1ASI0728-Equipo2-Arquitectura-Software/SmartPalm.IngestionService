namespace SmartPalm.IngestionService.Application.Contracts;

public record SensorReadingResponse(
    string EdgeDeviceMacAddress,
    string IotDeviceMacAddress,
    string SensorType,
    double Value,
    string Unit,
    DateTime MeasuredAt,
    long OwnerUserId = 0,
    long PlantationId = 0,
    long SectorId = 0);
