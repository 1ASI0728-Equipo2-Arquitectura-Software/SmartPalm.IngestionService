namespace SmartPalm.IngestionService.Infrastructure.Messaging;

public class InboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedOnUtc { get; set; }
}
