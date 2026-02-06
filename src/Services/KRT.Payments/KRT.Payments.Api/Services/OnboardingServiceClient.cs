using System.Net.Http.Json;
using System.Text.Json;
using KRT.Payments.Application.DTOs;
using KRT.Payments.Application.Services;

namespace KRT.Payments.Api.Services;

/// <summary>
/// Client HTTP para o servico Onboarding.
/// Implementa chamadas de debito/credito para a Saga.
/// </summary>
public class OnboardingServiceClient : IOnboardingServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OnboardingServiceClient> _logger;

    public OnboardingServiceClient(HttpClient httpClient, ILogger<OnboardingServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AccountOperationResponse> DebitAccountAsync(Guid accountId, decimal amount, string reason)
    {
        try
        {
            var payload = new { Amount = amount, Reason = reason };
            var response = await _httpClient.PostAsJsonAsync(
                string.Format("api/v1/accounts/{0}/debit", accountId), payload);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AccountOperationResponse>();
                return result ?? new AccountOperationResponse(false, "Resposta vazia", 0);
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Debito falhou para conta {AccountId}: {Error}", accountId, error);
            return new AccountOperationResponse(false, error, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao debitar conta {AccountId}", accountId);
            return new AccountOperationResponse(false, ex.Message, 0);
        }
    }

    public async Task<AccountOperationResponse> CreditAccountAsync(Guid accountId, decimal amount, string reason)
    {
        try
        {
            var payload = new { Amount = amount, Reason = reason };
            var response = await _httpClient.PostAsJsonAsync(
                string.Format("api/v1/accounts/{0}/credit", accountId), payload);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AccountOperationResponse>();
                return result ?? new AccountOperationResponse(false, "Resposta vazia", 0);
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Credito falhou para conta {AccountId}: {Error}", accountId, error);
            return new AccountOperationResponse(false, error, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao creditar conta {AccountId}", accountId);
            return new AccountOperationResponse(false, ex.Message, 0);
        }
    }
}
