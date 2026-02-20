using KRT.Payments.Domain.Entities;
using Xunit;

namespace KRT.UnitTests.Domain.Payments;

public class BoletoTests
{
    [Fact]
    public void Generate_ValidData_ShouldSucceed()
    {
        var b = Boleto.Generate(Guid.NewGuid(), "Empresa", "12345678000190", 500m, DateTime.UtcNow.AddDays(30), "Teste");
        Assert.Equal(BoletoStatus.Pending, b.Status);
        Assert.Equal(500m, b.Amount);
    }

    [Fact]
    public void Pay_Pending_ShouldSucceed()
    {
        var b = Boleto.Generate(Guid.NewGuid(), "Empresa", "", 100m, DateTime.UtcNow.AddDays(10), "");
        var (ok, _) = b.Pay();
        Assert.True(ok);
        Assert.Equal(BoletoStatus.Processing, b.Status);
        Assert.NotNull(b.PaidAt);
    }

    [Fact]
    public void Pay_AlreadyPaid_ShouldFail()
    {
        var b = Boleto.Generate(Guid.NewGuid(), "X", "", 50m, DateTime.UtcNow.AddDays(5), "");
        b.Pay();
        var (ok, _) = b.Pay();
        Assert.False(ok);
    }

    [Fact]
    public void Cancel_Paid_ShouldThrow()
    {
        var b = Boleto.Generate(Guid.NewGuid(), "X", "", 50m, DateTime.UtcNow.AddDays(5), "");
        b.Pay();
        b.Compensate();
        Assert.Throws<InvalidOperationException>(() => b.Cancel());
    }

    [Fact]
    public void Generate_NegativeAmount_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => Boleto.Generate(Guid.NewGuid(), "X", "", -10m, DateTime.UtcNow.AddDays(5), ""));
    }
}