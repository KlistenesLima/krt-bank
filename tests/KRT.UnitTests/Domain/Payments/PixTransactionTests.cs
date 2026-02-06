using FluentAssertions;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Enums;
using Xunit;

namespace KRT.UnitTests.Domain.Payments;

public class PixTransactionTests
{
    [Fact]
    public void Create_ShouldBePending()
    {
        var tx = MakeTx();
        tx.Id.Should().NotBeEmpty();
        tx.Status.Should().Be(PixTransactionStatus.Pending);
        tx.SourceDebited.Should().BeFalse();
        tx.DestinationCredited.Should().BeFalse();
        tx.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void MarkSourceDebited_FromPending_ShouldSucceed()
    {
        var tx = MakeTx();
        tx.MarkSourceDebited();
        tx.Status.Should().Be(PixTransactionStatus.SourceDebited);
        tx.SourceDebited.Should().BeTrue();
    }

    [Fact]
    public void MarkSourceDebited_FromFailed_ShouldThrow()
    {
        var tx = MakeTx();
        tx.MarkFailed("x");
        var act = () => tx.MarkSourceDebited();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkDestinationCredited_FromDebited_ShouldComplete()
    {
        var tx = MakeTx();
        tx.MarkSourceDebited();
        tx.MarkDestinationCredited();
        tx.Status.Should().Be(PixTransactionStatus.Completed);
        tx.DestinationCredited.Should().BeTrue();
        tx.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkDestinationCredited_FromPending_ShouldThrow()
    {
        var tx = MakeTx();
        var act = () => tx.MarkDestinationCredited();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkFailed_ShouldStoreReason()
    {
        var tx = MakeTx();
        tx.MarkFailed("Saldo insuficiente");
        tx.Status.Should().Be(PixTransactionStatus.Failed);
        tx.FailureReason.Should().Be("Saldo insuficiente");
    }

    [Fact]
    public void MarkCompensated_ShouldResetFlags()
    {
        var tx = MakeTx();
        tx.MarkSourceDebited();
        tx.MarkCompensated();
        tx.Status.Should().Be(PixTransactionStatus.Compensated);
        tx.SourceDebited.Should().BeFalse();
    }

    [Fact]
    public void FullFlow_HappyPath()
    {
        var tx = MakeTx();
        tx.MarkSourceDebited();
        tx.MarkDestinationCredited();
        tx.Status.Should().Be(PixTransactionStatus.Completed);
    }

    [Fact]
    public void FullFlow_CompensationPath()
    {
        var tx = MakeTx();
        tx.MarkSourceDebited();
        tx.MarkCompensated();
        tx.Status.Should().Be(PixTransactionStatus.Compensated);
    }

    private static PixTransaction MakeTx()
        => new(Guid.NewGuid(), Guid.NewGuid(), 100m, "12345678901", "Teste", Guid.NewGuid());
}

