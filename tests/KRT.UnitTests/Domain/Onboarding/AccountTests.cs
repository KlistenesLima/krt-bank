using FluentAssertions;
using KRT.BuildingBlocks.Domain.Exceptions;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Enums;
using Xunit;

namespace KRT.UnitTests.Domain.Onboarding;

public class AccountTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateActiveAccount()
    {
        var account = new Account("Joao Silva", "12345678901", "joao@email.com", AccountType.Checking);

        account.Id.Should().NotBeEmpty();
        account.CustomerName.Should().Be("Joao Silva");
        account.Document.Should().Be("12345678901");
        account.Status.Should().Be(AccountStatus.Active);
        account.Balance.Should().Be(0);
    }

    [Fact]
    public void Create_WithNullName_ShouldThrow()
    {
        var act = () => new Account(null!, "12345678901", "e@e.com", AccountType.Checking);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Create_WithNullDocument_ShouldThrow()
    {
        var act = () => new Account("Joao", null!, "e@e.com", AccountType.Checking);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Create_WithNullEmail_ShouldThrow()
    {
        var act = () => new Account("Joao", "12345678901", null!, AccountType.Checking);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Credit_PositiveAmount_ShouldIncrease()
    {
        var a = MakeAccount();
        a.Credit(100m);
        a.Balance.Should().Be(100m);
    }

    [Fact]
    public void Credit_MultipleTimes_ShouldAccumulate()
    {
        var a = MakeAccount();
        a.Credit(50m);
        a.Credit(30m);
        a.Balance.Should().Be(80m);
    }

    [Fact]
    public void Credit_Zero_ShouldThrow()
    {
        var act = () => MakeAccount().Credit(0);
        act.Should().Throw<BusinessRuleException>().WithMessage("*positivo*");
    }

    [Fact]
    public void Credit_Negative_ShouldThrow()
    {
        var act = () => MakeAccount().Credit(-10);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Debit_SufficientBalance_ShouldDecrease()
    {
        var a = MakeAccount();
        a.Credit(200m);
        a.Debit(50m);
        a.Balance.Should().Be(150m);
    }

    [Fact]
    public void Debit_ExactBalance_ShouldZeroOut()
    {
        var a = MakeAccount();
        a.Credit(100m);
        a.Debit(100m);
        a.Balance.Should().Be(0m);
    }

    [Fact]
    public void Debit_InsufficientBalance_ShouldThrow()
    {
        var a = MakeAccount();
        a.Credit(50m);
        var act = () => a.Debit(100m);
        act.Should().Throw<BusinessRuleException>().WithMessage("*insuficiente*");
    }

    [Fact]
    public void Block_ActiveAccount_ShouldBlock()
    {
        var a = MakeAccount();
        a.Block("Fraude");
        a.Status.Should().Be(AccountStatus.Blocked);
    }

    [Fact]
    public void Block_NonActive_ShouldThrow()
    {
        var a = MakeAccount();
        a.Block("x");
        var act = () => a.Block("y");
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Close_ZeroBalance_ShouldClose()
    {
        var a = MakeAccount();
        a.Close("Voluntario");
        a.Status.Should().Be(AccountStatus.Closed);
    }

    [Fact]
    public void Close_WithBalance_ShouldThrow()
    {
        var a = MakeAccount();
        a.Credit(100m);
        var act = () => a.Close("x");
        act.Should().Throw<BusinessRuleException>().WithMessage("*saldo zero*");
    }

    [Fact]
    public void Activate_Blocked_ShouldReactivate()
    {
        var a = MakeAccount();
        a.Block("x");
        a.Activate();
        a.Status.Should().Be(AccountStatus.Active);
    }

    [Fact]
    public void Activate_Closed_ShouldThrow()
    {
        var a = MakeAccount();
        a.Close("x");
        var act = () => a.Activate();
        act.Should().Throw<BusinessRuleException>();
    }

    private static Account MakeAccount()
        => new("Maria", "98765432100", "maria@email.com", AccountType.Checking);
}
