using FluentValidation;
using MediatR;

namespace KRT.Onboarding.Application.Commands;

public class CreateAccountCommand : IRequest<CommandResult>
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerDocument { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string BranchCode { get; set; } = "0001";
}

public class CommandResult
{
    public bool IsValid { get; set; }
    public Guid Id { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new();

    public static CommandResult Success(Guid id) => new() { IsValid = true, Id = id };
    public static CommandResult Fail(Dictionary<string, string[]> errors) => new() { IsValid = false, Errors = errors };
}

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MinimumLength(3);
        RuleFor(x => x.CustomerDocument).NotEmpty().CreditCard().When(x => false); // Mock validação simples
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress();
    }
}
