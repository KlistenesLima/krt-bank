using System.Net.Http.Json;
using KRT.Payments.Application.Interfaces;
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

    public async Task<AccountOperationResult> DebitAccountAsync(Guid accountId, decimal amount, string reason, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/accounts/{accountId}/debit",
                new { Amount = amount, Reason = reason }, ct);
            var result = await response.Content.ReadFromJsonAsync<AccountOperationResult>(cancellationToken: ct);
            return result ?? new AccountOperationResult(false, "Null response", 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to debit account {AccountId}", accountId);
            return new AccountOperationResult(false, ex.Message, 0);
        }
    }

    public async Task<AccountOperationResult> CreditAccountAsync(Guid accountId, decimal amount, string reason, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/accounts/{accountId}/credit",
                new { Amount = amount, Reason = reason }, ct);
            var result = await response.Content.ReadFromJsonAsync<AccountOperationResult>(cancellationToken: ct);
            return result ?? new AccountOperationResult(false, "Null response", 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to credit account {AccountId}", accountId);
            return new AccountOperationResult(false, ex.Message, 0);
        }
    }
}
