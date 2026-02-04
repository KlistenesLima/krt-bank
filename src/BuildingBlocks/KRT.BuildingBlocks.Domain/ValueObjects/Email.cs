using KRT.BuildingBlocks.Domain;

namespace KRT.BuildingBlocks.Domain.ValueObjects;

public record Email
{
    public string Value { get; }
    private Email(string value) => Value = value;

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) 
            return Result.Fail<Email>("Email vazio.", "EMPTY_EMAIL");
        
        // Correção: Usando aspas normais "@"
        if (!email.Contains("@")) 
            return Result.Fail<Email>("Email inválido.", "INVALID_EMAIL");

        return Result.Ok(new Email(email));
    }

    public override string ToString() => Value;
}
