using FluentAssertions;
using KRT.Onboarding.Application.Commands.Users;
using KRT.Onboarding.Application.Commands.Users.Handlers;
using KRT.Onboarding.Application.Interfaces;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KRT.UnitTests.Application.Users;

public class RequestPasswordResetHandlerTests
{
    private readonly Mock<IAppUserRepository> _userRepoMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<ILogger<RequestPasswordResetHandler>> _loggerMock = new();
    private readonly RequestPasswordResetHandler _handler;

    public RequestPasswordResetHandlerTests()
    {
        _handler = new RequestPasswordResetHandler(_userRepoMock.Object, _emailServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenEmailNotFound_ShouldReturnSuccessAnyway()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((AppUser?)null);

        var result = await _handler.Handle(new RequestPasswordResetCommand("missing@email.com"), CancellationToken.None);

        result.Success.Should().BeTrue();
        _emailServiceMock.Verify(e => e.SendPasswordResetAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSetResetCode()
    {
        var user = AppUser.Create("Test", "test@email.com", "12345678900", "hash");
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@email.com")).ReturnsAsync(user);

        await _handler.Handle(new RequestPasswordResetCommand("test@email.com"), CancellationToken.None);

        user.PasswordResetCode.Should().NotBeNullOrEmpty();
        user.PasswordResetExpiry.Should().NotBeNull();
        _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldSendEmail()
    {
        var user = AppUser.Create("Test User", "test@email.com", "12345678900", "hash");
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@email.com")).ReturnsAsync(user);

        await _handler.Handle(new RequestPasswordResetCommand("test@email.com"), CancellationToken.None);

        _emailServiceMock.Verify(e => e.SendPasswordResetAsync(
            "test@email.com", "Test User", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldTrimAndLowercaseEmail()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@email.com")).ReturnsAsync((AppUser?)null);

        await _handler.Handle(new RequestPasswordResetCommand("  TEST@EMAIL.COM  "), CancellationToken.None);

        _userRepoMock.Verify(r => r.GetByEmailAsync("test@email.com"), Times.Once);
    }
}
