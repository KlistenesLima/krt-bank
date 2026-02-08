using KRT.Payments.Domain.Entities;
using Xunit;

namespace KRT.UnitTests.Domain.Payments;

public class ScheduledPixTests
{
    private static ScheduledPix CreateTestScheduled(
        ScheduledPixFrequency freq = ScheduledPixFrequency.Once,
        int? maxExec = null)
    {
        return ScheduledPix.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            "test@email.com", "Test User", 100.00m,
            "Test payment",
            DateTime.UtcNow.AddDays(1),
            freq, null, maxExec);
    }

    [Fact]
    public void Create_Once_ShouldBePending()
    {
        var s = CreateTestScheduled();
        Assert.Equal(ScheduledPixStatus.Pending, s.Status);
        Assert.False(s.IsRecurring);
        Assert.Equal("Unico", s.GetFrequencyLabel());
    }

    [Fact]
    public void Create_Monthly_ShouldBeRecurring()
    {
        var s = CreateTestScheduled(ScheduledPixFrequency.Monthly, 6);
        Assert.True(s.IsRecurring);
        Assert.Equal(6, s.MaxExecutions);
        Assert.Equal("Mensal", s.GetFrequencyLabel());
    }

    [Fact]
    public void Execute_Once_ShouldMarkExecuted()
    {
        var s = CreateTestScheduled();
        var (success, _) = s.Execute();
        Assert.True(success);
        Assert.Equal(ScheduledPixStatus.Executed, s.Status);
        Assert.Equal(1, s.ExecutionCount);
        Assert.Null(s.NextExecutionDate);
    }

    [Fact]
    public void Execute_Recurring_ShouldScheduleNext()
    {
        var s = CreateTestScheduled(ScheduledPixFrequency.Monthly, 12);
        var (success, _) = s.Execute();
        Assert.True(success);
        Assert.Equal(1, s.ExecutionCount);
        Assert.Equal(ScheduledPixStatus.Pending, s.Status);
        Assert.NotNull(s.NextExecutionDate);
    }

    [Fact]
    public void Cancel_ShouldPreventExecution()
    {
        var s = CreateTestScheduled();
        s.Cancel();
        Assert.Equal(ScheduledPixStatus.Cancelled, s.Status);
        var (success, _) = s.Execute();
        Assert.False(success);
    }

    [Fact]
    public void Pause_Resume_Recurring()
    {
        var s = CreateTestScheduled(ScheduledPixFrequency.Weekly, 10);
        s.Pause();
        Assert.Equal(ScheduledPixStatus.Paused, s.Status);
        s.Resume();
        Assert.Equal(ScheduledPixStatus.Pending, s.Status);
        Assert.NotNull(s.NextExecutionDate);
    }

    [Fact]
    public void Pause_NonRecurring_ShouldThrow()
    {
        var s = CreateTestScheduled(ScheduledPixFrequency.Once);
        Assert.Throws<InvalidOperationException>(() => s.Pause());
    }

    [Fact]
    public void UpdateAmount_ShouldUpdate()
    {
        var s = CreateTestScheduled();
        s.UpdateAmount(200.00m);
        Assert.Equal(200.00m, s.Amount);
    }

    [Fact]
    public void UpdateAmount_Zero_ShouldThrow()
    {
        var s = CreateTestScheduled();
        Assert.Throws<ArgumentException>(() => s.UpdateAmount(0));
    }
}