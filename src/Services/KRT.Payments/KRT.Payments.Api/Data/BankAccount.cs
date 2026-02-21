namespace KRT.Payments.Api.Data;

/// <summary>
/// Lightweight entity mapping to the Accounts table (owned by Onboarding service).
/// Used for cross-service balance operations in simulate-payment endpoints.
/// </summary>
public class BankAccount
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = "";
    public string Document { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public decimal Balance { get; set; }
    public string Status { get; set; } = "Active";
    public string Type { get; set; } = "Checking";
    public string Role { get; set; } = "User";
    public byte[] RowVersion { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
