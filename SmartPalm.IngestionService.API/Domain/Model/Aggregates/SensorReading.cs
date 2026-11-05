namespace SmartPalm.IngestionService.Domain.Readings;

public class SensorReading
{
    private SensorReading() { }

    public SensorReading(
        Guid batchId,
        string gatewayMac,
        string deviceMac,
        SensorType type,
        DateTime measuredAt,
        double value,
        long ownerUserId = 0,
        long plantationId = 0,
        long sectorId = 0)
    {
        if (batchId == Guid.Empty) throw new ArgumentException("Batch identifier is required.", nameof(batchId));
        if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value));
        BatchId = batchId;
        GatewayMacAddress = NormalizeIdentifier(gatewayMac);
        IotDeviceMacAddress = NormalizeIdentifier(deviceMac);
        Type = type;
        Unit = SensorTypeRules.UnitFor(type);
        MeasuredAt = NormalizeUtc(measuredAt);
        Value = value;
        OwnerUserId = ownerUserId;
        PlantationId = plantationId;
        SectorId = sectorId;
        ReceivedAt = DateTime.UtcNow;
    }

    public long Id { get; private set; }
    public Guid BatchId { get; private set; }
    public string GatewayMacAddress { get; private set; } = string.Empty;
    public string IotDeviceMacAddress { get; private set; } = string.Empty;
    public DateTime MeasuredAt { get; private set; }
    public SensorType Type { get; private set; }
    public MeasureUnit Unit { get; private set; }
    public double Value { get; private set; }
    public long OwnerUserId { get; private set; }
    public long PlantationId { get; private set; }
    public long SectorId { get; private set; }
    public DateTime ReceivedAt { get; private set; }

    public static string NormalizeIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Device identifier is required.", nameof(value));
        return value.Trim().ToUpperInvariant();
    }

    private static DateTime NormalizeUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
