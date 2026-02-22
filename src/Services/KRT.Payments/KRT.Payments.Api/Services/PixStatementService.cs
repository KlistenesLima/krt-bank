using KRT.Payments.Api.Data;
using KRT.Payments.Application.Interfaces;
using KRT.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Api.Services;

public class PixStatementService : IPixStatementService
{
    private readonly PaymentsDbContext _db;

    public PixStatementService(PaymentsDbContext db) => _db = db;

    public async Task CreatePixStatementEntriesAsync(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        string pixKey,
        string? description,
        CancellationToken ct)
    {
        var source = await _db.BankAccounts.FindAsync([sourceAccountId], ct);
        var dest = await _db.BankAccounts.FindAsync([destinationAccountId], ct);

        var sourceName = source?.CustomerName ?? sourceAccountId.ToString()[..8];
        var destName = dest?.CustomerName ?? destinationAccountId.ToString()[..8];

        // Debit entry for sender
        _db.StatementEntries.Add(new StatementEntry
        {
            Id = Guid.NewGuid(),
            AccountId = sourceAccountId,
            Date = DateTime.UtcNow,
            Type = "PIX",
            Category = "Payment",
            Amount = amount,
            Description = string.IsNullOrEmpty(description) ? $"PIX para {destName}" : description,
            CounterpartyName = destName,
            CounterpartyBank = "KRT Bank",
            IsCredit = false,
            CreatedAt = DateTime.UtcNow
        });

        // Credit entry for receiver
        _db.StatementEntries.Add(new StatementEntry
        {
            Id = Guid.NewGuid(),
            AccountId = destinationAccountId,
            Date = DateTime.UtcNow,
            Type = "PIX",
            Category = "Receivable",
            Amount = amount,
            Description = $"PIX recebido de {sourceName}",
            CounterpartyName = sourceName,
            CounterpartyBank = "KRT Bank",
            IsCredit = true,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
    }
}
