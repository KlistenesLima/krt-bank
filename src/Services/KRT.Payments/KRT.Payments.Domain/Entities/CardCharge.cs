namespace KRT.Payments.Domain.Entities;

public enum CardChargeStatus { Approved, Declined, Pending, Refunded }

public class CardCharge
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public string ExternalId { get; set; } = "";
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public CardChargeStatus Status { get; set; } = CardChargeStatus.Pending;
    public string AuthorizationCode { get; set; } = "";
    public int Installments { get; set; } = 1;
    public decimal InstallmentAmount { get; set; }
    public string? MerchantId { get; set; }
    public string? WebhookUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
