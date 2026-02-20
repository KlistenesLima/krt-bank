using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
public class DashboardController : ControllerBase
{
    /// <summary>
    /// Resumo geral da conta (saldo, entradas, saidas, total transacoes).
    /// </summary>
    [HttpGet("summary/{accountId}")]
    [AllowAnonymous]
    public IActionResult GetSummary(Guid accountId)
    {
        // Em producao, buscaria do banco de dados
        var rng = new Random(accountId.GetHashCode());
        var balance = rng.Next(5000, 50000) + rng.NextDouble();
        var income = rng.Next(8000, 25000) + rng.NextDouble();
        var expenses = rng.Next(3000, 15000) + rng.NextDouble();

        return Ok(new
        {
            accountId,
            balance = Math.Round((decimal)balance, 2),
            incomeThisMonth = Math.Round((decimal)income, 2),
            expensesThisMonth = Math.Round((decimal)expenses, 2),
            totalTransactions = rng.Next(15, 80),
            pendingTransactions = rng.Next(0, 5),
            lastUpdate = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Historico de saldo dos ultimos 30 dias.
    /// </summary>
    [HttpGet("balance-history/{accountId}")]
    [AllowAnonymous]
    public IActionResult GetBalanceHistory(Guid accountId, [FromQuery] int days = 30)
    {
        var rng = new Random(accountId.GetHashCode());
        var baseBalance = rng.Next(10000, 30000);
        var history = Enumerable.Range(0, days).Select(i =>
        {
            var date = DateTime.UtcNow.Date.AddDays(-days + i + 1);
            baseBalance += rng.Next(-2000, 3000);
            if (baseBalance < 1000) baseBalance = 1000 + rng.Next(500, 2000);
            return new { date = date.ToString("yyyy-MM-dd"), balance = Math.Round((decimal)baseBalance + (decimal)rng.NextDouble(), 2) };
        }).ToList();

        return Ok(new { accountId, days, history });
    }

    /// <summary>
    /// Gastos por categoria no mes atual.
    /// </summary>
    [HttpGet("spending-categories/{accountId}")]
    [AllowAnonymous]
    public IActionResult GetSpendingCategories(Guid accountId)
    {
        var categories = new[]
        {
            new { category = "Alimentacao", amount = 1250.80m, color = "#FF6384", percentage = 28.5 },
            new { category = "Transporte", amount = 680.50m, color = "#36A2EB", percentage = 15.5 },
            new { category = "Moradia", amount = 1500.00m, color = "#FFCE56", percentage = 34.2 },
            new { category = "Lazer", amount = 420.30m, color = "#4BC0C0", percentage = 9.6 },
            new { category = "Saude", amount = 280.00m, color = "#9966FF", percentage = 6.4 },
            new { category = "Outros", amount = 255.40m, color = "#FF9F40", percentage = 5.8 }
        };

        return Ok(new { accountId, month = DateTime.UtcNow.ToString("yyyy-MM"), categories, total = categories.Sum(c => c.amount) });
    }

    /// <summary>
    /// Transacoes dos ultimos 6 meses (entradas vs saidas).
    /// </summary>
    [HttpGet("monthly-summary/{accountId}")]
    [AllowAnonymous]
    public IActionResult GetMonthlySummary(Guid accountId)
    {
        var rng = new Random(accountId.GetHashCode());
        var months = Enumerable.Range(0, 6).Select(i =>
        {
            var date = DateTime.UtcNow.AddMonths(-5 + i);
            return new
            {
                month = date.ToString("MMM/yy"),
                income = Math.Round((decimal)(rng.Next(5000, 20000) + rng.NextDouble()), 2),
                expenses = Math.Round((decimal)(rng.Next(3000, 15000) + rng.NextDouble()), 2),
                transactions = rng.Next(10, 60)
            };
        }).ToList();

        return Ok(new { accountId, months });
    }
}