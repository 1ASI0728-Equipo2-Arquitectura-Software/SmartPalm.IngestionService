namespace SmartPalm.IngestionService.Domain.Events;

public record SubscriptionCancelledIntegrationEvent(
    int SubscriptionId,
    int UserId,
    DateTime OccurredAtUtc);
