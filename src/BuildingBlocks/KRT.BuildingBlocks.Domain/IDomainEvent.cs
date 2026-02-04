using MediatR;
namespace KRT.BuildingBlocks.Domain;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
