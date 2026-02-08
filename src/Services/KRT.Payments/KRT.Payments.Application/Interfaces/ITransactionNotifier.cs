namespace KRT.Payments.Application.Interfaces;

/// <summary>
/// Abstrai o envio de notificacoes em tempo real para o frontend.
/// </summary>
public interface ITransactionNotifier
{
    /// <summary>
    /// Notifica atualizacao de saldo para uma conta especifica.
    /// </summary>
    Task NotifyBalanceUpdatedAsync(Guid accountId, decimal newBalance);

    /// <summary>
    /// Notifica mudanca de status de uma transacao Pix.
    /// </summary>
    Task NotifyTransactionStatusAsync(Guid accountId, Guid transactionId, string status, string description);

    /// <summary>
    /// Envia alerta generico para uma conta (fraude, limites, etc).
    /// </summary>
    Task NotifyAlertAsync(Guid accountId, string alertType, string title, string message);
}