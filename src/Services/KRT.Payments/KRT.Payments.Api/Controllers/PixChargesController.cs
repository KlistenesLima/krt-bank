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

    public PixChargesController(PaymentsDbContext db, QrCodeService qrCodeService)
    {
        _db = db;
        _qrCodeService = qrCodeService;
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

    /// POST /api/v1/pix/charges/{id}/simulate-payment — simulate payment (for demo)
    [HttpPost("{id:guid}/simulate-payment")]
    public async Task<IActionResult> SimulatePayment(Guid id, CancellationToken ct)
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
            amount = charge.Amount
        });
    }

    /// POST /api/v1/pix/charges/find-by-brcode — find pending charge by BRCode data
    [HttpPost("find-by-brcode")]
    public async Task<IActionResult> FindByBrCode([FromBody] FindByBrCodeRequest request, CancellationToken ct)
    {
        if (request.Amount <= 0)
            return BadRequest(new { error = "Valor invalido" });

        var query = _db.PixCharges
            .Where(c => c.Status == PixChargeStatus.Pending && c.ExpiresAt > DateTime.UtcNow);

        if (!string.IsNullOrEmpty(request.TxId))
            query = query.Where(c => c.QrCode.Contains(request.TxId));
        else
            query = query.Where(c => c.Amount == request.Amount);

        var charge = await query
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (charge == null)
            return NotFound(new { error = "Cobranca nao encontrada" });

        return Ok(new
        {
            chargeId = charge.Id,
            status = charge.Status.ToString(),
            amount = charge.Amount,
            description = charge.Description,
            externalId = charge.ExternalId,
            expiresAt = charge.ExpiresAt
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
    string? PixKey = null,
    decimal Amount = 0,
    string? TxId = null);
