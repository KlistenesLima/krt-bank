using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using KRT.Onboarding.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KRT.Onboarding.Api.Services;

public class KeycloakAdminService : IKeycloakAdminService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<KeycloakAdminService> _logger;

    private string BaseUrl => _config["Keycloak:BaseUrl"] ?? "http://localhost:8080";
    private string Realm => _config["Keycloak:Realm"] ?? "krt-bank";
    private string AdminUser => _config["Keycloak:AdminUser"] ?? "admin";
    private string AdminPassword => _config["Keycloak:AdminPassword"] ?? "admin";
    private string ClientId => _config["Keycloak:ClientId"] ?? "krt-web";
    private string ClientSecret => _config["Keycloak:ClientSecret"] ?? "";

    public KeycloakAdminService(HttpClient httpClient, IConfiguration config, ILogger<KeycloakAdminService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<KeycloakUserResult> CreateUserAsync(
        string username, string email, string firstName, string password, CancellationToken ct = default)
    {
        try
        {
            // 1. Get admin token
            var adminToken = await GetAdminTokenAsync(ct);
            if (adminToken == null)
                return new KeycloakUserResult(false, null, "Falha ao obter token admin do Keycloak");

            // 2. Create user
            var userPayload = new
            {
                username = username,
                email = email,
                firstName = firstName.Split(' ')[0],
                lastName = firstName.Contains(' ') ? string.Join(" ", firstName.Split(' ').Skip(1)) : firstName,
                enabled = true,
                emailVerified = true,
                credentials = new[]
                {
                    new { type = "password", value = password, temporary = false }
                }
            };

            var json = JsonSerializer.Serialize(userPayload);
            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/admin/realms/{Realm}/users")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var response = await _httpClient.SendAsync(request, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                return new KeycloakUserResult(false, null, "Usuário já existe no Keycloak");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Keycloak create user failed: {Status} {Error}", response.StatusCode, error);
                return new KeycloakUserResult(false, null, $"Keycloak error: {response.StatusCode}");
            }

            // 3. Get user ID from Location header
            var location = response.Headers.Location?.ToString() ?? "";
            var userId = location.Split('/').LastOrDefault() ?? "";

            _logger.LogInformation("Keycloak user created: {Username} ({UserId})", username, userId);
            return new KeycloakUserResult(true, userId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Keycloak user");
            return new KeycloakUserResult(false, null, ex.Message);
        }
    }

    public async Task<KeycloakTokenResult> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        try
        {
            var tokenUrl = $"{BaseUrl}/realms/{Realm}/protocol/openid-connect/token";

            var formData = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = ClientId,
                ["username"] = username,
                ["password"] = password
            };

            if (!string.IsNullOrEmpty(ClientSecret))
                formData["client_secret"] = ClientSecret;

            var response = await _httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(formData), ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Keycloak login failed for {User}: {Status}", username, response.StatusCode);
                return new KeycloakTokenResult(false, null, null, 0, "CPF ou senha inválidos");
            }

            var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(responseBody);
            return new KeycloakTokenResult(
                true,
                tokenResponse?.AccessToken,
                tokenResponse?.RefreshToken,
                tokenResponse?.ExpiresIn ?? 0,
                null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Keycloak login");
            return new KeycloakTokenResult(false, null, null, 0, ex.Message);
        }
    }

    private async Task<string?> GetAdminTokenAsync(CancellationToken ct)
    {
        var tokenUrl = $"{BaseUrl}/realms/master/protocol/openid-connect/token";
        var formData = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = AdminUser,
            ["password"] = AdminPassword
        };

        var response = await _httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(formData), ct);
        if (!response.IsSuccessStatusCode) return null;

        var body = await response.Content.ReadAsStringAsync(ct);
        var token = JsonSerializer.Deserialize<KeycloakTokenResponse>(body);
        return token?.AccessToken;
    }
}

internal class KeycloakTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

