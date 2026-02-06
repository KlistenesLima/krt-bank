using FluentValidation;
using KRT.Payments.Application.Commands;

namespace KRT.Payments.Application.Validators;

public class ProcessPixCommandValidator : AbstractValidator<ProcessPixCommand>
{
    public ProcessPixCommandValidator()
    {
        RuleFor(x => x.SourceAccountId)
            .NotEmpty().WithMessage("Conta de origem é obrigatória.");

        RuleFor(x => x.DestinationAccountId)
            .NotEmpty().WithMessage("Conta de destino é obrigatória.")
            .NotEqual(x => x.SourceAccountId).WithMessage("Contas de origem e destino devem ser diferentes.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("O valor deve ser positivo.")
            .LessThanOrEqualTo(100_000m).WithMessage("Valor máximo por transação: R$ 100.000.");

        RuleFor(x => x.PixKey)
            .NotEmpty().WithMessage("Chave Pix é obrigatória.")
            .MaximumLength(100).WithMessage("Chave Pix inválida.");
    }
}
