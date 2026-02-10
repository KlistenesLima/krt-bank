using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Enums;
using KRT.Onboarding.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KRT.Onboarding.Api.Controllers;

[ApiController]
[Route("api/v1/pix-keys")]
[Authorize]
public class PixKeysController : ControllerBase
{
    private readonly IPixKeyRepository _pixKeyRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<PixKeysController> _logger;
    private const int MaxKeysPerAccount = 5;

    public PixKeysController(
        IPixKeyRepository pixKeyRepository,
        IAccountRepository accountRepository,
        ILogger<PixKeysController> logger)
    {
        _pixKeyRepository = pixKeyRepository;
        _accountRepository = accountRepository;
        _logger = logger;
    }

    /// <summary>
    /// Registrar nova chave PIX
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterPixKeyRequest request,
        CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return NotFound(new { error = "Conta não encontrada." });

        var currentCount = await _pixKeyRepository.CountByAccountIdAsync(request.AccountId, ct);
        if (currentCount >= MaxKeysPerAccount)
            return BadRequest(new { error = $"Limite de {MaxKeysPerAccount} chaves PIX por conta atingido." });

        if (!Enum.TryParse<PixKeyType>(request.KeyType, true, out var keyType))
            return BadRequest(new { error = $"Tipo de chave inválido: {request.KeyType}. Use: Cpf, Email, Phone, Random." });

        var keyValue = keyType == PixKeyType.Random
            ? Guid.NewGuid().ToString("N")[..32]
            : request.KeyValue!;

        if (await _pixKeyRepository.ExistsAsync(keyType, keyValue, ct))
            return Conflict(new { error = "Esta chave PIX já está registrada para outra conta." });

        if (keyType == PixKeyType.Cpf)
        {
            var cpfDigits = new string(keyValue.Where(char.IsDigit).ToArray());
            var accountCpf = new string(account.Document.Where(char.IsDigit).ToArray());
            if (cpfDigits != accountCpf)
                return BadRequest(new { error = "O CPF informado deve ser o do titular da conta." });
        }

        try
        {
            var pixKey = PixKey.Create(request.AccountId, keyType, keyValue);
            await _pixKeyRepository.AddAsync(pixKey, ct);
            await _pixKeyRepository.SaveChangesAsync(ct);

            _logger.LogInformation("Chave PIX {KeyType} registrada para conta {AccountId}", keyType, request.AccountId);

            return Created($"/api/v1/pix-keys/{pixKey.Id}", new PixKeyResponse
            {
                Id = pixKey.Id,
                AccountId = pixKey.AccountId,
                KeyType = pixKey.KeyType.ToString(),
                KeyValue = pixKey.KeyValue,
                CreatedAt = pixKey.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar chave PIX");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Desativar chave PIX (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var pixKey = await _pixKeyRepository.GetByIdAsync(id, ct);
        if (pixKey is null)
            return NotFound(new { error = "Chave PIX não encontrada." });

        try
        {
            pixKey.Deactivate();
            _pixKeyRepository.Update(pixKey);
            await _pixKeyRepository.SaveChangesAsync(ct);

            _logger.LogInformation("Chave PIX {KeyId} desativada", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Listar chaves PIX de uma conta
    /// </summary>
    [HttpGet("account/{accountId:guid}")]
    public async Task<IActionResult> GetByAccount(Guid accountId, CancellationToken ct)
    {
        var keys = await _pixKeyRepository.GetByAccountIdAsync(accountId, ct);
        var response = keys.Select(pk => new PixKeyResponse
        {
            Id = pk.Id,
            AccountId = pk.AccountId,
            KeyType = pk.KeyType.ToString(),
            KeyValue = pk.KeyValue,
            CreatedAt = pk.CreatedAt
        });
        return Ok(response);
    }

    /// <summary>
    /// Resolver chave PIX → conta destino (endpoint critico para transferencias)
    /// </summary>
    [HttpGet("resolve")]
    public async Task<IActionResult> Resolve(
        [FromQuery] string type,
        [FromQuery] string value,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
            return BadRequest(new { error = "Parâmetros 'type' e 'value' são obrigatórios." });

        if (!Enum.TryParse<PixKeyType>(type, true, out var keyType))
            return BadRequest(new { error = $"Tipo de chave inválido: {type}." });

        var pixKey = await _pixKeyRepository.GetByKeyAsync(keyType, value, ct);
        if (pixKey is null)
            return NotFound(new { error = "Chave PIX não encontrada ou inativa." });

        return Ok(new ResolvePixKeyResponse
        {
            AccountId = pixKey.AccountId,
            OwnerName = MaskName(pixKey.Account?.CustomerName ?? ""),
            KeyType = pixKey.KeyType.ToString(),
            KeyValue = MaskKeyValue(pixKey.KeyType, pixKey.KeyValue)
        });
    }

    private static string MaskName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3) return "***";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0][..2] + new string('*', parts[0].Length - 2);
        return parts[0] + " " + parts[^1][0] + "***";
    }

    private static string MaskKeyValue(PixKeyType keyType, string value)
    {
        return keyType switch
        {
            PixKeyType.Cpf when value.Length == 11 => $"***.{value[3..6]}.***-{value[9..]}",
            PixKeyType.Email when value.Contains('@') => value[..2] + "***@" + value.Split('@')[1],
            PixKeyType.Phone when value.Length >= 8 => value[..5] + "****" + value[^2..],
            _ => value[..Math.Min(4, value.Length)] + "****"
        };
    }
}

// ── DTOs ──
public record RegisterPixKeyRequest
{
    public Guid AccountId { get; init; }
    public string KeyType { get; init; } = string.Empty;
    public string? KeyValue { get; init; }
}

public record PixKeyResponse
{
    public Guid Id { get; init; }
    public Guid AccountId { get; init; }
    public string KeyType { get; init; } = string.Empty;
    public string KeyValue { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record ResolvePixKeyResponse
{
    public Guid AccountId { get; init; }
    public string OwnerName { get; init; } = string.Empty;
    public string KeyType { get; init; } = string.Empty;
    public string KeyValue { get; init; } = string.Empty;
}

