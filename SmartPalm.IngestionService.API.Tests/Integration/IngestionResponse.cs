namespace SmartPalm.IngestionService.API.Tests.Integration;

internal record IngestionResponse(Guid BatchId, int AcceptedReadings, bool Duplicate);
