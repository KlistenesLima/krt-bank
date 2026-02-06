using KRT.BuildingBlocks.Domain;
using KRT.BuildingBlocks.Domain.Exceptions;
using KRT.Onboarding.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace KRT.Onboarding.Domain.Entities;

public class Account : AggregateRoot
{
    public string CustomerName { get; private set; } = null!;
    public string Document { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public decimal Balance { get; private set; }
    public AccountStatus Status { get; private set; }
    public AccountType Type { get; private set; }

    [Timestamp]
    public byte[] RowVersion { get; private set; } = null!;

    protected Account() { } // EF Core

    public Account(string name, string doc, string email, AccountType type)
    {
        Id = Guid.NewGuid();
        CustomerName = name ?? throw new BusinessRuleException("Nome é obrigatório");
        Document = doc ?? throw new BusinessRuleException("Documento é obrigatório");
        Email = email ?? throw new BusinessRuleException("Email é obrigatório");
        Type = type;
        Status = AccountStatus.Active;
        Balance = 0;
    }

    public void Credit(decimal amount)
    {
        if (amount <= 0) throw new BusinessRuleException("Valor deve ser positivo");
        Balance += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Debit(decimal amount)
    {
        if (amount <= 0) throw new BusinessRuleException("Valor deve ser positivo");
        if (Balance < amount) throw new BusinessRuleException("Saldo insuficiente");
        Balance -= amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Block(string reason)
    {
        if (Status != AccountStatus.Active)
            throw new BusinessRuleException("Apenas contas ativas podem ser bloqueadas");
        Status = AccountStatus.Blocked;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status == AccountStatus.Closed)
            throw new BusinessRuleException("Contas encerradas não podem ser reativadas");
        Status = AccountStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close(string reason)
    {
        if (Balance != 0)
            throw new BusinessRuleException("Conta deve ter saldo zero para ser encerrada");
        Status = AccountStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
    }
}
