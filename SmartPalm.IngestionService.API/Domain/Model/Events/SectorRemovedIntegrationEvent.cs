namespace SmartPalm.IngestionService.Domain.Events;

public record SectorRemovedIntegrationEvent(
    long SectorId, long PlantationId, string? IotDeviceMacAddress, DateTime OccurredAt);
