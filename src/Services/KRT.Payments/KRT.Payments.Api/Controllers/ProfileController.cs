using KRT.Payments.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/profile")]
public class ProfileController : ControllerBase
{
    private readonly PaymentsDbContext _db;
    public ProfileController(PaymentsDbContext db) => _db = db;

    private async Task<UserProfile> GetOrCreate(Guid accountId)
    {
        var p = await _db.UserProfiles.FindAsync(accountId);
        if (p != null) return p;
        p = new UserProfile
        {
            AccountId = accountId, Name = "Usuario KRT", Email = "usuario@krtbank.com",
            Phone = "(83) 99999-0000", Cpf = "***.***.***-00", BirthDate = new DateTime(1995, 6, 15),
            Address = new AddressInfo { Street = "Rua Example 123", City = "Cajazeiras", State = "PB" },
            Preferences = new UserPreferences { DarkMode = false, PushNotifications = true, EmailNotifications = true, BiometricLogin = false, Language = "pt-BR" },
            Security = new SecuritySettings { TwoFactorEnabled = false, BiometricEnabled = false, LoginAlerts = true, TransactionAlerts = true },
            CreatedAt = DateTime.UtcNow
        };
        _db.UserProfiles.Add(p);
        await _db.SaveChangesAsync();
        return p;
    }

    [HttpGet("{accountId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProfile(Guid accountId)
    {
        var p = await GetOrCreate(accountId);
        return Ok(p);
    }

    [HttpPut("{accountId}")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateProfile(Guid accountId, [FromBody] UpdateProfileRequest req)
    {
        var p = await GetOrCreate(accountId);
        if (!string.IsNullOrEmpty(req.Name)) p.Name = req.Name;
        if (!string.IsNullOrEmpty(req.Email)) p.Email = req.Email;
        if (!string.IsNullOrEmpty(req.Phone)) p.Phone = req.Phone;
        if (req.Address != null) p.Address = req.Address;
        p.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Perfil atualizado", profile = p });
    }

    [HttpPut("{accountId}/preferences")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdatePreferences(Guid accountId, [FromBody] UserPreferences prefs)
    {
        var p = await GetOrCreate(accountId);
        p.Preferences = prefs;
        p.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Preferencias atualizadas" });
    }

    [HttpPut("{accountId}/security")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateSecurity(Guid accountId, [FromBody] SecuritySettings sec)
    {
        var p = await GetOrCreate(accountId);
        p.Security = sec;
        p.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Configuracoes de seguranca atualizadas" });
    }

    [HttpGet("{accountId}/activity")]
    [AllowAnonymous]
    public IActionResult GetActivity(Guid accountId)
    {
        return Ok(new[]
        {
            new { date = DateTime.UtcNow.AddHours(-1), action = "Login", device = "Chrome - Windows", ip = "189.xxx.xxx.xx", location = "Cajazeiras, PB" },
            new { date = DateTime.UtcNow.AddDays(-1), action = "Pix Enviado", device = "App Android", ip = "189.xxx.xxx.xx", location = "Cajazeiras, PB" },
            new { date = DateTime.UtcNow.AddDays(-2), action = "Alteracao de senha", device = "Chrome - Windows", ip = "189.xxx.xxx.xx", location = "Cajazeiras, PB" },
            new { date = DateTime.UtcNow.AddDays(-3), action = "Login", device = "App iOS", ip = "200.xxx.xxx.xx", location = "Joao Pessoa, PB" }
        });
    }
}

public class UserProfile
{
    public Guid AccountId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Cpf { get; set; } = "";
    public DateTime BirthDate { get; set; }
    public AddressInfo Address { get; set; } = new();
    public UserPreferences Preferences { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AddressInfo { public string Street { get; set; } = ""; public string City { get; set; } = ""; public string State { get; set; } = ""; public string ZipCode { get; set; } = ""; public string Number { get; set; } = ""; }
public class UserPreferences { public bool DarkMode { get; set; } public bool PushNotifications { get; set; } = true; public bool EmailNotifications { get; set; } = true; public bool BiometricLogin { get; set; } public string Language { get; set; } = "pt-BR"; }
public class SecuritySettings { public bool TwoFactorEnabled { get; set; } public bool BiometricEnabled { get; set; } public bool LoginAlerts { get; set; } = true; public bool TransactionAlerts { get; set; } = true; }
public record UpdateProfileRequest(string? Name, string? Email, string? Phone, AddressInfo? Address);
