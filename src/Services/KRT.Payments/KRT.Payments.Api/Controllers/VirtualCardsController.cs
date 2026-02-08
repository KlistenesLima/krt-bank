using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/cards")]
public class VirtualCardsController : ControllerBase
{
    private readonly ILogger<VirtualCardsController> _logger;

    // In-memory store para demo (em producao seria o DbContext)
    private static readonly ConcurrentDictionary<Guid, List<VirtualCard>> _cards = new();

    public VirtualCardsController(ILogger<VirtualCardsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os cartoes virtuais de uma conta.
    /// </summary>
    [HttpGet("account/{accountId}")]
    [AllowAnonymous]
    public IActionResult GetCards(Guid accountId)
    {
        var cards = _cards.GetOrAdd(accountId, _ => new List<VirtualCard>());
        return Ok(cards.Select(c => MapToDto(c)));
    }

    /// <summary>
    /// Gera um novo cartao virtual.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public IActionResult CreateCard([FromBody] CreateCardRequest request)
    {
        var brand = request.Brand?.ToLower() == "mastercard" ? CardBrand.Mastercard : CardBrand.Visa;
        var card = VirtualCard.Create(request.AccountId, request.HolderName, brand);

        var cards = _cards.GetOrAdd(request.AccountId, _ => new List<VirtualCard>());
        cards.Add(card);

        _logger.LogInformation("Virtual card created: {Last4} for account {AccountId}",
            card.Last4Digits, request.AccountId);

        return Created($"/api/v1/cards/{card.Id}", MapToDto(card, showFull: true));
    }

    /// <summary>
    /// Detalhes de um cartao (com CVV se valido).
    /// </summary>
    [HttpGet("{cardId}")]
    [AllowAnonymous]
    public IActionResult GetCard(Guid cardId)
    {
        var card = FindCard(cardId);
        if (card == null) return NotFound(new { error = "Cartao nao encontrado" });
        return Ok(MapToDto(card, showFull: true));
    }

    /// <summary>
    /// Bloqueia o cartao.
    /// </summary>
    [HttpPost("{cardId}/block")]
    [AllowAnonymous]
    public IActionResult BlockCard(Guid cardId)
    {
        var card = FindCard(cardId);
        if (card == null) return NotFound(new { error = "Cartao nao encontrado" });

        card.Block();
        _logger.LogInformation("Card {Last4} blocked", card.Last4Digits);
        return Ok(new { message = "Cartao bloqueado", card = MapToDto(card) });
    }

    /// <summary>
    /// Desbloqueia o cartao.
    /// </summary>
    [HttpPost("{cardId}/unblock")]
    [AllowAnonymous]
    public IActionResult UnblockCard(Guid cardId)
    {
        var card = FindCard(cardId);
        if (card == null) return NotFound(new { error = "Cartao nao encontrado" });

        card.Unblock();
        _logger.LogInformation("Card {Last4} unblocked", card.Last4Digits);
        return Ok(new { message = "Cartao desbloqueado", card = MapToDto(card) });
    }

    /// <summary>
    /// Cancela o cartao permanentemente.
    /// </summary>
    [HttpDelete("{cardId}")]
    [AllowAnonymous]
    public IActionResult CancelCard(Guid cardId)
    {
        var card = FindCard(cardId);
        if (card == null) return NotFound(new { error = "Cartao nao encontrado" });

        card.Cancel();
        _logger.LogInformation("Card {Last4} cancelled", card.Last4Digits);
        return Ok(new { message = "Cartao cancelado permanentemente" });
    }

    /// <summary>
    /// Gera novo CVV dinamico (valido por 24h).
    /// </summary>
    [HttpPost("{cardId}/rotate-cvv")]
    [AllowAnonymous]
    public IActionResult RotateCvv(Guid cardId)
    {
        var card = FindCard(cardId);
        if (card == null) return NotFound(new { error = "Cartao nao encontrado" });

        var newCvv = card.RotateCvv();
        _logger.LogInformation("CVV rotated for card {Last4}", card.Last4Digits);
        return Ok(new { cvv = newCvv, expiresAt = card.CvvExpiresAt, message = "Novo CVV gerado (valido 24h)" });
    }

    /// <summary>
    /// Atualiza configuracoes do cartao.
    /// </summary>
    [HttpPut("{cardId}/settings")]
    [AllowAnonymous]
    public IActionResult UpdateSettings(Guid cardId, [FromBody] UpdateCardSettingsRequest request)
    {
        var card = FindCard(cardId);
        if (card == null) return NotFound(new { error = "Cartao nao encontrado" });

        if (request.SpendingLimit.HasValue) card.UpdateSpendingLimit(request.SpendingLimit.Value);
        if (request.IsContactless.HasValue) card.ToggleContactless(request.IsContactless.Value);
        if (request.IsOnlinePurchase.HasValue) card.ToggleOnlinePurchase(request.IsOnlinePurchase.Value);
        if (request.IsInternational.HasValue) card.ToggleInternational(request.IsInternational.Value);

        return Ok(new { message = "Configuracoes atualizadas", card = MapToDto(card) });
    }

    private VirtualCard? FindCard(Guid cardId) =>
        _cards.Values.SelectMany(x => x).FirstOrDefault(c => c.Id == cardId);

    private static object MapToDto(VirtualCard c, bool showFull = false) => new
    {
        c.Id,
        c.AccountId,
        cardNumber = showFull ? FormatCardNumber(c.CardNumber) : c.GetMaskedNumber(),
        maskedNumber = c.GetMaskedNumber(),
        c.CardholderName,
        expiration = $"{c.ExpirationMonth}/{c.ExpirationYear[^2..]}",
        cvv = showFull && c.IsCvvValid() ? c.Cvv : "***",
        cvvValid = c.IsCvvValid(),
        cvvExpiresAt = c.CvvExpiresAt,
        c.Last4Digits,
        brand = c.Brand.ToString(),
        status = c.Status.ToString(),
        c.SpendingLimit,
        c.SpentThisMonth,
        remainingLimit = c.SpendingLimit - c.SpentThisMonth,
        settings = new { c.IsContactless, c.IsOnlinePurchase, c.IsInternational }
    };

    private static string FormatCardNumber(string n) =>
        $"{n[..4]} {n[4..8]} {n[8..12]} {n[12..]}";
}

public record CreateCardRequest(Guid AccountId, string HolderName, string? Brand = "Visa");
public record UpdateCardSettingsRequest(decimal? SpendingLimit, bool? IsContactless, bool? IsOnlinePurchase, bool? IsInternational);