namespace KRT.Payments.Domain.Entities;

public enum PixChargeStatus { Pending, Confirmed, Expired, Cancelled }

public class PixCharge
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = ""; // orderId from e-commerce
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public string QrCode { get; set; } = ""; // PIX copy-paste text
    public string QrCodeBase64 { get; set; } = ""; // QR code image
    public PixChargeStatus Status { get; set; } = PixChargeStatus.Pending;
    public string? PayerCpf { get; set; }
    public string? MerchantId { get; set; }
    public string? WebhookUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
