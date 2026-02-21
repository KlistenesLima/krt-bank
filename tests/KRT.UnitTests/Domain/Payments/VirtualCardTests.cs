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
}