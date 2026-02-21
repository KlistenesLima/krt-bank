using KRT.Payments.Api.Data;
using KRT.Payments.Api.Services;
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

    /// GET /api/v1/cards/{cardId}/bill — consultar fatura atual do cartão
    [HttpGet("{cardId}/bill")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBill(Guid cardId, CancellationToken ct)
    {
        var card = await _db.VirtualCards.FindAsync([cardId], ct);
        if (card == null) return NotFound(new { error = "Cartao nao encontrado" });

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var charges = await _db.CardCharges
            .Where(c => c.CardId == cardId
                && (c.Status == CardChargeStatus.Approved || c.Status == CardChargeStatus.Settled)
                && c.CreatedAt >= startOfMonth)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        // Buscar pagamentos de fatura do mês (registrados como StatementEntry)
        var payments = await _db.StatementEntries
            .Where(s => s.AccountId == card.AccountId
                && s.Type == "PagamentoFatura"
                && s.IsCredit == false
                && s.Date >= startOfMonth)
            .OrderByDescending(s => s.Date)
            .ToListAsync(ct);

        var currentBill = card.SpentThisMonth;
        // Fechamento = último dia do mês atual
        var now = DateTime.UtcNow;
        var closingDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 0, 0, 0, DateTimeKind.Utc);
        // Vencimento = 10 dias após fechamento
        var dueDate = closingDate.AddDays(10);

        return Ok(new
        {
            cardId = card.Id,
            last4Digits = card.Last4Digits,
            brand = card.Brand.ToString(),
            spendingLimit = card.SpendingLimit,
            availableLimit = card.AvailableLimit,
            currentBill,
            minimumPayment = Math.Round(currentBill * 0.10m, 2),
            closingDate = closingDate.ToString("yyyy-MM-dd"),
            dueDate = dueDate.ToString("yyyy-MM-dd"),
            charges = charges.Select(c => new
            {
                c.Id, c.Description, c.Amount, c.Installments, c.InstallmentAmount,
                status = c.Status.ToString(), c.CreatedAt
            }),
            payments = payments.Select(p => new
            {
                p.Id, p.Description, p.Amount, p.Date
            })
        });
    }

    /// POST /api/v1/cards/{cardId}/pay-bill — pagar fatura (total, parcial ou adiantamento)
    [HttpPost("{cardId}/pay-bill")]
    [AllowAnonymous]
    public async Task<IActionResult> PayBill(Guid cardId, [FromBody] PayBillRequest request, CancellationToken ct)
    {
        var card = await _db.VirtualCards.FindAsync([cardId], ct);
        if (card == null) return NotFound(new { error = "Cartao nao encontrado" });

        if (card.SpentThisMonth == 0)
            return BadRequest(new { error = "Sem fatura pendente" });

        if (request.Amount <= 0)
            return BadRequest(new { error = "Valor deve ser maior que zero" });

        if (request.Amount > card.SpentThisMonth)
            return BadRequest(new { error = $"Valor excede fatura pendente (R$ {card.SpentThisMonth:N2})" });

        // Validar pagamento mínimo (10%) — só valida se NÃO é pagamento total
        var minimumPayment = Math.Round(card.SpentThisMonth * 0.10m, 2);
        if (request.Amount < card.SpentThisMonth && request.Amount < minimumPayment)
            return BadRequest(new { error = $"Valor minimo de pagamento: R$ {minimumPayment:N2} (10% da fatura)" });

        // Buscar conta do pagador
        BankAccount? payer = null;
        if (request.PayerAccountId.HasValue)
            payer = await _db.BankAccounts.FindAsync([request.PayerAccountId.Value], ct);

        if (payer == null)
            payer = await _db.BankAccounts.FindAsync([card.AccountId], ct);

        if (payer == null)
            return BadRequest(new { error = "Conta pagadora nao encontrada" });

        if (payer.Balance < request.Amount)
            return BadRequest(new { error = $"Saldo insuficiente (disponivel: R$ {payer.Balance:N2})" });

        // Transação atômica: debitar conta + restaurar limite
        payer.Balance -= request.Amount;
        payer.UpdatedAt = DateTime.UtcNow;
        payer.RowVersion = Guid.NewGuid().ToByteArray();

        card.ReduceSpending(request.Amount);

        var description = request.EarlyPayment == true
            ? $"Adiantamento fatura cartao final {card.Last4Digits}"
            : $"Pagamento fatura cartao final {card.Last4Digits}";

        _db.StatementEntries.Add(new StatementEntry
        {
            Id = Guid.NewGuid(),
            AccountId = payer.Id,
            Date = DateTime.UtcNow,
            Type = "PagamentoFatura",
            Category = "Payment",
            Amount = request.Amount,
            Description = description,
            CounterpartyName = $"Cartao *{card.Last4Digits}",
            CounterpartyBank = "KRT Bank",
            IsCredit = false,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            cardId = card.Id,
            amountPaid = request.Amount,
            remainingBill = card.SpentThisMonth,
            availableLimit = card.AvailableLimit,
            accountBalance = payer.Balance,
            description
        });
    }

    private static string FormatCardNumber(string n) => $"{n[..4]} {n[4..8]} {n[8..12]} {n[12..]}";
}

public record CreateCardRequest(Guid AccountId, string HolderName, string? Brand = "Visa");
public record UpdateCardSettingsRequest(decimal? SpendingLimit, bool? IsContactless, bool? IsOnlinePurchase, bool? IsInternational);
public record PayBillRequest(decimal Amount, Guid? PayerAccountId = null, bool? EarlyPayment = false);
