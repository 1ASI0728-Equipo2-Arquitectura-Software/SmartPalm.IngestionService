using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartPalm.IngestionService.Application.EventHandlers;
using SmartPalm.IngestionService.Domain.Events;
using SmartPalm.IngestionService.Infrastructure.Configuration;

namespace SmartPalm.IngestionService.Infrastructure.Messaging;

internal class IntegrationEventConsumer(IServiceScopeFactory scopes, IOptions<RabbitMqOptions> options, ILogger<IntegrationEventConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RabbitMqOptions _options = options.Value;
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        if (!_options.Enabled) return;
        while (!token.IsCancellationRequested)
        {
            try { await ConsumeAsync(token); }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception ex) { logger.LogWarning(ex, "Integration consumer disconnected; retrying."); await Task.Delay(TimeSpan.FromSeconds(3), token); }
        }
    }
    private async Task ConsumeAsync(CancellationToken token)
    {
        var factory = new ConnectionFactory { Uri = new Uri(_options.Uri), AutomaticRecoveryEnabled = true };
        await using var connection = await factory.CreateConnectionAsync(token);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: token);
        await channel.ExchangeDeclareAsync(_options.Exchange, ExchangeType.Topic, true, false, cancellationToken: token);
        await channel.QueueDeclareAsync(_options.Queue, true, false, false, cancellationToken: token);
        foreach (var key in new[]
        {
            "device.edge-gateway-registered.v1",
            "device.iot-device-registered.v1",
            "crop.sector-assigned.v1",
            "crop.sector-removed.v1",
            "smartpalm.identity.subscription-activated.v1",
            "smartpalm.identity.subscription-cancelled.v1",
            "smartpalm.identity.user-deletion-requested.v1"
        })
            await channel.QueueBindAsync(_options.Queue, _options.Exchange, key, cancellationToken: token);
        await channel.BasicQosAsync(0, 10, false, token);
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var type = args.BasicProperties.Type ?? args.RoutingKey;
                var id = Guid.TryParse(args.BasicProperties.MessageId, out var parsed) ? parsed : Guid.NewGuid();
                using var scope = scopes.CreateScope(); var handlers = scope.ServiceProvider.GetRequiredService<IntegrationEventHandlers>();
                var body = args.Body.ToArray();
                switch (type)
                {
                    case "device.edge-gateway-registered.v1": await handlers.HandleAsync(id, JsonSerializer.Deserialize<EdgeGatewayRegisteredIntegrationEvent>(body, JsonOptions)!, token); break;
                    case "device.iot-device-registered.v1": await handlers.HandleAsync(id, JsonSerializer.Deserialize<IotDeviceRegisteredIntegrationEvent>(body, JsonOptions)!, token); break;
                    case "crop.sector-assigned.v1": await handlers.HandleAsync(id, JsonSerializer.Deserialize<SectorAssignedIntegrationEvent>(body, JsonOptions)!, token); break;
                    case "crop.sector-removed.v1": await handlers.HandleAsync(id, JsonSerializer.Deserialize<SectorRemovedIntegrationEvent>(body, JsonOptions)!, token); break;
                    case "smartpalm.identity.subscription-activated.v1": await handlers.HandleAsync(id, JsonSerializer.Deserialize<SubscriptionActivatedIntegrationEvent>(body, JsonOptions)!, token); break;
                    case "smartpalm.identity.subscription-cancelled.v1": await handlers.HandleAsync(id, JsonSerializer.Deserialize<SubscriptionCancelledIntegrationEvent>(body, JsonOptions)!, token); break;
                    case "smartpalm.identity.user-deletion-requested.v1": await scope.ServiceProvider
                        .GetRequiredService<UserDeletionEventHandler>()
                        .HandleAsync(ReadPayload<UserDeletionRequestedIntegrationEvent>(body), token); break;
                }
                await channel.BasicAckAsync(args.DeliveryTag, false, token);
            }
            catch (Exception ex) { logger.LogError(ex, "Integration event {RoutingKey} failed.", args.RoutingKey); await channel.BasicNackAsync(args.DeliveryTag, false, true, token); }
        };
        await channel.BasicConsumeAsync(_options.Queue, false, consumer, token);
        await Task.Delay(Timeout.InfiniteTimeSpan, token);
    }

    private static T ReadPayload<T>(byte[] body)
    {
        using var document = JsonDocument.Parse(body);
        var payload = document.RootElement.TryGetProperty("payload", out var value)
            ? value
            : document.RootElement;
        return payload.Deserialize<T>(JsonOptions)
            ?? throw new JsonException($"Invalid {typeof(T).Name} payload.");
    }
}
