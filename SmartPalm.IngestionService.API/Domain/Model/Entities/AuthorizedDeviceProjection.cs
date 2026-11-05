namespace SmartPalm.IngestionService.Domain.Readings;

public class AuthorizedDeviceProjection
{
    private AuthorizedDeviceProjection() { }

    public AuthorizedDeviceProjection(
        string deviceMac,
        string gatewayMac,
        long ownerUserId,
        long plantationId,
        long sectorId)
    {
        if (ownerUserId <= 0) throw new ArgumentOutOfRangeException(nameof(ownerUserId));
        if (plantationId <= 0) throw new ArgumentOutOfRangeException(nameof(plantationId));
        if (sectorId <= 0) throw new ArgumentOutOfRangeException(nameof(sectorId));
        DeviceMac = SensorReading.NormalizeIdentifier(deviceMac);
        GatewayMac = SensorReading.NormalizeIdentifier(gatewayMac);
        OwnerUserId = ownerUserId;
        PlantationId = plantationId;
        SectorId = sectorId;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public string DeviceMac { get; private set; } = string.Empty;
    public string GatewayMac { get; private set; } = string.Empty;
    public long OwnerUserId { get; private set; }
    public long PlantationId { get; private set; }
    public long SectorId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void UpdateRegistration(string gatewayMac, long ownerUserId, long plantationId, long sectorId)
    {
        GatewayMac = SensorReading.NormalizeIdentifier(gatewayMac);
        OwnerUserId = ownerUserId;
        PlantationId = plantationId;
        SectorId = sectorId;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignSector(long sectorId)
    {
        if (sectorId <= 0) throw new ArgumentOutOfRangeException(nameof(sectorId));
        SectorId = sectorId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
