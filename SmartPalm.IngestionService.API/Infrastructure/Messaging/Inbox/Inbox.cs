using Microsoft.EntityFrameworkCore;
using SmartPalm.IngestionService.Application.Abstractions;
using SmartPalm.IngestionService.Infrastructure.Persistence;

namespace SmartPalm.IngestionService.Infrastructure.Messaging;

internal class Inbox(IngestionDbContext context) : IInbox
{
    public Task<bool> HasProcessedAsync(Guid messageId, CancellationToken cancellationToken) =>
        context.InboxMessages.AnyAsync(value => value.Id == messageId, cancellationToken);
    public void MarkProcessed(Guid messageId, string eventType) => context.InboxMessages.Add(new InboxMessage
    {
        Id = messageId, EventType = eventType, ProcessedOnUtc = DateTime.UtcNow
    });
}
