using SmartPalm.IngestionService.Application.Contracts;
using SmartPalm.IngestionService.Application.Exceptions;
using SmartPalm.IngestionService.Domain.Model.Queries;
using SmartPalm.IngestionService.Domain.Readings;
using SmartPalm.IngestionService.Domain.Repositories;
using SmartPalm.IngestionService.Domain.Services;

namespace SmartPalm.IngestionService.Application.Services;

public class IngestionQueryService(
    ISensorReadingRepository readings,
    IThresholdRepository thresholds,
    ISectorProjectionRepository sectors,
    IAuthorizedDeviceProjectionRepository devices,
    IGatewayAuthorizationProjectionRepository gateways,
    IAccountPlanProjectionRepository plans)
{
    public async Task<IReadOnlyList<SensorReadingResponse>> HandleAsync(GetGatewayReadingsQuery query, CancellationToken token)
    {
        await RequirePlanAsync(query.OwnerUserId, token);
        var gateway = await gateways.FindAsync(query.GatewayMac, token);
        if (gateway is null || gateway.OwnerUserId != query.OwnerUserId) throw new ForbiddenAccessException("Gateway is not owned by the authenticated user.");
        if (!gateway.IsActive) throw new ForbiddenAccessException("Gateway is inactive.");
        return (await readings.ListByGatewayAsync(query.OwnerUserId, query.GatewayMac, query.From, query.To, query.DeviceMac, query.Page, query.Size, token)).Select(Map).ToArray();
    }

    public async Task<IReadOnlyList<SensorReadingResponse>> HandleAsync(GetDeviceReadingsQuery query, CancellationToken token)
    {
        await RequirePlanAsync(query.OwnerUserId, token);
        var device = await RequireDeviceAsync(query.DeviceMac, query.OwnerUserId, token);
        return (await readings.ListByDeviceAsync(query.OwnerUserId, device.DeviceMac, query.From, query.To, query.Page, query.Size, token)).Select(Map).ToArray();
    }

    public async Task<IReadOnlyList<SensorReadingResponse>> HandleAsync(GetSectorReadingsQuery query, CancellationToken token)
    {
        await RequirePlanAsync(query.OwnerUserId, token);
        var projection = await sectors.FindAsync(query.SectorId, token)
            ?? throw new KeyNotFoundException($"Sector '{query.SectorId}' not found or has no IoT device assigned.");
        await RequireDeviceAsync(projection.IotDeviceMacAddress, query.OwnerUserId, token);
        return (await readings.ListByDeviceAsync(query.OwnerUserId, projection.IotDeviceMacAddress, query.From, query.To, 1, 10000, token)).Select(Map).ToArray();
    }

    public async Task<IReadOnlyList<ThresholdResponse>> HandleAsync(GetDeviceThresholdsQuery query, CancellationToken token)
    {
        await RequirePlanAsync(query.OwnerUserId, token);
        var device = await RequireDeviceAsync(query.DeviceMac, query.OwnerUserId, token);
        var values = await thresholds.ListByDeviceAsync(device.DeviceMac, token);
        if (values.Count == 0) throw new KeyNotFoundException($"IoT device '{query.DeviceMac}' not found.");
        return values.Select(value => new ThresholdResponse(value.GatewayMacAddress, value.IotDeviceMacAddress,
            value.Min, value.Max, value.Description, value.Type.ToString())).ToArray();
    }

    public async Task<IReadOnlyList<ThresholdResponse>> HandleAdminAsync(GetDeviceThresholdsQuery query, CancellationToken token)
    {
        var values = await thresholds.ListByDeviceAsync(query.DeviceMac, token);
        if (values.Count == 0) throw new KeyNotFoundException($"IoT device '{query.DeviceMac}' not found.");
        return values.Select(value => new ThresholdResponse(value.GatewayMacAddress, value.IotDeviceMacAddress,
            value.Min, value.Max, value.Description, value.Type.ToString())).ToArray();
    }

    public async Task<SectorHealthResponse?> HandleAsync(GetSectorHealthQuery query, CancellationToken token)
    {
        await RequirePlanAsync(query.OwnerUserId, token);
        var projection = await sectors.FindAsync(query.SectorId, token);
        if (projection is null) return null;
        await RequireDeviceAsync(projection.IotDeviceMacAddress, query.OwnerUserId, token);
        var latest = await readings.LatestByDeviceAsync(query.OwnerUserId, projection.IotDeviceMacAddress, token);
        var configured = await thresholds.ListByDeviceAsync(projection.IotDeviceMacAddress, token);
        var lookup = configured.ToDictionary(value => value.Type);
        var critical = false;
        var warning = false;
        var details = latest.Select(reading =>
        {
            lookup.TryGetValue(reading.Type, out var threshold);
            var exceeded = threshold?.IsExceededBy(reading.Value) == true;
            if (exceeded && ThresholdPolicy.Deviation(reading.Value, threshold!.Min, threshold.Max) > 0.2) critical = true;
            else if (exceeded) warning = true;
            return new SensorHealthDetail(reading.Type.ToString(), reading.Value, threshold?.Min, threshold?.Max, exceeded);
        }).ToArray();
        var status = critical ? SectorHealthStatus.Critical : warning ? SectorHealthStatus.Warning : SectorHealthStatus.Healthy;
        return new SectorHealthResponse(query.SectorId, status.ToString(), details);
    }

    private static SensorReadingResponse Map(SensorReading value) => new(
        value.GatewayMacAddress,
        value.IotDeviceMacAddress,
        value.Type.ToString(),
        value.Value,
        value.Unit.ToString(),
        value.MeasuredAt,
        value.OwnerUserId,
        value.PlantationId,
        value.SectorId);

    private async Task RequirePlanAsync(long ownerUserId, CancellationToken token)
    {
        if (ownerUserId <= 0 || (await plans.FindAsync(ownerUserId, token))?.Active != true)
            throw new ForbiddenAccessException("The authenticated owner account or plan is inactive.");
    }

    private async Task<AuthorizedDeviceProjection> RequireDeviceAsync(string deviceMac, long ownerUserId, CancellationToken token)
    {
        var device = await devices.FindAsync(deviceMac, token);
        if (device is null) throw new KeyNotFoundException($"IoT device '{deviceMac}' not found.");
        if (!device.IsActive || device.OwnerUserId != ownerUserId || device.PlantationId <= 0 || device.SectorId <= 0)
            throw new ForbiddenAccessException("IoT device is not authorized for the authenticated owner.");
        return device;
    }
}
