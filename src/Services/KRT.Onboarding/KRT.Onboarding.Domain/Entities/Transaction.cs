using KRT.BuildingBlocks.Domain;

namespace KRT.Onboarding.Domain.Entities;

public class Transaction : Entity, IAggregateRoot
{
    public Guid AccountId { get; private set; }
    public decimal Amount { get; private set; }
    public string Type { get; private set; } // Credit/Debit
    public string Description { get; private set; }
    public string ReferenceId { get; private set; } // Adicionado
    public string Status { get; private set; } // Adicionado
    public DateTime? CompletedAt { get; private set; } // Adicionado
    
    protected Transaction() { }

    public Transaction(Guid accountId, decimal amount, string type, string description)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        Amount = amount;
        Type = type;
        Description = description;
        ReferenceId = Guid.NewGuid().ToString(); // Gerado auto
        Status = "Completed";
        CompletedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }
}
