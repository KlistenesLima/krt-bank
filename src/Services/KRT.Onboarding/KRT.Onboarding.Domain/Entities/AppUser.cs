using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Domain.Enums;
using KRT.Onboarding.Domain.Exceptions;

namespace KRT.Onboarding.Domain.Entities;

public class AppUser : Entity
{
    public string FullName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Document { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }
    public string? EmailConfirmationCode { get; private set; }
    public DateTime? EmailConfirmationExpiry { get; private set; }
    public string? PasswordResetCode { get; private set; }
    public DateTime? PasswordResetExpiry { get; private set; }
    public string? KeycloakUserId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? ApprovedBy { get; private set; }

    protected AppUser() { } // EF Core

    public static AppUser Create(string fullName, string email, string document, string passwordHash)
    {
        return new AppUser
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email.ToLowerInvariant(),
            Document = document.Replace(".", "").Replace("-", ""),
            PasswordHash = passwordHash,
            Role = UserRole.Cliente,
            Status = UserStatus.PendingEmailConfirmation,
            EmailConfirmationCode = GenerateCode(),
            EmailConfirmationExpiry = DateTime.UtcNow.AddMinutes(30),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void ConfirmEmail()
    {
        if (Status != UserStatus.PendingEmailConfirmation)
            throw new OnboardingDomainException("Email já confirmado ou usuário em status inválido");
        Status = UserStatus.PendingApproval;
        EmailConfirmationCode = null;
        EmailConfirmationExpiry = null;
    }

    public void Approve(string adminId)
    {
        if (Status != UserStatus.PendingApproval)
            throw new OnboardingDomainException("Usuário não está pendente de aprovação");
        Status = UserStatus.Active;
        ApprovedAt = DateTime.UtcNow;
        ApprovedBy = adminId;
    }

    public void Reject(string adminId)
    {
        Status = UserStatus.Rejected;
        ApprovedBy = adminId;
    }

    public void Activate() => Status = UserStatus.Active;
    public void Deactivate() => Status = UserStatus.Inactive;

    public void ChangeRole(UserRole newRole) => Role = newRole;

    public void SetKeycloakUserId(string keycloakId) => KeycloakUserId = keycloakId;

    public void SetPasswordResetCode()
    {
        PasswordResetCode = GenerateCode();
        PasswordResetExpiry = DateTime.UtcNow.AddMinutes(30);
    }

    public void ResetPassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        PasswordResetCode = null;
        PasswordResetExpiry = null;
    }

    public void GenerateNewEmailConfirmationCode()
    {
        EmailConfirmationCode = GenerateCode();
        EmailConfirmationExpiry = DateTime.UtcNow.AddMinutes(30);
    }

    private static string GenerateCode() => new Random().Next(100000, 999999).ToString();
}
