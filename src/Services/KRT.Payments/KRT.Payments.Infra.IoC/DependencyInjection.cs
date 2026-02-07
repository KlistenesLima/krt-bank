using KRT.Payments.Application.Interfaces;
using KRT.Payments.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using KRT.Payments.Domain.Interfaces;
using KRT.Payments.Domain.Services;
using KRT.Payments.Infra.Data.Repositories;
using KRT.Payments.Infra.Data.Context;
using KRT.Payments.Infra.Http;

using KRT.BuildingBlocks.Domain;
using KRT.BuildingBlocks.EventBus;
using KRT.BuildingBlocks.EventBus.Kafka;
using KRT.BuildingBlocks.Infrastructure.Outbox;
using KRT.BuildingBlocks.MessageBus;
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

        // Fraud Analysis Engine
        services.AddScoped<IFraudAnalysisEngine, FraudAnalysisEngine>();

        // Fraud Analysis Worker (async background processing)
        services.AddHostedService<FraudAnalysisWorker>();

        // HTTP Client (Payments -> Onboarding) com Polly
        var onboardingUrl = configuration["Services:OnboardingUrl"] ?? "http://localhost:5001/";

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
            .AddPolicyHandler(circuitBreakerPolicy);

        // Kafka EventBus (eventos)
        services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));
        services.AddSingleton<IEventBus, KafkaEventBus>();

        // RabbitMQ (notificações)
        services.AddRabbitMqPublisher(configuration);

        // Outbox Processor
        services.Configure<OutboxSettings>(configuration.GetSection("Outbox"));
        services.AddHostedService<OutboxProcessor<PaymentsDbContext>>();

        return services;
    }
}



