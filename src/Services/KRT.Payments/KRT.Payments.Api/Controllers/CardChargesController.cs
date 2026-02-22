using KRT.Payments.Api.Data;
using KRT.Payments.Api.Services;
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
    private readonly ChargePaymentService _paymentService;

    public CardChargesController(PaymentsDbContext db, ChargePaymentService paymentService)
    {
        _db = db;
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCharge([FromBody] CreateCardChargeRequest request, CancellationToken ct)
    {
        if (request.Amount <= 0)
            return BadRequest(new { error = "Valor deve ser maior que zero" });

        VirtualCard? card = null;

        if (request.CardId.HasValue && request.CardId.Value != Guid.Empty)
        {
            card = await _db.VirtualCards.FindAsync(new object[] { request.CardId.Value }, ct);
        }
        else if (request.AccountId.HasValue && request.AccountId.Value != Guid.Empty)
        {
            card = await _db.VirtualCards
                .Where(c => c.AccountId == request.AccountId.Value && c.Status == CardStatus.Active)
                .FirstOrDefaultAsync(ct);
        }
        else
        {
            card = await _db.VirtualCards
                .Where(c => c.Status == CardStatus.Active)
                .FirstOrDefaultAsync(ct);
        }

        if (card == null) return NotFound(new { error = "Cartao nao encontrado" });

        var (allowed, reason) = card.ValidatePurchase(request.Amount, true, false);

        var installments = Math.Clamp(request.Installments ?? 1, 1, 12);
        var installmentAmount = Math.Round(request.Amount / installments, 2);

        if (!allowed)
        {
            var declined = new CardCharge
            {
                Id = Guid.NewGuid(),
                CardId = card.Id,
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
            CardId = card.Id,
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

        // Reduzir limite disponível — valor TOTAL reservado (mesmo parcelado)
        card.AddSpending(request.Amount);
        _db.VirtualCards.Update(card);

        // Cartão de crédito: NÃO debita conta corrente do cliente.
        // Apenas credita o merchant — o débito ocorre quando o cliente paga a fatura.
        var merchantAccountId = ChargePaymentService.MerchantAccountId;
        var merchant = await _db.BankAccounts.FindAsync([merchantAccountId], ct);
        if (merchant != null)
        {
            merchant.Balance += request.Amount;
            merchant.UpdatedAt = DateTime.UtcNow;
            merchant.RowVersion = Guid.NewGuid().ToByteArray();

            _db.StatementEntries.Add(new StatementEntry
            {
                Id = Guid.NewGuid(),
                AccountId = merchant.Id,
                Date = DateTime.UtcNow,
                Type = "Cartao",
                Category = "Receivable",
                Amount = request.Amount,
                Description = $"Cartao credito recebido - {request.Description ?? "Compra cartao credito"}",
                CounterpartyName = "Cliente cartao credito",
                CounterpartyBank = "KRT Bank",
                IsCredit = true,
                CreatedAt = DateTime.UtcNow
            });
        }

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
            cardLast4 = card.Last4Digits,
            spentThisMonth = card.SpentThisMonth,
            remainingLimit = card.AvailableLimit
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

    /// POST /api/v1/cards/charges/{id}/simulate-payment — settle card charge (credit merchant only, NO debit from checking)
    [HttpPost("{id:guid}/simulate-payment")]
    public async Task<IActionResult> SimulatePayment(Guid id, [FromBody] SimulatePaymentRequest? request, CancellationToken ct)
    {
        var charge = await _db.CardCharges.FindAsync(new object[] { id }, ct);
        if (charge == null) return NotFound(new { error = "Cobranca nao encontrada" });

        if (charge.Status != CardChargeStatus.Approved)
            return BadRequest(new { error = $"Cobranca em status {charge.Status}, nao pode ser paga" });

        // Cartão de crédito: NÃO debita conta corrente do cliente.
        // Apenas credita o merchant — o débito ocorre quando o cliente paga a fatura.
        var merchantAccountId = ChargePaymentService.MerchantAccountId;
        var merchant = await _db.BankAccounts.FindAsync([merchantAccountId], ct);
        if (merchant == null)
            return BadRequest(new { error = "Conta do recebedor nao encontrada" });

        merchant.Balance += charge.Amount;
        merchant.UpdatedAt = DateTime.UtcNow;
        merchant.RowVersion = Guid.NewGuid().ToByteArray();

        // Statement: merchant recebe crédito
        _db.StatementEntries.Add(new StatementEntry
        {
            Id = Guid.NewGuid(),
            AccountId = merchant.Id,
            Date = DateTime.UtcNow,
            Type = "Cartao",
            Category = "Receivable",
            Amount = charge.Amount,
            Description = $"Cartao credito recebido - {charge.Description}",
            CounterpartyName = "Cliente cartao credito",
            CounterpartyBank = "KRT Bank",
            IsCredit = true,
            CreatedAt = DateTime.UtcNow
        });

        charge.Status = CardChargeStatus.Settled;
        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            chargeId = charge.Id,
            status = "Settled",
            amount = charge.Amount,
            merchantCredited = charge.Amount
        });
    }
}

public record CreateCardChargeRequest(
    decimal Amount,
    Guid? CardId = null,
    Guid? AccountId = null,
    string? Description = null,
    string? ExternalId = null,
    int? Installments = 1,
    string? MerchantId = null,
    string? WebhookUrl = null);
