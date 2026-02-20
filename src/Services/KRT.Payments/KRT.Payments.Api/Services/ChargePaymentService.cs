using KRT.Payments.Api.Data;
using KRT.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Api.Services;

public record ChargePaymentResult(bool Success, string? Error, decimal? NewBalance, Guid? PayerAccountId)
{
    public static ChargePaymentResult Fail(string error) => new(false, error, null, null);
}

/// <summary>
/// Shared service for real banking operations on charge payments.
/// Prepares debit/credit/statement but does NOT call SaveChangesAsync — caller must save.
/// </summary>
public class ChargePaymentService
{
    private readonly PaymentsDbContext _db;
    public static readonly Guid MerchantAccountId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public ChargePaymentService(PaymentsDbContext db) => _db = db;

    public async Task<ChargePaymentResult> PreparePaymentAsync(
        Guid? payerAccountId,
        string? payerDocument,
        decimal amount,
        string type,
        string description,
        string merchantName,
        CancellationToken ct)
    {
        // 1. Find payer account
        BankAccount? payer = null;

        if (payerAccountId.HasValue)
            payer = await _db.BankAccounts.FindAsync([payerAccountId.Value], ct);

        if (payer == null && !string.IsNullOrEmpty(payerDocument))
            payer = await _db.BankAccounts
                .FirstOrDefaultAsync(a => a.Document == payerDocument && a.Status == "Active", ct);

        if (payer == null)
            payer = await _db.BankAccounts
                .Where(a => a.Status == "Active" && a.Id != MerchantAccountId)
                .OrderByDescending(a => a.Balance)
                .FirstOrDefaultAsync(ct);

        if (payer == null)
            return ChargePaymentResult.Fail("Conta pagadora nao encontrada");

        if (payer.Balance < amount)
            return ChargePaymentResult.Fail($"Saldo insuficiente (disponivel: R$ {payer.Balance:N2})");

        // 2. Find merchant account
        var merchant = await _db.BankAccounts.FindAsync([MerchantAccountId], ct);
        if (merchant == null)
            return ChargePaymentResult.Fail("Conta do recebedor nao encontrada");

        // 3. Debit payer
        payer.Balance -= amount;
        payer.UpdatedAt = DateTime.UtcNow;
        payer.RowVersion = Guid.NewGuid().ToByteArray();

        // 4. Credit merchant
        merchant.Balance += amount;
        merchant.UpdatedAt = DateTime.UtcNow;
        merchant.RowVersion = Guid.NewGuid().ToByteArray();

        // 5. Statement: payer (debit)
        _db.StatementEntries.Add(new StatementEntry
        {
            Id = Guid.NewGuid(),
            AccountId = payer.Id,
            Date = DateTime.UtcNow,
            Type = type,
            Category = "Payment",
            Amount = amount,
            Description = description,
            CounterpartyName = merchantName,
            CounterpartyBank = "KRT Bank",
            IsCredit = false,
            CreatedAt = DateTime.UtcNow
        });

        // 6. Statement: merchant (credit)
        _db.StatementEntries.Add(new StatementEntry
        {
            Id = Guid.NewGuid(),
            AccountId = merchant.Id,
            Date = DateTime.UtcNow,
            Type = type,
            Category = "Receivable",
            Amount = amount,
            Description = $"{type} recebido - {description}",
            CounterpartyName = payer.CustomerName,
            CounterpartyBank = "KRT Bank",
            IsCredit = true,
            CreatedAt = DateTime.UtcNow
        });

        // Caller must call _db.SaveChangesAsync() to persist all changes atomically
        return new ChargePaymentResult(true, null, payer.Balance, payer.Id);
    }
}
