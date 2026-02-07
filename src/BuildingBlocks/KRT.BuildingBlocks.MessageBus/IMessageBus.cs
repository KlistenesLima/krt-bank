namespace KRT.BuildingBlocks.MessageBus;

/// <summary>
/// Publica comandos/notificações via RabbitMQ.
/// Diferente do IEventBus (Kafka) que publica eventos.
/// Kafka = "o que aconteceu" (imutável, log)
/// RabbitMQ = "o que precisa ser feito" (comando, notificação)
/// </summary>
public interface IMessageBus : IDisposable
{
    /// <summary>
    /// Publica uma mensagem numa fila específica.
    /// </summary>
    void Publish<T>(T message, string queueName) where T : class;

    /// <summary>
    /// Publica uma mensagem com prioridade (0-9, maior = mais urgente).
    /// </summary>
    void Publish<T>(T message, string queueName, byte priority) where T : class;
}
