using KRT.Payments.Api.Data;
using KRT.Payments.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/pix/scheduled")]
public class ScheduledPixController : ControllerBase
{
    private readonly PaymentsDbContext _db;
    public ScheduledPixController(PaymentsDbContext db) => _db = db;

    [HttpGet("account/{accountId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(Guid accountId, [FromQuery] string? status)
    {
        var query = _db.ScheduledPixTransactions.Where(s => s.AccountId == accountId);
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ScheduledPixStatus>(status, true, out var st))
            query = query.Where(s => s.Status == st);
        var list = await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
        return Ok(new { accountId, total = list.Count, items = list });
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var s = await _db.ScheduledPixTransactions.FindAsync(id);
        if (s == null) return NotFound();
        return Ok(s);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateScheduledPixRequest req)
    {
        try
        {
            var freq = Enum.Parse<ScheduledPixFrequency>(req.Frequency, true);
            var sp = ScheduledPix.Create(
                req.AccountId,
                req.DestinationAccountId ?? Guid.NewGuid(),
                req.DestinationPixKey,
                req.DestinationName,
                req.Amount,
                req.Description ?? "",
                req.ScheduledDate,
                freq,
                req.EndDate,
                req.MaxExecutions
            );
            _db.ScheduledPixTransactions.Add(sp);
            await _db.SaveChangesAsync();
            return Created("", new { sp.Id, message = freq == ScheduledPixFrequency.Once ? "Pix agendado" : "Pix recorrente criado" });
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("{id}/execute")]
    [AllowAnonymous]
    public async Task<IActionResult> Execute(Guid id)
    {
        var s = await _db.ScheduledPixTransactions.FindAsync(id);
        if (s == null) return NotFound();
        try
        {
            var result = s.Execute();
            await _db.SaveChangesAsync();
            return Ok(new { message = result.message, s.ExecutionCount, s.NextExecutionDate });
        }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("{id}/cancel")]
    [AllowAnonymous]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var s = await _db.ScheduledPixTransactions.FindAsync(id);
        if (s == null) return NotFound();
        s.Cancel(); await _db.SaveChangesAsync();
        return Ok(new { message = "Agendamento cancelado" });
    }

    [HttpPost("{id}/pause")]
    [AllowAnonymous]
    public async Task<IActionResult> Pause(Guid id)
    {
        var s = await _db.ScheduledPixTransactions.FindAsync(id);
        if (s == null) return NotFound();
        s.Pause(); await _db.SaveChangesAsync();
        return Ok(new { message = "Agendamento pausado" });
    }

    [HttpPost("{id}/resume")]
    [AllowAnonymous]
    public async Task<IActionResult> Resume(Guid id)
    {
        var s = await _db.ScheduledPixTransactions.FindAsync(id);
        if (s == null) return NotFound();
        s.Resume(); await _db.SaveChangesAsync();
        return Ok(new { message = "Agendamento retomado" });
    }

    [HttpPut("{id}/amount")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateAmount(Guid id, [FromBody] UpdateAmountRequest req)
    {
        var s = await _db.ScheduledPixTransactions.FindAsync(id);
        if (s == null) return NotFound();
        try { s.UpdateAmount(req.Amount); await _db.SaveChangesAsync(); return Ok(new { message = "Valor atualizado" }); }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }
}

public record CreateScheduledPixRequest(
    Guid AccountId,
    Guid? DestinationAccountId,
    string DestinationPixKey,
    string DestinationName,
    decimal Amount,
    DateTime ScheduledDate,
    string Frequency = "Once",
    string? Description = null,
    DateTime? EndDate = null,
    int? MaxExecutions = null
);
public record UpdateAmountRequest(decimal Amount);
