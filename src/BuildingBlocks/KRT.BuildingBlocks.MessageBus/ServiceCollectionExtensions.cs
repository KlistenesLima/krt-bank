using Amazon.S3;
using KRT.BuildingBlocks.MessageBus.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KRT.BuildingBlocks.MessageBus;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra RabbitMQ (publisher apenas).
    /// </summary>
    public static IServiceCollection AddRabbitMqPublisher(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<IMessageBus, RabbitMqBus>();
        return services;
    }

    /// <summary>
    /// Registra RabbitMQ (publisher + notification consumer).
    /// </summary>
    public static IServiceCollection AddRabbitMqNotificationWorker(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRabbitMqPublisher(configuration);
        services.AddHostedService<NotificationWorker>();
        return services;
    }

    /// <summary>
    /// Registra RabbitMQ (publisher + receipt worker + Backblaze B2).
    /// </summary>
    public static IServiceCollection AddRabbitMqReceiptWorker(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRabbitMqPublisher(configuration);
        services.AddBackblazeB2(configuration);
        services.AddHostedService<ReceiptWorker>();
        return services;
    }

    /// <summary>
    /// Registra RabbitMQ completo (publisher + todos os workers + Backblaze B2).
    /// No KRT Bank: o Payments API usa este método.
    /// </summary>
    public static IServiceCollection AddRabbitMqFullWorkers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRabbitMqPublisher(configuration);
        services.AddBackblazeB2(configuration);
        services.AddHostedService<NotificationWorker>();
        services.AddHostedService<ReceiptWorker>();
        return services;
    }

    /// <summary>
    /// Registra Backblaze B2 via protocolo S3-compatible.
    /// 
    /// Usa AWSSDK.S3 com endpoint customizado apontando para B2.
    /// Mesma interface usada por AWS S3, MinIO, DigitalOcean Spaces.
    /// Para trocar de provider, basta mudar endpoint e credenciais.
    /// </summary>
    private static IServiceCollection AddBackblazeB2(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BackblazeB2Settings>(configuration.GetSection("BackblazeB2"));

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<BackblazeB2Settings>>().Value;

            var config = new AmazonS3Config
            {
                ServiceURL = settings.Endpoint,
                ForcePathStyle = true // Obrigatório para Backblaze B2
            };

            return new AmazonS3Client(settings.KeyId, settings.ApplicationKey, config);
        });

        services.AddSingleton<ICloudStorage, BackblazeB2Storage>();

        return services;
    }
}
