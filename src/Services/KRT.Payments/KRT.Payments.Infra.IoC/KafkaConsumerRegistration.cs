using KRT.BuildingBlocks.EventBus.Kafka;
using KRT.Payments.Application.Consumers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KRT.Payments.Infra.IoC;

public static class KafkaConsumerRegistration
{
    /// <summary>
    /// Registra todos os Kafka consumers do servico de Payments.
    /// 
    /// CONSUMERS:
    ///   FraudApprovedConsumer          -> krt.fraud.analysis-approved (Saga)
    ///   FraudRejectedConsumer          -> krt.fraud.analysis-rejected (Notificacoes)
    ///   PixTransferCompletedConsumer   -> krt.pix.transfer-completed (Bridge Kafka->RabbitMQ)
    ///   PixAuditLogConsumer            -> krt.pix.transfer-completed (Audit - group separado!)
    ///   PixFailedAuditLogConsumer      -> krt.pix.transfer-failed (Audit falhas)
    /// 
    /// NOTA: PixTransferCompletedConsumer e PixAuditLogConsumer leem do MESMO topico
    /// mas com consumer groups diferentes. SO possivel com Kafka.
    /// </summary>
    public static IServiceCollection AddKafkaConsumers(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));

        // Consumers de negocio
        services.AddHostedService<FraudApprovedConsumer>();
        services.AddHostedService<FraudRejectedConsumer>();
        services.AddHostedService<PixTransferCompletedConsumer>();

        // Consumers de auditoria (consumer group separado)
        services.AddHostedService<PixAuditLogConsumer>();
        services.AddHostedService<PixFailedAuditLogConsumer>();

        return services;
    }
}
