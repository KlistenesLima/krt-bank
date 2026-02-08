using KRT.Payments.Domain.Enums;
using System.Security.Cryptography;

namespace KRT.Payments.Domain.Entities;

public class VirtualCard
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public string CardNumber { get; private set; } = "";
    public string CardholderName { get; private set; } = "";
    public string ExpirationMonth { get; private set; } = "";
    public string ExpirationYear { get; private set; } = "";
    public string Cvv { get; private set; } = "";
    public string Last4Digits { get; private set; } = "";
    public CardBrand Brand { get; private set; }
    public CardStatus Status { get; private set; }
    public decimal SpendingLimit { get; private set; }
    public decimal SpentThisMonth { get; private set; }
    public bool IsContactless { get; private set; }
    public bool IsOnlinePurchase { get; private set; }
    public bool IsInternational { get; private set; }
    public DateTime CvvExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private VirtualCard() { }

    public static VirtualCard Create(Guid accountId, string holderName, CardBrand brand = CardBrand.Visa)
    {
        var number = GenerateCardNumber(brand);
        var now = DateTime.UtcNow;
        var expiry = now.AddYears(5);

        return new VirtualCard
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            CardNumber = number,
            CardholderName = holderName.ToUpper(),
            ExpirationMonth = expiry.Month.ToString("D2"),
            ExpirationYear = expiry.Year.ToString(),
            Cvv = GenerateCvv(),
            Last4Digits = number[^4..],
            Brand = brand,
            Status = CardStatus.Active,
            SpendingLimit = 5000.00m,
            SpentThisMonth = 0,
            IsContactless = true,
            IsOnlinePurchase = true,
            IsInternational = false,
            CvvExpiresAt = now.AddHours(24),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Block()
    {
        if (Status == CardStatus.Cancelled)
            throw new InvalidOperationException("Cartao cancelado nao pode ser bloqueado");
        Status = CardStatus.Blocked;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unblock()
    {
        if (Status != CardStatus.Blocked)
            throw new InvalidOperationException("Cartao nao esta bloqueado");
        Status = CardStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = CardStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gera novo CVV dinamico (valido por 24h).
    /// </summary>
    public string RotateCvv()
    {
        Cvv = GenerateCvv();
        CvvExpiresAt = DateTime.UtcNow.AddHours(24);
        UpdatedAt = DateTime.UtcNow;
        return Cvv;
    }

    /// <summary>
    /// Verifica se o CVV ainda e valido.
    /// </summary>
    public bool IsCvvValid() => DateTime.UtcNow < CvvExpiresAt;

    public void UpdateSpendingLimit(decimal newLimit)
    {
        if (newLimit < 0) throw new ArgumentException("Limite deve ser positivo");
        SpendingLimit = newLimit;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleContactless(bool enabled)
    {
        IsContactless = enabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleOnlinePurchase(bool enabled)
    {
        IsOnlinePurchase = enabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleInternational(bool enabled)
    {
        IsInternational = enabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public (bool Allowed, string? Reason) ValidatePurchase(decimal amount, bool isOnline, bool isInternational)
    {
        if (Status != CardStatus.Active)
            return (false, $"Cartao {Status}");
        if (isOnline && !IsOnlinePurchase)
            return (false, "Compras online desabilitadas");
        if (isInternational && !IsInternational)
            return (false, "Compras internacionais desabilitadas");
        if (SpentThisMonth + amount > SpendingLimit)
            return (false, $"Limite mensal excedido. Usado: R$ {SpentThisMonth:N2} / Limite: R$ {SpendingLimit:N2}");
        return (true, null);
    }

    /// <summary>Mascara numero: **** **** **** 1234</summary>
    public string GetMaskedNumber() => $"**** **** **** {Last4Digits}";

    private static string GenerateCardNumber(CardBrand brand)
    {
        var prefix = brand == CardBrand.Visa ? "4" : "5";
        var random = new byte[14];
        RandomNumberGenerator.Fill(random);
        var digits = prefix + string.Join("", random.Select(b => (b % 10).ToString()));
        digits = digits[..15];
        // Luhn check digit
        var sum = 0;
        for (int i = digits.Length - 1, alt = 0; i >= 0; i--, alt++)
        {
            var n = digits[i] - '0';
            if (alt % 2 == 0) { n *= 2; if (n > 9) n -= 9; }
            sum += n;
        }
        var check = (10 - (sum % 10)) % 10;
        return digits + check;
    }

    private static string GenerateCvv()
    {
        var bytes = new byte[2];
        RandomNumberGenerator.Fill(bytes);
        return (BitConverter.ToUInt16(bytes) % 900 + 100).ToString();
    }
}