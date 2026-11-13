namespace SmartPalm.IngestionService.Domain.Model.Queries;

public record GetGatewayReadingsQuery(long OwnerUserId, string GatewayMac, DateTime From, DateTime To, string? DeviceMac, int Page, int Size);
