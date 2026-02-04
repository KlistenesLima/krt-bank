using MediatR;
namespace KRT.BuildingBlocks.Domain;

// Mudança Crítica: class -> record
public abstract record DomainEvent : INotification
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
