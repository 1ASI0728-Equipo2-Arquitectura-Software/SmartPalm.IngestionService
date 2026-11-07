using Microsoft.EntityFrameworkCore;
using SmartPalm.IngestionService.Application.Abstractions;
using SmartPalm.IngestionService.Application.EventHandlers;
using SmartPalm.IngestionService.Application.Services;
using SmartPalm.IngestionService.Domain.Repositories;
using SmartPalm.IngestionService.Infrastructure.Configuration;
using SmartPalm.IngestionService.Infrastructure.Messaging;
using SmartPalm.IngestionService.Infrastructure.Persistence;

namespace SmartPalm.IngestionService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIngestionInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("IngestionDatabase") ?? throw new InvalidOperationException("ConnectionStrings:IngestionDatabase is required.");
        services.AddDbContext<IngestionDbContext>(options => options.UseNpgsql(connection).UseSnakeCaseNamingConvention());
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddScoped<ISensorReadingRepository, SensorReadingRepository>(); services.AddScoped<IThresholdRepository, ThresholdRepository>();
        services.AddScoped<IIngestionBatchRepository, IngestionBatchRepository>(); services.AddScoped<ISectorProjectionRepository, SectorProjectionRepository>();
        services.AddScoped<IGatewayAuthorizationProjectionRepository, GatewayAuthorizationProjectionRepository>();
        services.AddScoped<IAuthorizedDeviceProjectionRepository, AuthorizedDeviceProjectionRepository>();
        services.AddScoped<IAccountPlanProjectionRepository, AccountPlanProjectionRepository>();
        services.AddScoped<IUnitOfWork>(p => p.GetRequiredService<IngestionDbContext>()); services.AddScoped<IIntegrationEventWriter, IntegrationEventWriter>(); services.AddScoped<IInbox, Inbox>();
        services.AddScoped<IngestionCommandService>(); services.AddScoped<IngestionQueryService>(); services.AddScoped<IntegrationEventHandlers>(); services.AddScoped<UserDeletionEventHandler>();
        services.AddHostedService<OutboxPublisher>(); services.AddHostedService<IntegrationEventConsumer>(); return services;
    }
}
