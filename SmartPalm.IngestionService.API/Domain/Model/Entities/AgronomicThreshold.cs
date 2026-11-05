namespace SmartPalm.IngestionService.Domain.Readings;

public class AgronomicThreshold
{
    private AgronomicThreshold() { }

    public AgronomicThreshold(string gatewayMac, string deviceMac, SensorType type, double min, double max, string description)
    {
        GatewayMacAddress = SensorReading.NormalizeIdentifier(gatewayMac);
        IotDeviceMacAddress = SensorReading.NormalizeIdentifier(deviceMac);
        Type = type;
        Update(min, max, description);
    }

    public long Id { get; private set; }
    public string GatewayMacAddress { get; private set; } = string.Empty;
    public string IotDeviceMacAddress { get; private set; } = string.Empty;
    public SensorType Type { get; private set; }
    public double Min { get; private set; }
    public double Max { get; private set; }
    public string Description { get; private set; } = string.Empty;

    public bool IsExceededBy(double value) => value < Min || value > Max;

    public void Update(double? min, double? max, string? description)
    {
        var nextMin = min ?? Min;
        var nextMax = max ?? Max;
        if (nextMin > nextMax) throw new ArgumentException("Minimum threshold cannot exceed maximum threshold.");
        Min = nextMin;
        Max = nextMax;
        if (description is not null)
        {
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description cannot be empty.");
            Description = description.Trim();
        }
    }
}
