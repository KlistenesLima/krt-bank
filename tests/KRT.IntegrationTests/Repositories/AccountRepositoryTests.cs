using FluentAssertions;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Enums;
using KRT.Onboarding.Infra.Data.Context;
using KRT.Onboarding.Infra.Data.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace KRT.IntegrationTests.Repositories;

public class AccountRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly AccountRepository _repository;

    public AccountRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _context = new ApplicationDbContext(options);
        _repository = new AccountRepository(_context);
    }

    [Fact]
    public async Task Add_ShouldPersist()
    {
        var account = new Account("Ana", "11122233344", "ana@e.com", AccountType.Checking);
        await _repository.AddAsync(account, CancellationToken.None);
        await _context.SaveChangesAsync();

        var found = await _repository.GetByIdAsync(account.Id, CancellationToken.None);
        found.Should().NotBeNull();
        found!.CustomerName.Should().Be("Ana");
    }

    [Fact]
    public async Task GetByCpf_ShouldFind()
    {
        var account = new Account("Pedro", "99988877766", "p@e.com", AccountType.Savings);
        await _repository.AddAsync(account, CancellationToken.None);
        await _context.SaveChangesAsync();

        var found = await _repository.GetByCpfAsync("99988877766", CancellationToken.None);
        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByCpf_NonExistent_ShouldReturnNull()
    {
        var found = await _repository.GetByCpfAsync("00000000000", CancellationToken.None);
        found.Should().BeNull();
    }

    [Fact]
    public async Task Debit_ShouldPersistBalance()
    {
        var a = new Account("User", "11111111111", "u@e.com", AccountType.Checking);
        await _repository.AddAsync(a, CancellationToken.None);
        await _context.SaveChangesAsync();

        a.Credit(500m);
        a.Debit(200m);
        await _context.SaveChangesAsync();

        var found = await _repository.GetByIdAsync(a.Id, CancellationToken.None);
        found!.Balance.Should().Be(300m);
    }

    public void Dispose() => _context.Dispose();
}
