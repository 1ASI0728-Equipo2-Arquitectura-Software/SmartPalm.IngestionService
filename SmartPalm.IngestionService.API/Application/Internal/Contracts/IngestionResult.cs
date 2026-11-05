namespace SmartPalm.IngestionService.Application.Contracts;

public record IngestionResult(Guid BatchId, int AcceptedReadings, bool Duplicate);
