using FluentAssertions;
using KRT.BuildingBlocks.Domain;
using KRT.Payments.Application.Commands;
using KRT.Payments.Application.DTOs;
using KRT.Payments.Application.Services;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KRT.UnitTests.Application;

public class ProcessPixCommandHandlerTests
{
    private readonly Mock<IPixTransactionRepository> _repoMock = new();
    private readonly Mock<IOnboardingServiceClient> _clientMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly ProcessPixCommandHandler _handler;

    public ProcessPixCommandHandlerTests()
    {
        _repoMock.Setup(r => r.UnitOfWork).Returns(_uowMock.Object);
        _uowMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repoMock.Setup(r => r.GetByIdempotencyKeyAsync(It.IsAny<Guid>())).ReturnsAsync((PixTransaction?)null);

        _handler = new ProcessPixCommandHandler(
            _repoMock.Object,
            _clientMock.Object,
            Mock.Of<ILogger<ProcessPixCommandHandler>>());
    }

    [Fact]
    public async Task HappyPath_ShouldReturnSuccess()
    {
        SetupDebit(true);
        SetupCredit(true);
        var result = await _handler.Handle(MakeCommand(), CancellationToken.None);
        result.IsValid.Should().BeTrue();
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DebitFails_ShouldReturnFailure()
    {
        SetupDebit(false, "Saldo insuficiente");
        var result = await _handler.Handle(MakeCommand(), CancellationToken.None);
        result.IsValid.Should().BeFalse();
        _clientMock.Verify(c => c.CreditAccountAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreditFails_ShouldCompensate()
    {
        var cmd = MakeCommand();
        SetupDebit(true);
        _clientMock.Setup(c => c.CreditAccountAsync(cmd.DestinationAccountId, It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(new AccountOperationResponse(false, "Conta bloqueada", 0));
        _clientMock.Setup(c => c.CreditAccountAsync(cmd.SourceAccountId, It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(new AccountOperationResponse(true, null, 1000));

        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.IsValid.Should().BeFalse();
        _clientMock.Verify(c => c.CreditAccountAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task DuplicateKey_ShouldReturnExisting()
    {
        var cmd = MakeCommand();
        var existing = new PixTransaction(cmd.SourceAccountId, cmd.DestinationAccountId, 100m, cmd.PixKey, cmd.Description, cmd.IdempotencyKey);
        _repoMock.Setup(r => r.GetByIdempotencyKeyAsync(cmd.IdempotencyKey)).ReturnsAsync(existing);
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.IsValid.Should().BeTrue();
        result.Id.Should().Be(existing.Id);
    }

    [Fact]
    public async Task ZeroAmount_ShouldFail()
    {
        var cmd = MakeCommand(); cmd.Amount = 0;
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task SameAccount_ShouldFail()
    {
        var id = Guid.NewGuid();
        var cmd = MakeCommand(); cmd.SourceAccountId = id; cmd.DestinationAccountId = id;
        var result = await _handler.Handle(cmd, CancellationToken.None);
        result.IsValid.Should().BeFalse();
    }

    private ProcessPixCommand MakeCommand() => new()
    {
        SourceAccountId = Guid.NewGuid(), DestinationAccountId = Guid.NewGuid(),
        PixKey = "12345678901", Amount = 100m, Description = "Teste", IdempotencyKey = Guid.NewGuid()
    };

    private void SetupDebit(bool ok, string? err = null) =>
        _clientMock.Setup(c => c.DebitAccountAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(new AccountOperationResponse(ok, err, ok ? 900 : 0));

    private void SetupCredit(bool ok, string? err = null) =>
        _clientMock.Setup(c => c.CreditAccountAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(new AccountOperationResponse(ok, err, ok ? 100 : 0));
}
