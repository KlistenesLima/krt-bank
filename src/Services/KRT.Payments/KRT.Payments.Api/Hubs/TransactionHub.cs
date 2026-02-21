using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace KRT.Payments.Api.Hubs;

/// <summary>
/// Hub SignalR para notificacoes em tempo real.
/// Cada usuario se conecta e e adicionado ao grupo do seu accountId.
/// </summary>
public class TransactionHub : Hub
{
    private readonly ILogger<TransactionHub> _logger;

    public TransactionHub(ILogger<TransactionHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Cliente informa seu accountId para receber notificacoes da conta.
    /// </summary>
    public async Task JoinAccountGroup(string accountId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"account_{accountId}");
        _logger.LogInformation(
            "SignalR: Connection {ConnId} joined group account_{AccountId}",
            Context.ConnectionId, accountId);
        
        await Clients.Caller.SendAsync("Connected", new
        {
            message = "Conectado ao canal de notificacoes",
            accountId,
            connectionId = Context.ConnectionId
        });
    }

    /// <summary>
    /// Cliente sai do grupo da conta.
    /// </summary>
    public async Task LeaveAccountGroup(string accountId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"account_{accountId}");
        _logger.LogInformation(
            "SignalR: Connection {ConnId} left group account_{AccountId}",
            Context.ConnectionId, accountId);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("SignalR: Client connected {ConnId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            "SignalR: Client disconnected {ConnId}. Reason: {Reason}",
            Context.ConnectionId, exception?.Message ?? "Normal");
        await base.OnDisconnectedAsync(exception);
    }
}