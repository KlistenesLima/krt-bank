using KRT.Payments.Api.Data;
using KRT.Payments.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly PaymentsDbContext _db;
    public NotificationsController(PaymentsDbContext db) => _db = db;

    private async Task EnsureSeed(Guid accountId)
    {
        if (await _db.Notifications.AnyAsync(n => n.AccountId == accountId)) return;
        _db.Notifications.AddRange(
            Notification.Create(accountId, "Pix Recebido", "Voce recebeu R$ 1.500,00 de Maria Silva", "pix", "info"),
            Notification.Create(accountId, "Cartao Criado", "Seu cartao virtual Visa foi criado com sucesso", "cartao", "success"),
            Notification.Create(accountId, "Alerta de Seguranca", "Detectamos um login de novo dispositivo", "seguranca", "warning"),
            Notification.Create(accountId, "Meta Atingida", "Parabens! Voce atingiu 50% da meta Viagem Europa", "meta", "success"),
            Notification.Create(accountId, "Boleto Vencendo", "Seu boleto de R$ 350,00 vence amanha", "boleto", "warning")
        );
        await _db.SaveChangesAsync();
    }

    [HttpGet("{accountId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(Guid accountId, [FromQuery] bool? unreadOnly, [FromQuery] string? category)
    {
        await EnsureSeed(accountId);
        var query = _db.Notifications.Where(n => n.AccountId == accountId);
        if (unreadOnly == true) query = query.Where(n => !n.IsRead);
        if (!string.IsNullOrEmpty(category)) query = query.Where(n => n.Category == category);
        return Ok(await query.OrderByDescending(n => n.CreatedAt).ToListAsync());
    }

    [HttpGet("{accountId}/unread-count")]
    [AllowAnonymous]
    public async Task<IActionResult> UnreadCount(Guid accountId)
    {
        await EnsureSeed(accountId);
        var count = await _db.Notifications.CountAsync(n => n.AccountId == accountId && !n.IsRead);
        return Ok(new { count });
    }

    [HttpPost("{accountId}/{id}/read")]
    [AllowAnonymous]
    public async Task<IActionResult> MarkRead(Guid accountId, Guid id)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.AccountId == accountId);
        if (n == null) return NotFound();
        n.MarkAsRead();
        await _db.SaveChangesAsync();
        return Ok(new { message = "Lida" });
    }

    [HttpPost("{accountId}/read-all")]
    [AllowAnonymous]
    public async Task<IActionResult> ReadAll(Guid accountId)
    {
        var unread = await _db.Notifications.Where(n => n.AccountId == accountId && !n.IsRead).ToListAsync();
        foreach (var n in unread) n.MarkAsRead();
        await _db.SaveChangesAsync();
        return Ok(new { message = $"{unread.Count} marcadas como lidas" });
    }

    [HttpDelete("{accountId}/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> Delete(Guid accountId, Guid id)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.AccountId == accountId);
        if (n == null) return NotFound();
        _db.Notifications.Remove(n);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Removida" });
    }

    [HttpPost("{accountId}")]
    [AllowAnonymous]
    public async Task<IActionResult> Create(Guid accountId, [FromBody] CreateNotificationRequest req)
    {
        var n = Notification.Create(accountId, req.Title, req.Message, req.Category ?? "geral", req.Severity ?? "info");
        _db.Notifications.Add(n);
        await _db.SaveChangesAsync();
        return Created("", new { n.Id });
    }
}

public record CreateNotificationRequest(string Title, string Message, string? Category, string? Severity);