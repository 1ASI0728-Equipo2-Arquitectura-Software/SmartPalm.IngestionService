using SmartPalm.IngestionService.Api.Resources;
using SmartPalm.IngestionService.Domain.Model.Commands;
using SmartPalm.IngestionService.Domain.Readings;

namespace SmartPalm.IngestionService.Api.Transform;

public static class IngestionResourceAssemblers
{
    public static IngestSensorReadingsCommand ToCommand(
        this ReadDeviceSensorsDataResource resource,
        string gatewayMac,
        string gatewayCredential)
    {
        if (resource.Devices is null || resource.Devices.Count == 0) throw new ArgumentException("At least one device is required.");
        var readings = resource.Devices.SelectMany(device =>
        {
            if (device.Readings is null || device.Readings.Count == 0) throw new ArgumentException($"Device '{device.DeviceMac}' has no readings.");
            return device.Readings.Select(reading => new SensorReadingInput(device.DeviceMac, Parse(reading.SensorType), reading.MeasuredAt, reading.Value));
        }).ToArray();
        return new IngestSensorReadingsCommand(gatewayMac, gatewayCredential, readings, resource.SyncedAt);
    }

    public static UpdateAgronomicThresholdCommand ToCommand(this UpdateAgronomicThresholdResource resource, string deviceMac) =>
        new(deviceMac, Parse(resource.Type), resource.Min, resource.Max, resource.Description);

    private static SensorType Parse(string value) => Enum.TryParse<SensorType>(value, true, out var type)
        ? type : throw new ArgumentException($"Unsupported sensor type '{value}'.");
}
