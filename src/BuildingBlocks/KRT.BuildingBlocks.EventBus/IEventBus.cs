namespace KRT.BuildingBlocks.EventBus;

/// <summary>
/// Interface para publicação de eventos de integração
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publica um evento no tópico padrão baseado no tipo
    /// </summary>
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IntegrationEvent;

    /// <summary>
    /// Publica um evento em um tópico específico
    /// </summary>
    Task PublishAsync<T>(T @event, string topic, CancellationToken ct = default) where T : IntegrationEvent;

    /// <summary>
    /// Publica múltiplos eventos em batch
    /// </summary>
    Task PublishBatchAsync<T>(IEnumerable<T> events, CancellationToken ct = default) where T : IntegrationEvent;
}

/// <summary>
/// Interface para handlers de eventos
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : IntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}

/// <summary>
/// Attribute para definir o nome do tópico
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TopicAttribute : Attribute
{
    public string Name { get; }
    
    public TopicAttribute(string name)
    {
        Name = name;
    }
}
