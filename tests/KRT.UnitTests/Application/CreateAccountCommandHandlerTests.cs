using KRT.Onboarding.Application.Interfaces;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Application.Commands;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Interfaces;
using Moq;
using Xunit;

namespace KRT.UnitTests.Application;

public class CreateAccountCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IKeycloakAdminService> _keycloakMock = new();
    private readonly Mock<ILogger<CreateAccountCommandHandler>> _loggerMock = new();
    private readonly CreateAccountCommandHandler _handler;

    public CreateAccountCommandHandlerTests()
    {
        _uowMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _handler = new CreateAccountCommandHandler(_repoMock.Object, _uowMock.Object, _keycloakMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_Valid_ShouldReturnSuccess()
    {
        var cmd = new CreateAccountCommand
        {
            CustomerName = "Maria",
            CustomerDocument = "12345678901",
            CustomerEmail = "maria@test.com"
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsValid.Should().BeTrue();
        result.Id.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCaptureCorrectData()
    {
        Account? captured = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .Callback<Account, CancellationToken>((a, _) => captured = a).Returns(Task.CompletedTask);

        await _handler.Handle(new CreateAccountCommand
        {
            CustomerName = "Pedro",
            CustomerDocument = "99988877766",
            CustomerEmail = "pedro@test.com"
        }, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.CustomerName.Should().Be("Pedro");
        captured.Balance.Should().Be(0);
    }
}

