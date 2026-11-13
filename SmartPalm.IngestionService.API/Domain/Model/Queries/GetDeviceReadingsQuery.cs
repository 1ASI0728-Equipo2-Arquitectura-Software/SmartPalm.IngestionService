namespace SmartPalm.IngestionService.Domain.Model.Queries;

public record GetDeviceReadingsQuery(long OwnerUserId, string DeviceMac, DateTime From, DateTime To, int Page, int Size);
