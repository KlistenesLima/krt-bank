using KRT.Payments.Api.Data;
using KRT.Payments.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/statement")]
public class StatementController : ControllerBase
{
    private readonly PaymentsDbContext _db;
    public StatementController(PaymentsDbContext db) => _db = db;

    private async Task EnsureSeed(Guid accountId)
    {
        if (await _db.StatementEntries.AnyAsync(s => s.AccountId == accountId)) return;
        var rng = new Random(accountId.GetHashCode());
        var types = new[] { "PIX_SENT", "PIX_RECEIVED", "BOLETO", "CARD_PURCHASE", "TRANSFER_IN", "SALARY" };
        var cats = new[] { "Alimentacao", "Transporte", "Educacao", "Lazer", "Saude", "Salario" };
        var names = new[] { "Maria Silva", "Supermercado ABC", "Netflix", "Uber", "Farmacia Popular", "Empresa XYZ" };
        var entries = new List<StatementEntry>();
        for (int i = 0; i < 50; i++)
        {
            var isCredit = rng.Next(100) > 60;
            entries.Add(new StatementEntry
            {
                Id = Guid.NewGuid(), AccountId = accountId,
                Date = DateTime.UtcNow.AddDays(-rng.Next(0, 90)).AddHours(rng.Next(8, 20)),
                Type = types[rng.Next(types.Length)], Category = cats[rng.Next(cats.Length)],
                Amount = isCredit ? Math.Round((decimal)(rng.Next(100, 8000) + rng.NextDouble()), 2) : -Math.Round((decimal)(rng.Next(10, 2000) + rng.NextDouble()), 2),
                Description = isCredit ? "Recebido" : "Pagamento",
                CounterpartyName = names[rng.Next(names.Length)], CounterpartyBank = "Banco " + rng.Next(1, 5),
                IsCredit = isCredit, CreatedAt = DateTime.UtcNow
            });
        }
        _db.StatementEntries.AddRange(entries);
        await _db.SaveChangesAsync();
    }

    [HttpGet("{accountId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStatement(Guid accountId, [FromQuery] int page = 1, [FromQuery] int size = 20,
        [FromQuery] string? type = null, [FromQuery] string? search = null,
        [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null,
        [FromQuery] string? sortBy = "date", [FromQuery] string? sortOrder = "desc")
    {
        await EnsureSeed(accountId);
        var query = _db.StatementEntries.Where(s => s.AccountId == accountId);
        if (!string.IsNullOrEmpty(type)) query = query.Where(s => s.Type == type);
        if (!string.IsNullOrEmpty(search)) query = query.Where(s => EF.Functions.ILike(s.CounterpartyName, $"%{search}%") || EF.Functions.ILike(s.Description, $"%{search}%"));
        if (startDate.HasValue) query = query.Where(s => s.Date >= startDate.Value);
        if (endDate.HasValue) query = query.Where(s => s.Date <= endDate.Value);

        query = sortBy?.ToLower() switch
        {
            "amount" => sortOrder == "asc" ? query.OrderBy(s => s.Amount) : query.OrderByDescending(s => s.Amount),
            "type" => sortOrder == "asc" ? query.OrderBy(s => s.Type) : query.OrderByDescending(s => s.Type),
            _ => sortOrder == "asc" ? query.OrderBy(s => s.Date) : query.OrderByDescending(s => s.Date)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        var allForSummary = _db.StatementEntries.Where(s => s.AccountId == accountId);
        if (startDate.HasValue) allForSummary = allForSummary.Where(s => s.Date >= startDate.Value);
        if (endDate.HasValue) allForSummary = allForSummary.Where(s => s.Date <= endDate.Value);

        var income = await allForSummary.Where(s => s.IsCredit).SumAsync(s => s.Amount);
        var expenses = await allForSummary.Where(s => !s.IsCredit).SumAsync(s => Math.Abs(s.Amount));

        return Ok(new { items, totalItems = total, totalPages = (int)Math.Ceiling((double)total / size),
            summary = new { totalIncome = income, totalExpenses = expenses, net = income - expenses } });
    }

    [HttpGet("{accountId}/export/csv")]
    [AllowAnonymous]
    public async Task<IActionResult> ExportCsv(Guid accountId)
    {
        await EnsureSeed(accountId);
        var items = await _db.StatementEntries.Where(s => s.AccountId == accountId).OrderByDescending(s => s.Date).ToListAsync();
        var csv = "Data;Tipo;Descricao;Contraparte;Valor\n" + string.Join("\n", items.Select(i => $"{i.Date:dd/MM/yyyy HH:mm};{i.Type};{i.Description};{i.CounterpartyName};{i.Amount:F2}"));
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "extrato.csv");
    }

    [HttpGet("{accountId}/export/pdf")]
    [AllowAnonymous]
    public async Task<IActionResult> ExportPdf(Guid accountId)
    {
        await EnsureSeed(accountId);
        return Ok(new { message = "PDF export disponivel via QuestPDF" });
    }
}