using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
public class AdminController : ControllerBase
{
    /// <summary>
    /// Dashboard gerencial com metricas do sistema.
    /// </summary>
    [HttpGet("dashboard")]
    [AllowAnonymous]
    public IActionResult GetDashboard()
    {
        var rng = new Random(DateTime.UtcNow.Day);
        return Ok(new
        {
            overview = new
            {
                totalAccounts = rng.Next(8000, 12000),
                activeAccounts = rng.Next(6000, 10000),
                pendingApproval = rng.Next(15, 50),
                blockedAccounts = rng.Next(20, 80)
            },
            transactions = new
            {
                today = rng.Next(500, 2000),
                thisWeek = rng.Next(3000, 10000),
                thisMonth = rng.Next(15000, 45000),
                totalVolume = Math.Round((decimal)(rng.Next(5000000, 20000000) + rng.NextDouble()), 2),
                avgTicket = Math.Round((decimal)(rng.Next(100, 800) + rng.NextDouble()), 2)
            },
            fraud = new
            {
                alertsToday = rng.Next(5, 25),
                blockedToday = rng.Next(1, 10),
                falsePositiveRate = Math.Round(rng.NextDouble() * 15 + 5, 1),
                totalFraudPrevented = Math.Round((decimal)(rng.Next(50000, 200000) + rng.NextDouble()), 2)
            },
            revenue = new
            {
                pixFees = Math.Round((decimal)(rng.Next(10000, 50000) + rng.NextDouble()), 2),
                cardFees = Math.Round((decimal)(rng.Next(5000, 20000) + rng.NextDouble()), 2),
                insurance = Math.Round((decimal)(rng.Next(8000, 30000) + rng.NextDouble()), 2),
                loanInterest = Math.Round((decimal)(rng.Next(20000, 80000) + rng.NextDouble()), 2)
            },
            systemHealth = new
            {
                apiLatency = $"{rng.Next(5, 30)}ms",
                uptime = "99.97%",
                errorRate = $"{Math.Round(rng.NextDouble() * 0.5, 2)}%",
                activeSessions = rng.Next(200, 800)
            }
        });
    }

    /// <summary>
    /// Lista contas pendentes de aprovacao.
    /// </summary>
    [HttpGet("accounts/pending")]
    [AllowAnonymous]
    public IActionResult GetPendingAccounts()
    {
        var rng = new Random(42);
        var accounts = Enumerable.Range(1, 12).Select(i => new
        {
            id = Guid.NewGuid(),
            name = new[] { "Ana Costa", "Pedro Mendes", "Julia Ferreira", "Lucas Oliveira", "Mariana Santos", "Rafael Lima", "Beatriz Alves", "Gabriel Souza", "Camila Rocha", "Diego Martins", "Isabela Nunes", "Thiago Pereira" }[i - 1],
            cpf = $"***.***.{rng.Next(100, 999)}-{rng.Next(10, 99)}",
            email = $"user{i}@email.com",
            createdAt = DateTime.UtcNow.AddHours(-rng.Next(1, 72)),
            kycStatus = new[] { "Documentos enviados", "Aguardando selfie", "Em analise", "Documentos enviados" }[i % 4],
            riskScore = rng.Next(0, 100)
        }).ToList();

        return Ok(new { total = accounts.Count, accounts });
    }

    /// <summary>
    /// Aprova ou rejeita uma conta.
    /// </summary>
    [HttpPost("accounts/{accountId}/review")]
    [AllowAnonymous]
    public IActionResult ReviewAccount(Guid accountId, [FromBody] AdminReviewRequest req)
    {
        return Ok(new
        {
            accountId,
            action = req.Approved ? "Aprovado" : "Rejeitado",
            message = req.Approved ? "Conta aprovada com sucesso" : "Conta rejeitada",
            reviewedBy = "admin@krtbank.com",
            reviewedAt = DateTime.UtcNow,
            notes = req.Notes
        });
    }

    /// <summary>
    /// Lista alertas de fraude.
    /// </summary>
    [HttpGet("fraud/alerts")]
    [AllowAnonymous]
    public IActionResult GetFraudAlerts()
    {
        var alerts = new[]
        {
            new { id = Guid.NewGuid(), type = "Transacao suspeita", severity = "high", accountName = "Carlos M.", amount = 8500.00m, description = "Pix acima do limite noturno", status = "Pendente", createdAt = DateTime.UtcNow.AddMinutes(-15) },
            new { id = Guid.NewGuid(), type = "Login anomalo", severity = "medium", accountName = "Julia F.", amount = 0m, description = "Login de IP desconhecido (outro pais)", status = "Pendente", createdAt = DateTime.UtcNow.AddMinutes(-45) },
            new { id = Guid.NewGuid(), type = "Multiplas tentativas", severity = "high", accountName = "Pedro S.", amount = 15000.00m, description = "5 tentativas de Pix em 2 minutos", status = "Bloqueado", createdAt = DateTime.UtcNow.AddHours(-1) },
            new { id = Guid.NewGuid(), type = "Conta nova alto valor", severity = "medium", accountName = "Ana R.", amount = 25000.00m, description = "Pix alto em conta com menos de 7 dias", status = "Pendente", createdAt = DateTime.UtcNow.AddHours(-2) },
            new { id = Guid.NewGuid(), type = "Padrao de fraude", severity = "critical", accountName = "Diego L.", amount = 3200.00m, description = "Transferencias rapidas para multiplas contas", status = "Bloqueado", createdAt = DateTime.UtcNow.AddHours(-3) },
            new { id = Guid.NewGuid(), type = "Chargeback", severity = "low", accountName = "Maria C.", amount = 450.00m, description = "Contestacao de compra no cartao", status = "Em analise", createdAt = DateTime.UtcNow.AddHours(-5) }
        };

        return Ok(new { total = alerts.Length, pendingCount = alerts.Count(a => a.status == "Pendente"), alerts });
    }

    /// <summary>
    /// Tomar acao sobre alerta de fraude.
    /// </summary>
    [HttpPost("fraud/alerts/{alertId}/action")]
    [AllowAnonymous]
    public IActionResult FraudAction(Guid alertId, [FromBody] FraudActionRequest req)
    {
        return Ok(new
        {
            alertId,
            action = req.Action,
            message = req.Action switch
            {
                "block" => "Conta bloqueada e transacao revertida",
                "approve" => "Transacao liberada",
                "investigate" => "Encaminhado para investigacao",
                _ => "Acao registrada"
            },
            actionBy = "admin@krtbank.com",
            actionAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Metricas por periodo.
    /// </summary>
    [HttpGet("metrics")]
    [AllowAnonymous]
    public IActionResult GetMetrics([FromQuery] int days = 30)
    {
        var rng = new Random(days);
        var daily = Enumerable.Range(0, days).Select(i =>
        {
            var date = DateTime.UtcNow.Date.AddDays(-days + i + 1);
            return new
            {
                date = date.ToString("yyyy-MM-dd"),
                transactions = rng.Next(300, 2000),
                volume = Math.Round((decimal)(rng.Next(100000, 800000) + rng.NextDouble()), 2),
                newAccounts = rng.Next(5, 40),
                fraudAlerts = rng.Next(0, 15)
            };
        }).ToList();

        return Ok(new { days, daily });
    }
}

public record AdminReviewRequest(bool Approved, string? Notes);
public record FraudActionRequest(string Action, string? Notes);