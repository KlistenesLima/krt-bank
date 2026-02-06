using KRT.BuildingBlocks.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace KRT.BuildingBlocks.Infrastructure.Data;

/// <summary>
/// Implementação Enterprise do Unit of Work.
/// Gerencia Transações e dispara Domain Events antes do Commit.
/// </summary>
public class UnitOfWork<TContext> : IUnitOfWork, IDisposable where TContext : DbContext
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
        // 1. Dispara eventos de domínio (ex: "ContaCriada").
        // Os Handlers desses eventos podem gravar na tabela OutboxMessage
        // dentro desta mesma transação.
        await DispatchDomainEventsAsync(ct);

        // 2. Salva tudo (Entidade + Outbox) atomicamente
        return await _context.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null) return;
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
        // Pega todas as entidades rastreadas que têm eventos pendentes
        var entities = _context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Limpa os eventos para não dispararem duas vezes
        entities.ForEach(e => e.ClearDomainEvents());

        // Publica no MediatR (In-Memory).
        // Se um Handler falhar, o CommitAsync lá em cima falha e o banco não é alterado.
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, ct);
        }
    }

    public void Dispose()
    {
        // CORREÇÃO: Não damos dispose no _context aqui, pois ele é injetado (Scoped).
        // O container de DI cuida dele. Damos dispose apenas na transação que nós abrimos.
        _transaction?.Dispose();
        GC.SuppressFinalize(this);
    }
}
