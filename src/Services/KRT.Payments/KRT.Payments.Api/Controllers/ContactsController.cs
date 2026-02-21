using KRT.Payments.Api.Data;
using KRT.Payments.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/contacts")]
public class ContactsController : ControllerBase
{
    private readonly PaymentsDbContext _db;
    public ContactsController(PaymentsDbContext db) => _db = db;

    private async Task EnsureSeed(Guid accountId)
    {
        if (await _db.PixContacts.AnyAsync(c => c.AccountId == accountId)) return;
        var seeds = new[]
        {
            PixContact.Create(accountId, "Maria Silva", "123.456.789-00", "CPF", "Banco Inter", "Maria"),
            PixContact.Create(accountId, "Joao Santos", "joao@email.com", "EMAIL", "Nubank", null),
            PixContact.Create(accountId, "Ana Oliveira", "(11)99999-0001", "PHONE", "Itau", "Aninha"),
            PixContact.Create(accountId, "Carlos Souza", "carlos@empresa.com", "EMAIL", "Bradesco", null),
            PixContact.Create(accountId, "Supermercado ABC", "12.345.678/0001-90", "CNPJ", "Banco do Brasil", "Mercado"),
        };
        seeds[0].ToggleFavorite(); seeds[1].ToggleFavorite(); seeds[4].ToggleFavorite();
        _db.PixContacts.AddRange(seeds);
        await _db.SaveChangesAsync();
    }

    [HttpGet("{accountId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetContacts(Guid accountId, [FromQuery] bool? favoritesOnly, [FromQuery] string? search)
    {
        await EnsureSeed(accountId);
        var query = _db.PixContacts.Where(c => c.AccountId == accountId);
        if (favoritesOnly == true) query = query.Where(c => c.IsFavorite);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(c => EF.Functions.ILike(c.Name, $"%{search}%") || EF.Functions.ILike(c.PixKey, $"%{search}%"));
        var list = await query.OrderByDescending(c => c.IsFavorite).ThenBy(c => c.Name).ToListAsync();
        return Ok(new { contacts = list, total = list.Count });
    }

    [HttpPost("{accountId}")]
    [AllowAnonymous]
    public async Task<IActionResult> Add(Guid accountId, [FromBody] AddContactRequest req)
    {
        try {
            var c = PixContact.Create(accountId, req.Name, req.PixKey, req.PixKeyType, req.BankName, req.Nickname);
            _db.PixContacts.Add(c);
            await _db.SaveChangesAsync();
            return Created("", new { c.Id, message = "Contato adicionado" });
        } catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("{accountId}/{contactId}/favorite")]
    [AllowAnonymous]
    public async Task<IActionResult> ToggleFavorite(Guid accountId, Guid contactId)
    {
        var c = await _db.PixContacts.FirstOrDefaultAsync(x => x.Id == contactId && x.AccountId == accountId);
        if (c == null) return NotFound();
        c.ToggleFavorite();
        await _db.SaveChangesAsync();
        return Ok(new { c.IsFavorite });
    }

    [HttpDelete("{accountId}/{contactId}")]
    [AllowAnonymous]
    public async Task<IActionResult> Delete(Guid accountId, Guid contactId)
    {
        var c = await _db.PixContacts.FirstOrDefaultAsync(x => x.Id == contactId && x.AccountId == accountId);
        if (c == null) return NotFound();
        _db.PixContacts.Remove(c);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Contato removido" });
    }
}

public record AddContactRequest(string Name, string PixKey, string PixKeyType, string? BankName, string? Nickname);