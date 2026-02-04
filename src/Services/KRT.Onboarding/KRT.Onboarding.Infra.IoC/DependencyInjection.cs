using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Alias para resolver ambiguidade
using IUnitOfWork = KRT.BuildingBlocks.Domain.IUnitOfWork;

// Building Blocks
using KRT.BuildingBlocks.EventBus;
using KRT.BuildingBlocks.EventBus.Kafka;
using KRT.BuildingBlocks.Infrastructure.Data; // Implementações
using KRT.BuildingBlocks.Infrastructure.Outbox;

// Application
using KRT.Onboarding.Application.Accounts.Services;
using KRT.Onboarding.Application.Mappings;
using KRT.Onboarding.Application.Validations;

// Domain
using KRT.Onboarding.Domain.Interfaces;

// Infra
using KRT.Onboarding.Infra.Cache.Redis;
using KRT.Onboarding.Infra.Data.Context;
using KRT.Onboarding.Infra.Data.Repositories;
using KRT.Onboarding.Infra.MessageQueue.Handlers;

namespace KRT.Onboarding.Infra.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddCaching(configuration);
        services.AddMessaging(configuration);
        services.AddRepositories();
        services.AddApplicationServices();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<AccountService>();
            cfg.RegisterServicesFromAssemblyContaining<AccountDomainEventHandler>();
        });

        services.AddAutoMapper(typeof(MappingProfile));
        services.AddValidatorsFromAssemblyContaining<CreateAccountValidator>();

        return services;
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(3);
            });
        });

        // Resolve IUnitOfWork usando o DbContext (que implementa a interface no nosso design)
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RedisSettings>(configuration.GetSection("Redis"));
        services.AddSingleton<ICacheService, RedisCacheService>();
        return services;
    }

    private static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));
        services.AddSingleton<IEventBus, KafkaEventBus>();

        services.Configure<OutboxSettings>(configuration.GetSection("Outbox"));
        services.AddHostedService<OutboxProcessor<ApplicationDbContext>>();
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
        return services;
    }
}
