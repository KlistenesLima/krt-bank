using FluentValidation;
using MediatR;
using KRT.BuildingBlocks.Domain;

namespace KRT.Onboarding.Application.Commands;

public class CreateAccountCommand : IRequest<CommandResult>
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerDocument { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string BranchCode { get; set; } = "0001";
}

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres");
        RuleFor(x => x.CustomerDocument).NotEmpty().WithMessage("Documento obrigatório");
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress().WithMessage("Email inválido");
    }
}