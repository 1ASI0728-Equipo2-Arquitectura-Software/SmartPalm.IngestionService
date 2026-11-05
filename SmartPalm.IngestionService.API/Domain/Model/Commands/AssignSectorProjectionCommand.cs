namespace SmartPalm.IngestionService.Domain.Model.Commands;

public record AssignSectorProjectionCommand(long SectorId, string DeviceMac);
