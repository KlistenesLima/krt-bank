using KRT.BuildingBlocks.Domain.Exceptions;

namespace KRT.Onboarding.Domain.Exceptions;

/// <summary>
/// Exceção de domínio concreta para o contexto Onboarding.
/// Herda de BuildingBlocks para ser capturada pelo ExceptionHandlingMiddleware.
/// </summary>
public class OnboardingDomainException : BusinessRuleException
{
    public OnboardingDomainException(string message) : base(message, "OnboardingError") { }
}
