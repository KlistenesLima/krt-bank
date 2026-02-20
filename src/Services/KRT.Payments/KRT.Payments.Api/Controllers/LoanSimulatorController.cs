using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/loans")]
public class LoanSimulatorController : ControllerBase
{
    [HttpGet("rates")]
    [AllowAnonymous]
    public IActionResult GetRates()
    {
        return Ok(new[]
        {
            new { type = "Pessoal", minRate = 1.49, maxRate = 4.99, minMonths = 6, maxMonths = 60, minAmount = 1000m, maxAmount = 100000m },
            new { type = "Consignado", minRate = 0.89, maxRate = 2.30, minMonths = 12, maxMonths = 84, minAmount = 500m, maxAmount = 200000m },
            new { type = "Imobiliario", minRate = 0.65, maxRate = 1.20, minMonths = 60, maxMonths = 420, minAmount = 50000m, maxAmount = 1500000m },
            new { type = "Veicular", minRate = 0.99, maxRate = 2.80, minMonths = 12, maxMonths = 60, minAmount = 5000m, maxAmount = 300000m }
        });
    }

    [HttpPost("simulate")]
    [AllowAnonymous]
    public IActionResult Simulate([FromBody] LoanSimulationRequest req)
    {
        if (req.Amount <= 0 || req.Months <= 0 || req.AnnualRate <= 0)
            return BadRequest(new { error = "Valores devem ser positivos" });

        var monthlyRate = req.AnnualRate / 100.0 / 12.0;

        // Tabela PRICE (parcelas fixas)
        var priceInstallment = req.Amount * (decimal)(monthlyRate * Math.Pow(1 + monthlyRate, req.Months) / (Math.Pow(1 + monthlyRate, req.Months) - 1));
        var priceSchedule = new List<object>();
        var priceBalance = req.Amount;
        var priceTotalPaid = 0m;

        for (int i = 1; i <= req.Months; i++)
        {
            var interest = priceBalance * (decimal)monthlyRate;
            var amortization = priceInstallment - interest;
            priceBalance -= amortization;
            if (priceBalance < 0) priceBalance = 0;
            priceTotalPaid += priceInstallment;
            priceSchedule.Add(new { month = i, installment = Math.Round(priceInstallment, 2), interest = Math.Round(interest, 2), amortization = Math.Round(amortization, 2), balance = Math.Round(priceBalance, 2) });
        }

        // Tabela SAC (amortizacao fixa)
        var sacAmortization = req.Amount / req.Months;
        var sacSchedule = new List<object>();
        var sacBalance = req.Amount;
        var sacTotalPaid = 0m;

        for (int i = 1; i <= req.Months; i++)
        {
            var interest = sacBalance * (decimal)monthlyRate;
            var installment = sacAmortization + interest;
            sacBalance -= sacAmortization;
            if (sacBalance < 0) sacBalance = 0;
            sacTotalPaid += installment;
            sacSchedule.Add(new { month = i, installment = Math.Round(installment, 2), interest = Math.Round(interest, 2), amortization = Math.Round(sacAmortization, 2), balance = Math.Round(sacBalance, 2) });
        }

        return Ok(new
        {
            parameters = new { req.Amount, req.Months, req.AnnualRate, monthlyRate = Math.Round(monthlyRate * 100, 4) },
            price = new
            {
                name = "Tabela Price (parcelas fixas)",
                monthlyInstallment = Math.Round(priceInstallment, 2),
                totalPaid = Math.Round(priceTotalPaid, 2),
                totalInterest = Math.Round(priceTotalPaid - req.Amount, 2),
                effectiveCost = Math.Round((priceTotalPaid / req.Amount - 1) * 100, 2),
                schedule = priceSchedule
            },
            sac = new
            {
                name = "Tabela SAC (amortizacao constante)",
                firstInstallment = sacSchedule.Count > 0 ? sacSchedule[0] : null,
                lastInstallment = sacSchedule.Count > 0 ? sacSchedule[^1] : null,
                totalPaid = Math.Round(sacTotalPaid, 2),
                totalInterest = Math.Round(sacTotalPaid - req.Amount, 2),
                effectiveCost = Math.Round((sacTotalPaid / req.Amount - 1) * 100, 2),
                schedule = sacSchedule
            },
            comparison = new
            {
                priceTotalInterest = Math.Round(priceTotalPaid - req.Amount, 2),
                sacTotalInterest = Math.Round(sacTotalPaid - req.Amount, 2),
                savings = Math.Round((priceTotalPaid - sacTotalPaid), 2),
                recommendation = (priceTotalPaid - sacTotalPaid) > 0 ? "SAC e mais economico no total" : "Price e mais economico no total"
            }
        });
    }
}

public record LoanSimulationRequest(decimal Amount, int Months, double AnnualRate);
