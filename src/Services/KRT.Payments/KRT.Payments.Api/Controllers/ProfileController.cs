using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/profile")]
public class ProfileController : ControllerBase
{
    private static readonly ConcurrentDictionary<Guid, UserProfile> _store = new();

    private static UserProfile GetOrCreate(Guid accountId)
    {
        return _store.GetOrAdd(accountId, id => new UserProfile
        {
            AccountId = id,
            Name = "Usuario KRT Bank",
            Email = $"usuario{id.ToString()[..8]}@email.com",
            Phone = "+55 83 99999-0000",
            Cpf = "***.***.***-00",
            BirthDate = new DateTime(1990, 6, 15),
            Address = new AddressInfo { Street = "Rua Principal, 100", City = "Cajazeiras", State = "PB", ZipCode = "58900-000" },
            Preferences = new UserPreferences(),
            Security = new SecuritySettings(),
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        });
    }

    [HttpGet("{accountId}")]
    [AllowAnonymous]
    public IActionResult GetProfile(Guid accountId) => Ok(GetOrCreate(accountId));

    [HttpPut("{accountId}")]
    [AllowAnonymous]
    public IActionResult UpdateProfile(Guid accountId, [FromBody] UpdateProfileRequest request)
    {
        var p = GetOrCreate(accountId);
        if (!string.IsNullOrWhiteSpace(request.Name)) p.Name = request.Name;
        if (!string.IsNullOrWhiteSpace(request.Phone)) p.Phone = request.Phone;
        if (request.Address != null)
        {
            p.Address.Street = request.Address.Street ?? p.Address.Street;
            p.Address.City = request.Address.City ?? p.Address.City;
            p.Address.State = request.Address.State ?? p.Address.State;
            p.Address.ZipCode = request.Address.ZipCode ?? p.Address.ZipCode;
        }
        p.UpdatedAt = DateTime.UtcNow;
        return Ok(new { message = "Perfil atualizado" });
    }

    [HttpPut("{accountId}/preferences")]
    [AllowAnonymous]
    public IActionResult UpdatePreferences(Guid accountId, [FromBody] UserPreferences prefs)
    {
        var p = GetOrCreate(accountId);
        p.Preferences = prefs;
        p.UpdatedAt = DateTime.UtcNow;
        return Ok(new { message = "Preferencias atualizadas" });
    }

    [HttpPut("{accountId}/security")]
    [AllowAnonymous]
    public IActionResult UpdateSecurity(Guid accountId, [FromBody] UpdateSecurityRequest request)
    {
        var p = GetOrCreate(accountId);
        if (request.TwoFactorEnabled.HasValue) p.Security.TwoFactorEnabled = request.TwoFactorEnabled.Value;
        if (request.BiometricEnabled.HasValue) p.Security.BiometricEnabled = request.BiometricEnabled.Value;
        if (request.TransactionNotifications.HasValue) p.Security.TransactionNotifications = request.TransactionNotifications.Value;
        if (request.LoginNotifications.HasValue) p.Security.LoginNotifications = request.LoginNotifications.Value;
        p.UpdatedAt = DateTime.UtcNow;
        return Ok(new { message = "Seguranca atualizada" });
    }

    [HttpGet("{accountId}/activity")]
    [AllowAnonymous]
    public IActionResult GetActivity(Guid accountId)
    {
        var activities = new[]
        {
            new { action = "Login", device = "Chrome / Windows", ip = "189.xxx.xxx.42", date = DateTime.UtcNow.AddHours(-1) },
            new { action = "Pix enviado", device = "App Android", ip = "189.xxx.xxx.42", date = DateTime.UtcNow.AddHours(-3) },
            new { action = "Senha alterada", device = "Chrome / Windows", ip = "189.xxx.xxx.42", date = DateTime.UtcNow.AddDays(-2) },
            new { action = "Login", device = "App iOS", ip = "200.xxx.xxx.15", date = DateTime.UtcNow.AddDays(-3) },
            new { action = "Cartao virtual criado", device = "Chrome / Windows", ip = "189.xxx.xxx.42", date = DateTime.UtcNow.AddDays(-5) }
        };
        return Ok(activities);
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

public class AddressInfo
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string ZipCode { get; set; } = "";
}

public class UserPreferences
{
    public bool DarkMode { get; set; } = false;
    public string Language { get; set; } = "pt-BR";
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public string Currency { get; set; } = "BRL";
}

public class SecuritySettings
{
    public bool TwoFactorEnabled { get; set; } = false;
    public bool BiometricEnabled { get; set; } = false;
    public bool TransactionNotifications { get; set; } = true;
    public bool LoginNotifications { get; set; } = true;
}

public record UpdateProfileRequest(string? Name, string? Phone, AddressInfo? Address);
public record UpdateSecurityRequest(bool? TwoFactorEnabled, bool? BiometricEnabled, bool? TransactionNotifications, bool? LoginNotifications);