using FluentAssertions;
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

public class RejectUserHandlerTests
{
    private readonly Mock<IAppUserRepository> _userRepoMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<ILogger<RejectUserHandler>> _loggerMock = new();
    private readonly RejectUserHandler _handler;

    public RejectUserHandlerTests()
    {
        _handler = new RejectUserHandler(_userRepoMock.Object, _emailServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnError()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((AppUser?)null);

        var result = await _handler.Handle(new RejectUserCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("não encontrado");
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldRejectUser()
    {
        var user = AppUser.Create("Test", "test@email.com", "12345678900", "hash");
        user.ConfirmEmail();
        var adminId = Guid.NewGuid();

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await _handler.Handle(new RejectUserCommand(user.Id, adminId), CancellationToken.None);

        result.Success.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Rejected);
        _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSendRejectionEmail()
    {
        var user = AppUser.Create("Test User", "test@email.com", "12345678900", "hash");
        user.ConfirmEmail();

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        await _handler.Handle(new RejectUserCommand(user.Id, Guid.NewGuid()), CancellationToken.None);

        _emailServiceMock.Verify(e => e.SendRejectionNotificationAsync("test@email.com", "Test User"), Times.Once);
    }
}
