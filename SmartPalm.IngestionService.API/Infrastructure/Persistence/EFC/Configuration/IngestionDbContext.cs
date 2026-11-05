using Microsoft.EntityFrameworkCore;
using SmartPalm.IngestionService.Domain.Readings;
using SmartPalm.IngestionService.Domain.Repositories;
using SmartPalm.IngestionService.Infrastructure.Messaging;

namespace SmartPalm.IngestionService.Infrastructure.Persistence;

public class IngestionDbContext(DbContextOptions<IngestionDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<AgronomicThreshold> Thresholds => Set<AgronomicThreshold>();
    public DbSet<IngestionBatch> IngestionBatches => Set<IngestionBatch>();
    public DbSet<SectorDeviceProjection> SectorDeviceProjections => Set<SectorDeviceProjection>();
    public DbSet<GatewayAuthorizationProjection> GatewayAuthorizationProjections => Set<GatewayAuthorizationProjection>();
    public DbSet<AuthorizedDeviceProjection> AuthorizedDeviceProjections => Set<AuthorizedDeviceProjection>();
    public DbSet<AccountPlanProjection> AccountPlanProjections => Set<AccountPlanProjection>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ingestion");
        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.ToTable("sensor_readings"); entity.HasKey(x => x.Id); entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.GatewayMacAddress).HasMaxLength(100).IsRequired(); entity.Property(x => x.IotDeviceMacAddress).HasMaxLength(100).IsRequired();
            entity.Property(x => x.OwnerUserId).IsRequired(); entity.Property(x => x.PlantationId).IsRequired(); entity.Property(x => x.SectorId).IsRequired();
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(30); entity.Property(x => x.Unit).HasConversion<string>().HasMaxLength(30);
            entity.HasIndex(x => new { x.GatewayMacAddress, x.MeasuredAt }); entity.HasIndex(x => new { x.IotDeviceMacAddress, x.MeasuredAt });
            entity.HasOne<IngestionBatch>().WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<AgronomicThreshold>(entity =>
        {
            entity.ToTable("agronomic_thresholds"); entity.HasKey(x => x.Id); entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.GatewayMacAddress).HasMaxLength(100).IsRequired(); entity.Property(x => x.IotDeviceMacAddress).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(30); entity.Property(x => x.Description).HasMaxLength(250).IsRequired();
            entity.HasIndex(x => new { x.IotDeviceMacAddress, x.Type }).IsUnique(); entity.HasIndex(x => x.GatewayMacAddress);
        });
        modelBuilder.Entity<IngestionBatch>(entity =>
        {
            entity.ToTable("ingestion_batches"); entity.HasKey(x => x.Id); entity.Property(x => x.Fingerprint).HasMaxLength(64).IsRequired();
            entity.Property(x => x.GatewayMacAddress).HasMaxLength(100).IsRequired(); entity.HasIndex(x => x.Fingerprint).IsUnique();
        });
        modelBuilder.Entity<SectorDeviceProjection>(entity =>
        {
            entity.ToTable("sector_device_projections"); entity.HasKey(x => x.SectorId); entity.Property(x => x.IotDeviceMacAddress).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.IotDeviceMacAddress);
        });
        modelBuilder.Entity<GatewayAuthorizationProjection>(entity =>
        {
            entity.ToTable("gateway_authorization_projections");
            entity.HasKey(x => x.GatewayMac);
            entity.Property(x => x.GatewayMac).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CredentialHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.OwnerUserId);
        });
        modelBuilder.Entity<AuthorizedDeviceProjection>(entity =>
        {
            entity.ToTable("authorized_device_projections");
            entity.HasKey(x => x.DeviceMac);
            entity.Property(x => x.DeviceMac).HasMaxLength(100).IsRequired();
            entity.Property(x => x.GatewayMac).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.GatewayMac);
            entity.HasIndex(x => new { x.PlantationId, x.SectorId });
        });
        modelBuilder.Entity<AccountPlanProjection>(entity =>
        {
            entity.ToTable("account_plan_projections");
            entity.HasKey(x => x.OwnerUserId);
            entity.Property(x => x.Active).IsRequired();
        });
        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.ToTable("inbox_messages"); entity.HasKey(x => x.Id); entity.Property(x => x.EventType).HasMaxLength(200).IsRequired();
        });
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages"); entity.HasKey(x => x.Id); entity.Property(x => x.EventType).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Content).HasColumnType("jsonb").IsRequired(); entity.Property(x => x.Error).HasMaxLength(2000);
            entity.HasIndex(x => new { x.ProcessedOnUtc, x.OccurredOnUtc });
        });
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken)
    {
        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
        try { var result = await operation(cancellationToken); await transaction.CommitAsync(cancellationToken); return result; }
        catch { await transaction.RollbackAsync(cancellationToken); throw; }
    }
}
