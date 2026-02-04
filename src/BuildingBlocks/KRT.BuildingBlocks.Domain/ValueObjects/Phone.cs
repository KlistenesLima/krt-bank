namespace KRT.BuildingBlocks.Domain.ValueObjects;
public record Phone
{
    public string Value { get; }
    private Phone(string value) => Value = value;
    public static Result<Phone> Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return Result.Fail<Phone>("Telefone vazio.", "EMPTY_PHONE");
        return Result.Ok(new Phone(phone));
    }
    public override string ToString() => Value;
}
