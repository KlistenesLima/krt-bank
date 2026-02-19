namespace KRT.Payments.Domain.Entities;

public enum BoletoChargeStatus { Pending, Processing, Confirmed, Expired, Cancelled }

public class BoletoCharge
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = "";
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public string Barcode { get; set; } = "";
    public string DigitableLine { get; set; } = "";
    public BoletoChargeStatus Status { get; set; } = BoletoChargeStatus.Pending;
    public string? PayerCpf { get; set; }
    public string? PayerName { get; set; }
    public string? MerchantId { get; set; }
    public string? WebhookUrl { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
}
