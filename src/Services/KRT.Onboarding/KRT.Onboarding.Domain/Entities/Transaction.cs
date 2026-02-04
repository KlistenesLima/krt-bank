using KRT.BuildingBlocks.Domain;

namespace KRT.Onboarding.Domain.Entities;

// CORREÇÃO: Adicionado IAggregateRoot para satisfazer a restrição do Repositório Genérico
public class Transaction : Entity, IAggregateRoot
{
    public Guid AccountId { get; private set; }
    public decimal Amount { get; private set; }
    public string Type { get; private set; } 
    public string Description { get; private set; }
    public string ReferenceId { get; private set; } 

    protected Transaction() { }

    public Transaction(Guid accountId, decimal amount, string type, string description)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        Amount = amount;
        Type = type;
        Description = description;
        ReferenceId = Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
    }
}
