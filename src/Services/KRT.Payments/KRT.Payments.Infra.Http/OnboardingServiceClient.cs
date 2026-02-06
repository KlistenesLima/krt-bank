using System.Net.Http.Json;
using KRT.Payments.Application.DTOs;
using KRT.Payments.Application.Services;
using Microsoft.Extensions.Logging;

namespace KRT.Payments.Infra.Http;

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
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/accounts/{accountId}/debit", new { Amount = amount, Reason = reason });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AccountOperationResponse>();
                return result ?? new AccountOperationResponse(false, "Resposta vazia", 0);
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Débito falhou {AccountId}: {StatusCode}", accountId, response.StatusCode);
            return new AccountOperationResponse(false, error, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro HTTP ao debitar {AccountId}", accountId);
            return new AccountOperationResponse(false, ex.Message, 0);
        }
    }

    public async Task<AccountOperationResponse> CreditAccountAsync(Guid accountId, decimal amount, string reason)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/accounts/{accountId}/credit", new { Amount = amount, Reason = reason });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AccountOperationResponse>();
                return result ?? new AccountOperationResponse(false, "Resposta vazia", 0);
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Crédito falhou {AccountId}: {StatusCode}", accountId, response.StatusCode);
            return new AccountOperationResponse(false, error, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro HTTP ao creditar {AccountId}", accountId);
            return new AccountOperationResponse(false, ex.Message, 0);
        }
    }
}
