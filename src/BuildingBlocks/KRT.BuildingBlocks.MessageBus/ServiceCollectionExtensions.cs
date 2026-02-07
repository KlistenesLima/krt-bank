using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KRT.BuildingBlocks.MessageBus;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra RabbitMQ (publisher).
    /// Usar nos serviços que publicam notificações (Onboarding, Payments).
    /// </summary>
    public static IServiceCollection AddRabbitMqPublisher(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<IMessageBus, RabbitMqBus>();
        return services;
    }

    /// <summary>
    /// Registra RabbitMQ (publisher + consumer).
    /// Usar no serviço que processa notificações.
    /// </summary>
    public static IServiceCollection AddRabbitMqNotificationWorker(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRabbitMqPublisher(configuration);
        services.AddHostedService<NotificationWorker>();
        return services;
    }
}
