using System.Text.RegularExpressions;
namespace KRT.BuildingBlocks.Domain.ValueObjects;
public record Cpf
{
    public string Value { get; }
    private Cpf(string value) => Value = value;
    public static Result<Cpf> Create(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return Result.Fail<Cpf>("CPF inválido.", "INVALID_CPF");
        if (cpf.Length < 11) return Result.Fail<Cpf>("CPF deve ter 11 dígitos.", "INVALID_CPF_LENGTH");
        return Result.Ok(new Cpf(cpf));
    }
    public override string ToString() => Value;
}
