namespace SmartPalm.IngestionService.Domain.Model.Commands;

public record RegisterDeviceProjectionCommand(string GatewayMac, string DeviceMac);
