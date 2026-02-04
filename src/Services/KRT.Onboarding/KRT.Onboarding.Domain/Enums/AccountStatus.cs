namespace KRT.Onboarding.Domain.Enums;

/// <summary>
/// Status da conta
/// </summary>
public enum AccountStatus
{
    /// <summary>
    /// Conta criada, aguardando ativaÃ§Ã£o
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Conta ativa e operacional
    /// </summary>
    Active = 2,

    /// <summary>
    /// Conta bloqueada temporariamente
    /// </summary>
    Blocked = 3,

    /// <summary>
    /// Conta encerrada definitivamente
    /// </summary>
    Closed = 4,

    /// <summary>
    /// Conta suspensa por compliance
    /// </summary>
    Suspended = 5
}
