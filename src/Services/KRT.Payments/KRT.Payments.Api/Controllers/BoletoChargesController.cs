using KRT.Payments.Api.Data;
using KRT.Payments.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/boletos/charges")]
[AllowAnonymous]
public class BoletoChargesController : ControllerBase
{
    private readonly PaymentsDbContext _db;

    public BoletoChargesController(PaymentsDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> CreateCharge([FromBody] CreateBoletoChargeRequest request, CancellationToken ct)
    {
        if (request.Amount <= 0)
            return BadRequest(new { error = "Valor deve ser maior que zero" });

        var rng = new Random();
        var barcode = $"23793{rng.Next(10000, 99999):D5}{rng.Next(10000, 99999):D5}{rng.Next(100000, 999999):D6}{rng.Next(10000, 99999):D5}{rng.Next(100000, 999999):D6}{rng.Next(1, 9)}{rng.Next(10000000, 99999999):D8}";
        var digitableLine = $"{barcode[..5]}.{barcode[5..10]} {barcode[10..15]}.{barcode[15..21]} {barcode[21..26]}.{barcode[26..32]} {barcode[32]} {barcode[33..]}";

        var dueDate = request.DueDate ?? DateTime.UtcNow.AddDays(3);

        var charge = new BoletoCharge
        {
            Id = Guid.NewGuid(),
            ExternalId = request.ExternalId ?? "",
            Amount = request.Amount,
            Description = request.Description ?? "",
            Barcode = barcode,
            DigitableLine = digitableLine,
            Status = BoletoChargeStatus.Pending,
            PayerCpf = request.PayerCpf,
            PayerName = request.PayerName,
            MerchantId = request.MerchantId,
            WebhookUrl = request.WebhookUrl,
            DueDate = dueDate,
            CreatedAt = DateTime.UtcNow
        };

        _db.BoletoCharges.Add(charge);
        await _db.SaveChangesAsync(ct);

        return Created($"/api/v1/boletos/charges/{charge.Id}", new
        {
            chargeId = charge.Id,
            barcode = charge.Barcode,
            digitableLine = charge.DigitableLine,
            status = charge.Status.ToString(),
            amount = charge.Amount,
            dueDate = charge.DueDate
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCharge(Guid id, CancellationToken ct)
    {
        var charge = await _db.BoletoCharges.FindAsync(new object[] { id }, ct);
        if (charge == null) return NotFound(new { error = "Cobranca nao encontrada" });

        if (charge.Status == BoletoChargeStatus.Pending && charge.DueDate < DateTime.UtcNow)
        {
            charge.Status = BoletoChargeStatus.Expired;
            await _db.SaveChangesAsync(ct);
        }

        return Ok(new
        {
            chargeId = charge.Id,
            externalId = charge.ExternalId,
            amount = charge.Amount,
            status = charge.Status.ToString(),
            barcode = charge.Barcode,
            digitableLine = charge.DigitableLine,
            paidAt = charge.PaidAt,
            dueDate = charge.DueDate,
            createdAt = charge.CreatedAt
        });
    }

    [HttpPost("{id:guid}/simulate-payment")]
    public async Task<IActionResult> SimulatePayment(Guid id, CancellationToken ct)
    {
        var charge = await _db.BoletoCharges.FindAsync(new object[] { id }, ct);
        if (charge == null) return NotFound(new { error = "Cobranca nao encontrada" });

        if (charge.Status != BoletoChargeStatus.Pending)
            return BadRequest(new { error = $"Cobranca em status {charge.Status}, nao pode ser paga" });

        charge.Status = BoletoChargeStatus.Confirmed;
        charge.PaidAt = DateTime.UtcNow;
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
                        status = "Confirmed",
                        paidAt = charge.PaidAt,
                        amount = charge.Amount,
                        method = "boleto"
                    });
                }
                catch { }
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
}

public record CreateBoletoChargeRequest(
    decimal Amount,
    string? Description = null,
    string? ExternalId = null,
    string? PayerCpf = null,
    string? PayerName = null,
    string? MerchantId = null,
    string? WebhookUrl = null,
    DateTime? DueDate = null);
