namespace SmartPalm.IngestionService.Domain.Events;

public record EdgeGatewayRegisteredIntegrationEvent(
    string EdgeMac,
    long MonitoringZoneId,
    DateTime CreatedAt,
    long OwnerUserId = 0,
    string CredentialHash = "");
