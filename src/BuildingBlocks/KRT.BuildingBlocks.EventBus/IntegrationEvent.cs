namespace KRT.BuildingBlocks.EventBus;

/// <summary>
/// Evento de integração para comunicação entre serviços
/// </summary>
public abstract record IntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = string.Empty;
    public string CausationId { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public int Version { get; init; } = 1;

    /// <summary>
    /// Nome do tipo do evento para serialização
    /// </summary>
    public string EventType => GetType().Name;
}
