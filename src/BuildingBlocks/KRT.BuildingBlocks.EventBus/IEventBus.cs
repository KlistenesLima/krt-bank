namespace KRT.BuildingBlocks.EventBus;

/// <summary>
/// Interface para publicaÃ§Ã£o de eventos de integraÃ§Ã£o
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publica um evento no tÃ³pico padrÃ£o baseado no tipo
    /// </summary>
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IntegrationEvent;

    /// <summary>
    /// Publica um evento em um tÃ³pico especÃ­fico
    /// </summary>
    Task PublishAsync<T>(T @event, string topic, CancellationToken ct = default) where T : IntegrationEvent;

    /// <summary>
    /// Publica mÃºltiplos eventos em batch
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
/// Attribute para definir o nome do tÃ³pico
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
