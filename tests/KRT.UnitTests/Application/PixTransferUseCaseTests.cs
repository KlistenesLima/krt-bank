using FluentAssertions;
using KRT.BuildingBlocks.Domain;
using KRT.Payments.Application.DTOs;
using KRT.Payments.Application.Services;
using KRT.Payments.Application.UseCases;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KRT.UnitTests.Application;

public class PixTransferUseCaseTests
{
    private readonly Mock<IPixTransactionRepository> _repoMock = new();
    private readonly Mock<IOnboardingServiceClient> _clientMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly PixTransferUseCase _useCase;

    public PixTransferUseCaseTests()
    {
        _repoMock.Setup(r => r.UnitOfWork).Returns(_uowMock.Object);
        _uowMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repoMock.Setup(r => r.GetByIdempotencyKeyAsync(It.IsAny<Guid>())).ReturnsAsync((PixTransaction?)null);

        _useCase = new PixTransferUseCase(
            _repoMock.Object,
            _clientMock.Object,
            Mock.Of<ILogger<PixTransferUseCase>>());
    }

    [Fact]
    public async Task HappyPath_ShouldReturnCompleted()
    {
        SetupDebit(true);
        SetupCredit(true);

        var result = await _useCase.ExecuteAsync(MakeRequest());

        result.Status.Should().Be("Completed");
        result.Amount.Should().Be(100m);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<PixTransaction>()), Times.Once);
    }

    [Fact]
    public async Task DebitFails_ShouldReturnFailed()
    {
        SetupDebit(false, "Saldo insuficiente");

        var result = await _useCase.ExecuteAsync(MakeRequest());

        result.Status.Should().Be("Failed");
        _clientMock.Verify(c => c.CreditAccountAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreditFails_ShouldCompensate()
    {
        var req = MakeRequest();
        SetupDebit(true);

        // Credito falha para destino
        _clientMock.Setup(c => c.CreditAccountAsync(req.DestinationAccountId, It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(new AccountOperationResponse(false, "Conta bloqueada", 0));
        // Compensacao (credito de volta para origem) funciona
        _clientMock.Setup(c => c.CreditAccountAsync(req.SourceAccountId, It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(new AccountOperationResponse(true, null, 1000));

        var result = await _useCase.ExecuteAsync(req);

        result.Status.Should().Be("Compensated");
    }

    [Fact]
    public async Task DuplicateKey_ShouldReturnExisting()
    {
        var req = MakeRequest();
        var existing = new PixTransaction(req.SourceAccountId, req.DestinationAccountId, req.PixKey, req.Amount, req.Description, req.IdempotencyKey);
        _repoMock.Setup(r => r.GetByIdempotencyKeyAsync(req.IdempotencyKey)).ReturnsAsync(existing);

        var result = await _useCase.ExecuteAsync(req);

        result.TransactionId.Should().Be(existing.Id);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<PixTransaction>()), Times.Never);
    }

    [Fact]
    public async Task ZeroAmount_ShouldThrow()
    {
        var act = () => _useCase.ExecuteAsync(MakeRequest() with { Amount = 0 });
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SameAccount_ShouldThrow()
    {
        var id = Guid.NewGuid();
        var act = () => _useCase.ExecuteAsync(MakeRequest() with { SourceAccountId = id, DestinationAccountId = id });
        await act.Should().ThrowAsync<ArgumentException>();
    }

    private PixTransferRequest MakeRequest() => new(
        Guid.NewGuid(), Guid.NewGuid(), "12345678901", 100m, "Teste", Guid.NewGuid());

    private void SetupDebit(bool success, string? error = null)
    {
        _clientMock.Setup(c => c.DebitAccountAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(new AccountOperationResponse(success, error, success ? 900 : 0));
    }

    private void SetupCredit(bool success, string? error = null)
    {
        _clientMock.Setup(c => c.CreditAccountAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(new AccountOperationResponse(success, error, success ? 100 : 0));
    }
}
