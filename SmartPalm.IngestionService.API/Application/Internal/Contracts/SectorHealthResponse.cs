namespace SmartPalm.IngestionService.Application.Contracts;

public record SectorHealthResponse(long SectorId, string Status, IReadOnlyList<SensorHealthDetail> SensorDetails);
