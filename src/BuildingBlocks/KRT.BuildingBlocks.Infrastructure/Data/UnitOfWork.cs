using KRT.BuildingBlocks.Domain;
using KRT.BuildingBlocks.Infrastructure.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace KRT.BuildingBlocks.Infrastructure.Data;

/// <summary>
/// Implementação base do Unit of Work com suporte a Domain Events e Outbox
/// </summary>
public class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    private readonly TContext _context;
    private readonly IMediator _mediator;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(TContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        // Processa Domain Events antes de salvar
        await DispatchDomainEventsAsync(ct);

        // Salva mudanças
        return await _context.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        try
        {
            await CommitAsync(ct);

            if (_transaction != null)
            {
                await _transaction.CommitAsync(ct);
            }
        }
        catch
        {
            await RollbackTransactionAsync(ct);
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(ct);
            _transaction.Dispose();
            _transaction = null;
        }
    }

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        var entities = _context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Limpa os eventos das entidades
        entities.ForEach(e => e.ClearDomainEvents());

        // Publica cada evento via MediatR
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, ct);
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
