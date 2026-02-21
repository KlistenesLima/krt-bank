using FluentAssertions;
using KRT.BuildingBlocks.Domain.Exceptions;
using KRT.Onboarding.Application.Commands.Users;
using KRT.Onboarding.Application.Commands.Users.Handlers;
using KRT.Onboarding.Application.Interfaces;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Enums;
using KRT.Onboarding.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KRT.UnitTests.Application.Users;

public class ApproveUserHandlerTests
{
    private readonly Mock<IAppUserRepository> _userRepoMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IKeycloakAdminService> _keycloakMock = new();
    private readonly Mock<ILogger<ApproveUserHandler>> _loggerMock = new();
    private readonly ApproveUserHandler _handler;

    public ApproveUserHandlerTests()
    {
        _keycloakMock.Setup(k => k.CreateUserAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeycloakUserResult(true, "kc-123", null));

        _handler = new ApproveUserHandler(
            _userRepoMock.Object, _emailServiceMock.Object,
            _keycloakMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldApproveAndSendEmail()
    {
        var user = AppUser.Create("Test", "test@email.com", "12345678900", "hash");
        user.ConfirmEmail(); // PendingApproval

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await _handler.Handle(
            new ApproveUserCommand(user.Id, Guid.NewGuid()), CancellationToken.None);

        result.Success.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
        _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
        _emailServiceMock.Verify(e => e.SendApprovalNotificationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnError()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((AppUser?)null);

        var result = await _handler.Handle(
            new ApproveUserCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("não encontrado");
    }

    [Fact]
    public async Task Handle_WhenNotPending_ShouldThrowDomainException()
    {
        var user = AppUser.Create("Test", "test@email.com", "12345678900", "hash");
        // user is PendingEmailConfirmation, not PendingApproval
        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var act = async () => await _handler.Handle(
            new ApproveUserCommand(user.Id, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSetApprovedAtAndApprovedBy()
    {
        var user = AppUser.Create("Test", "test@email.com", "12345678900", "hash");
        user.ConfirmEmail();
        var adminId = Guid.NewGuid();

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var before = DateTime.UtcNow;
        await _handler.Handle(new ApproveUserCommand(user.Id, adminId), CancellationToken.None);

        user.ApprovedAt.Should().NotBeNull();
        user.ApprovedAt.Should().BeOnOrAfter(before);
        user.ApprovedBy.Should().Be(adminId.ToString());
    }
}
