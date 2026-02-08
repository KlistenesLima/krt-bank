using KRT.Payments.Api.Data;
using KRT.Payments.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/boletos")]
public class BoletoController : ControllerBase
{
    private readonly PaymentsDbContext _db;
    public BoletoController(PaymentsDbContext db) => _db = db;

    [HttpGet("account/{accountId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBoletos(Guid accountId, [FromQuery] string? status)
    {
        var query = _db.Boletos.Where(b => b.AccountId == accountId);
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<BoletoStatus>(status, true, out var s))
            query = query.Where(b => b.Status == s);
        var list = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
        foreach (var b in list) b.CheckOverdue();
        await _db.SaveChangesAsync();
        return Ok(list.Select(b => new { b.Id, b.Barcode, b.DigitableLine, b.Amount, b.PaidAmount, b.BeneficiaryName, b.BeneficiaryCnpj, b.Description, b.DueDate, status = b.GetStatusLabel(), statusCode = b.Status.ToString(), b.PaidAt, b.CreatedAt }));
    }

    [HttpPost("generate")]
    [AllowAnonymous]
    public async Task<IActionResult> Generate([FromBody] GenerateBoletoRequest req)
    {
        try {
            var b = Boleto.Generate(req.AccountId, req.BeneficiaryName, req.BeneficiaryCnpj ?? "", req.Amount, req.DueDate, req.Description ?? "");
            _db.Boletos.Add(b);
            await _db.SaveChangesAsync();
            return Created("", new { b.Id, b.Barcode, b.DigitableLine, message = "Boleto gerado" });
        } catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("pay/{boletoId}")]
    [AllowAnonymous]
    public async Task<IActionResult> Pay(Guid boletoId)
    {
        var b = await _db.Boletos.FindAsync(boletoId);
        if (b == null) return NotFound();
        try { b.Pay(b.Amount); await _db.SaveChangesAsync(); return Ok(new { message = "Boleto pago" }); }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("cancel/{boletoId}")]
    [AllowAnonymous]
    public async Task<IActionResult> Cancel(Guid boletoId)
    {
        var b = await _db.Boletos.FindAsync(boletoId);
        if (b == null) return NotFound();
        try { b.Cancel(); await _db.SaveChangesAsync(); return Ok(new { message = "Boleto cancelado" }); }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("{boletoId}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid boletoId)
    {
        var b = await _db.Boletos.FindAsync(boletoId);
        if (b == null) return NotFound();
        return Ok(b);
    }

    [HttpPost("pay-barcode")]
    [AllowAnonymous]
    public async Task<IActionResult> PayByBarcode([FromBody] PayBarcodeRequest req)
    {
        var b = Boleto.FromBarcode(Guid.NewGuid(), req.Barcode, req.Amount, req.BeneficiaryName ?? "Beneficiario");
        b.Pay(req.Amount);
        _db.Boletos.Add(b);
        await _db.SaveChangesAsync();
        return Ok(new { b.Id, message = "Boleto pago via codigo de barras" });
    }
}

public record GenerateBoletoRequest(Guid AccountId, string BeneficiaryName, string? BeneficiaryCnpj, decimal Amount, DateTime DueDate, string? Description);
public record PayBarcodeRequest(string Barcode, string? BeneficiaryName, decimal Amount);
