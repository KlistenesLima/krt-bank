using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using KRT.Onboarding.Domain.Interfaces;
using KRT.Onboarding.Infra.Data.Repositories;
using KRT.Onboarding.Infra.Data.Context;
using KRT.Onboarding.Infra.Cache.Redis;
using KRT.BuildingBlocks.Domain;
using KRT.BuildingBlocks.EventBus;
using KRT.BuildingBlocks.EventBus.Kafka;
using KRT.BuildingBlocks.Infrastructure.Outbox;
using KRT.BuildingBlocks.MessageBus;
using Microsoft.EntityFrameworkCore;

namespace KRT.Onboarding.Infra.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddOnboardingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Repositories
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IPixKeyRepository, PixKeyRepository>();
        services.AddScoped<IAppUserRepository, AppUserRepository>();

        // Redis Cache
        services.Configure<RedisSettings>(configuration.GetSection("Redis"));
        services.AddSingleton<ICacheService, RedisCacheService>();

        // Kafka EventBus (eventos)
        services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));
        services.AddSingleton<IEventBus, KafkaEventBus>();

        // RabbitMQ (notificações) + NotificationWorker consumer
        services.AddRabbitMqNotificationWorker(configuration);

        // Outbox Processor
        services.Configure<OutboxSettings>(configuration.GetSection("Outbox"));
        services.AddHostedService<OutboxProcessor<ApplicationDbContext>>();

        return services;
    }
}

