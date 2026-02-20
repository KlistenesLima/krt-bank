using KRT.Payments.Domain.Enums;

namespace KRT.Payments.Domain.Entities;

/// <summary>
/// Limites Pix por conta — diurno e noturno, por transacao e diario.
/// </summary>
public class PixLimit
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }

    /// <summary>Limite maximo por transacao no periodo diurno (06h-20h).</summary>
    public decimal DaytimePerTransaction { get; private set; }

    /// <summary>Limite diario total no periodo diurno.</summary>
    public decimal DaytimeDaily { get; private set; }

    /// <summary>Limite maximo por transacao no periodo noturno (20h-06h).</summary>
    public decimal NighttimePerTransaction { get; private set; }

    /// <summary>Limite diario total no periodo noturno.</summary>
    public decimal NighttimeDaily { get; private set; }

    /// <summary>Total transferido hoje no periodo diurno.</summary>
    public decimal DaytimeUsedToday { get; private set; }

    /// <summary>Total transferido hoje no periodo noturno.</summary>
    public decimal NighttimeUsedToday { get; private set; }

    /// <summary>Data da ultima resetagem dos contadores diarios.</summary>
    public DateTime LastResetDate { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // EF
    private PixLimit() { }

    /// <summary>
    /// Cria limites padrao para uma conta.
    /// </summary>
    public static PixLimit CreateDefault(Guid accountId) => new()
    {
        Id = Guid.NewGuid(),
        AccountId = accountId,
        DaytimePerTransaction = 5000.00m,
        DaytimeDaily = 20000.00m,
        NighttimePerTransaction = 1000.00m,
        NighttimeDaily = 5000.00m,
        DaytimeUsedToday = 0,
        NighttimeUsedToday = 0,
        LastResetDate = DateTime.UtcNow.Date,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Valida se a transferencia esta dentro dos limites.
    /// Retorna (isAllowed, reason).
    /// </summary>
    public (bool IsAllowed, string? Reason) ValidateTransfer(decimal amount, DateTime now)
    {
        ResetDailyIfNeeded(now);

        var period = GetCurrentPeriod(now);
        var perTx = period == PixLimitPeriod.Daytime ? DaytimePerTransaction : NighttimePerTransaction;
        var daily = period == PixLimitPeriod.Daytime ? DaytimeDaily : NighttimeDaily;
        var used = period == PixLimitPeriod.Daytime ? DaytimeUsedToday : NighttimeUsedToday;
        var periodName = period == PixLimitPeriod.Daytime ? "diurno" : "noturno";

        if (amount > perTx)
            return (false, $"Valor R$ {amount:N2} excede o limite por transacao {periodName} de R$ {perTx:N2}");

        if (used + amount > daily)
            return (false, $"Transferencia excede o limite diario {periodName}. Usado: R$ {used:N2} / Limite: R$ {daily:N2}");

        return (true, null);
    }

    /// <summary>
    /// Registra uso apos transferencia aprovada.
    /// </summary>
    public void RegisterUsage(decimal amount, DateTime now)
    {
        ResetDailyIfNeeded(now);

        if (GetCurrentPeriod(now) == PixLimitPeriod.Daytime)
            DaytimeUsedToday += amount;
        else
            NighttimeUsedToday += amount;

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Atualiza limites personalizados.
    /// </summary>
    public void UpdateLimits(decimal? daytimePerTx, decimal? daytimeDaily, decimal? nightPerTx, decimal? nightDaily)
    {
        if (daytimePerTx.HasValue) DaytimePerTransaction = daytimePerTx.Value;
        if (daytimeDaily.HasValue) DaytimeDaily = daytimeDaily.Value;
        if (nightPerTx.HasValue) NighttimePerTransaction = nightPerTx.Value;
        if (nightDaily.HasValue) NighttimeDaily = nightDaily.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    private void ResetDailyIfNeeded(DateTime now)
    {
        if (now.Date > LastResetDate)
        {
            DaytimeUsedToday = 0;
            NighttimeUsedToday = 0;
            LastResetDate = now.Date;
        }
    }

    private static PixLimitPeriod GetCurrentPeriod(DateTime now)
        => now.Hour >= 6 && now.Hour < 20 ? PixLimitPeriod.Daytime : PixLimitPeriod.Nighttime;
}