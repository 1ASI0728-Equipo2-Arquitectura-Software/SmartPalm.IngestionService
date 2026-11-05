namespace SmartPalm.IngestionService.Api.Resources;

public record UpdateAgronomicThresholdResource(string Type, double? Min, double? Max, string? Description);
