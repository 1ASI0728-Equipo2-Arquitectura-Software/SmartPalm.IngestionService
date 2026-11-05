using SmartPalm.IngestionService.Application.Abstractions;
using SmartPalm.IngestionService.Domain.Events;
using SmartPalm.IngestionService.Domain.Readings;
using SmartPalm.IngestionService.Domain.Repositories;
using SmartPalm.IngestionService.Domain.Services;

namespace SmartPalm.IngestionService.Application.EventHandlers;

public class IntegrationEventHandlers(
    IThresholdRepository thresholds,
    ISectorProjectionRepository sectors,
    IGatewayAuthorizationProjectionRepository gateways,
    IAuthorizedDeviceProjectionRepository devices,
    IAccountPlanProjectionRepository plans,
    IInbox inbox,
    IUnitOfWork unitOfWork)
{
    public Task HandleAsync(Guid messageId, EdgeGatewayRegisteredIntegrationEvent message, CancellationToken token) =>
        ProcessOnceAsync(messageId, "device.edge-gateway-registered.v1", async ct =>
        {
            if (message.OwnerUserId <= 0 || string.IsNullOrWhiteSpace(message.CredentialHash)) return;
            var projection = await gateways.FindAsync(message.EdgeMac, ct);
            if (projection is null)
            {
                projection = new GatewayAuthorizationProjection(
                    message.EdgeMac, message.OwnerUserId, message.CredentialHash);
                await gateways.AddAsync(projection, ct);
            }
            else projection.UpdateRegistration(message.OwnerUserId, message.CredentialHash);
            var activePlan = await plans.FindAsync(message.OwnerUserId, ct);
            projection.SetPlanActive(activePlan?.Active == true);
        }, token);

    public Task HandleAsync(Guid messageId, IotDeviceRegisteredIntegrationEvent message, CancellationToken token) =>
        ProcessOnceAsync(messageId, "device.iot-device-registered.v1", async ct =>
        {
            if ((await thresholds.ListByDeviceAsync(message.IotDeviceMacAddress, ct)).Count == 0)
                await thresholds.AddRangeAsync(ThresholdPolicy.CreateDefaults(
                    message.EdgeDeviceMacAddress, message.IotDeviceMacAddress), ct);
            if (message.OwnerUserId <= 0 || message.SectorId <= 0) return;
            var projection = await devices.FindAsync(message.IotDeviceMacAddress, ct);
            if (projection is null)
                await devices.AddAsync(new AuthorizedDeviceProjection(
                    message.IotDeviceMacAddress,
                    message.EdgeDeviceMacAddress,
                    message.OwnerUserId,
                    message.PlantationId,
                    message.SectorId), ct);
            else projection.UpdateRegistration(
                message.EdgeDeviceMacAddress,
                message.OwnerUserId,
                message.PlantationId,
                message.SectorId);
        }, token);

    public Task HandleAsync(Guid messageId, SubscriptionActivatedIntegrationEvent message, CancellationToken token) =>
        ProcessOnceAsync(messageId, "smartpalm.identity.subscription-activated.v1",
            async ct =>
            {
                var plan = await plans.FindAsync(message.UserId, ct);
                if (plan is null) await plans.AddAsync(new AccountPlanProjection(message.UserId, true), ct);
                else plan.SetActive(true);
                await gateways.SetPlanActiveForOwnerAsync(message.UserId, true, ct);
            }, token);

    public Task HandleAsync(Guid messageId, SubscriptionCancelledIntegrationEvent message, CancellationToken token) =>
        ProcessOnceAsync(messageId, "smartpalm.identity.subscription-cancelled.v1",
            async ct =>
            {
                var plan = await plans.FindAsync(message.UserId, ct);
                if (plan is null) await plans.AddAsync(new AccountPlanProjection(message.UserId, false), ct);
                else plan.SetActive(false);
                await gateways.SetPlanActiveForOwnerAsync(message.UserId, false, ct);
            }, token);

    public Task HandleAsync(Guid messageId, SectorAssignedIntegrationEvent message, CancellationToken token) =>
        ProcessOnceAsync(messageId, "crop.sector-assigned.v1", async ct =>
        {
            if (message.IotDeviceMacAddress is null) return;
            var projection = await sectors.FindAsync(message.SectorId, ct);
            if (projection is null) await sectors.AddAsync(new SectorDeviceProjection(message.SectorId, message.IotDeviceMacAddress), ct);
            else projection.Assign(message.IotDeviceMacAddress);
            var device = await devices.FindAsync(message.IotDeviceMacAddress, ct);
            device?.AssignSector(message.SectorId);
        }, token);

    public Task HandleAsync(Guid messageId, SectorRemovedIntegrationEvent message, CancellationToken token) =>
        ProcessOnceAsync(messageId, "crop.sector-removed.v1", async ct =>
        {
            var projection = await sectors.FindAsync(message.SectorId, ct);
            if (projection is not null) sectors.Remove(projection);
        }, token);

    private async Task ProcessOnceAsync(Guid messageId, string eventType, Func<CancellationToken, Task> action, CancellationToken token)
    {
        if (await inbox.HasProcessedAsync(messageId, token)) return;
        await unitOfWork.ExecuteAsync(async transactionToken =>
        {
            if (await inbox.HasProcessedAsync(messageId, transactionToken)) return true;
            await action(transactionToken);
            inbox.MarkProcessed(messageId, eventType);
            await unitOfWork.SaveChangesAsync(transactionToken);
            return true;
        }, token);
    }
}
