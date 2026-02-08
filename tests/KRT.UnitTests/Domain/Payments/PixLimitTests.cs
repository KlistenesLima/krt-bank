using FluentAssertions;
using KRT.Payments.Domain.Entities;
using Xunit;

namespace KRT.UnitTests.Domain.Payments;

public class PixLimitTests
{
    [Fact]
    public void CreateDefault_ShouldHaveCorrectLimits()
    {
        var limit = PixLimit.CreateDefault(Guid.NewGuid());

        limit.DaytimePerTransaction.Should().Be(5000m);
        limit.DaytimeDaily.Should().Be(20000m);
        limit.NighttimePerTransaction.Should().Be(1000m);
        limit.NighttimeDaily.Should().Be(5000m);
    }

    [Fact]
    public void ValidateTransfer_Daytime_WithinLimits_ShouldAllow()
    {
        var limit = PixLimit.CreateDefault(Guid.NewGuid());
        var daytime = new DateTime(2099, 6, 15, 14, 0, 0); // 14h

        var (isAllowed, reason) = limit.ValidateTransfer(1000m, daytime);

        isAllowed.Should().BeTrue();
        reason.Should().BeNull();
    }

    [Fact]
    public void ValidateTransfer_Daytime_ExceedsPerTransaction_ShouldReject()
    {
        var limit = PixLimit.CreateDefault(Guid.NewGuid());
        var daytime = new DateTime(2099, 6, 15, 14, 0, 0);

        var (isAllowed, reason) = limit.ValidateTransfer(6000m, daytime);

        isAllowed.Should().BeFalse();
        reason.Should().Contain("limite por transacao");
    }

    [Fact]
    public void ValidateTransfer_Nighttime_ShouldUseLowerLimits()
    {
        var limit = PixLimit.CreateDefault(Guid.NewGuid());
        var nighttime = new DateTime(2099, 6, 15, 22, 0, 0); // 22h

        var (isAllowed, _) = limit.ValidateTransfer(1500m, nighttime);

        isAllowed.Should().BeFalse(); // Noturno per-tx = 1000
    }

    [Fact]
    public void RegisterUsage_ShouldAccumulate()
    {
        var limit = PixLimit.CreateDefault(Guid.NewGuid());
        var daytime = new DateTime(2099, 6, 15, 10, 0, 0);

        limit.RegisterUsage(2000m, daytime);
        limit.RegisterUsage(3000m, daytime);

        var (isAllowed, reason) = limit.ValidateTransfer(4000m, daytime);
        // 2000 + 3000 = 5000 usado, + 4000 = 9000 < 20000 daily â€” OK
        isAllowed.Should().BeTrue();
    }

    [Fact]
    public void RegisterUsage_ExceedsDailyLimit_ShouldReject()
    {
        var limit = PixLimit.CreateDefault(Guid.NewGuid());
        var daytime = new DateTime(2099, 6, 15, 10, 0, 0);

        limit.RegisterUsage(5000m, daytime);
        limit.RegisterUsage(5000m, daytime);
        limit.RegisterUsage(5000m, daytime);
        limit.RegisterUsage(5000m, daytime); // 20000 total

        var (isAllowed, reason) = limit.ValidateTransfer(100m, daytime);

        isAllowed.Should().BeFalse();
        reason.Should().Contain("limite diario");
    }

    [Fact]
    public void ResetDaily_NextDay_ShouldResetCounters()
    {
        var limit = PixLimit.CreateDefault(Guid.NewGuid());
        var day1 = new DateTime(2099, 6, 15, 10, 0, 0);
        var day2 = new DateTime(2099, 6, 16, 10, 0, 0);

        limit.RegisterUsage(5000m, day1);
        limit.RegisterUsage(5000m, day1);
        limit.RegisterUsage(5000m, day1);
        limit.RegisterUsage(5000m, day1); // 20000 â€” esgotado

        // Proximo dia â€” deve resetar
        var (isAllowed, _) = limit.ValidateTransfer(1000m, day2);
        isAllowed.Should().BeTrue();
    }
}