using FluentValidation;
using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Domain.Interfaces;
using MediatR;

namespace KRT.Onboarding.Application.Commands;

public class CreateAccountCommand : IRequest<CommandResult>
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerDocument { get; set; } = string.Empty;  // CPF
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BranchCode { get; set; } = "0001";
}

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    private readonly IAccountRepository _repository;

    public CreateAccountCommandValidator(IAccountRepository repository)
    {
        _repository = repository;

        // === NOME ===
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres")
            .Matches(@"^[a-zA-ZÀ-ÿ\s]+$").WithMessage("Nome deve conter apenas letras");

        // === CPF ===
        RuleFor(x => x.CustomerDocument)
            .NotEmpty().WithMessage("CPF é obrigatório")
            .Must(BeValidCpf).WithMessage("CPF inválido")
            .MustAsync(BeUniqueCpf).WithMessage("CPF já cadastrado");

        // === EMAIL ===
        RuleFor(x => x.CustomerEmail)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Formato de email inválido")
            .MustAsync(BeUniqueEmail).WithMessage("Email já cadastrado");

        // === TELEFONE ===
        RuleFor(x => x.CustomerPhone)
            .NotEmpty().WithMessage("Telefone é obrigatório")
            .Must(BeValidPhone).WithMessage("Telefone inválido. Use (XX) XXXXX-XXXX");

        // === SENHA ===
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres")
            .MaximumLength(50).WithMessage("Senha deve ter no máximo 50 caracteres");
    }

    // === VALIDAÇÃO DE CPF COM DÍGITOS VERIFICADORES ===
    private bool BeValidCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;

        // Remove formatação
        cpf = cpf.Replace(".", "").Replace("-", "").Trim();

        if (cpf.Length != 11) return false;
        if (!cpf.All(char.IsDigit)) return false;

        // Rejeita CPFs com todos os dígitos iguais
        if (cpf.Distinct().Count() == 1) return false;

        // Cálculo do 1º dígito verificador
        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += (cpf[i] - '0') * (10 - i);

        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;

        if ((cpf[9] - '0') != digit1) return false;

        // Cálculo do 2º dígito verificador
        sum = 0;
        for (var i = 0; i < 10; i++)
            sum += (cpf[i] - '0') * (11 - i);

        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;

        return (cpf[10] - '0') == digit2;
    }

    // === VALIDAÇÃO DE TELEFONE ===
    private bool BeValidPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length == 10 || digits.Length == 11; // fixo ou celular
    }

    // === UNICIDADE CPF ===
    private async Task<bool> BeUniqueCpf(string cpf, CancellationToken ct)
    {
        var cleanCpf = cpf?.Replace(".", "").Replace("-", "").Trim() ?? "";
        var existing = await _repository.GetByCpfAsync(cleanCpf, ct);
        return existing == null;
    }

    // === UNICIDADE EMAIL ===
    private async Task<bool> BeUniqueEmail(string email, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email)) return true;
        var existing = await _repository.GetByEmailAsync(email.Trim().ToLower(), ct);
        return existing == null;
    }
}
