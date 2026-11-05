namespace SmartPalm.IngestionService.Application.Contracts;

public record SensorHealthDetail(string SensorType, double Value, double? MinThreshold, double? MaxThreshold, bool IsExceeded);
