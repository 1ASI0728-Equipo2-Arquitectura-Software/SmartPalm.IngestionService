namespace SmartPalm.IngestionService.Infrastructure.Messaging;

public class InboxMessage
{
    public InboxMessage() { }
    public InboxMessage(Guid id, string eventType)
    {
        Id = id;
        EventType = eventType;
        ProcessedOnUtc = DateTime.UtcNow;
    }

    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedOnUtc { get; set; }
}
