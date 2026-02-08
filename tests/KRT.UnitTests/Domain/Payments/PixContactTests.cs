using KRT.Payments.Domain.Entities;
using Xunit;

namespace KRT.UnitTests.Domain.Payments;

public class PixContactTests
{
    [Fact]
    public void Create_ValidData_ShouldSucceed()
    {
        var c = PixContact.Create(Guid.NewGuid(), "Maria Silva", "maria@email.com", "EMAIL", "Nubank");
        Assert.Equal("Maria Silva", c.Name);
        Assert.False(c.IsFavorite);
        Assert.Equal(0, c.TransferCount);
    }

    [Fact]
    public void Create_EmptyName_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => PixContact.Create(Guid.NewGuid(), "", "key", "CPF"));
    }

    [Fact]
    public void ToggleFavorite_ShouldToggle()
    {
        var c = PixContact.Create(Guid.NewGuid(), "Test", "key", "CPF");
        Assert.False(c.IsFavorite);
        c.ToggleFavorite();
        Assert.True(c.IsFavorite);
        c.ToggleFavorite();
        Assert.False(c.IsFavorite);
    }

    [Fact]
    public void RecordTransfer_ShouldIncrement()
    {
        var c = PixContact.Create(Guid.NewGuid(), "Test", "key", "CPF");
        c.RecordTransfer();
        c.RecordTransfer();
        Assert.Equal(2, c.TransferCount);
        Assert.NotNull(c.LastTransferAt);
    }

    [Fact]
    public void GetDisplayName_WithNickname_ShouldShowBoth()
    {
        var c = PixContact.Create(Guid.NewGuid(), "Maria Silva", "key", "CPF", null, null);
        Assert.Equal("Maria Silva", c.GetDisplayName());
        c.Update("Maria Silva", "Mari", null);
        Assert.Equal("Mari (Maria Silva)", c.GetDisplayName());
    }
}