namespace SmartPalm.IngestionService.Application.Contracts;

public record ThresholdResponse(string EdgeMac, string IotMac, double Min, double Max, string Description, string Type);
