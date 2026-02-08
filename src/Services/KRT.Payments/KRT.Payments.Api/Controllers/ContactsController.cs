using KRT.Payments.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/contacts")]
public class ContactsController : ControllerBase
{
    private static readonly ConcurrentDictionary<Guid, List<PixContact>> _store = new();

    private static List<PixContact> GetOrCreate(Guid accountId)
    {
        return _store.GetOrAdd(accountId, _ => GenerateSeedContacts(accountId));
    }

    [HttpGet("{accountId}")]
    [AllowAnonymous]
    public IActionResult GetContacts(Guid accountId, [FromQuery] bool? favoritesOnly, [FromQuery] string? search)
    {
        var items = GetOrCreate(accountId).AsEnumerable();
        if (favoritesOnly == true) items = items.Where(c => c.IsFavorite);
        if (!string.IsNullOrEmpty(search))
            items = items.Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || c.PixKey.Contains(search, StringComparison.OrdinalIgnoreCase)
                || (c.Nickname?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));

        var list = items.OrderByDescending(c => c.IsFavorite).ThenByDescending(c => c.TransferCount).ToList();
        return Ok(new
        {
            accountId,
            total = list.Count,
            favorites = list.Count(c => c.IsFavorite),
            contacts = list.Select(c => new
            {
                c.Id, c.Name, c.PixKey, c.PixKeyType, c.BankName, c.Nickname,
                c.IsFavorite, c.TransferCount, c.LastTransferAt, c.CreatedAt,
                displayName = c.GetDisplayName(),
                initials = string.Join("", c.Name.Split(' ').Where(w => w.Length > 0).Take(2).Select(w => w[0])).ToUpper()
            })
        });
    }

    [HttpPost("{accountId}")]
    [AllowAnonymous]
    public IActionResult AddContact(Guid accountId, [FromBody] AddContactRequest request)
    {
        try
        {
            var contacts = GetOrCreate(accountId);
            if (contacts.Any(c => c.PixKey.Equals(request.PixKey, StringComparison.OrdinalIgnoreCase)))
                return Conflict(new { error = "Contato com esta chave Pix ja existe" });

            var contact = PixContact.Create(accountId, request.Name, request.PixKey, request.PixKeyType ?? "CPF", request.BankName, request.Nickname);
            contacts.Add(contact);
            return Created("", new { contact.Id, message = "Contato adicionado" });
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("{accountId}/{contactId}")]
    [AllowAnonymous]
    public IActionResult UpdateContact(Guid accountId, Guid contactId, [FromBody] UpdateContactRequest request)
    {
        var contact = GetOrCreate(accountId).FirstOrDefault(c => c.Id == contactId);
        if (contact == null) return NotFound(new { error = "Contato nao encontrado" });
        contact.Update(request.Name, request.Nickname, request.BankName);
        return Ok(new { message = "Contato atualizado" });
    }

    [HttpPost("{accountId}/{contactId}/favorite")]
    [AllowAnonymous]
    public IActionResult ToggleFavorite(Guid accountId, Guid contactId)
    {
        var contact = GetOrCreate(accountId).FirstOrDefault(c => c.Id == contactId);
        if (contact == null) return NotFound(new { error = "Contato nao encontrado" });
        contact.ToggleFavorite();
        return Ok(new { message = contact.IsFavorite ? "Adicionado aos favoritos" : "Removido dos favoritos", contact.IsFavorite });
    }

    [HttpDelete("{accountId}/{contactId}")]
    [AllowAnonymous]
    public IActionResult DeleteContact(Guid accountId, Guid contactId)
    {
        var contacts = GetOrCreate(accountId);
        var removed = contacts.RemoveAll(c => c.Id == contactId);
        return removed > 0 ? Ok(new { message = "Contato removido" }) : NotFound(new { error = "Contato nao encontrado" });
    }

    private static List<PixContact> GenerateSeedContacts(Guid accountId)
    {
        var seeds = new (string name, string key, string type, string bank, bool fav, int count)[]
        {
            ("Maria Silva", "maria@email.com", "EMAIL", "Nubank", true, 15),
            ("Joao Santos", "123.456.789-00", "CPF", "Bradesco", true, 8),
            ("Ana Oliveira", "+5583999887766", "PHONE", "Itau", true, 12),
            ("Carlos Souza", "carlos.souza@pix.com", "EMAIL", "BB", false, 3),
            ("Pedro Lima", "987.654.321-00", "CPF", "Santander", false, 5),
            ("Julia Costa", "+5583988776655", "PHONE", "C6 Bank", false, 2),
            ("Supermercado ABC", "12345678000190", "CNPJ", "Caixa", false, 20),
            ("Farmacia Popular", "98765432000111", "CNPJ", "Inter", false, 6)
        };
        return seeds.Select(s =>
        {
            var c = PixContact.Create(accountId, s.name, s.key, s.type, s.bank, null);
            if (s.fav) c.ToggleFavorite();
            for (int i = 0; i < s.count; i++) c.RecordTransfer();
            return c;
        }).ToList();
    }
}

public record AddContactRequest(string Name, string PixKey, string? PixKeyType, string? BankName, string? Nickname);
public record UpdateContactRequest(string Name, string? Nickname, string? BankName);