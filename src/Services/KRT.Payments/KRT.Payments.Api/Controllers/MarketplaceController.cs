using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/marketplace")]
public class MarketplaceController : ControllerBase
{
    private static readonly ConcurrentDictionary<Guid, UserPoints> _points = new();

    [HttpGet("offers")]
    [AllowAnonymous]
    public IActionResult GetOffers()
    {
        return Ok(new[]
        {
            new { id = "cb-ifood", partner = "iFood", type = "cashback", value = "10% cashback", description = "Ganhe 10% de volta em pedidos acima de R$ 30", minPurchase = 30m, icon = "🍕", expiresAt = DateTime.UtcNow.AddDays(15), category = "alimentacao" },
            new { id = "cb-uber", partner = "Uber", type = "cashback", value = "15% cashback", description = "15% de volta nas corridas ate R$ 50", minPurchase = 0m, icon = "🚗", expiresAt = DateTime.UtcNow.AddDays(10), category = "transporte" },
            new { id = "cp-netflix", partner = "Netflix", type = "cupom", value = "R$ 20 OFF", description = "Desconto de R$ 20 na assinatura mensal", minPurchase = 0m, icon = "🎬", expiresAt = DateTime.UtcNow.AddDays(30), category = "entretenimento" },
            new { id = "cp-amazon", partner = "Amazon", type = "cupom", value = "R$ 50 OFF", description = "R$ 50 de desconto em compras acima de R$ 200", minPurchase = 200m, icon = "📦", expiresAt = DateTime.UtcNow.AddDays(20), category = "compras" },
            new { id = "cb-shell", partner = "Shell", type = "cashback", value = "5% cashback", description = "5% de volta em abastecimentos", minPurchase = 50m, icon = "⛽", expiresAt = DateTime.UtcNow.AddDays(45), category = "transporte" },
            new { id = "cb-drogasil", partner = "Drogasil", type = "cashback", value = "8% cashback", description = "8% de volta em compras na farmacia", minPurchase = 20m, icon = "💊", expiresAt = DateTime.UtcNow.AddDays(25), category = "saude" },
            new { id = "pt-spotify", partner = "Spotify", type = "pontos", value = "500 pontos", description = "Troque 500 pontos por 1 mes gratis", minPurchase = 0m, icon = "🎵", expiresAt = DateTime.UtcNow.AddDays(60), category = "entretenimento" },
            new { id = "pt-cinema", partner = "Cinemark", type = "pontos", value = "300 pontos", description = "Troque por 1 ingresso de cinema", minPurchase = 0m, icon = "🎬", expiresAt = DateTime.UtcNow.AddDays(30), category = "entretenimento" }
        });
    }

    [HttpGet("{accountId}/points")]
    [AllowAnonymous]
    public IActionResult GetPoints(Guid accountId)
    {
        var p = _points.GetOrAdd(accountId, _ => new UserPoints { AccountId = accountId, Balance = 2450, TotalEarned = 5200, TotalRedeemed = 2750 });
        return Ok(p);
    }

    [HttpPost("{accountId}/redeem")]
    [AllowAnonymous]
    public IActionResult RedeemOffer(Guid accountId, [FromBody] RedeemRequest req)
    {
        var p = _points.GetOrAdd(accountId, _ => new UserPoints { AccountId = accountId, Balance = 2450, TotalEarned = 5200, TotalRedeemed = 2750 });
        if (req.PointsCost > 0 && p.Balance < req.PointsCost)
            return BadRequest(new { error = "Pontos insuficientes" });

        if (req.PointsCost > 0) { p.Balance -= req.PointsCost; p.TotalRedeemed += req.PointsCost; }

        return Ok(new
        {
            message = $"Oferta resgatada! {req.OfferName}",
            code = $"KRT-{new Random().Next(100000, 999999)}",
            pointsRemaining = p.Balance,
            expiresAt = DateTime.UtcNow.AddDays(30)
        });
    }

    [HttpGet("{accountId}/history")]
    [AllowAnonymous]
    public IActionResult GetHistory(Guid accountId)
    {
        return Ok(new[]
        {
            new { action = "Cashback iFood", points = 85, type = "earned", date = DateTime.UtcNow.AddDays(-1) },
            new { action = "Pix enviado (bonus)", points = 10, type = "earned", date = DateTime.UtcNow.AddDays(-2) },
            new { action = "Resgate Netflix", points = -500, type = "redeemed", date = DateTime.UtcNow.AddDays(-5) },
            new { action = "Cashback Uber", points = 120, type = "earned", date = DateTime.UtcNow.AddDays(-7) },
            new { action = "Bonus indicacao", points = 200, type = "earned", date = DateTime.UtcNow.AddDays(-10) }
        });
    }
}

public class UserPoints { public Guid AccountId { get; set; } public int Balance { get; set; } public int TotalEarned { get; set; } public int TotalRedeemed { get; set; } }
public record RedeemRequest(string OfferId, string OfferName, int PointsCost);