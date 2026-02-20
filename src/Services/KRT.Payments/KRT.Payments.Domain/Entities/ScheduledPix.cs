namespace KRT.Payments.Domain.Entities;

public class ScheduledPix
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid DestinationAccountId { get; private set; }
    public string PixKey { get; private set; } = "";
    public string DestinationName { get; private set; } = "";
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = "";

    // Agendamento
    public DateTime ScheduledDate { get; private set; }
    public ScheduledPixFrequency Frequency { get; private set; }
    public bool IsRecurring => Frequency != ScheduledPixFrequency.Once;
    public DateTime? EndDate { get; private set; }
    public int ExecutionCount { get; private set; }
    public int? MaxExecutions { get; private set; }

    // Status
    public ScheduledPixStatus Status { get; private set; }
    public DateTime? LastExecutedAt { get; private set; }
    public DateTime? NextExecutionDate { get; private set; }
    public string? LastError { get; private set; }

    // Audit
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private ScheduledPix() { }

    public static ScheduledPix Create(
        Guid accountId,
        Guid destinationAccountId,
        string pixKey,
        string destinationName,
        decimal amount,
        string description,
        DateTime scheduledDate,
        ScheduledPixFrequency frequency = ScheduledPixFrequency.Once,
        DateTime? endDate = null,
        int? maxExecutions = null)
    {
        if (amount <= 0) throw new ArgumentException("Valor deve ser positivo");
        if (scheduledDate < DateTime.UtcNow.AddMinutes(-5))
            throw new ArgumentException("Data de agendamento deve ser futura");
        if (frequency != ScheduledPixFrequency.Once && endDate == null && maxExecutions == null)
        {
            // Recorrente sem fim: padrao 12 execucoes
            maxExecutions = 12;
        }

        return new ScheduledPix
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            DestinationAccountId = destinationAccountId,
            PixKey = pixKey,
            DestinationName = destinationName,
            Amount = amount,
            Description = description,
            ScheduledDate = scheduledDate,
            Frequency = frequency,
            EndDate = endDate,
            MaxExecutions = maxExecutions,
            ExecutionCount = 0,
            Status = ScheduledPixStatus.Pending,
            NextExecutionDate = scheduledDate,
            CreatedAt = DateTime.UtcNow
        };
    }

    public (bool success, string message) Execute()
    {
        if (Status == ScheduledPixStatus.Cancelled)
            return (false, "Agendamento cancelado");
        if (Status == ScheduledPixStatus.Paused)
            return (false, "Agendamento pausado");

        ExecutionCount++;
        LastExecutedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Simular execucao (em producao chamaria o ProcessPixCommand)
        Status = ScheduledPixStatus.Executed;

        // Calcular proxima execucao para recorrente
        if (IsRecurring)
        {
            var next = CalculateNextExecution();
            if (next.HasValue && !HasReachedLimit())
            {
                NextExecutionDate = next.Value;
                Status = ScheduledPixStatus.Pending;
            }
            else
            {
                NextExecutionDate = null;
            }
        }
        else
        {
            NextExecutionDate = null;
        }

        return (true, $"Pix executado. Execucao #{ExecutionCount}");
    }

    public void MarkFailed(string error)
    {
        Status = ScheduledPixStatus.Failed;
        LastError = error;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = ScheduledPixStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        NextExecutionDate = null;
    }

    public void Pause()
    {
        if (!IsRecurring) throw new InvalidOperationException("Apenas agendamentos recorrentes podem ser pausados");
        Status = ScheduledPixStatus.Paused;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Resume()
    {
        if (Status != ScheduledPixStatus.Paused) throw new InvalidOperationException("Apenas agendamentos pausados podem ser retomados");
        Status = ScheduledPixStatus.Pending;
        NextExecutionDate = CalculateNextExecution() ?? DateTime.UtcNow.AddDays(1);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAmount(decimal newAmount)
    {
        if (newAmount <= 0) throw new ArgumentException("Valor deve ser positivo");
        Amount = newAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    private bool HasReachedLimit()
    {
        if (MaxExecutions.HasValue && ExecutionCount >= MaxExecutions.Value) return true;
        if (EndDate.HasValue && DateTime.UtcNow >= EndDate.Value) return true;
        return false;
    }

    private DateTime? CalculateNextExecution()
    {
        var baseDate = LastExecutedAt ?? ScheduledDate;
        return Frequency switch
        {
            ScheduledPixFrequency.Weekly => baseDate.AddDays(7),
            ScheduledPixFrequency.BiWeekly => baseDate.AddDays(14),
            ScheduledPixFrequency.Monthly => baseDate.AddMonths(1),
            ScheduledPixFrequency.Yearly => baseDate.AddYears(1),
            _ => null
        };
    }

    public string GetFrequencyLabel() => Frequency switch
    {
        ScheduledPixFrequency.Once => "Unico",
        ScheduledPixFrequency.Weekly => "Semanal",
        ScheduledPixFrequency.BiWeekly => "Quinzenal",
        ScheduledPixFrequency.Monthly => "Mensal",
        ScheduledPixFrequency.Yearly => "Anual",
        _ => "Desconhecido"
    };

    public string GetStatusLabel() => Status switch
    {
        ScheduledPixStatus.Pending => "Agendado",
        ScheduledPixStatus.Executed => "Executado",
        ScheduledPixStatus.Failed => "Falhou",
        ScheduledPixStatus.Cancelled => "Cancelado",
        ScheduledPixStatus.Paused => "Pausado",
        _ => "Desconhecido"
    };
}