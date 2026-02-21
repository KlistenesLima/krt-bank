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

public class ConfirmEmailHandlerTests
{
    private readonly Mock<IAppUserRepository> _userRepoMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<ILogger<ConfirmEmailHandler>> _loggerMock = new();
    private readonly ConfirmEmailHandler _handler;

    public ConfirmEmailHandlerTests()
    {
        _handler = new ConfirmEmailHandler(_userRepoMock.Object, _emailServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCodeValid_ShouldConfirm()
    {
        var user = AppUser.Create("Test", "test@email.com", "12345678900", "hash");
        var code = user.EmailConfirmationCode!;

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@email.com")).ReturnsAsync(user);

        var result = await _handler.Handle(new ConfirmEmailCommand("test@email.com", code), CancellationToken.None);

        result.Success.Should().BeTrue();
        user.Status.Should().Be(UserStatus.PendingApproval);
        _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
        _emailServiceMock.Verify(e => e.SendRegistrationPendingAsync(
            It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCodeInvalid_ShouldReturnError()
    {
        var user = AppUser.Create("Test", "test@email.com", "12345678900", "hash");
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@email.com")).ReturnsAsync(user);

        var result = await _handler.Handle(new ConfirmEmailCommand("test@email.com", "000000"), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("inválido");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnError()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((AppUser?)null);

        var result = await _handler.Handle(new ConfirmEmailCommand("missing@email.com", "123456"), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("não encontrado");
    }
}
