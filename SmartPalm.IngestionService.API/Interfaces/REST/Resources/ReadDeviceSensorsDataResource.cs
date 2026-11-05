namespace SmartPalm.IngestionService.Api.Resources;

public record ReadDeviceSensorsDataResource(IReadOnlyList<DeviceReadingsResource> Devices, DateTime SyncedAt);
