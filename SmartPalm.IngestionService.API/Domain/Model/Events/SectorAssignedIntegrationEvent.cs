namespace SmartPalm.IngestionService.Domain.Events;

public record SectorAssignedIntegrationEvent(
    long SectorId, long PlantationId, string? IotDeviceMacAddress, string SectorName, DateTime OccurredAt);
