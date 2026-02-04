namespace KRT.Onboarding.Application.DTOs;

public class AccountDto
{
    public Guid AccountId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerDocument { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class BalanceDto
{
    public Guid AccountId { get; set; }
    public decimal AvailableAmount { get; set; }
}

public class StatementDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
