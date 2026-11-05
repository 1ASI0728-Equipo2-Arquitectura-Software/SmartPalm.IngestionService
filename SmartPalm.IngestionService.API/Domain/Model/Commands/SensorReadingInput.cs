using SmartPalm.IngestionService.Domain.Readings;

namespace SmartPalm.IngestionService.Domain.Model.Commands;

public record SensorReadingInput(string DeviceMac, SensorType SensorType, DateTime MeasuredAt, double Value);
