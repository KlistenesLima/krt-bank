using Xunit;
using FluentAssertions;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Enums;

namespace KRT.UnitTests.Domain.Payments;

public class PixTransactionTests
{
    private PixTransaction CreateTx() =>
        new(Guid.NewGuid(), Guid.NewGuid(), 100m, "pix@test.com", "Test", Guid.NewGuid());

    private PixTransaction CreateApprovedTx()
    {
        var tx = CreateTx();
        tx.Approve(10, "OK");
        tx.StartSaga();
        return tx;
    }

    [Fact]
    public void Create_ShouldBePendingAnalysis()
    {
        var tx = CreateTx();
        tx.Status.Should().Be(PixTransactionStatus.PendingAnalysis);
    }

    [Fact]
    public void Approve_ShouldTransitionToApproved()
    {
        var tx = CreateTx();
        tx.Approve(10, "Low risk");
        tx.Status.Should().Be(PixTransactionStatus.Approved);
        tx.FraudScore.Should().Be(10);
    }

    [Fact]
    public void Reject_ShouldTransitionToRejected()
    {
        var tx = CreateTx();
        tx.Reject(80, "High risk");
        tx.Status.Should().Be(PixTransactionStatus.Rejected);
        tx.FraudScore.Should().Be(80);
    }

    [Fact]
    public void HoldForReview_ShouldTransitionToUnderReview()
    {
        var tx = CreateTx();
        tx.HoldForReview(50, "Medium risk");
        tx.Status.Should().Be(PixTransactionStatus.UnderReview);
    }

    [Fact]
    public void MarkSourceDebited_FromPending_ShouldSucceed()
    {
        var tx = CreateApprovedTx();
        tx.MarkSourceDebited();
        tx.Status.Should().Be(PixTransactionStatus.SourceDebited);
        tx.SourceDebited.Should().BeTrue();
    }

    [Fact]
    public void Complete_FromDebited_ShouldComplete()
    {
        var tx = CreateApprovedTx();
        tx.MarkSourceDebited();
        tx.Complete();
        tx.Status.Should().Be(PixTransactionStatus.Completed);
        tx.DestinationCredited.Should().BeTrue();
        tx.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Fail_ShouldSetReason()
    {
        var tx = CreateTx();
        tx.Fail("Saldo insuficiente");
        tx.Status.Should().Be(PixTransactionStatus.Failed);
        tx.FailureReason.Should().Be("Saldo insuficiente");
    }

    [Fact]
    public void Compensate_ShouldResetFlags()
    {
        var tx = CreateApprovedTx();
        tx.MarkSourceDebited();
        tx.Compensate("Credit failed");
        tx.Status.Should().Be(PixTransactionStatus.Compensated);
        tx.SourceDebited.Should().BeFalse();
    }

    [Fact]
    public void FullFlow_HappyPath()
    {
        var tx = CreateTx();
        tx.Approve(5, "Clean");
        tx.StartSaga();
        tx.MarkSourceDebited();
        tx.Complete();
        tx.Status.Should().Be(PixTransactionStatus.Completed);
    }

    [Fact]
    public void FullFlow_CompensationPath()
    {
        var tx = CreateTx();
        tx.Approve(5, "Clean");
        tx.StartSaga();
        tx.MarkSourceDebited();
        tx.Compensate("Destination failed");
        tx.Status.Should().Be(PixTransactionStatus.Compensated);
    }
}

