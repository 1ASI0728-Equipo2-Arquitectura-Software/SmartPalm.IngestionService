namespace SmartPalm.IngestionService.Api.Resources;

public record SensorDataResource(string SensorType, DateTime MeasuredAt, double Value);
