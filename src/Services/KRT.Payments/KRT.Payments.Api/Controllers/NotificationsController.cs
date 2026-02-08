using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/notifications")]
public class NotificationsController : ControllerBase
{
    private static readonly ConcurrentDictionary<Guid, List<NotificationItem>> _store = new();

    private static List<NotificationItem> GetOrCreate(Guid accountId)
    {
        return _store.GetOrAdd(accountId, _ => GenerateSeedNotifications(accountId));
    }

    /// <summary>
    /// Lista notificacoes com filtros.
    /// </summary>
    [HttpGet("{accountId}")]
    [AllowAnonymous]
    public IActionResult GetNotifications(
        Guid accountId,
        [FromQuery] bool? unreadOnly,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var items = GetOrCreate(accountId).AsEnumerable();

        if (unreadOnly == true)
            items = items.Where(n => !n.IsRead);
        if (!string.IsNullOrEmpty(category))
            items = items.Where(n => n.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

        var sorted = items.OrderByDescending(n => n.CreatedAt).ToList();
        var total = sorted.Count;
        var unread = GetOrCreate(accountId).Count(n => !n.IsRead);
        var paged = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new
        {
            accountId,
            totalNotifications = total,
            unreadCount = unread,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)total / pageSize),
            items = paged
        });
    }

    /// <summary>
    /// Contador de nao lidas (para badge).
    /// </summary>
    [HttpGet("{accountId}/unread-count")]
    [AllowAnonymous]
    public IActionResult GetUnreadCount(Guid accountId)
    {
        var count = GetOrCreate(accountId).Count(n => !n.IsRead);
        return Ok(new { accountId, unreadCount = count });
    }

    /// <summary>
    /// Marca uma notificacao como lida.
    /// </summary>
    [HttpPost("{accountId}/read/{notificationId}")]
    [AllowAnonymous]
    public IActionResult MarkAsRead(Guid accountId, Guid notificationId)
    {
        var items = GetOrCreate(accountId);
        var item = items.FirstOrDefault(n => n.Id == notificationId);
        if (item == null) return NotFound(new { error = "Notificacao nao encontrada" });

        item.IsRead = true;
        item.ReadAt = DateTime.UtcNow;
        return Ok(new { message = "Marcada como lida", notificationId });
    }

    /// <summary>
    /// Marca todas como lidas.
    /// </summary>
    [HttpPost("{accountId}/read-all")]
    [AllowAnonymous]
    public IActionResult MarkAllAsRead(Guid accountId)
    {
        var items = GetOrCreate(accountId);
        var count = 0;
        foreach (var n in items.Where(n => !n.IsRead))
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
            count++;
        }
        return Ok(new { message = $"{count} notificacoes marcadas como lidas" });
    }

    /// <summary>
    /// Deleta uma notificacao.
    /// </summary>
    [HttpDelete("{accountId}/{notificationId}")]
    [AllowAnonymous]
    public IActionResult Delete(Guid accountId, Guid notificationId)
    {
        var items = GetOrCreate(accountId);
        var removed = items.RemoveAll(n => n.Id == notificationId);
        return removed > 0
            ? Ok(new { message = "Notificacao removida" })
            : NotFound(new { error = "Notificacao nao encontrada" });
    }

    /// <summary>
    /// Cria notificacao manual (para testes/demo).
    /// </summary>
    [HttpPost("{accountId}")]
    [AllowAnonymous]
    public IActionResult CreateNotification(Guid accountId, [FromBody] CreateNotificationRequest request)
    {
        var item = new NotificationItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Message = request.Message,
            Category = request.Category ?? "sistema",
            Icon = GetIconForCategory(request.Category ?? "sistema"),
            Priority = request.Priority ?? "normal",
            CreatedAt = DateTime.UtcNow
        };

        GetOrCreate(accountId).Insert(0, item);
        return Created("", new { item.Id, message = "Notificacao criada" });
    }

    private static string GetIconForCategory(string category) => category.ToLower() switch
    {
        "pix" => "⚡",
        "transferencia" => "🏦",
        "cartao" => "💳",
        "seguranca" => "🔒",
        "limite" => "📊",
        "promocao" => "🎉",
        _ => "🔔"
    };

    private static List<NotificationItem> GenerateSeedNotifications(Guid accountId)
    {
        var rng = new Random(accountId.GetHashCode());
        var notifications = new List<(string title, string msg, string cat, string icon, string priority, int hoursAgo, bool read)>
        {
            ("Pix recebido", "Voce recebeu R$ 1.500,00 de Maria Silva via Pix", "pix", "⚡", "normal", 1, false),
            ("Pix enviado", "Pix de R$ 250,00 para Joao Santos realizado com sucesso", "pix", "⚡", "normal", 3, false),
            ("Cartao virtual criado", "Seu novo cartao virtual Visa foi criado com sucesso", "cartao", "💳", "normal", 5, false),
            ("Alerta de seguranca", "Novo login detectado em dispositivo Windows. Foi voce?", "seguranca", "🔒", "high", 8, false),
            ("Limite atualizado", "Seu limite Pix noturno foi atualizado para R$ 2.000,00", "limite", "📊", "normal", 12, true),
            ("Pix agendado executado", "Pix recorrente de R$ 800,00 para Aluguel foi executado", "pix", "⚡", "normal", 24, true),
            ("Compra no cartao", "Compra de R$ 89,90 em Supermercado ABC aprovada", "cartao", "💳", "normal", 30, true),
            ("Extrato disponivel", "Seu extrato do mes de janeiro esta disponivel para download", "sistema", "📄", "low", 48, true),
            ("TED recebida", "Voce recebeu TED de R$ 3.200,00 de Empresa XYZ", "transferencia", "🏦", "normal", 52, true),
            ("Atualizacao de seguranca", "Sua senha foi alterada com sucesso", "seguranca", "🔒", "high", 72, true),
            ("Pix devolvido", "Pix de R$ 50,00 foi devolvido. Motivo: chave incorreta", "pix", "⚡", "normal", 96, true),
            ("Novidade KRT Bank", "Agora voce pode agendar Pix recorrentes! Confira", "promocao", "🎉", "low", 120, true),
            ("Cartao bloqueado", "Seu cartao **** 4532 foi bloqueado por tentativa suspeita", "seguranca", "🔒", "high", 168, true),
            ("Fatura disponivel", "Sua fatura do cartao virtual esta disponivel: R$ 1.247,30", "cartao", "💳", "normal", 200, true),
            ("Bem-vindo ao KRT Bank!", "Sua conta foi criada com sucesso. Explore nossos servicos!", "sistema", "🎉", "normal", 500, true)
        };

        return notifications.Select(n => new NotificationItem
        {
            Id = Guid.NewGuid(),
            Title = n.title,
            Message = n.msg,
            Category = n.cat,
            Icon = n.icon,
            Priority = n.priority,
            IsRead = n.read,
            CreatedAt = DateTime.UtcNow.AddHours(-n.hoursAgo),
            ReadAt = n.read ? DateTime.UtcNow.AddHours(-n.hoursAgo + 1) : null
        }).ToList();
    }
}

public class NotificationItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Category { get; set; } = "sistema";
    public string Icon { get; set; } = "🔔";
    public string Priority { get; set; } = "normal";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public record CreateNotificationRequest(
    string Title,
    string Message,
    string? Category = null,
    string? Priority = null);