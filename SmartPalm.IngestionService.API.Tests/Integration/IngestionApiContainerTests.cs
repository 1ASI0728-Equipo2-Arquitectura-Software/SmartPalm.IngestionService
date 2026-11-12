using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using RabbitMQ.Client;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace SmartPalm.IngestionService.API.Tests.Integration;

public class IngestionApiContainerTests : IAsyncLifetime
{
    private const string JwtKey = "ingestion-tests-signing-key-with-at-least-32-characters";
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").WithDatabase("smartpalm_ingestion").WithUsername("smartpalm_ingestion").WithPassword("tests").Build();
    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder("rabbitmq:4-management-alpine").Build();
    public async Task InitializeAsync() => await Task.WhenAll(_postgres.StartAsync(), _rabbit.StartAsync());
    public async Task DisposeAsync() => await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _rabbit.DisposeAsync().AsTask());

    [DockerRequiredFact, Trait("Category", "Integration")]
    public async Task DeviceEventThenRepeatedBatch_SeedsThresholdsAndIngestsExactlyOnce()
    {
        using var factory = CreateFactory(); using var client = factory.CreateClient();
        var messageId = Guid.NewGuid();
        await using var connection = await new ConnectionFactory { Uri = new Uri(_rabbit.GetConnectionString()) }.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync("smartpalm.events", ExchangeType.Topic, true, false);

        var seeded = false;
        for (var attempt = 0; attempt < 10 && !seeded; attempt++)
        {
            var properties = new BasicProperties { Persistent = true, MessageId = messageId.ToString(), Type = "device.iot-device-registered.v1", ContentType = "application/json" };
            var payload = JsonSerializer.SerializeToUtf8Bytes(new { edgeDeviceMacAddress = "AA:BB:CC:DD:EE:01", iotDeviceMacAddress = "SMART-PALM-001", plantationId = 1, ownerUserId = 42, sectorId = 7, createdAt = DateTime.UtcNow }, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            await channel.BasicPublishAsync("smartpalm.events", "device.iot-device-registered.v1", false, properties, payload);
            await channel.BasicPublishAsync("smartpalm.events", "device.edge-gateway-registered.v1", false,
                new BasicProperties { Persistent = true, MessageId = Guid.NewGuid().ToString(), Type = "device.edge-gateway-registered.v1", ContentType = "application/json" },
                JsonSerializer.SerializeToUtf8Bytes(new { edgeMac = "AA:BB:CC:DD:EE:01", monitoringZoneId = 1, ownerUserId = 42, credentialHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("gateway-test-key"))), createdAt = DateTime.UtcNow }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
            await channel.BasicPublishAsync("smartpalm.events", "crop.sector-assigned.v1", false,
                new BasicProperties { Persistent = true, MessageId = Guid.NewGuid().ToString(), Type = "crop.sector-assigned.v1", ContentType = "application/json" },
                JsonSerializer.SerializeToUtf8Bytes(new { sectorId = 7, plantationId = 1, iotDeviceMacAddress = "SMART-PALM-001", sectorName = "North", occurredAt = DateTime.UtcNow }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
            await channel.BasicPublishAsync("smartpalm.events", "smartpalm.identity.subscription-activated.v1", false,
                new BasicProperties { Persistent = true, MessageId = Guid.NewGuid().ToString(), Type = "smartpalm.identity.subscription-activated.v1", ContentType = "application/json" },
                JsonSerializer.SerializeToUtf8Bytes(new { userId = 42, activatedAt = DateTime.UtcNow }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
            await Task.Delay(700);
            seeded = await ScalarAsync("SELECT COUNT(*) FROM ingestion.agronomic_thresholds") == 5;
        }
        Assert.True(seeded, "Device integration event did not seed thresholds.");
        var authorized = false;
        for (var attempt = 0; attempt < 10 && !authorized; attempt++)
        {
            authorized = await ScalarAsync("SELECT COUNT(*) FROM ingestion.authorized_device_projections WHERE owner_user_id = 42") == 1
                && await ScalarAsync("SELECT COUNT(*) FROM ingestion.gateway_authorization_projections WHERE owner_user_id = 42 AND is_active") == 1
                && await ScalarAsync("SELECT COUNT(*) FROM ingestion.account_plan_projections WHERE owner_user_id = 42 AND active") == 1;
            if (!authorized) await Task.Delay(700);
        }
        Assert.True(authorized, "Ownership projections were not synchronized.");
        Assert.True(await ScalarAsync("SELECT COUNT(*) FROM ingestion.inbox_messages") >= 4);

        var syncedAt = DateTime.UtcNow;
        var body = new { devices = new[] { new { deviceMac = "smart-palm-001", readings = new[] { new { sensorType = "Temperature", measuredAt = syncedAt, value = 45.0 } } } }, syncedAt };
        client.DefaultRequestHeaders.Add("X-Gateway-Key", "gateway-test-key");
        var first = await client.PostAsJsonAsync("/api/v1/edge-gateways/aa:bb:cc:dd:ee:01/sensor-readings", body);
        var second = await client.PostAsJsonAsync("/api/v1/edge-gateways/aa:bb:cc:dd:ee:01/sensor-readings", body);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode); Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        var duplicate = await second.Content.ReadFromJsonAsync<IngestionResponse>(); Assert.True(duplicate?.Duplicate);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM ingestion.sensor_readings"));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM ingestion.sensor_readings WHERE owner_user_id = 42 AND plantation_id = 1 AND sector_id = 7"));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM ingestion.ingestion_batches"));

        var published = false;
        for (var attempt = 0; attempt < 10 && !published; attempt++)
        {
            published = await ScalarAsync("SELECT COUNT(*) FROM ingestion.outbox_messages WHERE processed_on_utc IS NOT NULL") == 2;
            if (!published) await Task.Delay(700);
        }
        Assert.True(published, "Ingested and threshold-exceeded events were not published.");
    }

    [DockerRequiredFact, Trait("Category", "Integration")]
    public async Task ProtectedHistory_WithoutJwt_ReturnsUnauthorized()
    {
        using var factory = CreateFactory(); using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/devices/SMART-PALM-001/sensor-readings")).StatusCode);
    }

    private WebApplicationFactory<Program> CreateFactory() => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
    {
        builder.UseEnvironment("Testing"); builder.UseSetting("ConnectionStrings:IngestionDatabase", _postgres.GetConnectionString());
        builder.UseSetting("Jwt:Issuer", "smartpalm.identity.tests"); builder.UseSetting("Jwt:Audience", "smartpalm.clients.tests"); builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("RabbitMq:Enabled", "true"); builder.UseSetting("RabbitMq:Uri", _rabbit.GetConnectionString()); builder.UseSetting("RabbitMq:Queue", "smartpalm.ingestion.tests");
    });
    private async Task<long> ScalarAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString()); await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection); return Convert.ToInt64(await command.ExecuteScalarAsync());
    }
}
