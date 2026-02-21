using FluentAssertions;
using KRT.Onboarding.Application.Commands.Users;
using KRT.Onboarding.Application.Commands.Users.Handlers;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KRT.UnitTests.Application.Users;

public class ResetPasswordHandlerTests
{
    private readonly Mock<IAppUserRepository> _userRepoMock = new();
    private readonly Mock<ILogger<ResetPasswordHandler>> _loggerMock = new();
    private readonly ResetPasswordHandler _handler;

    public ResetPasswordHandlerTests()
    {
        _handler = new ResetPasswordHandler(_userRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnError()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((AppUser?)null);

        var result = await _handler.Handle(
            new ResetPasswordCommand("missing@email.com", "123456", "NewPass123"), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("não encontrado");
    }

    [Fact]
    public async Task Handle_WhenCodeIncorrect_ShouldReturnError()
    {
        var user = AppUser.Create("Test", "test@email.com", "12345678900", "hash");
        user.SetPasswordResetCode();
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@email.com")).ReturnsAsync(user);

        var result = await _handler.Handle(
            new ResetPasswordCommand("test@email.com", "000000", "NewPass123"), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("inválido");
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldResetPassword()
    {
        var user = AppUser.Create("Test", "test@email.com", "12345678900", "hash");
        user.SetPasswordResetCode();
        var code = user.PasswordResetCode!;
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@email.com")).ReturnsAsync(user);

        var result = await _handler.Handle(
            new ResetPasswordCommand("test@email.com", code, "NewPassword123"), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("redefinida");
        BCrypt.Net.BCrypt.Verify("NewPassword123", user.PasswordHash).Should().BeTrue();
        _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldClearResetCode()
    {
        var user = AppUser.Create("Test", "test@email.com", "12345678900", "hash");
        user.SetPasswordResetCode();
        var code = user.PasswordResetCode!;
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@email.com")).ReturnsAsync(user);

        await _handler.Handle(
            new ResetPasswordCommand("test@email.com", code, "NewPassword123"), CancellationToken.None);

        user.PasswordResetCode.Should().BeNull();
        user.PasswordResetExpiry.Should().BeNull();
    }
}
