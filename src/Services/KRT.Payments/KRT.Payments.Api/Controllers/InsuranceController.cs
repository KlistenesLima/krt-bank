using KRT.Payments.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/insurance")]
public class InsuranceController : ControllerBase
{
    private readonly PaymentsDbContext _db;
    public InsuranceController(PaymentsDbContext db) => _db = db;

    [HttpGet("plans")]
    [AllowAnonymous]
    public IActionResult GetPlans()
    {
        return Ok(new[]
        {
            new { id = "pix-protect", name = "Seguro Pix", description = "Protecao contra Pix sob coacao, fraude e roubo de celular", monthlyPrice = 4.90m, coverage = 5000m, icon = "shield",
                benefits = new[] { "Reembolso em caso de coacao", "Protecao contra fraude", "Assistencia juridica", "Cobertura 24h" } },
            new { id = "phone-protect", name = "Seguro Celular", description = "Cobertura completa para seu smartphone contra roubo, furto e quebra", monthlyPrice = 19.90m, coverage = 5000m, icon = "phone",
                benefits = new[] { "Roubo e furto qualificado", "Quebra acidental", "Dano por liquido", "Reparo ou substituicao" } },
            new { id = "life-basic", name = "Seguro Vida Basico", description = "Protecao financeira para voce e sua familia", monthlyPrice = 29.90m, coverage = 100000m, icon = "heart",
                benefits = new[] { "Morte natural ou acidental", "Invalidez permanente", "Assistencia funeral", "Antecipacao doenca grave" } },
            new { id = "card-protect", name = "Seguro Cartao", description = "Protecao para compras com cartao virtual", monthlyPrice = 7.90m, coverage = 10000m, icon = "card",
                benefits = new[] { "Compra protegida 30 dias", "Garantia estendida", "Roubo pos-compra", "Preco protegido" } }
        });
    }

    [HttpGet("{accountId}/policies")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPolicies(Guid accountId)
    {
        var policies = await _db.InsurancePolicies
            .Where(p => p.AccountId == accountId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return Ok(policies);
    }

    [HttpPost("{accountId}/subscribe")]
    [AllowAnonymous]
    public async Task<IActionResult> Subscribe(Guid accountId, [FromBody] SubscribeRequest req)
    {
        if (await _db.InsurancePolicies.AnyAsync(p => p.AccountId == accountId && p.PlanId == req.PlanId && p.Status == "Ativo"))
            return Conflict(new { error = "Voce ja possui este seguro ativo" });

        var policy = new InsurancePolicy
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            PlanId = req.PlanId,
            PlanName = req.PlanName ?? req.PlanId,
            MonthlyPrice = req.MonthlyPrice,
            Coverage = req.Coverage,
            Status = "Ativo",
            StartDate = DateTime.UtcNow,
            NextPayment = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        };
        _db.InsurancePolicies.Add(policy);
        await _db.SaveChangesAsync();
        return Created("", new { policy.Id, message = "Seguro contratado com sucesso" });
    }

    [HttpPost("{accountId}/cancel/{policyId}")]
    [AllowAnonymous]
    public async Task<IActionResult> Cancel(Guid accountId, Guid policyId)
    {
        var policy = await _db.InsurancePolicies.FirstOrDefaultAsync(p => p.Id == policyId && p.AccountId == accountId);
        if (policy == null) return NotFound(new { error = "Apolice nao encontrada" });
        policy.Status = "Cancelado";
        policy.CancelledAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Seguro cancelado" });
    }

    [HttpPost("{accountId}/claim")]
    [AllowAnonymous]
    public IActionResult FileClaim(Guid accountId, [FromBody] ClaimRequest req)
    {
        return Ok(new
        {
            claimId = Guid.NewGuid(),
            message = "Sinistro registrado com sucesso. Analisaremos em ate 48h.",
            status = "Em analise",
            protocol = $"KRT-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(10000, 99999)}"
        });
    }
}

public class InsurancePolicy
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string PlanId { get; set; } = "";
    public string PlanName { get; set; } = "";
    public decimal MonthlyPrice { get; set; }
    public decimal Coverage { get; set; }
    public string Status { get; set; } = "Ativo";
    public DateTime StartDate { get; set; }
    public DateTime NextPayment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}

public record SubscribeRequest(string PlanId, string? PlanName, decimal MonthlyPrice, decimal Coverage);
public record ClaimRequest(Guid PolicyId, string Description, string? EventDate);
