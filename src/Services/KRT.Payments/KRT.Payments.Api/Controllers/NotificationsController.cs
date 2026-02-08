using KRT.Payments.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly ITransactionNotifier _notifier;

    public NotificationsController(ITransactionNotifier notifier)
    {
        _notifier = notifier;
    }

    /// <summary>
    /// Endpoint de teste: envia notificacao de saldo para uma conta.
    /// </summary>
    [HttpPost("test/balance/{accountId}")]
    [AllowAnonymous]
    public async Task<IActionResult> TestBalanceNotification(Guid accountId, [FromQuery] decimal balance = 1500.00m)
    {
        await _notifier.NotifyBalanceUpdatedAsync(accountId, balance);
        return Ok(new { sent = true, accountId, balance });
    }

    /// <summary>
    /// Endpoint de teste: envia alerta para uma conta.
    /// </summary>
    [HttpPost("test/alert/{accountId}")]
    [AllowAnonymous]
    public async Task<IActionResult> TestAlert(
        Guid accountId,
        [FromQuery] string type = "info",
        [FromQuery] string title = "Teste",
        [FromQuery] string message = "Esta e uma notificacao de teste via SignalR")
    {
        await _notifier.NotifyAlertAsync(accountId, type, title, message);
        return Ok(new { sent = true, accountId, type, title });
    }

    /// <summary>
    /// Endpoint de teste: envia status de transacao.
    /// </summary>
    [HttpPost("test/transaction/{accountId}")]
    [AllowAnonymous]
    public async Task<IActionResult> TestTransactionStatus(
        Guid accountId,
        [FromQuery] string status = "Completed",
        [FromQuery] string description = "Pix de teste concluido com sucesso")
    {
        var txId = Guid.NewGuid();
        await _notifier.NotifyTransactionStatusAsync(accountId, txId, status, description);
        return Ok(new { sent = true, accountId, transactionId = txId, status });
    }
}