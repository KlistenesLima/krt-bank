using System.Linq.Expressions;
using KRT.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;

namespace KRT.BuildingBlocks.Infrastructure.Data;

/// <summary>
/// Interface base para repositÃ³rios
/// </summary>
public interface IRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}

/// <summary>
/// ImplementaÃ§Ã£o base do repositÃ³rio genÃ©rico
/// </summary>
public class Repository<T, TContext> : IRepository<T>
    where T : Entity
    where TContext : DbContext
{
    protected readonly TContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(TContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet.FindAsync(new object[] { id }, ct);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await DbSet.ToListAsync(ct);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default)
    {
        return await DbSet.Where(predicate).ToListAsync(ct);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(predicate, ct);
    }

    public virtual async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default)
    {
        return await DbSet.AnyAsync(predicate, ct);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default)
    {
        return predicate == null
            ? await DbSet.CountAsync(ct)
            : await DbSet.CountAsync(predicate, ct);
    }

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await DbSet.AddAsync(entity, ct);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        await DbSet.AddRangeAsync(entities, ct);
    }

    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Remove(T entity)
    {
        DbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);
    }
}
