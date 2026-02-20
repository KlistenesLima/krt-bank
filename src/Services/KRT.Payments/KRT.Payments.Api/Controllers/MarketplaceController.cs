using KRT.Payments.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/marketplace")]
public class MarketplaceController : ControllerBase
{
    private readonly PaymentsDbContext _db;
    public MarketplaceController(PaymentsDbContext db) => _db = db;

    private async Task EnsurePoints(Guid accountId)
    {
        if (!await _db.UserPointsTable.AnyAsync(p => p.AccountId == accountId))
        {
            _db.UserPointsTable.Add(new UserPoints { AccountId = accountId, Balance = 2450, TotalEarned = 3200, TotalRedeemed = 750 });
            await _db.SaveChangesAsync();
        }
    }

    [HttpGet("offers")]
    [AllowAnonymous]
    public IActionResult GetOffers()
    {
        return Ok(new[]
        {
            new { id = "cashback-5", name = "5% Cashback iFood", description = "Ganhe 5% de volta em pedidos iFood", pointsCost = 500, category = "Cashback", partner = "iFood", icon = "food" },
            new { id = "cashback-10", name = "10% Cashback Amazon", description = "Desconto em compras na Amazon", pointsCost = 1000, category = "Cashback", partner = "Amazon", icon = "shopping" },
            new { id = "movie-ticket", name = "Ingresso Cinema", description = "1 ingresso para qualquer filme", pointsCost = 800, category = "Entretenimento", partner = "Cinemark", icon = "movie" },
            new { id = "spotify-1m", name = "1 Mes Spotify Premium", description = "Assinatura mensal gratuita", pointsCost = 1500, category = "Assinatura", partner = "Spotify", icon = "music" },
            new { id = "uber-20", name = "R$ 20 Uber", description = "Credito para corridas Uber", pointsCost = 600, category = "Transporte", partner = "Uber", icon = "car" }
        });
    }

    [HttpGet("{accountId}/points")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPoints(Guid accountId)
    {
        await EnsurePoints(accountId);
        var points = await _db.UserPointsTable.FindAsync(accountId);
        return Ok(points);
    }

    [HttpPost("{accountId}/redeem")]
    [AllowAnonymous]
    public async Task<IActionResult> Redeem(Guid accountId, [FromBody] RedeemRequest req)
    {
        await EnsurePoints(accountId);
        var points = await _db.UserPointsTable.FindAsync(accountId);
        if (points!.Balance < req.PointsCost)
            return BadRequest(new { error = "Pontos insuficientes" });
        points.Balance -= req.PointsCost;
        points.TotalRedeemed += req.PointsCost;
        await _db.SaveChangesAsync();
        return Ok(new { message = $"Resgate de '{req.OfferName}' realizado!", remainingPoints = points.Balance, code = $"KRT-{Guid.NewGuid().ToString()[..8].ToUpper()}" });
    }

    [HttpGet("{accountId}/history")]
    [AllowAnonymous]
    public IActionResult GetHistory(Guid accountId)
    {
        return Ok(new[]
        {
            new { date = DateTime.UtcNow.AddDays(-2), type = "earn", points = 150, description = "Pix enviado - bonus" },
            new { date = DateTime.UtcNow.AddDays(-5), type = "earn", points = 300, description = "Pagamento de boleto" },
            new { date = DateTime.UtcNow.AddDays(-7), type = "redeem", points = -500, description = "Resgate: 5% Cashback iFood" },
            new { date = DateTime.UtcNow.AddDays(-10), type = "earn", points = 200, description = "Recarga celular" }
        });
    }
}

public class UserPoints { public Guid AccountId { get; set; } public int Balance { get; set; } public int TotalEarned { get; set; } public int TotalRedeemed { get; set; } }
public record RedeemRequest(string OfferId, string OfferName, int PointsCost);
