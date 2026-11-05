namespace SmartPalm.IngestionService.Api.Resources;

public record DeviceReadingsResource(string DeviceMac, IReadOnlyList<SensorDataResource> Readings);
