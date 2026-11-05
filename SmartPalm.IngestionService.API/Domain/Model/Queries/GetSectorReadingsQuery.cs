namespace SmartPalm.IngestionService.Domain.Model.Queries;

public record GetSectorReadingsQuery(long SectorId, DateTime From, DateTime To);
