using FluentValidation;
using KRT.BuildingBlocks.Domain;
using MediatR;

namespace KRT.BuildingBlocks.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior que executa FluentValidation ANTES de cada Handler.
/// Se houver erros, retorna CommandResult.Failure sem chegar ao Handler.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : CommandResult
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            var errors = failures.Select(f => f.ErrorMessage).ToList();
            // Cria CommandResult com erros via reflection para suportar o tipo genérico
            var result = CommandResult.Failure(errors.First());
            foreach (var error in errors.Skip(1))
            {
                result.Errors.Add(error);
            }
            return (TResponse)result;
        }

        return await next();
    }
}
