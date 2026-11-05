using System.Text.Json;
using SmartPalm.IngestionService.Application.Abstractions;
using SmartPalm.IngestionService.Infrastructure.Persistence;

namespace SmartPalm.IngestionService.Infrastructure.Messaging;

internal class IntegrationEventWriter(IngestionDbContext context) : IIntegrationEventWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public void Enqueue<T>(string eventType, T payload) where T : notnull => context.OutboxMessages.Add(new OutboxMessage
    {
        Id = Guid.NewGuid(), EventType = eventType, Content = JsonSerializer.Serialize(payload, JsonOptions), OccurredOnUtc = DateTime.UtcNow
    });
}
