using KRT.Payments.Api.Data;
using KRT.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KRT.Payments.Api.Workers;

/// <summary>
/// Worker que compensa boletos após 1 minuto de processamento.
/// Simula o prazo real de compensação bancária (D+1~D+3) de forma acelerada.
/// </summary>
public class BoletoCompensationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BoletoCompensationWorker> _logger;
    private static readonly TimeSpan CompensationDelay = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);

    public BoletoCompensationWorker(IServiceScopeFactory scopeFactory, ILogger<BoletoCompensationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BoletoCompensationWorker started (delay: {Delay})", CompensationDelay);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingCompensations(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no BoletoCompensationWorker");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingCompensations(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

        var cutoff = DateTime.UtcNow - CompensationDelay;

        // 1. Compensar BoletoCharges (cobranças via gateway/storefront)
        var charges = await db.BoletoCharges
            .Where(b => b.Status == BoletoChargeStatus.Processing && b.PaidAt != null && b.PaidAt <= cutoff)
            .ToListAsync(ct);

        foreach (var boleto in charges)
        {
            boleto.Status = BoletoChargeStatus.Confirmed;
            _logger.LogInformation("BoletoCharge {Id} compensado (pago em {PaidAt})", boleto.Id, boleto.PaidAt);

            if (!string.IsNullOrEmpty(boleto.WebhookUrl))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                        await http.PostAsJsonAsync(boleto.WebhookUrl, new
                        {
                            chargeId = boleto.Id,
                            externalId = boleto.ExternalId,
                            status = "Confirmed",
                            paidAt = boleto.PaidAt,
                            amount = boleto.Amount,
                            method = "boleto"
                        });
                    }
                    catch { }
                }, ct);
            }
        }

        // 2. Compensar Boletos internos (gerados pelo banco)
        var boletos = await db.Boletos
            .Where(b => b.Status == BoletoStatus.Processing && b.PaidAt != null && b.PaidAt <= cutoff)
            .ToListAsync(ct);

        foreach (var b in boletos)
        {
            b.Compensate();
            _logger.LogInformation("Boleto {Id} compensado (pago em {PaidAt})", b.Id, b.PaidAt);
        }

        var total = charges.Count + boletos.Count;
        if (total > 0)
        {
            _logger.LogInformation("Compensados {Count} boleto(s) no total", total);
            await db.SaveChangesAsync(ct);
        }
    }
}
