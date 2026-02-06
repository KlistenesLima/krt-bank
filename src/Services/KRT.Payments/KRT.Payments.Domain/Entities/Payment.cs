using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Enums;

namespace KRT.Payments.Domain.Entities;

public class Payment : Entity, IAggregateRoot
{
    public Guid AccountId { get; private set; }
    public string ReceiverKey { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    
    // REMOVIDO: public DateTime CreatedAt... (Usa da base Entity)

    // Construtor EF Core (Inicializa propriedades não-nuláveis com null!)
    protected Payment() 
    { 
        ReceiverKey = null!;
    }

    public Payment(Guid accountId, string receiverKey, decimal amount)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        ReceiverKey = receiverKey;
        Amount = amount;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow; // Usa setter da base
    }

    public void Complete() => Status = PaymentStatus.Completed;
    public void Fail() => Status = PaymentStatus.Failed;
}
