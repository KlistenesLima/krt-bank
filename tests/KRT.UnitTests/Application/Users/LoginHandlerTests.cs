using FluentAssertions;
using KRT.Onboarding.Application.Commands.Users;
using KRT.Onboarding.Application.Commands.Users.Handlers;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Enums;
using KRT.Onboarding.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KRT.UnitTests.Application.Users;

public class LoginHandlerTests
{
    private readonly Mock<IAppUserRepository> _userRepoMock = new();
    private readonly Mock<ILogger<LoginHandler>> _loggerMock = new();
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "KRT-Bank-Super-Secret-Key-2026-Minimum-32-Chars!",
                ["Jwt:Issuer"] = "KRT.Onboarding",
                ["Jwt:Audience"] = "KRT.Bank",
                ["Jwt:ExpiryMinutes"] = "480"
            })
            .Build();

        _handler = new LoginHandler(_userRepoMock.Object, config, _loggerMock.Object);
    }

    private AppUser CreateActiveUser()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Senha123");
        var user = AppUser.Create("Test User", "test@email.com", "12345678900", passwordHash);
        user.ConfirmEmail();
        user.Approve("admin");
        return user;
    }

    [Fact]
    public async Task Handle_WhenNotActive_ShouldReturnError()
    {
        var user = AppUser.Create("Test", "test@email.com", "12345678900",
            BCrypt.Net.BCrypt.HashPassword("Senha123"));
        // user is PendingEmailConfirmation

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@email.com")).ReturnsAsync(user);

        var result = await _handler.Handle(new LoginCommand("test@email.com", "Senha123"), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Confirme seu email");
    }

    [Fact]
    public async Task Handle_WhenPasswordWrong_ShouldReturnError()
    {
        var user = CreateActiveUser();
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@email.com")).ReturnsAsync(user);

        var result = await _handler.Handle(new LoginCommand("test@email.com", "WrongPassword"), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("inválidos");
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldReturnToken()
    {
        var user = CreateActiveUser();
        _userRepoMock.Setup(r => r.GetByEmailAsync("test@email.com")).ReturnsAsync(user);

        var result = await _handler.Handle(new LoginCommand("test@email.com", "Senha123"), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.Role.Should().Be(UserRole.Cliente);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnError()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((AppUser?)null);

        var result = await _handler.Handle(new LoginCommand("missing@email.com", "Senha123"), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("inválidos");
    }

    [Fact]
    public async Task Handle_WhenLoginByDocument_ShouldWork()
    {
        var user = CreateActiveUser();
        _userRepoMock.Setup(r => r.GetByDocumentAsync("12345678900")).ReturnsAsync(user);

        var result = await _handler.Handle(new LoginCommand("12345678900", "Senha123"), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WhenPendingApproval_ShouldReturnError()
    {
        var user = AppUser.Create("Test", "test@email.com", "12345678900",
            BCrypt.Net.BCrypt.HashPassword("Senha123"));
        user.ConfirmEmail(); // Now PendingApproval

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@email.com")).ReturnsAsync(user);

        var result = await _handler.Handle(new LoginCommand("test@email.com", "Senha123"), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("aguardando aprovação");
    }
}
