using SmartPalm.IngestionService.Domain.Readings;

namespace SmartPalm.IngestionService.Domain.Services;

public static class ThresholdPolicy
{
    public static IReadOnlyList<AgronomicThreshold> CreateDefaults(string gatewayMac, string deviceMac) =>
    [
        new(gatewayMac, deviceMac, SensorType.Humidity, 0, 100, "Humidity Sensor Threshold"),
        new(gatewayMac, deviceMac, SensorType.PH, 0, 14, "PH Sensor Threshold"),
        new(gatewayMac, deviceMac, SensorType.Luminosity, 0, 100, "Luminosity Sensor Threshold"),
        new(gatewayMac, deviceMac, SensorType.Temperature, 10, 40, "Temperature Sensor Threshold"),
        new(gatewayMac, deviceMac, SensorType.SoilMoisture, 20, 80, "Soil Moisture Sensor Threshold")
    ];

    public static double Deviation(double value, double min, double max)
    {
        if (value < min) return min == 0 ? 1 : (min - value) / Math.Abs(min);
        if (value > max) return max == 0 ? 1 : (value - max) / Math.Abs(max);
        return 0;
    }
}
