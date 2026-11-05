namespace SmartPalm.IngestionService.Domain.Readings;

public class SectorDeviceProjection
{
    private SectorDeviceProjection() { }
    public SectorDeviceProjection(long sectorId, string deviceMacAddress)
    {
        if (sectorId <= 0) throw new ArgumentOutOfRangeException(nameof(sectorId));
        SectorId = sectorId;
        IotDeviceMacAddress = SensorReading.NormalizeIdentifier(deviceMacAddress);
        UpdatedAt = DateTime.UtcNow;
    }
    public long SectorId { get; private set; }
    public string IotDeviceMacAddress { get; private set; } = string.Empty;
    public DateTime UpdatedAt { get; private set; }
    public void Assign(string deviceMacAddress)
    {
        IotDeviceMacAddress = SensorReading.NormalizeIdentifier(deviceMacAddress);
        UpdatedAt = DateTime.UtcNow;
    }
}
