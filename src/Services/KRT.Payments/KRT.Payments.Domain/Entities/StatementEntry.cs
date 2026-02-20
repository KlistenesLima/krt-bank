namespace KRT.Payments.Domain.Entities;

public class StatementEntry
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public string CounterpartyName { get; set; } = "";
    public string CounterpartyBank { get; set; } = "";
    public bool IsCredit { get; set; }
    public DateTime CreatedAt { get; set; }
}