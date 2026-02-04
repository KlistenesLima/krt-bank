using FluentValidation;
using KRT.Onboarding.Application.Accounts.DTOs.Requests;

namespace KRT.Onboarding.Application.Validations;

public class CreateAccountValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().Length(3, 100);
        RuleFor(x => x.CustomerDocument).NotEmpty().Length(11, 14); // CPF/CNPJ
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress();
    }
}

public class TransferValidator : AbstractValidator<TransferRequest>
{
    public TransferValidator()
    {
        RuleFor(x => x.SourceAccountId).NotEmpty();
        RuleFor(x => x.DestinationAccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
