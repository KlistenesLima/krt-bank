using KRT.Payments.Api.Data;
using KRT.Payments.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/goals")]
public class GoalsController : ControllerBase
{
    private readonly PaymentsDbContext _db;
    public GoalsController(PaymentsDbContext db) => _db = db;

    private async Task EnsureSeed(Guid accountId)
    {
        if (await _db.FinancialGoals.AnyAsync(g => g.AccountId == accountId)) return;
        var g1 = FinancialGoal.Create(accountId, "Viagem para Europa", 15000, DateTime.UtcNow.AddMonths(8), "✈️", "Viagem"); g1.Deposit(6750);
        var g2 = FinancialGoal.Create(accountId, "Reserva de emergencia", 30000, DateTime.UtcNow.AddMonths(18), "🛡️", "Reserva"); g2.Deposit(18900);
        var g3 = FinancialGoal.Create(accountId, "iPhone novo", 5500, DateTime.UtcNow.AddMonths(3), "📱", "Compra"); g3.Deposit(4200);
        _db.FinancialGoals.AddRange(g1, g2, g3);
        await _db.SaveChangesAsync();
    }

    [HttpGet("{accountId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGoals(Guid accountId, [FromQuery] string? status)
    {
        await EnsureSeed(accountId);
        var query = _db.FinancialGoals.Where(g => g.AccountId == accountId);
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<FinancialGoalStatus>(status, true, out var s))
            query = query.Where(g => g.Status == s);
        var list = await query.OrderByDescending(g => g.CreatedAt).ToListAsync();
        var active = list.Where(g => g.Status != FinancialGoalStatus.Cancelled);
        return Ok(new {
            accountId,
            summary = new { totalGoals = list.Count, totalSaved = active.Sum(g => g.CurrentAmount), totalTarget = active.Sum(g => g.TargetAmount), overallProgress = active.Sum(g => g.TargetAmount) > 0 ? Math.Round(active.Sum(g => g.CurrentAmount) / active.Sum(g => g.TargetAmount) * 100, 1) : 0 },
            goals = list.Select(g => new { g.Id, g.Title, g.Icon, g.Category, g.TargetAmount, g.CurrentAmount, g.ProgressPercent, g.RemainingAmount, g.DaysRemaining, g.MonthlyRequired, g.Deadline, status = g.GetStatusLabel(), statusCode = g.Status.ToString(), g.IsCompleted, g.CreatedAt, g.CompletedAt })
        });
    }

    [HttpPost("{accountId}")]
    [AllowAnonymous]
    public async Task<IActionResult> Create(Guid accountId, [FromBody] CreateGoalRequest req)
    {
        try {
            var g = FinancialGoal.Create(accountId, req.Title, req.TargetAmount, req.Deadline, req.Icon ?? "🎯", req.Category ?? "Outros");
            _db.FinancialGoals.Add(g);
            await _db.SaveChangesAsync();
            return Created("", new { g.Id, message = "Meta criada" });
        } catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("{accountId}/{goalId}/deposit")]
    [AllowAnonymous]
    public async Task<IActionResult> Deposit(Guid accountId, Guid goalId, [FromBody] GoalAmountRequest req)
    {
        var g = await _db.FinancialGoals.FirstOrDefaultAsync(x => x.Id == goalId && x.AccountId == accountId);
        if (g == null) return NotFound();
        try { g.Deposit(req.Amount); await _db.SaveChangesAsync(); return Ok(new { message = g.IsCompleted ? "Meta concluida!" : "Deposito realizado", g.CurrentAmount, g.ProgressPercent }); }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("{accountId}/{goalId}/withdraw")]
    [AllowAnonymous]
    public async Task<IActionResult> Withdraw(Guid accountId, Guid goalId, [FromBody] GoalAmountRequest req)
    {
        var g = await _db.FinancialGoals.FirstOrDefaultAsync(x => x.Id == goalId && x.AccountId == accountId);
        if (g == null) return NotFound();
        try { g.Withdraw(req.Amount); await _db.SaveChangesAsync(); return Ok(new { message = "Resgate realizado", g.CurrentAmount, g.ProgressPercent }); }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("{accountId}/{goalId}/cancel")]
    [AllowAnonymous]
    public async Task<IActionResult> Cancel(Guid accountId, Guid goalId)
    {
        var g = await _db.FinancialGoals.FirstOrDefaultAsync(x => x.Id == goalId && x.AccountId == accountId);
        if (g == null) return NotFound();
        g.Cancel(); await _db.SaveChangesAsync();
        return Ok(new { message = "Meta cancelada" });
    }
}

public record CreateGoalRequest(string Title, decimal TargetAmount, DateTime Deadline, string? Icon = null, string? Category = null);
public record GoalAmountRequest(decimal Amount);
