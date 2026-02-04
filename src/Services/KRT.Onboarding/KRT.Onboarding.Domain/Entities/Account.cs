using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Domain.Enums;
using KRT.Onboarding.Domain.Events;

namespace KRT.Onboarding.Domain.Entities;

public class Account : Entity, IAggregateRoot
{
    public string CustomerName { get; private set; }
    public string Cpf { get; private set; }
    public string Email { get; private set; }
    public string AccountNumber { get; private set; }
    public AccountStatus Status { get; private set; }
    public decimal Balance { get; private set; }

    // Propriedades Computadas para Mapeamento (DTO Compatibilidade)
    public string CustomerDocument => Cpf;
    public string CustomerEmail => Email;
    public Guid CustomerId => Id; // Simplificação: 1 Cliente = 1 Conta
    public string BranchCode => "0001"; // Fixo por enquanto
    public string Type => "Checking";   // Fixo por enquanto
    public string Currency => "BRL";    // Fixo
    public decimal AvailableBalance => Balance;

    protected Account() { }

    public Account(string name, string cpf, string email)
    {
        Id = Guid.NewGuid();
        CustomerName = name;
        Cpf = cpf;
        Email = email;
        AccountNumber = new Random().Next(10000, 99999).ToString();
        Status = AccountStatus.Active;
        Balance = 0;
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new AccountCreatedEvent(Id, name, cpf, email, AccountNumber));
    }

    public void Block(string reason = "Bloqueio administrativo") 
    {
        Status = AccountStatus.Blocked;
        AddDomainEvent(new AccountBlockedEvent(Id, AccountNumber, reason));
    }

    public void Unblock()
    {
        Status = AccountStatus.Active;
        AddDomainEvent(new AccountUnblockedEvent(Id, AccountNumber));
    }

    public void Close(string reason = "Solicitação do cliente")
    {
        Status = AccountStatus.Closed;
        AddDomainEvent(new AccountClosedEvent(Id, AccountNumber, reason));
    }
}
