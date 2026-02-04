using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Enums;

namespace KRT.Payments.Domain.Entities;

public class Payment : Entity, IAggregateRoot
{
    public Guid AccountId { get; private set; }
    public string ReceiverKey { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected Payment() { }

    public Payment(Guid accountId, string receiverKey, decimal amount)
    {
        AccountId = accountId;
        ReceiverKey = receiverKey;
        Amount = amount;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Complete() => Status = PaymentStatus.Completed;
    public void Fail() => Status = PaymentStatus.Failed;
}
