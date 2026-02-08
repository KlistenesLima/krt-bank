using KRT.Payments.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace KRT.Payments.Api.Hubs;

/// <summary>
/// Implementacao que envia notificacoes via SignalR Hub.
/// </summary>
public class SignalRTransactionNotifier : ITransactionNotifier
{
    private readonly IHubContext<TransactionHub> _hubContext;
    private readonly ILogger<SignalRTransactionNotifier> _logger;

    public SignalRTransactionNotifier(
        IHubContext<TransactionHub> hubContext,
        ILogger<SignalRTransactionNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyBalanceUpdatedAsync(Guid accountId, decimal newBalance)
    {
        var group = $"account_{accountId}";
        await _hubContext.Clients.Group(group).SendAsync("BalanceUpdated", new
        {
            accountId,
            balance = newBalance,
            timestamp = DateTime.UtcNow
        });
        _logger.LogInformation(
            "SignalR: BalanceUpdated sent to {Group} — R$ {Balance:N2}",
            group, newBalance);
    }

    public async Task NotifyTransactionStatusAsync(
        Guid accountId, Guid transactionId, string status, string description)
    {
        var group = $"account_{accountId}";
        await _hubContext.Clients.Group(group).SendAsync("TransactionStatus", new
        {
            accountId,
            transactionId,
            status,
            description,
            timestamp = DateTime.UtcNow
        });
        _logger.LogInformation(
            "SignalR: TransactionStatus [{Status}] sent to {Group} — Tx {TxId}",
            status, group, transactionId);
    }

    public async Task NotifyAlertAsync(Guid accountId, string alertType, string title, string message)
    {
        var group = $"account_{accountId}";
        await _hubContext.Clients.Group(group).SendAsync("Alert", new
        {
            accountId,
            alertType,
            title,
            message,
            timestamp = DateTime.UtcNow
        });
        _logger.LogInformation(
            "SignalR: Alert [{AlertType}] sent to {Group} — {Title}",
            alertType, group, title);
    }
}