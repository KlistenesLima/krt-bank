using FluentAssertions;
using KRT.BuildingBlocks.Domain.Exceptions;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Enums;
using Xunit;

namespace KRT.UnitTests.Domain.Onboarding;

public class PixKeyTests
{
    private static readonly Guid AccountId = Guid.NewGuid();

    [Fact]
    public void Create_Cpf_Valid_ShouldSucceed()
    {
        var key = PixKey.Create(AccountId, PixKeyType.Cpf, "123.456.789-01");
        key.KeyType.Should().Be(PixKeyType.Cpf);
        key.KeyValue.Should().Be("12345678901");
        key.IsActive.Should().BeTrue();
        key.AccountId.Should().Be(AccountId);
    }

    [Fact]
    public void Create_Cpf_ShouldNormalizeDigitsOnly()
    {
        var key = PixKey.Create(AccountId, PixKeyType.Cpf, "999.888.777-66");
        key.KeyValue.Should().Be("99988877766");
    }

    [Fact]
    public void Create_Cpf_InvalidLength_ShouldThrow()
    {
        var act = () => PixKey.Create(AccountId, PixKeyType.Cpf, "12345");
        act.Should().Throw<BusinessRuleException>().WithMessage("*11*");
    }

    [Fact]
    public void Create_Email_Valid_ShouldSucceed()
    {
        var key = PixKey.Create(AccountId, PixKeyType.Email, "Teste@Email.COM");
        key.KeyType.Should().Be(PixKeyType.Email);
        key.KeyValue.Should().Be("teste@email.com");
    }

    [Fact]
    public void Create_Email_NoAt_ShouldThrow()
    {
        var act = () => PixKey.Create(AccountId, PixKeyType.Email, "invalidemail.com");
        act.Should().Throw<BusinessRuleException>().WithMessage("*Email*");
    }

    [Fact]
    public void Create_Email_NoDot_ShouldThrow()
    {
        var act = () => PixKey.Create(AccountId, PixKeyType.Email, "test@emailcom");
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Create_Email_TooLong_ShouldThrow()
    {
        var longEmail = new string('a', 70) + "@test.com";
        var act = () => PixKey.Create(AccountId, PixKeyType.Email, longEmail);
        act.Should().Throw<BusinessRuleException>().WithMessage("*77*");
    }

    [Fact]
    public void Create_Phone_Valid11Digits_ShouldSucceed()
    {
        var key = PixKey.Create(AccountId, PixKeyType.Phone, "(83) 99999-8888");
        key.KeyType.Should().Be(PixKeyType.Phone);
        key.KeyValue.Should().Be("+5583999998888");
    }

    [Fact]
    public void Create_Phone_Valid10Digits_ShouldSucceed()
    {
        var key = PixKey.Create(AccountId, PixKeyType.Phone, "8332221111");
        key.KeyValue.Should().Be("+558332221111");
    }

    [Fact]
    public void Create_Phone_TooShort_ShouldThrow()
    {
        var act = () => PixKey.Create(AccountId, PixKeyType.Phone, "12345");
        act.Should().Throw<BusinessRuleException>().WithMessage("*10 ou 11*");
    }

    [Fact]
    public void Create_Phone_TooLong_ShouldThrow()
    {
        var act = () => PixKey.Create(AccountId, PixKeyType.Phone, "123456789012");
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Create_Random_Valid_ShouldSucceed()
    {
        var uuid = Guid.NewGuid().ToString();
        var key = PixKey.Create(AccountId, PixKeyType.Random, uuid);
        key.KeyType.Should().Be(PixKeyType.Random);
        key.KeyValue.Should().Be(uuid);
    }

    [Fact]
    public void Create_Random_TooLong_ShouldThrow()
    {
        var act = () => PixKey.Create(AccountId, PixKeyType.Random, new string('x', 37));
        act.Should().Throw<BusinessRuleException>().WithMessage("*36*");
    }

    [Fact]
    public void Create_EmptyAccountId_ShouldThrow()
    {
        var act = () => PixKey.Create(Guid.Empty, PixKeyType.Cpf, "12345678901");
        act.Should().Throw<BusinessRuleException>().WithMessage("*AccountId*");
    }

    [Fact]
    public void Create_EmptyKeyValue_ShouldThrow()
    {
        var act = () => PixKey.Create(AccountId, PixKeyType.Cpf, "");
        act.Should().Throw<BusinessRuleException>().WithMessage("*obrigat*");
    }

    [Fact]
    public void Create_WhitespaceKeyValue_ShouldThrow()
    {
        var act = () => PixKey.Create(AccountId, PixKeyType.Email, "   ");
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Deactivate_ActiveKey_ShouldDeactivate()
    {
        var key = PixKey.Create(AccountId, PixKeyType.Cpf, "12345678901");
        key.Deactivate();
        key.IsActive.Should().BeFalse();
        key.DeactivatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_AlreadyInactive_ShouldThrow()
    {
        var key = PixKey.Create(AccountId, PixKeyType.Cpf, "12345678901");
        key.Deactivate();
        var act = () => key.Deactivate();
        act.Should().Throw<BusinessRuleException>().WithMessage("*inativa*");
    }

    [Fact]
    public void Reactivate_InactiveKey_ShouldReactivate()
    {
        var key = PixKey.Create(AccountId, PixKeyType.Cpf, "12345678901");
        key.Deactivate();
        key.Reactivate();
        key.IsActive.Should().BeTrue();
        key.DeactivatedAt.Should().BeNull();
    }

    [Fact]
    public void Reactivate_AlreadyActive_ShouldThrow()
    {
        var key = PixKey.Create(AccountId, PixKeyType.Cpf, "12345678901");
        var act = () => key.Reactivate();
        act.Should().Throw<BusinessRuleException>().WithMessage("*ativa*");
    }

    [Fact]
    public void FullFlow_CreateDeactivateReactivate()
    {
        var key = PixKey.Create(AccountId, PixKeyType.Email, "test@krt.com");
        key.IsActive.Should().BeTrue();
        key.Deactivate();
        key.IsActive.Should().BeFalse();
        key.Reactivate();
        key.IsActive.Should().BeTrue();
    }
}
