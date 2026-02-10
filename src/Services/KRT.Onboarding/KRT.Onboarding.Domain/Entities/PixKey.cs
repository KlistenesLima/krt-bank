using KRT.BuildingBlocks.Domain;
using KRT.BuildingBlocks.Domain.Exceptions;
using KRT.Onboarding.Domain.Enums;

namespace KRT.Onboarding.Domain.Entities;

public class PixKey : Entity
{
    public Guid AccountId { get; private set; }
    public PixKeyType KeyType { get; private set; }
    public string KeyValue { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }

    // Navigation
    public virtual Account? Account { get; private set; }

    protected PixKey() { }

    private PixKey(Guid accountId, PixKeyType keyType, string keyValue)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        KeyType = keyType;
        KeyValue = NormalizeKeyValue(keyType, keyValue);
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static PixKey Create(Guid accountId, PixKeyType keyType, string keyValue)
    {
        if (accountId == Guid.Empty)
            throw new BusinessRuleException("AccountId é obrigatório.");

        if (string.IsNullOrWhiteSpace(keyValue))
            throw new BusinessRuleException("Valor da chave PIX é obrigatório.");

        ValidateKeyValue(keyType, keyValue);
        return new PixKey(accountId, keyType, keyValue);
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new BusinessRuleException("Chave PIX já está inativa.");
        IsActive = false;
        DeactivatedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        if (IsActive)
            throw new BusinessRuleException("Chave PIX já está ativa.");
        IsActive = true;
        DeactivatedAt = null;
    }

    private static void ValidateKeyValue(PixKeyType keyType, string keyValue)
    {
        switch (keyType)
        {
            case PixKeyType.Cpf:
                var cpfDigits = new string(keyValue.Where(char.IsDigit).ToArray());
                if (cpfDigits.Length != 11)
                    throw new BusinessRuleException("CPF deve ter 11 dígitos.");
                break;
            case PixKeyType.Email:
                if (!keyValue.Contains('@') || !keyValue.Contains('.'))
                    throw new BusinessRuleException("Email inválido.");
                if (keyValue.Length > 77)
                    throw new BusinessRuleException("Email deve ter no máximo 77 caracteres.");
                break;
            case PixKeyType.Phone:
                var phoneDigits = new string(keyValue.Where(char.IsDigit).ToArray());
                if (phoneDigits.Length < 10 || phoneDigits.Length > 11)
                    throw new BusinessRuleException("Telefone deve ter 10 ou 11 dígitos (com DDD).");
                break;
            case PixKeyType.Random:
                if (keyValue.Length > 36)
                    throw new BusinessRuleException("Chave aleatória deve ter no máximo 36 caracteres.");
                break;
            default:
                throw new BusinessRuleException($"Tipo de chave PIX inválido: {keyType}");
        }
    }

    private static string NormalizeKeyValue(PixKeyType keyType, string keyValue)
    {
        return keyType switch
        {
            PixKeyType.Cpf => new string(keyValue.Where(char.IsDigit).ToArray()),
            PixKeyType.Email => keyValue.Trim().ToLowerInvariant(),
            PixKeyType.Phone => "+55" + new string(keyValue.Where(char.IsDigit).ToArray()),
            PixKeyType.Random => keyValue.Trim(),
            _ => keyValue.Trim()
        };
    }
}
