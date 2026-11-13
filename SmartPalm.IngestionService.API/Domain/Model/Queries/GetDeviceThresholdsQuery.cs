namespace SmartPalm.IngestionService.Domain.Model.Queries;

public record GetDeviceThresholdsQuery(long OwnerUserId, string DeviceMac);
