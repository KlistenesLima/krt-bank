using KRT.Payments.Api.Data;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/cards")]
public class VirtualCardsController : ControllerBase
{
    private readonly PaymentsDbContext _db;
    private readonly ILogger<VirtualCardsController> _logger;

    public VirtualCardsController(PaymentsDbContext db, ILogger<VirtualCardsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet("account/{accountId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCards(Guid accountId)
    {
        var cards = await _db.VirtualCards.Where(c => c.AccountId == accountId).ToListAsync();
        return Ok(cards.Select(c => MapToDto(c)));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateCard([FromBody] CreateCardRequest request)
    {
        var brand = request.Brand?.ToLower() == "mastercard" ? CardBrand.Mastercard : CardBrand.Visa;
        var card = VirtualCard.Create(request.AccountId, request.HolderName, brand);
        _db.VirtualCards.Add(card);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Virtual card created: {Last4} for account {AccountId}", card.Last4Digits, request.AccountId);
        return Created($"/api/v1/cards/{card.Id}", MapToDto(card, showFull: true));
    }

    [HttpGet("{cardId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCard(Guid cardId)
    {
        var card = await _db.VirtualCards.FindAsync(cardId);
        if (card == null) return NotFound(new { error = "Cartao nao encontrado" });
        return Ok(MapToDto(card, showFull: true));
    }

    [HttpPost("{cardId}/block")]
    [AllowAnonymous]
    public async Task<IActionResult> BlockCard(Guid cardId)
    {
        var card = await _db.VirtualCards.FindAsync(cardId);
        if (card == null) return NotFound(new { error = "Cartao nao encontrado" });
        card.Block(); await _db.SaveChangesAsync();
        _logger.LogInformation("Card {Last4} blocked", card.Last4Digits);
        return Ok(new { message = "Cartao bloqueado", card = MapToDto(card) });
    }

    [HttpPost("{cardId}/unblock")]
    [AllowAnonymous]
    public async Task<IActionResult> UnblockCard(Guid cardId)
    {
        var card = await _db.VirtualCards.FindAsync(cardId);
        if (card == null) return NotFound(new { error = "Cartao nao encontrado" });
        card.Unblock(); await _db.SaveChangesAsync();
        _logger.LogInformation("Card {Last4} unblocked", card.Last4Digits);
        return Ok(new { message = "Cartao desbloqueado", card = MapToDto(card) });
    }

    [HttpDelete("{cardId}")]
    [AllowAnonymous]
    public async Task<IActionResult> CancelCard(Guid cardId)
    {
        var card = await _db.VirtualCards.FindAsync(cardId);
        if (card == null) return NotFound(new { error = "Cartao nao encontrado" });
        card.Cancel(); await _db.SaveChangesAsync();
        _logger.LogInformation("Card {Last4} cancelled", card.Last4Digits);
        return Ok(new { message = "Cartao cancelado permanentemente" });
    }

    [HttpPost("{cardId}/rotate-cvv")]
    [AllowAnonymous]
    public async Task<IActionResult> RotateCvv(Guid cardId)
    {
        var card = await _db.VirtualCards.FindAsync(cardId);
        if (card == null) return NotFound(new { error = "Cartao nao encontrado" });
        var newCvv = card.RotateCvv(); await _db.SaveChangesAsync();
        _logger.LogInformation("CVV rotated for card {Last4}", card.Last4Digits);
        return Ok(new { cvv = newCvv, expiresAt = card.CvvExpiresAt, message = "Novo CVV gerado (valido 24h)" });
    }

    [HttpPut("{cardId}/settings")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateSettings(Guid cardId, [FromBody] UpdateCardSettingsRequest request)
    {
        var card = await _db.VirtualCards.FindAsync(cardId);
        if (card == null) return NotFound(new { error = "Cartao nao encontrado" });
        if (request.SpendingLimit.HasValue) card.UpdateSpendingLimit(request.SpendingLimit.Value);
        if (request.IsContactless.HasValue) card.ToggleContactless(request.IsContactless.Value);
        if (request.IsOnlinePurchase.HasValue) card.ToggleOnlinePurchase(request.IsOnlinePurchase.Value);
        if (request.IsInternational.HasValue) card.ToggleInternational(request.IsInternational.Value);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Configuracoes atualizadas", card = MapToDto(card) });
    }

    private static object MapToDto(VirtualCard c, bool showFull = false) => new
    {
        c.Id, c.AccountId,
        cardNumber = showFull ? FormatCardNumber(c.CardNumber) : c.GetMaskedNumber(),
        maskedNumber = c.GetMaskedNumber(), c.CardholderName,
        expiration = $"{c.ExpirationMonth}/{c.ExpirationYear[^2..]}",
        cvv = showFull && c.IsCvvValid() ? c.Cvv : "***",
        cvvValid = c.IsCvvValid(), cvvExpiresAt = c.CvvExpiresAt, c.Last4Digits,
        brand = c.Brand.ToString(), status = c.Status.ToString(),
        c.SpendingLimit, c.SpentThisMonth, remainingLimit = c.SpendingLimit - c.SpentThisMonth,
        settings = new { c.IsContactless, c.IsOnlinePurchase, c.IsInternational }
    };

    private static string FormatCardNumber(string n) => $"{n[..4]} {n[4..8]} {n[8..12]} {n[12..]}";
}

public record CreateCardRequest(Guid AccountId, string HolderName, string? Brand = "Visa");
public record UpdateCardSettingsRequest(decimal? SpendingLimit, bool? IsContactless, bool? IsOnlinePurchase, bool? IsInternational);
