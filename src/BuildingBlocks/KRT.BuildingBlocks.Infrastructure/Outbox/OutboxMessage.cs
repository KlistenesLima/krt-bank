namespace KRT.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Mensagem do Outbox para garantir consistÃªncia entre DB e eventos
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }

    public static OutboxMessage Create<T>(T @event, string? correlationId = null, string? causationId = null)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(T).AssemblyQualifiedName ?? typeof(T).FullName ?? typeof(T).Name,
            Content = System.Text.Json.JsonSerializer.Serialize(@event),
            OccurredOn = DateTime.UtcNow,
            CorrelationId = correlationId,
            CausationId = causationId
        };
    }
}
