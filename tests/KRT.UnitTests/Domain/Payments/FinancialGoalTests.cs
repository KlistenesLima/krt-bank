using FluentAssertions;
using KRT.Payments.Domain.Entities;
using Xunit;

namespace KRT.UnitTests.Domain.Payments;

public class FinancialGoalTests
{
    private static FinancialGoal MakeGoal(decimal target = 10000m) =>
        FinancialGoal.Create(Guid.NewGuid(), "Reserva de emergencia", target, DateTime.UtcNow.AddMonths(6), "shield", "Reserva");

    [Fact]
    public void Create_ValidData_ShouldSucceed()
    {
        var goal = MakeGoal();
        goal.Title.Should().Be("Reserva de emergencia");
        goal.TargetAmount.Should().Be(10000m);
        goal.CurrentAmount.Should().Be(0m);
        goal.Status.Should().Be(FinancialGoalStatus.Active);
        goal.Icon.Should().Be("shield");
        goal.Category.Should().Be("Reserva");
    }

    [Fact]
    public void Create_EmptyTitle_ShouldThrow()
    {
        var act = () => FinancialGoal.Create(Guid.NewGuid(), "", 1000m, DateTime.UtcNow.AddMonths(1));
        act.Should().Throw<ArgumentException>().WithMessage("*Titulo*");
    }

    [Fact]
    public void Create_WhitespaceTitle_ShouldThrow()
    {
        var act = () => FinancialGoal.Create(Guid.NewGuid(), "   ", 1000m, DateTime.UtcNow.AddMonths(1));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ZeroTarget_ShouldThrow()
    {
        var act = () => FinancialGoal.Create(Guid.NewGuid(), "Meta", 0m, DateTime.UtcNow.AddMonths(1));
        act.Should().Throw<ArgumentException>().WithMessage("*positivo*");
    }

    [Fact]
    public void Create_NegativeTarget_ShouldThrow()
    {
        var act = () => FinancialGoal.Create(Guid.NewGuid(), "Meta", -500m, DateTime.UtcNow.AddMonths(1));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_PastDeadline_ShouldThrow()
    {
        var act = () => FinancialGoal.Create(Guid.NewGuid(), "Meta", 1000m, DateTime.UtcNow.AddDays(-1));
        act.Should().Throw<ArgumentException>().WithMessage("*futuro*");
    }

    [Fact]
    public void Deposit_PositiveAmount_ShouldIncrease()
    {
        var goal = MakeGoal();
        goal.Deposit(2500m);
        goal.CurrentAmount.Should().Be(2500m);
        goal.Status.Should().Be(FinancialGoalStatus.Active);
    }

    [Fact]
    public void Deposit_MultipleTimes_ShouldAccumulate()
    {
        var goal = MakeGoal();
        goal.Deposit(3000m);
        goal.Deposit(2000m);
        goal.CurrentAmount.Should().Be(5000m);
    }

    [Fact]
    public void Deposit_ReachTarget_ShouldComplete()
    {
        var goal = MakeGoal(5000m);
        goal.Deposit(5000m);
        goal.Status.Should().Be(FinancialGoalStatus.Completed);
        goal.CompletedAt.Should().NotBeNull();
        goal.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Deposit_ExceedTarget_ShouldComplete()
    {
        var goal = MakeGoal(1000m);
        goal.Deposit(1500m);
        goal.Status.Should().Be(FinancialGoalStatus.Completed);
        goal.CurrentAmount.Should().Be(1500m);
    }

    [Fact]
    public void Deposit_Zero_ShouldThrow()
    {
        var act = () => MakeGoal().Deposit(0m);
        act.Should().Throw<ArgumentException>().WithMessage("*positivo*");
    }

    [Fact]
    public void Deposit_Negative_ShouldThrow()
    {
        var act = () => MakeGoal().Deposit(-100m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deposit_CancelledGoal_ShouldThrow()
    {
        var goal = MakeGoal();
        goal.Cancel();
        var act = () => goal.Deposit(100m);
        act.Should().Throw<InvalidOperationException>().WithMessage("*cancelada*");
    }

    [Fact]
    public void Withdraw_ValidAmount_ShouldDecrease()
    {
        var goal = MakeGoal();
        goal.Deposit(5000m);
        goal.Withdraw(2000m);
        goal.CurrentAmount.Should().Be(3000m);
    }

    [Fact]
    public void Withdraw_ExceedsBalance_ShouldThrow()
    {
        var goal = MakeGoal();
        goal.Deposit(1000m);
        var act = () => goal.Withdraw(2000m);
        act.Should().Throw<InvalidOperationException>().WithMessage("*insuficiente*");
    }

    [Fact]
    public void Withdraw_Zero_ShouldThrow()
    {
        var goal = MakeGoal();
        goal.Deposit(1000m);
        var act = () => goal.Withdraw(0m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Withdraw_FromCompleted_ShouldReactivate()
    {
        var goal = MakeGoal(1000m);
        goal.Deposit(1000m);
        goal.Status.Should().Be(FinancialGoalStatus.Completed);

        goal.Withdraw(100m);
        goal.Status.Should().Be(FinancialGoalStatus.Active);
        goal.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Withdraw_CancelledGoal_ShouldThrow()
    {
        var goal = MakeGoal();
        goal.Deposit(1000m);
        goal.Cancel();
        var act = () => goal.Withdraw(500m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_ShouldSetStatusCancelled()
    {
        var goal = MakeGoal();
        goal.Cancel();
        goal.Status.Should().Be(FinancialGoalStatus.Cancelled);
    }

    [Fact]
    public void ProgressPercent_ShouldCalculateCorrectly()
    {
        var goal = MakeGoal(10000m);
        goal.Deposit(2500m);
        goal.ProgressPercent.Should().Be(25.0m);
    }

    [Fact]
    public void ProgressPercent_Empty_ShouldBeZero()
    {
        var goal = MakeGoal();
        goal.ProgressPercent.Should().Be(0m);
    }

    [Fact]
    public void RemainingAmount_ShouldCalculateCorrectly()
    {
        var goal = MakeGoal(10000m);
        goal.Deposit(3000m);
        goal.RemainingAmount.Should().Be(7000m);
    }

    [Fact]
    public void RemainingAmount_WhenExceeded_ShouldBeZero()
    {
        var goal = MakeGoal(1000m);
        goal.Deposit(1500m);
        goal.RemainingAmount.Should().Be(0m);
    }

    [Fact]
    public void GetStatusLabel_Active_ShouldReturnCorrect()
    {
        MakeGoal().GetStatusLabel().Should().Be("Em andamento");
    }

    [Fact]
    public void GetStatusLabel_Completed_ShouldReturnCorrect()
    {
        var goal = MakeGoal(100m);
        goal.Deposit(100m);
        goal.GetStatusLabel().Should().Be("Concluida");
    }

    [Fact]
    public void GetStatusLabel_Cancelled_ShouldReturnCorrect()
    {
        var goal = MakeGoal();
        goal.Cancel();
        goal.GetStatusLabel().Should().Be("Cancelada");
    }

    [Fact]
    public void FullFlow_DepositWithdrawComplete()
    {
        var goal = MakeGoal(5000m);
        goal.Deposit(3000m);
        goal.Withdraw(1000m);
        goal.Deposit(3000m);
        goal.Status.Should().Be(FinancialGoalStatus.Completed);
        goal.CurrentAmount.Should().Be(5000m);
    }
}
