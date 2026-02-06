using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using KRT.Payments.Domain.Interfaces;
using KRT.Payments.Infra.Data.Repositories;
using KRT.Payments.Infra.Data.Context;
using KRT.Payments.Infra.Http;
using KRT.Payments.Application.Interfaces;
using KRT.BuildingBlocks.Domain;
using KRT.BuildingBlocks.EventBus;
using KRT.BuildingBlocks.EventBus.Kafka;
using KRT.BuildingBlocks.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Infra.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PaymentsDbContext>());

        // Repositories
        services.AddScoped<IPixTransactionRepository, PixTransactionRepository>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();

        // HTTP Client (Payments -> Onboarding)
        var onboardingUrl = configuration["Services:OnboardingUrl"] ?? "http://localhost:5001/";
        services.AddHttpClient<IOnboardingServiceClient, OnboardingServiceClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(onboardingUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
            });

        // Kafka EventBus
        services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));
        services.AddSingleton<IEventBus, KafkaEventBus>();

        // Outbox Processor
        services.Configure<OutboxSettings>(configuration.GetSection("Outbox"));
        services.AddHostedService<OutboxProcessor<PaymentsDbContext>>();

        return services;
    }
}
