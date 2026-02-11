using FluentAssertions;
using KRT.Payments.Domain.Entities;
using Xunit;

namespace KRT.UnitTests.Domain.Payments;

public class NotificationTests
{
    [Fact]
    public void Create_ShouldSetDefaults()
    {
        var n = Notification.Create(Guid.NewGuid(), "PIX recebido", "Voce recebeu R$ 100,00");
        n.Id.Should().NotBeEmpty();
        n.Title.Should().Be("PIX recebido");
        n.Message.Should().Be("Voce recebeu R$ 100,00");
        n.Category.Should().Be("geral");
        n.Severity.Should().Be("info");
        n.IsRead.Should().BeFalse();
        n.ReadAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithCategory_ShouldSetCategory()
    {
        var n = Notification.Create(Guid.NewGuid(), "Alerta", "Compra suspeita", "seguranca", "warning");
        n.Category.Should().Be("seguranca");
        n.Severity.Should().Be("warning");
    }

    [Fact]
    public void MarkAsRead_ShouldUpdateFields()
    {
        var n = Notification.Create(Guid.NewGuid(), "Test", "Msg");
        n.MarkAsRead();
        n.IsRead.Should().BeTrue();
        n.ReadAt.Should().NotBeNull();
        n.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void MarkAsRead_Twice_ShouldNotThrow()
    {
        var n = Notification.Create(Guid.NewGuid(), "Test", "Msg");
        n.MarkAsRead();
        var firstReadAt = n.ReadAt;
        n.MarkAsRead();
        n.IsRead.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldHaveCreatedAt()
    {
        var n = Notification.Create(Guid.NewGuid(), "T", "M");
        n.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldAssociateAccount()
    {
        var accountId = Guid.NewGuid();
        var n = Notification.Create(accountId, "T", "M");
        n.AccountId.Should().Be(accountId);
    }
}
