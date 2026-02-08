using KRT.Payments.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/boletos")]
public class BoletoController : ControllerBase
{
    private static readonly ConcurrentDictionary<Guid, Boleto> _store = new();

    [HttpGet("account/{accountId}")]
    [AllowAnonymous]
    public IActionResult GetByAccount(Guid accountId, [FromQuery] string? status)
    {
        var items = _store.Values.Where(b => b.AccountId == accountId);
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<BoletoStatus>(status, true, out var st))
            items = items.Where(b => b.Status == st);

        // Check overdue
        foreach (var b in items) b.CheckOverdue();

        return Ok(items.OrderByDescending(b => b.CreatedAt).Select(b => new
        {
            b.Id, b.AccountId, b.Barcode, b.DigitableLine, b.Amount, b.PaidAmount,
            b.BeneficiaryName, b.BeneficiaryCnpj, b.Description, b.DueDate,
            status = b.GetStatusLabel(), statusCode = b.Status.ToString(),
            b.PaidAt, b.CreatedAt,
            isOverdue = b.Status == BoletoStatus.Overdue,
            daysUntilDue = (b.DueDate.Date - DateTime.UtcNow.Date).Days
        }));
    }

    [HttpPost("generate")]
    [AllowAnonymous]
    public IActionResult Generate([FromBody] GenerateBoletoRequest request)
    {
        try
        {
            var boleto = Boleto.Generate(request.AccountId, request.BeneficiaryName, request.BeneficiaryCnpj ?? "", request.Amount, request.DueDate, request.Description ?? "");
            _store[boleto.Id] = boleto;
            return Created("", new { boleto.Id, boleto.Barcode, boleto.DigitableLine, boleto.Amount, boleto.DueDate, message = "Boleto gerado" });
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("pay/{boletoId}")]
    [AllowAnonymous]
    public IActionResult Pay(Guid boletoId)
    {
        if (!_store.TryGetValue(boletoId, out var boleto))
            return NotFound(new { error = "Boleto nao encontrado" });
        var (success, msg) = boleto.Pay();
        return success ? Ok(new { message = msg, boleto.PaidAt, boleto.PaidAmount }) : BadRequest(new { error = msg });
    }

    [HttpPost("pay-barcode")]
    [AllowAnonymous]
    public IActionResult PayByBarcode([FromBody] PayBarcodeRequest request)
    {
        var boleto = Boleto.FromBarcode(request.AccountId, request.Barcode, request.Amount, request.BeneficiaryName ?? "Favorecido");
        _store[boleto.Id] = boleto;
        boleto.Pay();
        return Ok(new { boleto.Id, message = "Boleto pago com sucesso", boleto.PaidAt, boleto.Amount });
    }

    [HttpPost("cancel/{boletoId}")]
    [AllowAnonymous]
    public IActionResult Cancel(Guid boletoId)
    {
        if (!_store.TryGetValue(boletoId, out var boleto))
            return NotFound(new { error = "Boleto nao encontrado" });
        try { boleto.Cancel(); return Ok(new { message = "Boleto cancelado" }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("{boletoId}")]
    [AllowAnonymous]
    public IActionResult GetById(Guid boletoId)
    {
        if (!_store.TryGetValue(boletoId, out var b))
            return NotFound(new { error = "Boleto nao encontrado" });
        b.CheckOverdue();
        return Ok(new { b.Id, b.Barcode, b.DigitableLine, b.Amount, b.PaidAmount, b.BeneficiaryName, b.Description, b.DueDate, status = b.GetStatusLabel(), b.PaidAt });
    }
}

public record GenerateBoletoRequest(Guid AccountId, string BeneficiaryName, string? BeneficiaryCnpj, decimal Amount, DateTime DueDate, string? Description);
public record PayBarcodeRequest(Guid AccountId, string Barcode, decimal Amount, string? BeneficiaryName);