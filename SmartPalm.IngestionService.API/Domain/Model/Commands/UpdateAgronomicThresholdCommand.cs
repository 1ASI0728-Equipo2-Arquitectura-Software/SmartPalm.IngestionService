using SmartPalm.IngestionService.Domain.Readings;

namespace SmartPalm.IngestionService.Domain.Model.Commands;

public record UpdateAgronomicThresholdCommand(string DeviceMac, SensorType Type, double? Min, double? Max, string? Description);
