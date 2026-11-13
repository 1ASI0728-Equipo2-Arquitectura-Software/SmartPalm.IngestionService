namespace SmartPalm.IngestionService.Domain.Model.Queries;

public record GetSectorReadingsQuery(long OwnerUserId, long SectorId, DateTime From, DateTime To);
