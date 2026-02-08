namespace KRT.Payments.Domain.Entities;

public class Boleto
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public string Barcode { get; private set; } = "";
    public string DigitableLine { get; private set; } = "";
    public decimal Amount { get; private set; }
    public decimal? PaidAmount { get; private set; }
    public string BeneficiaryName { get; private set; } = "";
    public string BeneficiaryCnpj { get; private set; } = "";
    public string Description { get; private set; } = "";
    public DateTime DueDate { get; private set; }
    public BoletoStatus Status { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Boleto() { }

    public static Boleto Generate(Guid accountId, string beneficiaryName, string beneficiaryCnpj, decimal amount, DateTime dueDate, string description)
    {
        if (amount <= 0) throw new ArgumentException("Valor deve ser positivo");
        if (dueDate.Date < DateTime.UtcNow.Date) throw new ArgumentException("Vencimento deve ser futuro");

        var rng = new Random();
        var barcode = $"23793.{rng.Next(10000, 99999)} {rng.Next(10000, 99999)}.{rng.Next(100000, 999999)} {rng.Next(10000, 99999)}.{rng.Next(100000, 999999)} {rng.Next(1, 9)} {rng.Next(10000000, 99999999)}";

        return new Boleto
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Barcode = barcode.Replace(" ", "").Replace(".", ""),
            DigitableLine = barcode,
            Amount = amount,
            BeneficiaryName = beneficiaryName,
            BeneficiaryCnpj = beneficiaryCnpj,
            Description = description,
            DueDate = dueDate,
            Status = BoletoStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Boleto FromBarcode(Guid accountId, string barcode, decimal amount, string beneficiaryName)
    {
        return new Boleto
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Barcode = barcode.Replace(" ", "").Replace(".", ""),
            DigitableLine = barcode,
            Amount = amount,
            BeneficiaryName = beneficiaryName,
            BeneficiaryCnpj = "",
            Description = "Pagamento de boleto",
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = BoletoStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public (bool success, string message) Pay(decimal? paidAmount = null)
    {
        if (Status == BoletoStatus.Paid) return (false, "Boleto ja pago");
        if (Status == BoletoStatus.Cancelled) return (false, "Boleto cancelado");

        PaidAmount = paidAmount ?? Amount;
        PaidAt = DateTime.UtcNow;
        Status = BoletoStatus.Paid;
        return (true, "Boleto pago com sucesso");
    }

    public void Cancel()
    {
        if (Status == BoletoStatus.Paid) throw new InvalidOperationException("Boleto ja pago nao pode ser cancelado");
        Status = BoletoStatus.Cancelled;
    }

    public void CheckOverdue()
    {
        if (Status == BoletoStatus.Pending && DueDate.Date < DateTime.UtcNow.Date)
            Status = BoletoStatus.Overdue;
    }

    public string GetStatusLabel() => Status switch
    {
        BoletoStatus.Pending => "Pendente",
        BoletoStatus.Paid => "Pago",
        BoletoStatus.Overdue => "Vencido",
        BoletoStatus.Cancelled => "Cancelado",
        _ => "Desconhecido"
    };
}