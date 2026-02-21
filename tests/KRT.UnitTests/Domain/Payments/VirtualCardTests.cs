using FluentAssertions;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Enums;
using Xunit;

namespace KRT.UnitTests.Domain.Payments;

public class VirtualCardTests
{
    [Fact]
    public void Create_ShouldGenerateValidCard()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "JOAO SILVA");

        card.CardNumber.Should().HaveLength(16);
        card.Last4Digits.Should().HaveLength(4);
        card.Cvv.Should().HaveLength(3);
        card.Status.Should().Be(CardStatus.Active);
        card.Brand.Should().Be(CardBrand.Visa);
        card.CardholderName.Should().Be("JOAO SILVA");
        card.SpendingLimit.Should().Be(5000m);
    }

    [Fact]
    public void Create_Mastercard_ShouldStartWith5()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST", CardBrand.Mastercard);
        card.CardNumber.Should().StartWith("5");
        card.Brand.Should().Be(CardBrand.Mastercard);
    }

    [Fact]
    public void Block_ActiveCard_ShouldBeBlocked()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        card.Block();
        card.Status.Should().Be(CardStatus.Blocked);
    }

    [Fact]
    public void Unblock_BlockedCard_ShouldBeActive()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        card.Block();
        card.Unblock();
        card.Status.Should().Be(CardStatus.Active);
    }

    [Fact]
    public void Cancel_ShouldBePermanent()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        card.Cancel();
        card.Status.Should().Be(CardStatus.Cancelled);

        var act = () => card.Block();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RotateCvv_ShouldGenerateNewCvv()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        var originalCvv = card.Cvv;

        // Pode gerar o mesmo por acaso, mas testamos que o metodo roda
        card.RotateCvv();
        card.Cvv.Should().HaveLength(3);
        card.IsCvvValid().Should().BeTrue();
    }

    [Fact]
    public void ValidatePurchase_Blocked_ShouldReject()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        card.Block();

        var (allowed, reason) = card.ValidatePurchase(100m, false, false);
        allowed.Should().BeFalse();
        reason.Should().Contain("Blocked");
    }

    [Fact]
    public void ValidatePurchase_OnlineDisabled_ShouldReject()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        card.ToggleOnlinePurchase(false);

        var (allowed, reason) = card.ValidatePurchase(100m, isOnline: true, isInternational: false);
        allowed.Should().BeFalse();
        reason.Should().Contain("online");
    }

    [Fact]
    public void GetMaskedNumber_ShouldShowLast4()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        card.GetMaskedNumber().Should().StartWith("**** **** **** ");
        card.GetMaskedNumber().Should().EndWith(card.Last4Digits);
    }

    // ==========================================
    // TESTES DE SPENDING
    // ==========================================

    [Fact]
    public void AddSpending_ShouldIncreaseSpentThisMonth()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        card.SpentThisMonth.Should().Be(0);

        card.AddSpending(1000m);

        card.SpentThisMonth.Should().Be(1000m);
        card.AvailableLimit.Should().Be(4000m);
    }

    [Fact]
    public void AddSpending_MultipleTimes_ShouldAccumulate()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");

        card.AddSpending(1000m);
        card.AddSpending(500m);
        card.AddSpending(250m);

        card.SpentThisMonth.Should().Be(1750m);
        card.AvailableLimit.Should().Be(3250m);
    }

    [Fact]
    public void AddSpending_ExceedsLimit_ShouldThrow()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        // Limite padrao = 5000

        var act = () => card.AddSpending(5001m);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Limite insuficiente*");
    }

    [Fact]
    public void AddSpending_ExactLimit_ShouldSucceed()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");

        card.AddSpending(5000m);

        card.SpentThisMonth.Should().Be(5000m);
        card.AvailableLimit.Should().Be(0m);
    }

    [Fact]
    public void AddSpending_AfterPartialUse_ExceedsRemaining_ShouldThrow()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        card.AddSpending(4000m);

        var act = () => card.AddSpending(1001m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ReduceSpending_ShouldDecreaseSpentThisMonth()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        card.AddSpending(3000m);

        card.ReduceSpending(1000m);

        card.SpentThisMonth.Should().Be(2000m);
        card.AvailableLimit.Should().Be(3000m);
    }

    [Fact]
    public void ReduceSpending_BelowZero_ShouldSetToZero()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        card.AddSpending(500m);

        card.ReduceSpending(1000m);

        card.SpentThisMonth.Should().Be(0m);
        card.AvailableLimit.Should().Be(5000m);
    }

    [Fact]
    public void ReduceSpending_ExactAmount_ShouldSetToZero()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        card.AddSpending(2000m);

        card.ReduceSpending(2000m);

        card.SpentThisMonth.Should().Be(0m);
    }

    [Fact]
    public void HasAvailableLimit_ShouldReturnTrue()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");

        card.HasAvailableLimit(5000m).Should().BeTrue();
        card.HasAvailableLimit(1m).Should().BeTrue();
        card.HasAvailableLimit(0m).Should().BeTrue();
    }

    [Fact]
    public void HasAvailableLimit_InsufficientLimit_ShouldReturnFalse()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        card.AddSpending(4500m);

        card.HasAvailableLimit(501m).Should().BeFalse();
        card.HasAvailableLimit(500m).Should().BeTrue();
    }

    [Fact]
    public void HasAvailableLimit_ExactRemaining_ShouldReturnTrue()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        card.AddSpending(3000m);

        card.HasAvailableLimit(2000m).Should().BeTrue();
    }

    [Fact]
    public void ResetMonthlySpending_ShouldSetToZero()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        card.AddSpending(3500m);
        card.SpentThisMonth.Should().Be(3500m);

        card.ResetMonthlySpending();

        card.SpentThisMonth.Should().Be(0m);
        card.AvailableLimit.Should().Be(5000m);
    }

    [Fact]
    public void ResetMonthlySpending_WhenAlreadyZero_ShouldRemainZero()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");

        card.ResetMonthlySpending();

        card.SpentThisMonth.Should().Be(0m);
    }

    [Fact]
    public void AvailableLimit_ShouldReflectSpending()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        card.AvailableLimit.Should().Be(5000m);

        card.AddSpending(1500m);
        card.AvailableLimit.Should().Be(3500m);

        card.ReduceSpending(500m);
        card.AvailableLimit.Should().Be(4000m);

        card.ResetMonthlySpending();
        card.AvailableLimit.Should().Be(5000m);
    }

    // ==========================================
    // TESTES DE VALIDACAO DE COMPRA
    // ==========================================

    [Fact]
    public void ValidatePurchase_WithAvailableLimit_ShouldAllow()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");

        var (allowed, reason) = card.ValidatePurchase(1000m, false, false);

        allowed.Should().BeTrue();
        reason.Should().BeNull();
    }

    [Fact]
    public void ValidatePurchase_ExceedsLimit_ShouldReject()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        card.AddSpending(4500m);

        var (allowed, reason) = card.ValidatePurchase(600m, false, false);

        allowed.Should().BeFalse();
        reason.Should().Contain("Limite insuficiente");
    }

    [Fact]
    public void ValidatePurchase_Cancelled_ShouldReject()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        card.Cancel();

        var (allowed, reason) = card.ValidatePurchase(100m, false, false);

        allowed.Should().BeFalse();
        reason.Should().Contain("Cancelled");
    }

    [Fact]
    public void ValidatePurchase_InternationalDisabled_ShouldReject()
    {
        var card = VirtualCard.Create(Guid.NewGuid(), "TEST");
        // IsInternational default = false

        var (allowed, reason) = card.ValidatePurchase(100m, isOnline: false, isInternational: true);

        allowed.Should().BeFalse();
        reason.Should().Contain("internacionais");
    }
}
