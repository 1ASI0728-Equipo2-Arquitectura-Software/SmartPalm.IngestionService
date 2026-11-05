namespace SmartPalm.IngestionService.Domain.Readings;

public class GatewayAuthorizationProjection
{
    private GatewayAuthorizationProjection() { }

    public GatewayAuthorizationProjection(
        string gatewayMac,
        long ownerUserId,
        string credentialHash,
        DateTime? updatedAt = null)
    {
        GatewayMac = SensorReading.NormalizeIdentifier(gatewayMac);
        OwnerUserId = ownerUserId;
        CredentialHash = credentialHash.Trim().ToUpperInvariant();
        IsActive = true;
        UpdatedAt = updatedAt ?? DateTime.UtcNow;
    }

    public string GatewayMac { get; private set; } = string.Empty;
    public long OwnerUserId { get; private set; }
    public string CredentialHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public bool PlanActive { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void UpdateRegistration(long ownerUserId, string credentialHash)
    {
        OwnerUserId = ownerUserId;
        CredentialHash = credentialHash.Trim().ToUpperInvariant();
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPlanActive(bool active)
    {
        PlanActive = active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
