namespace KRT.Payments.Api.Data;

/// <summary>
/// Lightweight read-only entity mapping to the PixKeys table (owned by Onboarding service).
/// Used for cross-service PIX key resolution in pay-brcode endpoint.
/// </summary>
public class BankPixKey
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public int KeyType { get; set; }
    public string KeyValue { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }
}
