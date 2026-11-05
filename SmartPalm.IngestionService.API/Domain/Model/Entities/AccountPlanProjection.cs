namespace SmartPalm.IngestionService.Domain.Readings;

public class AccountPlanProjection
{
    private AccountPlanProjection() { }

    public AccountPlanProjection(long ownerUserId, bool active)
    {
        OwnerUserId = ownerUserId;
        Active = active;
        UpdatedAt = DateTime.UtcNow;
    }

    public long OwnerUserId { get; private set; }
    public bool Active { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void SetActive(bool active)
    {
        Active = active;
        UpdatedAt = DateTime.UtcNow;
    }
}
