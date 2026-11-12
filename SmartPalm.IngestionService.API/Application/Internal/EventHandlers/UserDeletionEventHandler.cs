using Microsoft.EntityFrameworkCore;
using SmartPalm.IngestionService.Application.Abstractions;
using SmartPalm.IngestionService.Domain.Events;
using SmartPalm.IngestionService.Infrastructure.Messaging;
using SmartPalm.IngestionService.Infrastructure.Persistence;

namespace SmartPalm.IngestionService.Application.EventHandlers;

public class UserDeletionEventHandler(IngestionDbContext db, IIntegrationEventWriter events)
{
    public async Task HandleAsync(UserDeletionRequestedIntegrationEvent message, CancellationToken token)
    {
        var eventType = "smartpalm.identity.user-deletion-requested.v1";
        if (await db.InboxMessages.AnyAsync(x => x.Id == message.OperationId, token)) return;

        var deviceMacs = await db.AuthorizedDeviceProjections
            .Where(x => x.OwnerUserId == message.UserId)
            .Select(x => x.DeviceMac)
            .ToArrayAsync(token);
        var gatewayMacs = await db.GatewayAuthorizationProjections
            .Where(x => x.OwnerUserId == message.UserId)
            .Select(x => x.GatewayMac)
            .ToArrayAsync(token);
        var plantationIds = await db.AuthorizedDeviceProjections
            .Where(x => x.OwnerUserId == message.UserId)
            .Select(x => x.PlantationId)
            .Distinct()
            .ToArrayAsync(token);
        var sectorIds = await db.AuthorizedDeviceProjections
            .Where(x => x.OwnerUserId == message.UserId)
            .Select(x => x.SectorId)
            .Distinct()
            .ToArrayAsync(token);
        var readingBatchIds = await db.SensorReadings
            .Where(x => x.OwnerUserId == message.UserId
                || deviceMacs.Contains(x.IotDeviceMacAddress)
                || gatewayMacs.Contains(x.GatewayMacAddress)
                || plantationIds.Contains(x.PlantationId)
                || sectorIds.Contains(x.SectorId))
            .Select(x => x.BatchId)
            .Distinct()
            .ToArrayAsync(token);

        var deleted = 0;
        deleted += await db.SensorReadings
            .Where(x => x.OwnerUserId == message.UserId
                || deviceMacs.Contains(x.IotDeviceMacAddress)
                || gatewayMacs.Contains(x.GatewayMacAddress)
                || plantationIds.Contains(x.PlantationId)
                || sectorIds.Contains(x.SectorId))
            .ExecuteDeleteAsync(token);
        deleted += await db.IngestionBatches
            .Where(x => gatewayMacs.Contains(x.GatewayMacAddress) || readingBatchIds.Contains(x.Id))
            .ExecuteDeleteAsync(token);
        deleted += await db.Thresholds
            .Where(x => deviceMacs.Contains(x.IotDeviceMacAddress) || gatewayMacs.Contains(x.GatewayMacAddress))
            .ExecuteDeleteAsync(token);
        deleted += await db.SectorDeviceProjections
            .Where(x => deviceMacs.Contains(x.IotDeviceMacAddress) || sectorIds.Contains(x.SectorId))
            .ExecuteDeleteAsync(token);
        deleted += await db.AuthorizedDeviceProjections
            .Where(x => x.OwnerUserId == message.UserId)
            .ExecuteDeleteAsync(token);
        deleted += await db.GatewayAuthorizationProjections
            .Where(x => x.OwnerUserId == message.UserId)
            .ExecuteDeleteAsync(token);
        deleted += await db.AccountPlanProjections
            .Where(x => x.OwnerUserId == message.UserId)
            .ExecuteDeleteAsync(token);

        db.InboxMessages.Add(new InboxMessage(message.OperationId, eventType));
        events.Enqueue("smartpalm.identity.user-deletion-service-completed.v1",
            new UserDeletionServiceCompletedIntegrationEvent(
                message.OperationId, message.UserId, "ingestion", deleted, DateTime.UtcNow));
        await db.SaveChangesAsync(token);
    }
}
