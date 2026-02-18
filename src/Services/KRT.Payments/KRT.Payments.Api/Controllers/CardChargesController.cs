using KRT.Payments.Api.Data;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/cards/charges")]
[AllowAnonymous]
public class CardChargesController : ControllerBase
{
    private readonly PaymentsDbContext _db;

    public CardChargesController(PaymentsDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> CreateCharge([FromBody] CreateCardChargeRequest request, CancellationToken ct)
    {
        if (request.Amount <= 0)
            return BadRequest(new { error = "Valor deve ser maior que zero" });

        var card = await _db.VirtualCards.FindAsync(new object[] { request.CardId }, ct);
        if (card == null) return NotFound(new { error = "Cartao nao encontrado" });

        var (allowed, reason) = card.ValidatePurchase(request.Amount, true, false);

        var installments = Math.Clamp(request.Installments ?? 1, 1, 12);
        var installmentAmount = Math.Round(request.Amount / installments, 2);

        if (!allowed)
        {
            var declined = new CardCharge
            {
                Id = Guid.NewGuid(),
                CardId = request.CardId,
                ExternalId = request.ExternalId ?? "",
                Amount = request.Amount,
                Description = request.Description ?? "",
                Status = CardChargeStatus.Declined,
                AuthorizationCode = "",
                Installments = installments,
                InstallmentAmount = installmentAmount,
                MerchantId = request.MerchantId,
                WebhookUrl = request.WebhookUrl,
                CreatedAt = DateTime.UtcNow
            };
            _db.CardCharges.Add(declined);
            await _db.SaveChangesAsync(ct);

            return Ok(new
            {
                chargeId = declined.Id,
                status = "Declined",
                reason,
                installments,
                installmentAmount
            });
        }

        var authCode = $"AUTH{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var charge = new CardCharge
        {
            Id = Guid.NewGuid(),
            CardId = request.CardId,
            ExternalId = request.ExternalId ?? "",
            Amount = request.Amount,
            Description = request.Description ?? "",
            Status = CardChargeStatus.Approved,
            AuthorizationCode = authCode,
            Installments = installments,
            InstallmentAmount = installmentAmount,
            MerchantId = request.MerchantId,
            WebhookUrl = request.WebhookUrl,
            CreatedAt = DateTime.UtcNow
        };

        _db.CardCharges.Add(charge);
        await _db.SaveChangesAsync(ct);

        if (!string.IsNullOrEmpty(charge.WebhookUrl))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                    await http.PostAsJsonAsync(charge.WebhookUrl, new
                    {
                        chargeId = charge.Id,
                        externalId = charge.ExternalId,
                        status = "Approved",
                        amount = charge.Amount,
                        authorizationCode = charge.AuthorizationCode,
                        method = "credit_card"
                    });
                }
                catch { }
            });
        }

        return Created($"/api/v1/cards/charges/{charge.Id}", new
        {
            chargeId = charge.Id,
            status = "Approved",
            authorizationCode = authCode,
            installments,
            installmentAmount,
            amount = charge.Amount,
            cardLast4 = card.Last4Digits
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCharge(Guid id, CancellationToken ct)
    {
        var charge = await _db.CardCharges.FindAsync(new object[] { id }, ct);
        if (charge == null) return NotFound(new { error = "Cobranca nao encontrada" });

        return Ok(new
        {
            chargeId = charge.Id,
            cardId = charge.CardId,
            externalId = charge.ExternalId,
            amount = charge.Amount,
            status = charge.Status.ToString(),
            authorizationCode = charge.AuthorizationCode,
            installments = charge.Installments,
            installmentAmount = charge.InstallmentAmount,
            createdAt = charge.CreatedAt
        });
    }

    [HttpGet("card/{cardId:guid}/statement")]
    public async Task<IActionResult> GetStatement(Guid cardId, CancellationToken ct)
    {
        var card = await _db.VirtualCards.FindAsync(new object[] { cardId }, ct);
        if (card == null) return NotFound(new { error = "Cartao nao encontrado" });

        var charges = await _db.CardCharges
            .Where(c => c.CardId == cardId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        var totalDue = charges
            .Where(c => c.Status == CardChargeStatus.Approved)
            .Sum(c => c.Amount);

        return Ok(new
        {
            cardId,
            cardLast4 = card.Last4Digits,
            totalDue,
            availableLimit = card.SpendingLimit - card.SpentThisMonth,
            spendingLimit = card.SpendingLimit,
            charges = charges.Select(c => new
            {
                c.Id,
                c.Amount,
                c.Description,
                status = c.Status.ToString(),
                c.Installments,
                c.InstallmentAmount,
                c.AuthorizationCode,
                c.CreatedAt
            })
        });
    }
}

public record CreateCardChargeRequest(
    Guid CardId,
    decimal Amount,
    string? Description = null,
    string? ExternalId = null,
    int? Installments = 1,
    string? MerchantId = null,
    string? WebhookUrl = null);
