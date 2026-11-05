using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SmartPalm.IngestionService.Infrastructure.Configuration;
using SmartPalm.IngestionService.Infrastructure.Persistence;

namespace SmartPalm.IngestionService.Infrastructure.Messaging;

internal class OutboxPublisher(IServiceScopeFactory scopes, IOptions<RabbitMqOptions> options, ILogger<OutboxPublisher> logger) : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        if (!_options.Enabled) { logger.LogInformation("RabbitMQ messaging is disabled."); return; }
        while (!token.IsCancellationRequested)
        {
            try { await PublishAsync(token); } catch (Exception ex) { logger.LogWarning(ex, "Outbox dispatch failed; retrying."); }
            await Task.Delay(TimeSpan.FromSeconds(3), token);
        }
    }
    private async Task PublishAsync(CancellationToken token)
    {
        using var scope = scopes.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<IngestionDbContext>();
        var messages = await db.OutboxMessages.Where(x => x.ProcessedOnUtc == null && x.RetryCount < 10).OrderBy(x => x.OccurredOnUtc).Take(100).ToArrayAsync(token);
        if (messages.Length == 0) return;
        var factory = new ConnectionFactory { Uri = new Uri(_options.Uri), AutomaticRecoveryEnabled = true, RequestedHeartbeat = TimeSpan.FromSeconds(30) };
        await using var connection = await factory.CreateConnectionAsync(token);
        await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true), token);
        await channel.ExchangeDeclareAsync(_options.Exchange, ExchangeType.Topic, true, false, cancellationToken: token);
        foreach (var message in messages)
        {
            try
            {
                var properties = new BasicProperties { Persistent = true, MessageId = message.Id.ToString(), Type = message.EventType, ContentType = "application/json" };
                await channel.BasicPublishAsync(_options.Exchange, message.EventType, false, properties, Encoding.UTF8.GetBytes(message.Content), token);
                message.ProcessedOnUtc = DateTime.UtcNow; message.Error = null;
            }
            catch (Exception ex) { message.RetryCount++; message.Error = ex.Message[..Math.Min(ex.Message.Length, 2000)]; }
        }
        await db.SaveChangesAsync(token);
    }
}
