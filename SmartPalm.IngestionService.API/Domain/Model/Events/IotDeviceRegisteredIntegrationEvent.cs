namespace SmartPalm.IngestionService.Domain.Events;

public record IotDeviceRegisteredIntegrationEvent(
    string EdgeDeviceMacAddress,
    string IotDeviceMacAddress,
    long PlantationId,
    DateTime CreatedAt,
    long OwnerUserId = 0,
    long SectorId = 0,
    string BootstrapCredentialHash = "",
    bool IsProvisioned = false);
