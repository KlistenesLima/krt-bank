using FluentAssertions;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Infra.Data.Context;
using KRT.Payments.Infra.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KRT.IntegrationTests.Repositories;

public class PixTransactionRepositoryTests : IDisposable
{
    private readonly PaymentsDbContext _context;
    private readonly PixTransactionRepository _repository;

    public PixTransactionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _context = new PaymentsDbContext(options);
        _repository = new PixTransactionRepository(_context);
    }

    [Fact]
    public async Task Add_ShouldPersist()
    {
        var tx = new PixTransaction(Guid.NewGuid(), Guid.NewGuid(), 200m, "k", "Test", Guid.NewGuid());
        await _repository.AddAsync(tx);
        await _context.SaveChangesAsync();

        var found = await _repository.GetByIdAsync(tx.Id);
        found.Should().NotBeNull();
        found!.Amount.Should().Be(200m);
    }

    [Fact]
    public async Task GetByIdempotencyKey_ShouldFind()
    {
        var key = Guid.NewGuid();
        await _repository.AddAsync(new PixTransaction(Guid.NewGuid(), Guid.NewGuid(), 100m, "k", null, key));
        await _context.SaveChangesAsync();

        var found = await _repository.GetByIdempotencyKeyAsync(key);
        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByAccountId_ShouldReturnAll()
    {
        var accountId = Guid.NewGuid();
        await _repository.AddAsync(new PixTransaction(accountId, Guid.NewGuid(), 100m, "k", null, Guid.NewGuid()));
        await _repository.AddAsync(new PixTransaction(accountId, Guid.NewGuid(), 200m, "k", null, Guid.NewGuid()));
        await _repository.AddAsync(new PixTransaction(Guid.NewGuid(), accountId, 50m, "k", null, Guid.NewGuid()));
        await _context.SaveChangesAsync();

        var results = await _repository.GetByAccountIdAsync(accountId);
        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task Update_ShouldPersist()
    {
        var tx = new PixTransaction(Guid.NewGuid(), Guid.NewGuid(), 100m, "k", null, Guid.NewGuid());
        await _repository.AddAsync(tx);
        await _context.SaveChangesAsync();

        tx.MarkSourceDebited();
        _repository.Update(tx);
        await _context.SaveChangesAsync();

        var found = await _repository.GetByIdAsync(tx.Id);
        found!.SourceDebited.Should().BeTrue();
    }

    public void Dispose() => _context.Dispose();
}

