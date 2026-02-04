using System.Globalization;

namespace KRT.BuildingBlocks.Domain.ValueObjects;

/// <summary>
/// Value Object para representar valores monetÃ¡rios
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = Math.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    public static Money Create(decimal amount, string currency = "BRL")
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code", nameof(currency));

        return new Money(amount, currency);
    }

    public static Money Zero(string currency = "BRL") => new(0, currency);

    public static Money FromCents(long cents, string currency = "BRL") 
        => new(cents / 100m, currency);

    public long ToCents() => (long)(Amount * 100);

    public static Money operator +(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator *(Money a, decimal multiplier)
    {
        return new Money(a.Amount * multiplier, a.Currency);
    }

    public static Money operator /(Money a, decimal divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Cannot divide money by zero");

        return new Money(a.Amount / divisor, a.Currency);
    }

    public static bool operator <(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a.Amount < b.Amount;
    }

    public static bool operator >(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a.Amount > b.Amount;
    }

    public static bool operator <=(Money a, Money b) => !(a > b);
    public static bool operator >=(Money a, Money b) => !(a < b);

    public bool IsZero() => Amount == 0;
    public bool IsPositive() => Amount > 0;
    public bool IsNegative() => Amount < 0;

    public Money Abs() => new(Math.Abs(Amount), Currency);
    public Money Negate() => new(-Amount, Currency);

    private static void EnsureSameCurrency(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException(
                $"Cannot operate on different currencies: {a.Currency} and {b.Currency}");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() 
        => Amount.ToString("C2", GetCultureInfo());

    public string ToStringWithCode() 
        => $"{Currency} {Amount:N2}";

    private CultureInfo GetCultureInfo()
    {
        return Currency switch
        {
            "BRL" => new CultureInfo("pt-BR"),
            "USD" => new CultureInfo("en-US"),
            "EUR" => new CultureInfo("de-DE"),
            _ => CultureInfo.InvariantCulture
        };
    }
}
