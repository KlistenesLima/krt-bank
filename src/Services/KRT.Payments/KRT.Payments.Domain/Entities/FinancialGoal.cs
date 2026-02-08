namespace KRT.Payments.Domain.Entities;

public class FinancialGoal
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public string Title { get; private set; } = "";
    public string Icon { get; private set; } = "";
    public string Category { get; private set; } = "";
    public decimal TargetAmount { get; private set; }
    public decimal CurrentAmount { get; private set; }
    public DateTime Deadline { get; private set; }
    public FinancialGoalStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // Computed properties
    public decimal ProgressPercent => TargetAmount > 0
        ? Math.Round(CurrentAmount / TargetAmount * 100, 1)
        : 0;

    public decimal RemainingAmount => Math.Max(0, TargetAmount - CurrentAmount);

    public int DaysRemaining => Math.Max(0, (int)(Deadline - DateTime.UtcNow).TotalDays);

    public decimal MonthlyRequired
    {
        get
        {
            var monthsLeft = Math.Max(1, (int)Math.Ceiling((Deadline - DateTime.UtcNow).TotalDays / 30.0));
            return RemainingAmount > 0 ? Math.Round(RemainingAmount / monthsLeft, 2) : 0;
        }
    }

    public bool IsCompleted => Status == FinancialGoalStatus.Completed;

    // Private constructor for EF Core
    private FinancialGoal() { }

    // Factory method
    public static FinancialGoal Create(Guid accountId, string title, decimal targetAmount, DateTime deadline, string icon = "", string category = "Outros")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Titulo e obrigatorio");
        if (targetAmount <= 0)
            throw new ArgumentException("Valor alvo deve ser positivo");
        if (deadline <= DateTime.UtcNow)
            throw new ArgumentException("Prazo deve ser futuro");

        return new FinancialGoal
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Title = title,
            TargetAmount = targetAmount,
            Deadline = deadline,
            Icon = icon,
            Category = category,
            CurrentAmount = 0,
            Status = FinancialGoalStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Valor deve ser positivo");
        if (Status == FinancialGoalStatus.Cancelled)
            throw new InvalidOperationException("Meta cancelada");

        CurrentAmount += amount;

        if (CurrentAmount >= TargetAmount)
        {
            Status = FinancialGoalStatus.Completed;
            CompletedAt = DateTime.UtcNow;
        }
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Valor deve ser positivo");
        if (amount > CurrentAmount)
            throw new InvalidOperationException("Saldo insuficiente na meta");
        if (Status == FinancialGoalStatus.Cancelled)
            throw new InvalidOperationException("Meta cancelada");

        CurrentAmount -= amount;

        if (Status == FinancialGoalStatus.Completed && CurrentAmount < TargetAmount)
        {
            Status = FinancialGoalStatus.Active;
            CompletedAt = null;
        }
    }

    public void Cancel()
    {
        Status = FinancialGoalStatus.Cancelled;
    }

    public string GetStatusLabel() => Status switch
    {
        FinancialGoalStatus.Active => "Em andamento",
        FinancialGoalStatus.Completed => "Concluida",
        FinancialGoalStatus.Cancelled => "Cancelada",
        _ => "Desconhecido"
    };
}

public enum FinancialGoalStatus
{
    Active,
    Completed,
    Cancelled
}
