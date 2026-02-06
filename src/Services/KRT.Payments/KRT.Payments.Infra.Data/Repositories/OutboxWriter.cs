using KRT.BuildingBlocks.Infrastructure.Outbox;
using KRT.Payments.Application.Interfaces;
using KRT.Payments.Infra.Data.Context;

namespace KRT.Payments.Infra.Data.Repositories;

public class OutboxWriter : IOutboxWriter
{
    private readonly PaymentsDbContext _ctx;
    public OutboxWriter(PaymentsDbContext ctx) => _ctx = ctx;

    public void Add(OutboxMessage message) => _ctx.OutboxMessages.Add(message);
    public async Task SaveAsync(CancellationToken ct) => await _ctx.SaveChangesAsync(ct);
}
