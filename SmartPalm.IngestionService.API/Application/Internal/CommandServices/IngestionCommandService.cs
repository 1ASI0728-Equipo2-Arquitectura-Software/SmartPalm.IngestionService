using System.Security.Cryptography;
using System.Text;
using SmartPalm.IngestionService.Application.Abstractions;
using SmartPalm.IngestionService.Application.Contracts;
using SmartPalm.IngestionService.Domain.Model.Commands;
using SmartPalm.IngestionService.Domain.Events;
using SmartPalm.IngestionService.Domain.Readings;
using SmartPalm.IngestionService.Domain.Repositories;

namespace SmartPalm.IngestionService.Application.Services;

public class IngestionCommandService(
    ISensorReadingRepository readings,
    IThresholdRepository thresholds,
    IIngestionBatchRepository batches,
    IGatewayAuthorizationProjectionRepository gatewayAuthorizations,
    IAuthorizedDeviceProjectionRepository authorizedDevices,
    IUnitOfWork unitOfWork,
    IIntegrationEventWriter events)
{
    public async Task<IngestionResult> IngestAsync(IngestSensorReadingsCommand command, CancellationToken cancellationToken)
    {
        if (command.Readings.Count == 0) throw new ArgumentException("At least one sensor reading is required.");
        var gateway = SensorReading.NormalizeIdentifier(command.GatewayMac);
        var authorization = await gatewayAuthorizations.FindAsync(gateway, cancellationToken)
            ?? throw new UnauthorizedAccessException("Gateway is not provisioned for cloud ingestion.");
        if (!authorization.IsActive || !authorization.PlanActive)
            throw new UnauthorizedAccessException("Gateway owner does not have an active capture plan.");
        if (string.IsNullOrWhiteSpace(command.GatewayCredential))
            throw new UnauthorizedAccessException("Gateway credential is required.");
        var providedHash = Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(command.GatewayCredential.Trim())));
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(providedHash),
                Encoding.UTF8.GetBytes(authorization.CredentialHash)))
            throw new UnauthorizedAccessException("Invalid gateway credential.");

        var deviceAuthorizations = new Dictionary<string, AuthorizedDeviceProjection>(StringComparer.Ordinal);
        foreach (var item in command.Readings)
        {
            var deviceMac = SensorReading.NormalizeIdentifier(item.DeviceMac);
            var device = await authorizedDevices.FindAsync(deviceMac, cancellationToken)
                ?? throw new UnauthorizedAccessException($"IoT device '{deviceMac}' is not provisioned.");
            if (!device.IsActive || device.GatewayMac != gateway || device.OwnerUserId != authorization.OwnerUserId)
                throw new UnauthorizedAccessException($"IoT device '{deviceMac}' does not belong to this gateway account.");
            if (device.SectorId <= 0 || device.PlantationId <= 0)
                throw new InvalidOperationException($"IoT device '{deviceMac}' has no plantation sector assignment.");
            deviceAuthorizations[deviceMac] = device;
        }
        var fingerprint = Fingerprint(gateway, command);
        if (await batches.ExistsAsync(fingerprint, cancellationToken))
            return new IngestionResult(Guid.Empty, 0, Duplicate: true);

        return await unitOfWork.ExecuteAsync(async transactionToken =>
        {
            var batch = new IngestionBatch(fingerprint, gateway, command.SyncedAt, command.Readings.Count);
            var entities = command.Readings.Select(value => new SensorReading(
                batch.Id,
                gateway,
                value.DeviceMac,
                value.SensorType,
                value.MeasuredAt,
                value.Value,
                authorization.OwnerUserId,
                deviceAuthorizations[SensorReading.NormalizeIdentifier(value.DeviceMac)].PlantationId,
                deviceAuthorizations[SensorReading.NormalizeIdentifier(value.DeviceMac)].SectorId)).ToArray();
            await batches.AddAsync(batch, transactionToken);
            await readings.AddRangeAsync(entities, transactionToken);

            var configured = await thresholds.ListByGatewayAsync(gateway, transactionToken);
            var lookup = configured.ToDictionary(value => (value.IotDeviceMacAddress, value.Type));
            foreach (var reading in entities)
            {
                if (!lookup.TryGetValue((reading.IotDeviceMacAddress, reading.Type), out var threshold)) continue;
                if (!threshold.IsExceededBy(reading.Value)) continue;
                events.Enqueue("ingestion.threshold-exceeded.v1", new ThresholdExceededIntegrationEvent(
                    batch.Id, gateway, reading.IotDeviceMacAddress, reading.Type,
                    reading.Value, threshold.Min, threshold.Max, reading.MeasuredAt));
            }
            events.Enqueue("ingestion.sensor-readings-ingested.v1", new SensorReadingsIngestedIntegrationEvent(
                batch.Id, gateway, entities.Length, command.SyncedAt, DateTime.UtcNow));
            await unitOfWork.SaveChangesAsync(transactionToken);
            return new IngestionResult(batch.Id, entities.Length, Duplicate: false);
        }, cancellationToken);
    }

    public async Task UpdateThresholdAsync(UpdateAgronomicThresholdCommand command, CancellationToken cancellationToken)
    {
        var threshold = await thresholds.FindAsync(command.DeviceMac, command.Type, cancellationToken)
            ?? throw new KeyNotFoundException($"IoT device '{command.DeviceMac}' or threshold '{command.Type}' not found.");
        threshold.Update(command.Min, command.Max, command.Description);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string Fingerprint(string gateway, IngestSensorReadingsCommand command)
    {
        var canonical = new StringBuilder(gateway).Append('|').Append(command.SyncedAt.ToUniversalTime().Ticks);
        foreach (var item in command.Readings.OrderBy(x => x.DeviceMac).ThenBy(x => x.MeasuredAt).ThenBy(x => x.SensorType))
            canonical.Append('|').Append(item.DeviceMac.Trim().ToUpperInvariant()).Append(':').Append(item.SensorType)
                .Append(':').Append(item.MeasuredAt.ToUniversalTime().Ticks).Append(':').Append(item.Value.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString())));
    }
}
