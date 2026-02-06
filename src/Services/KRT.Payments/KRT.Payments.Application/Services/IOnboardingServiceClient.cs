using KRT.Payments.Application.DTOs;

namespace KRT.Payments.Application.Services;

/// <summary>
/// Client HTTP para comunicacao com o servico de Onboarding (Accounts).
/// Usado pelo Saga Orchestrator para debitar/creditar contas.
/// </summary>
public interface IOnboardingServiceClient
{
    Task<AccountOperationResponse> DebitAccountAsync(Guid accountId, decimal amount, string reason);
    Task<AccountOperationResponse> CreditAccountAsync(Guid accountId, decimal amount, string reason);
}
