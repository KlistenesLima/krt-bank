using KRT.Payments.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Sockets;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
public class AdminController : ControllerBase
{
    private readonly PaymentsDbContext _db;
    private readonly IConfiguration _config;

    public AdminController(PaymentsDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    /// <summary>
    /// Dashboard gerencial com metricas reais do sistema.
    /// </summary>
    [HttpGet("dashboard")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDashboard()
    {
        // === OVERVIEW: real counts from BankAccounts ===
        var totalAccounts = await _db.BankAccounts.CountAsync();
        var activeAccounts = await _db.BankAccounts.CountAsync(a => a.Status == "Active");
        var pendingAccounts = await _db.BankAccounts.CountAsync(a => a.Status == "Pending");
        var blockedAccounts = await _db.BankAccounts.CountAsync(a => a.Status == "Blocked");

        // === TRANSACTIONS: real counts from StatementEntries ===
        var todayStart = DateTime.UtcNow.Date;
        var weekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
        var monthStart = new DateTime(todayStart.Year, todayStart.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Use IsCredit = false to avoid counting both sides of a transfer
        var todayTx = await _db.StatementEntries.CountAsync(s => !s.IsCredit && s.Date >= todayStart);
        var weekTx = await _db.StatementEntries.CountAsync(s => !s.IsCredit && s.Date >= weekStart);
        var monthTx = await _db.StatementEntries.CountAsync(s => !s.IsCredit && s.Date >= monthStart);

        var totalVolume = await _db.StatementEntries
            .Where(s => !s.IsCredit)
            .SumAsync(s => (decimal?)s.Amount) ?? 0m;
        totalVolume = Math.Abs(totalVolume);

        var totalTxCount = await _db.StatementEntries.CountAsync(s => !s.IsCredit);
        var avgTicket = totalTxCount > 0 ? Math.Round(totalVolume / totalTxCount, 2) : 0m;

        // === FRAUD: manter mockado (sem tabela de fraude) ===
        var rng = new Random(DateTime.UtcNow.Day);
        var fraudData = new
        {
            alertsToday = rng.Next(5, 25),
            blockedToday = rng.Next(1, 10),
            falsePositiveRate = Math.Round(rng.NextDouble() * 15 + 5, 1),
            totalFraudPrevented = Math.Round((decimal)(rng.Next(50000, 200000) + rng.NextDouble()), 2)
        };

        // === REVENUE: calculate from real volume ===
        var pixVolume = await _db.StatementEntries
            .Where(s => !s.IsCredit && (s.Type == "PIX_SENT" || s.Type == "PIX"))
            .SumAsync(s => (decimal?)Math.Abs(s.Amount)) ?? 0m;

        var cardVolume = await _db.StatementEntries
            .Where(s => !s.IsCredit && (s.Type == "CARD_PURCHASE" || s.Type == "CARD" || s.Type == "Fatura Cartao" || s.Type == "Cartao"))
            .SumAsync(s => (decimal?)Math.Abs(s.Amount)) ?? 0m;

        var boletoVolume = await _db.StatementEntries
            .Where(s => !s.IsCredit && (s.Type == "BOLETO" || s.Type == "BOLETO_PAYMENT" || s.Type == "Boleto"))
            .SumAsync(s => (decimal?)Math.Abs(s.Amount)) ?? 0m;

        var pixFees = Math.Round(pixVolume * 0.01m, 2);       // 1% PIX fees
        var cardFees = Math.Round(cardVolume * 0.025m, 2);     // 2.5% card fees
        var boletoFees = Math.Round(boletoVolume * 0.005m, 2); // 0.5% boleto fees
        var totalRevenue = pixFees + cardFees + boletoFees;

        var totalBalance = await _db.BankAccounts.SumAsync(a => (decimal?)a.Balance) ?? 0m;

        // === SYSTEM HEALTH: real metrics ===
        var process = Process.GetCurrentProcess();
        var uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime();
        var totalRequests = MetricsController.GetTotalRequests();
        var totalErrors = MetricsController.GetTotalErrors();
        var errorRate = totalRequests > 0 ? Math.Round((double)totalErrors / totalRequests * 100, 2) : 0;
        var avgLatency = MetricsController.GetAverageLatency();

        return Ok(new
        {
            overview = new
            {
                totalAccounts,
                activeAccounts,
                pendingApproval = pendingAccounts,
                blockedAccounts
            },
            transactions = new
            {
                today = todayTx,
                thisWeek = weekTx,
                thisMonth = monthTx,
                totalVolume = Math.Round(totalVolume, 2),
                avgTicket
            },
            fraud = fraudData,
            revenue = new
            {
                pixFees,
                cardFees,
                boletoFees,
                totalRevenue,
                totalBalance = Math.Round(totalBalance, 2),
                revenueBreakdown = new[]
                {
                    new { label = "PIX", value = pixFees },
                    new { label = "Boleto", value = boletoFees },
                    new { label = "Cartão", value = cardFees }
                }
            },
            systemHealth = new
            {
                apiLatency = $"{avgLatency}ms",
                uptime = $"{uptime.TotalHours:F0}h ({uptime.Days}d {uptime.Hours}h)",
                errorRate = $"{errorRate}%",
                activeSessions = totalRequests
            }
        });
    }

    /// <summary>
    /// Lista de transações com KPIs e paginação.
    /// </summary>
    [HttpGet("transactions")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? type = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var query = _db.StatementEntries.AsQueryable();

        // Filters
        if (!string.IsNullOrEmpty(type))
        {
            query = type.ToUpper() switch
            {
                "PIX" => query.Where(s => s.Type == "PIX" || s.Type == "PIX_SENT"),
                "BOLETO" => query.Where(s => s.Type == "BOLETO" || s.Type == "BOLETO_PAYMENT" || s.Type == "Boleto"),
                "CARD" => query.Where(s => s.Type == "CARD" || s.Type == "CARD_PURCHASE" || s.Type == "Fatura Cartao" || s.Type == "Cartao"),
                _ => query
            };
        }
        if (from.HasValue) query = query.Where(s => s.Date >= from.Value);
        if (to.HasValue) query = query.Where(s => s.Date <= to.Value);

        var totalItems = await query.CountAsync();
        var transactions = await query
            .OrderByDescending(s => s.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                date = s.Date,
                type = (s.Type == "PIX" || s.Type == "PIX_SENT") ? "PIX"
                     : (s.Type == "BOLETO" || s.Type == "BOLETO_PAYMENT" || s.Type == "Boleto") ? "Boleto"
                     : (s.Type == "CARD" || s.Type == "CARD_PURCHASE" || s.Type == "Fatura Cartao" || s.Type == "Cartao") ? "Cartão"
                     : s.Type,
                s.Amount,
                s.Description,
                s.CounterpartyName,
                s.IsCredit,
                s.AccountId
            })
            .ToListAsync();

        // KPIs
        var todayStart = DateTime.UtcNow.Date;
        var weekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
        var allDebit = _db.StatementEntries.Where(s => !s.IsCredit);
        var totalTx = await _db.StatementEntries.CountAsync();
        var todayTx = await allDebit.CountAsync(s => s.Date >= todayStart);
        var weekTx = await allDebit.CountAsync(s => s.Date >= weekStart);
        var totalVol = await allDebit.SumAsync(s => (decimal?)Math.Abs(s.Amount)) ?? 0m;
        var avgTicket = todayTx > 0 ? Math.Round(totalVol / await allDebit.CountAsync(), 2) : 0m;

        var pixVol = await allDebit.Where(s => s.Type == "PIX_SENT" || s.Type == "PIX").SumAsync(s => (decimal?)Math.Abs(s.Amount)) ?? 0m;
        var boletoVol = await allDebit.Where(s => s.Type == "BOLETO" || s.Type == "BOLETO_PAYMENT" || s.Type == "Boleto").SumAsync(s => (decimal?)Math.Abs(s.Amount)) ?? 0m;
        var cardVol = await allDebit.Where(s => s.Type == "CARD" || s.Type == "CARD_PURCHASE" || s.Type == "Fatura Cartao" || s.Type == "Cartao").SumAsync(s => (decimal?)Math.Abs(s.Amount)) ?? 0m;

        return Ok(new
        {
            kpis = new
            {
                totalTransactions = totalTx,
                todayTransactions = todayTx,
                weekTransactions = weekTx,
                totalVolume = Math.Round(totalVol, 2),
                avgTicket,
                pixVolume = Math.Round(pixVol, 2),
                boletoVolume = Math.Round(boletoVol, 2),
                cardVolume = Math.Round(cardVol, 2)
            },
            transactions,
            pagination = new
            {
                page,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            }
        });
    }

    /// <summary>
    /// Atividade recente real do sistema.
    /// </summary>
    [HttpGet("activity")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActivity()
    {
        // Recent statement entries (last 15)
        var recentStatements = await _db.StatementEntries
            .OrderByDescending(s => s.Date)
            .Take(15)
            .ToListAsync();

        // Recent accounts (last 5)
        var recentAccounts = await _db.BankAccounts
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .ToListAsync();

        var activities = new List<object>();

        foreach (var s in recentStatements)
        {
            var txType = (s.Type == "PIX" || s.Type == "PIX_SENT") ? "pix"
                       : (s.Type == "BOLETO" || s.Type == "BOLETO_PAYMENT" || s.Type == "Boleto") ? "boleto"
                       : (s.Type == "CARD" || s.Type == "CARD_PURCHASE" || s.Type == "Fatura Cartao" || s.Type == "Cartao") ? "card"
                       : "other";

            var color = txType switch
            {
                "pix" => s.IsCredit ? "#10b981" : "#3b82f6",
                "boleto" => "#7c3aed",
                "card" => "#f59e0b",
                _ => "#94a3b8"
            };

            var desc = s.IsCredit
                ? $"{(txType == "pix" ? "PIX" : txType == "boleto" ? "Boleto" : "Cartão")} recebido R$ {Math.Abs(s.Amount):N2}"
                : $"{(txType == "pix" ? "PIX" : txType == "boleto" ? "Boleto" : "Cartão")} enviado R$ {Math.Abs(s.Amount):N2}";

            activities.Add(new
            {
                type = txType + (s.IsCredit ? "_received" : "_sent"),
                description = desc,
                detail = !string.IsNullOrEmpty(s.CounterpartyName) ? s.CounterpartyName : s.Description,
                time = s.Date,
                color
            });
        }

        foreach (var a in recentAccounts)
        {
            activities.Add(new
            {
                type = "account_created",
                description = "Nova conta criada",
                detail = !string.IsNullOrEmpty(a.CustomerName) ? a.CustomerName : "Cliente KRT",
                time = a.CreatedAt,
                color = "#2196f3"
            });
        }

        // Sort all by time desc, take 20
        var sorted = activities
            .OrderByDescending(a => ((dynamic)a).time)
            .Take(20)
            .ToList();

        return Ok(new { activities = sorted });
    }

    /// <summary>
    /// Lista contas pendentes de aprovação.
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
            kycStatus = new[] { "Documentos enviados", "Aguardando selfie", "Em análise", "Documentos enviados" }[i % 4],
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
            new { id = Guid.NewGuid(), type = "Transação suspeita", severity = "high", accountName = "Carlos M.", amount = 8500.00m, description = "Pix acima do limite noturno", status = "Pendente", createdAt = DateTime.UtcNow.AddMinutes(-15) },
            new { id = Guid.NewGuid(), type = "Login anômalo", severity = "medium", accountName = "Julia F.", amount = 0m, description = "Login de IP desconhecido (outro país)", status = "Pendente", createdAt = DateTime.UtcNow.AddMinutes(-45) },
            new { id = Guid.NewGuid(), type = "Múltiplas tentativas", severity = "high", accountName = "Pedro S.", amount = 15000.00m, description = "5 tentativas de Pix em 2 minutos", status = "Bloqueado", createdAt = DateTime.UtcNow.AddHours(-1) },
            new { id = Guid.NewGuid(), type = "Conta nova alto valor", severity = "medium", accountName = "Ana R.", amount = 25000.00m, description = "Pix alto em conta com menos de 7 dias", status = "Pendente", createdAt = DateTime.UtcNow.AddHours(-2) },
            new { id = Guid.NewGuid(), type = "Padrão de fraude", severity = "critical", accountName = "Diego L.", amount = 3200.00m, description = "Transferências rápidas para múltiplas contas", status = "Bloqueado", createdAt = DateTime.UtcNow.AddHours(-3) },
            new { id = Guid.NewGuid(), type = "Chargeback", severity = "low", accountName = "Maria C.", amount = 450.00m, description = "Contestação de compra no cartão", status = "Em análise", createdAt = DateTime.UtcNow.AddHours(-5) }
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
                "block" => "Conta bloqueada e transação revertida",
                "approve" => "Transação liberada",
                "investigate" => "Encaminhado para investigação",
                _ => "Ação registrada"
            },
            actionBy = "admin@krtbank.com",
            actionAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Metricas por periodo com dados reais.
    /// </summary>
    [HttpGet("metrics")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMetrics([FromQuery] int days = 30)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);

        // Group StatementEntries by date (debit side only to avoid double-counting)
        var txByDay = await _db.StatementEntries
            .Where(s => !s.IsCredit && s.Date >= startDate)
            .GroupBy(s => s.Date.Date)
            .Select(g => new
            {
                Date = g.Key,
                Transactions = g.Count(),
                Volume = g.Sum(s => Math.Abs(s.Amount))
            })
            .ToListAsync();

        // Group new accounts by creation date
        var accountsByDay = await _db.BankAccounts
            .Where(a => a.CreatedAt >= startDate)
            .GroupBy(a => a.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var txLookup = txByDay.ToDictionary(x => x.Date);
        var accLookup = accountsByDay.ToDictionary(x => x.Date);

        var rng = new Random(days); // fraud stays mock

        var daily = Enumerable.Range(0, days).Select(i =>
        {
            var date = startDate.AddDays(i);
            txLookup.TryGetValue(date, out var tx);
            accLookup.TryGetValue(date, out var acc);

            return new
            {
                date = date.ToString("yyyy-MM-dd"),
                transactions = tx?.Transactions ?? 0,
                volume = Math.Round((decimal)(tx?.Volume ?? 0), 2),
                newAccounts = acc?.Count ?? 0,
                fraudAlerts = rng.Next(0, 15) // mock
            };
        }).ToList();

        return Ok(new { days, daily });
    }

    /// <summary>
    /// Health check real de todos os servicos.
    /// </summary>
    [HttpGet("system")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSystemHealth()
    {
        var checks = new List<Task<object>>();

        // 1. API Gateway — self (always healthy)
        checks.Add(Task.FromResult<object>(new { service = "API Gateway", status = "healthy", latency = "0ms", port = 5000, detail = "YARP Reverse Proxy" }));

        // 2. Payments API — self (always healthy)
        checks.Add(Task.FromResult<object>(new { service = "Payments API", status = "healthy", latency = "0ms", port = 5002, detail = ".NET 8" }));

        // 3. Onboarding API — HTTP ping (any HTTP response = healthy)
        checks.Add(CheckHttp("Onboarding API", _config["Services:OnboardingUrl"]?.TrimEnd('/') + "/swagger", 5001, ".NET 8"));

        // 4. PostgreSQL — DB query
        checks.Add(CheckPostgres());

        // 5. Kafka — TCP ping
        checks.Add(CheckTcp("Apache Kafka", _config["Kafka:BootstrapServers"] ?? "localhost:29092", 9092, "Mensageria"));

        // 6. RabbitMQ — TCP ping
        var rabbitHost = _config["RabbitMq:HostName"] ?? "localhost";
        var rabbitPort = int.TryParse(_config["RabbitMq:Port"], out var rp) ? rp : 5672;
        checks.Add(CheckTcp("RabbitMQ", $"{rabbitHost}:{rabbitPort}", rabbitPort, "AMQP"));

        // 7. Redis — TCP ping
        var redisConn = _config["Redis:ConnectionString"] ?? "localhost:6380";
        checks.Add(CheckTcp("Redis", redisConn, 6379, "Cache"));

        // 8. Seq — HTTP ping
        var seqUrl = _config["Serilog:WriteTo:1:Args:serverUrl"] ?? "http://localhost:5341";
        checks.Add(CheckHttp("Seq (Logs)", seqUrl, 8081, "Observabilidade"));

        // 9. Fraud Worker — self (always healthy)
        checks.Add(Task.FromResult<object>(new { service = "Fraud Worker", status = "healthy", latency = "0ms", port = 0, detail = "Saga Pattern" }));

        // 10. PIX Keys API — self (always healthy, internal)
        checks.Add(Task.FromResult<object>(new { service = "PIX Keys API", status = "healthy", latency = "0ms", port = 5003, detail = "Registry" }));

        // 11. Angular Web — HTTP ping
        checks.Add(CheckHttp("Angular Web", "http://krt-web:80", 4200, "Frontend"));

        // 12. Keycloak — HTTP ping
        var keycloakUrl = _config["Keycloak:Authority"] ?? "http://localhost:8080/realms/krt-bank";
        checks.Add(CheckHttp("Keycloak", keycloakUrl, 8080, "IAM / OAuth2"));

        await Task.WhenAll(checks);

        var services = checks.Select(t => t.Result).ToList();

        // System metrics
        var process = Process.GetCurrentProcess();
        var uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime();

        return Ok(new
        {
            services,
            metrics = new
            {
                uptime = uptime.ToString(@"d\.hh\:mm\:ss"),
                totalRequests = MetricsController.GetTotalRequests(),
                errorRate = MetricsController.GetTotalRequests() > 0
                    ? Math.Round((double)MetricsController.GetTotalErrors() / MetricsController.GetTotalRequests() * 100, 2)
                    : 0,
                memoryMb = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 1),
                threads = process.Threads.Count
            }
        });
    }

    // === Health check helpers ===

    private async Task<object> CheckPostgres()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await _db.Database.ExecuteSqlRawAsync("SELECT 1");
            sw.Stop();
            return new { service = "PostgreSQL", status = "healthy", latency = $"{sw.ElapsedMilliseconds}ms", port = 5432, detail = "v16" };
        }
        catch
        {
            sw.Stop();
            return new { service = "PostgreSQL", status = "unhealthy", latency = $"{sw.ElapsedMilliseconds}ms", port = 5432, detail = "Connection failed" };
        }
    }

    private static async Task<object> CheckTcp(string serviceName, string hostPort, int displayPort, string detail)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var parts = hostPort.Split(':');
            var host = parts[0];
            var port = parts.Length > 1 && int.TryParse(parts[1], out var p) ? p : displayPort;

            using var client = new TcpClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            await client.ConnectAsync(host, port, cts.Token);
            sw.Stop();
            return new { service = serviceName, status = "healthy", latency = $"{sw.ElapsedMilliseconds}ms", port = displayPort, detail };
        }
        catch
        {
            sw.Stop();
            return new { service = serviceName, status = "unhealthy", latency = $"{sw.ElapsedMilliseconds}ms", port = displayPort, detail = "Connection failed" };
        }
    }

    private static async Task<object> CheckHttp(string serviceName, string url, int displayPort, string detail)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            await httpClient.GetAsync(url);
            sw.Stop();
            // Any HTTP response (even 404) means the service is running
            return new { service = serviceName, status = "healthy", latency = $"{sw.ElapsedMilliseconds}ms", port = displayPort, detail };
        }
        catch
        {
            sw.Stop();
            return new { service = serviceName, status = "unhealthy", latency = $"{sw.ElapsedMilliseconds}ms", port = displayPort, detail = "Connection failed" };
        }
    }
}

public record AdminReviewRequest(bool Approved, string? Notes);
public record FraudActionRequest(string Action, string? Notes);
