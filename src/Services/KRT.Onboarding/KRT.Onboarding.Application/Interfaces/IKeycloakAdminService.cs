namespace KRT.Onboarding.Application.Interfaces;

public record KeycloakUserResult(bool Success, string? UserId, string? Error);
public record KeycloakTokenResult(bool Success, string? AccessToken, string? RefreshToken, int ExpiresIn, string? Error);

public interface IKeycloakAdminService
{
    Task<KeycloakUserResult> CreateUserAsync(string username, string email, string firstName, string password, CancellationToken ct = default);
    Task<KeycloakTokenResult> LoginAsync(string username, string password, CancellationToken ct = default);
}
