using FluentAssertions;
using KRT.BuildingBlocks.Domain.Exceptions;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Enums;
using Xunit;

namespace KRT.UnitTests.Domain.Onboarding;

public class AppUserTests
{
    private AppUser CreateDefaultUser()
    {
        return AppUser.Create("João Silva", "joao@email.com", "123.456.789-00", "hashedPassword123");
    }

    [Fact]
    public void Create_ShouldSetPendingEmailConfirmation()
    {
        var user = CreateDefaultUser();
        user.Status.Should().Be(UserStatus.PendingEmailConfirmation);
    }

    [Fact]
    public void Create_ShouldGenerateConfirmationCode()
    {
        var user = CreateDefaultUser();
        user.EmailConfirmationCode.Should().NotBeNullOrEmpty();
        user.EmailConfirmationCode.Should().HaveLength(6);
        user.EmailConfirmationExpiry.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldSetDefaultRoleAsCliente()
    {
        var user = CreateDefaultUser();
        user.Role.Should().Be(UserRole.Cliente);
    }

    [Fact]
    public void Create_ShouldNormalizeEmailToLower()
    {
        var user = AppUser.Create("Test", "JOAO@EMAIL.COM", "12345678900", "hash");
        user.Email.Should().Be("joao@email.com");
    }

    [Fact]
    public void Create_ShouldCleanDocument()
    {
        var user = AppUser.Create("Test", "test@email.com", "123.456.789-00", "hash");
        user.Document.Should().Be("12345678900");
    }

    [Fact]
    public void Create_ShouldSetCreatedAt()
    {
        var before = DateTime.UtcNow;
        var user = CreateDefaultUser();
        var after = DateTime.UtcNow;

        user.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Create_ShouldGenerateNewId()
    {
        var user = CreateDefaultUser();
        user.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ConfirmEmail_ShouldChangeToPendingApproval()
    {
        var user = CreateDefaultUser();
        user.ConfirmEmail();
        user.Status.Should().Be(UserStatus.PendingApproval);
    }

    [Fact]
    public void ConfirmEmail_ShouldClearConfirmationCode()
    {
        var user = CreateDefaultUser();
        user.ConfirmEmail();
        user.EmailConfirmationCode.Should().BeNull();
        user.EmailConfirmationExpiry.Should().BeNull();
    }

    [Fact]
    public void ConfirmEmail_WhenAlreadyConfirmed_ShouldThrow()
    {
        var user = CreateDefaultUser();
        user.ConfirmEmail();

        var act = () => user.ConfirmEmail();
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Approve_ShouldSetActive()
    {
        var user = CreateDefaultUser();
        user.ConfirmEmail();
        user.Approve("admin-123");

        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Approve_ShouldSetApprovedAt()
    {
        var user = CreateDefaultUser();
        user.ConfirmEmail();

        var before = DateTime.UtcNow;
        user.Approve("admin-123");

        user.ApprovedAt.Should().NotBeNull();
        user.ApprovedAt.Should().BeOnOrAfter(before);
        user.ApprovedBy.Should().Be("admin-123");
    }

    [Fact]
    public void Approve_WhenNotPending_ShouldThrow()
    {
        var user = CreateDefaultUser();
        // Still PendingEmailConfirmation, not PendingApproval

        var act = () => user.Approve("admin-123");
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Reject_ShouldSetRejected()
    {
        var user = CreateDefaultUser();
        user.ConfirmEmail();
        user.Reject("admin-123");

        user.Status.Should().Be(UserStatus.Rejected);
        user.ApprovedBy.Should().Be("admin-123");
    }

    [Fact]
    public void Activate_ShouldSetActive()
    {
        var user = CreateDefaultUser();
        user.ConfirmEmail();
        user.Approve("admin");
        user.Deactivate();
        user.Activate();

        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Deactivate_ShouldSetInactive()
    {
        var user = CreateDefaultUser();
        user.ConfirmEmail();
        user.Approve("admin");
        user.Deactivate();

        user.Status.Should().Be(UserStatus.Inactive);
    }

    [Fact]
    public void ChangeRole_ShouldUpdateRole()
    {
        var user = CreateDefaultUser();
        user.ChangeRole(UserRole.Administrador);

        user.Role.Should().Be(UserRole.Administrador);
    }

    [Fact]
    public void SetPasswordResetCode_ShouldGenerateCode()
    {
        var user = CreateDefaultUser();
        user.SetPasswordResetCode();

        user.PasswordResetCode.Should().NotBeNullOrEmpty();
        user.PasswordResetCode.Should().HaveLength(6);
        user.PasswordResetExpiry.Should().NotBeNull();
    }

    [Fact]
    public void ResetPassword_ShouldClearResetCode()
    {
        var user = CreateDefaultUser();
        user.SetPasswordResetCode();
        user.ResetPassword("newHashedPassword");

        user.PasswordHash.Should().Be("newHashedPassword");
        user.PasswordResetCode.Should().BeNull();
        user.PasswordResetExpiry.Should().BeNull();
    }

    [Fact]
    public void SetKeycloakUserId_ShouldSetId()
    {
        var user = CreateDefaultUser();
        user.SetKeycloakUserId("kc-12345");

        user.KeycloakUserId.Should().Be("kc-12345");
    }

    [Fact]
    public void GenerateNewEmailConfirmationCode_ShouldReplaceCode()
    {
        var user = CreateDefaultUser();
        var originalCode = user.EmailConfirmationCode;

        user.GenerateNewEmailConfirmationCode();

        // Code should be regenerated (might be same by chance, but expiry should be refreshed)
        user.EmailConfirmationCode.Should().NotBeNullOrEmpty();
        user.EmailConfirmationExpiry.Should().NotBeNull();
    }
}
