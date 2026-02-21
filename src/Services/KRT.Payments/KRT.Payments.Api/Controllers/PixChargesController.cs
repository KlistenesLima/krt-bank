using KRT.Payments.Api.Data;
using KRT.Payments.Api.Services;
using KRT.Payments.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/pix/charges")]
[AllowAnonymous]
public class PixChargesController : ControllerBase
{
    private readonly PaymentsDbContext _db;
    private readonly QrCodeService _qrCodeService;
    private readonly ChargePaymentService _paymentService;

    public PixChargesController(PaymentsDbContext db, QrCodeService qrCodeService, ChargePaymentService paymentService)
    {
        _db = db;
        _qrCodeService = qrCodeService;
        _paymentService = paymentService;
    }

    /// POST /api/v1/pix/charges — create a PIX charge for e-commerce
    [HttpPost]
    public async Task<IActionResult> CreateCharge([FromBody] CreatePixChargeRequest request, CancellationToken ct)
    {
        if (request.Amount <= 0)
            return BadRequest(new { error = "Valor deve ser maior que zero" });

        var txId = Guid.NewGuid().ToString("N")[..25];
        var payload = _qrCodeService.GeneratePixPayload(
            "aurea@krtbank.com.br",
            "AUREA Maison",
            "Sao Paulo",
            request.Amount,
            txId);
        var qrBase64 = _qrCodeService.GenerateQrCodeBase64(payload);

        var charge = new PixCharge
        {
            Id = Guid.NewGuid(),
            ExternalId = request.ExternalId ?? "",
            Amount = request.Amount,
            Description = request.Description ?? "",
            QrCode = payload,
            QrCodeBase64 = qrBase64,
            Status = PixChargeStatus.Pending,
            PayerCpf = request.PayerCpf,
            MerchantId = request.MerchantId,
            WebhookUrl = request.WebhookUrl,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        _db.PixCharges.Add(charge);
        await _db.SaveChangesAsync(ct);

        return Created($"/api/v1/pix/charges/{charge.Id}", new
        {
            chargeId = charge.Id,
            qrCode = charge.QrCode,
            qrCodeBase64 = charge.QrCodeBase64,
            status = charge.Status.ToString(),
            amount = charge.Amount,
            expiresAt = charge.ExpiresAt
        });
    }

    /// GET /api/v1/pix/charges/{id} — get charge status
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCharge(Guid id, CancellationToken ct)
    {
        var charge = await _db.PixCharges.FindAsync(new object[] { id }, ct);
        if (charge == null) return NotFound(new { error = "Cobranca nao encontrada" });

        // Auto-expire
        if (charge.Status == PixChargeStatus.Pending && charge.ExpiresAt < DateTime.UtcNow)
        {
            charge.Status = PixChargeStatus.Expired;
            await _db.SaveChangesAsync(ct);
        }

        return Ok(new
        {
            chargeId = charge.Id,
            externalId = charge.ExternalId,
            amount = charge.Amount,
            status = charge.Status.ToString(),
            paidAt = charge.PaidAt,
            createdAt = charge.CreatedAt,
            expiresAt = charge.ExpiresAt
        });
    }

    /// POST /api/v1/pix/charges/{id}/simulate-payment — real banking payment
    [HttpPost("{id:guid}/simulate-payment")]
    public async Task<IActionResult> SimulatePayment(Guid id, [FromBody] SimulatePaymentRequest? request, CancellationToken ct)
    {
        var charge = await _db.PixCharges.FindAsync(new object[] { id }, ct);
        if (charge == null) return NotFound(new { error = "Cobranca nao encontrada" });

        if (charge.Status != PixChargeStatus.Pending)
            return BadRequest(new { error = $"Cobranca em status {charge.Status}, nao pode ser paga" });

        if (charge.ExpiresAt < DateTime.UtcNow)
        {
            charge.Status = PixChargeStatus.Expired;
            await _db.SaveChangesAsync(ct);
            return BadRequest(new { error = "Cobranca expirada" });
        }

        var result = await _paymentService.PreparePaymentAsync(
            request?.PayerAccountId, charge.PayerCpf, charge.Amount,
            "PIX", $"PIX cobranca - {charge.Description}", "AUREA Maison Joalheria", ct);

        if (!result.Success)
            return BadRequest(new { error = result.Error });

        charge.Status = PixChargeStatus.Confirmed;
        charge.PaidAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        // Call webhook if configured
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
                        status = "Confirmed",
                        paidAt = charge.PaidAt,
                        amount = charge.Amount
                    });
                }
                catch { /* webhook delivery is best-effort */ }
            });
        }

        return Ok(new
        {
            chargeId = charge.Id,
            status = "Confirmed",
            paidAt = charge.PaidAt,
            amount = charge.Amount,
            payerAccountId = result.PayerAccountId,
            newBalance = result.NewBalance
        });
    }

    /// POST /api/v1/pix/charges/find-by-brcode — parse BRCode and find pending charge
    [HttpPost("find-by-brcode")]
    public async Task<IActionResult> FindByBrCode([FromBody] FindByBrCodeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.BrCode))
            return BadRequest(new { error = "Codigo BRCode e obrigatorio" });

        var parser = new BRCodeParser();
        var parsed = parser.Parse(request.BrCode);

        if (!parsed.IsValid)
            return BadRequest(new { error = "Codigo PIX invalido" });

        // Try to find a pending charge matching this BRCode
        var query = _db.PixCharges
            .Where(c => c.Status == PixChargeStatus.Pending && c.ExpiresAt > DateTime.UtcNow);

        if (!string.IsNullOrEmpty(parsed.TxId))
            query = query.Where(c => c.QrCode.Contains(parsed.TxId));
        else
            query = query.Where(c => c.Amount == parsed.Amount);

        var charge = await query
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);

        return Ok(new
        {
            chargeId = charge?.Id,
            chargeFound = charge != null,
            status = charge?.Status.ToString(),
            amount = parsed.Amount,
            description = charge?.Description ?? "",
            pixKey = parsed.PixKey,
            merchantName = parsed.MerchantName,
            merchantCity = parsed.MerchantCity,
            txId = parsed.TxId,
            externalId = charge?.ExternalId,
            expiresAt = charge?.ExpiresAt
        });
    }

    /// POST /api/v1/pix/pay-brcode — pay via BRCode (charge or P2P)
    [HttpPost("/api/v1/pix/pay-brcode")]
    public async Task<IActionResult> PayBrCode([FromBody] PayBrCodeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.BrCode))
            return BadRequest(new { error = "Codigo BRCode e obrigatorio" });

        var parser = new BRCodeParser();
        var parsed = parser.Parse(request.BrCode);

        if (!parsed.IsValid)
            return BadRequest(new { error = "Codigo PIX invalido" });

        // 1. Try to find a pending charge
        PixCharge? charge = null;
        var chargeQuery = _db.PixCharges
            .Where(c => c.Status == PixChargeStatus.Pending && c.ExpiresAt > DateTime.UtcNow);

        if (!string.IsNullOrEmpty(parsed.TxId))
            chargeQuery = chargeQuery.Where(c => c.QrCode.Contains(parsed.TxId));
        else
            chargeQuery = chargeQuery.Where(c => c.Amount == parsed.Amount);

        charge = await chargeQuery.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(ct);

        if (charge != null)
        {
            // Pay the existing charge via ChargePaymentService
            var chargeResult = await _paymentService.PreparePaymentAsync(
                request.PayerAccountId, null, charge.Amount,
                "PIX", $"PIX Copia e Cola - {charge.Description}", parsed.MerchantName, ct);

            if (!chargeResult.Success)
                return BadRequest(new { error = chargeResult.Error });

            charge.Status = PixChargeStatus.Confirmed;
            charge.PaidAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            return Ok(new
            {
                success = true,
                amount = charge.Amount,
                merchantName = parsed.MerchantName,
                newBalance = chargeResult.NewBalance,
                payerAccountId = chargeResult.PayerAccountId,
                chargeId = charge.Id
            });
        }

        // 2. No charge found — P2P transfer via PIX key
        var pixKeyRecord = await _db.BankPixKeys
            .FirstOrDefaultAsync(k => k.KeyValue == parsed.PixKey && k.IsActive, ct);

        if (pixKeyRecord == null)
            return BadRequest(new { error = "Chave PIX nao encontrada neste banco" });

        var receiverAccount = await _db.BankAccounts.FindAsync([pixKeyRecord.AccountId], ct);
        if (receiverAccount == null)
            return BadRequest(new { error = "Conta do recebedor nao encontrada" });

        // Find payer account
        BankAccount? payer = null;
        if (request.PayerAccountId.HasValue)
            payer = await _db.BankAccounts.FindAsync([request.PayerAccountId.Value], ct);
        if (payer == null)
            payer = await _db.BankAccounts
                .Where(a => a.Status == "Active" && a.Id != ChargePaymentService.MerchantAccountId)
                .OrderByDescending(a => a.Balance)
                .FirstOrDefaultAsync(ct);
        if (payer == null)
            return BadRequest(new { error = "Conta pagadora nao encontrada" });

        if (payer.Id == receiverAccount.Id)
            return BadRequest(new { error = "Nao e possivel enviar PIX para sua propria conta" });

        if (payer.Balance < parsed.Amount)
            return BadRequest(new { error = $"Saldo insuficiente (disponivel: R$ {payer.Balance:N2})" });

        // Debit payer
        payer.Balance -= parsed.Amount;
        payer.UpdatedAt = DateTime.UtcNow;
        payer.RowVersion = Guid.NewGuid().ToByteArray();

        // Credit receiver
        receiverAccount.Balance += parsed.Amount;
        receiverAccount.UpdatedAt = DateTime.UtcNow;
        receiverAccount.RowVersion = Guid.NewGuid().ToByteArray();

        // Statement entries
        _db.StatementEntries.Add(new StatementEntry
        {
            Id = Guid.NewGuid(),
            AccountId = payer.Id,
            Date = DateTime.UtcNow,
            Type = "PIX",
            Category = "Payment",
            Amount = parsed.Amount,
            Description = $"PIX Copia e Cola para {receiverAccount.CustomerName}",
            CounterpartyName = receiverAccount.CustomerName,
            CounterpartyBank = "KRT Bank",
            IsCredit = false,
            CreatedAt = DateTime.UtcNow
        });

        _db.StatementEntries.Add(new StatementEntry
        {
            Id = Guid.NewGuid(),
            AccountId = receiverAccount.Id,
            Date = DateTime.UtcNow,
            Type = "PIX",
            Category = "Receivable",
            Amount = parsed.Amount,
            Description = $"PIX recebido de {payer.CustomerName}",
            CounterpartyName = payer.CustomerName,
            CounterpartyBank = "KRT Bank",
            IsCredit = true,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            success = true,
            amount = parsed.Amount,
            merchantName = receiverAccount.CustomerName,
            newBalance = payer.Balance,
            payerAccountId = payer.Id
        });
    }
}

public record CreatePixChargeRequest(
    decimal Amount,
    string? Description = null,
    string? ExternalId = null,
    string? PayerCpf = null,
    string? MerchantId = null,
    string? WebhookUrl = null);

public record FindByBrCodeRequest(
    string? BrCode = null,
    string? PixKey = null,
    decimal Amount = 0,
    string? TxId = null);

public record PayBrCodeRequest(
    string BrCode,
    Guid? PayerAccountId = null);

public record SimulatePaymentRequest(
    Guid? PayerAccountId = null,
    string? PayerDocument = null);
