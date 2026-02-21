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

public class RegisterUserHandlerTests
{
    private readonly Mock<IAppUserRepository> _userRepoMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<ILogger<RegisterUserHandler>> _loggerMock = new();
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        _handler = new RegisterUserHandler(_userRepoMock.Object, _emailServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenEmailExists_ShouldReturnError()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(AppUser.Create("Existing", "test@email.com", "12345678900", "hash"));

        var command = new RegisterUserCommand("Test User", "test@email.com", "98765432100", "Senha123");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Email já cadastrado");
    }

    [Fact]
    public async Task Handle_WhenDocumentExists_ShouldReturnError()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);
        _userRepoMock.Setup(r => r.GetByDocumentAsync(It.IsAny<string>()))
            .ReturnsAsync(AppUser.Create("Existing", "other@email.com", "12345678900", "hash"));

        var command = new RegisterUserCommand("Test User", "new@email.com", "12345678900", "Senha123");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("CPF já cadastrado");
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldCreateUserAndSendEmail()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);
        _userRepoMock.Setup(r => r.GetByDocumentAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

        var command = new RegisterUserCommand("Test User", "test@email.com", "12345678900", "Senha123");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.UserId.Should().NotBeNull();
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<AppUser>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendEmailConfirmationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}
