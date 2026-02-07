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
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

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

        // HTTP Client (Payments -> Onboarding) com Polly
        var onboardingUrl = configuration["Services:OnboardingUrl"] ?? "http://localhost:5001/";

        // Polly: Circuit Breaker deve ser SINGLETON (estado compartilhado entre requests)
        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));

        services.AddHttpClient<IOnboardingServiceClient, OnboardingServiceClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(onboardingUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            // RETRY: 3 tentativas com backoff exponencial (1s, 2s, 4s)
            .AddPolicyHandler((sp, request) =>
            {
                var logger = sp.GetService<ILoggerFactory>()?.CreateLogger("Polly.Retry");
                return HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    .WaitAndRetryAsync(3,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)),
                        onRetry: (outcome, timespan, retryCount, ctx) =>
                        {
                            logger?.LogWarning(
                                "Retry {RetryCount} for {Url} after {Delay}s. Status: {Status}",
                                retryCount, request.RequestUri, timespan.TotalSeconds,
                                outcome.Result?.StatusCode.ToString() ?? outcome.Exception?.Message);
                        });
            })
            // CIRCUIT BREAKER: Abre após 5 falhas consecutivas, fica aberto 30s
            .AddPolicyHandler(circuitBreakerPolicy);

        // Kafka EventBus
        services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));
        services.AddSingleton<IEventBus, KafkaEventBus>();

        // Outbox Processor
        services.Configure<OutboxSettings>(configuration.GetSection("Outbox"));
        services.AddHostedService<OutboxProcessor<PaymentsDbContext>>();

        return services;
    }
}
