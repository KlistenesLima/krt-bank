using KRT.BuildingBlocks.Infrastructure.Outbox;

namespace KRT.Payments.Application.Interfaces;

public interface IOutboxWriter
{
    void Add(OutboxMessage message);
    Task SaveAsync(CancellationToken ct = default);
}
