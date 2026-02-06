using FluentAssertions;
using KRT.BuildingBlocks.Domain;
using KRT.BuildingBlocks.Domain.ValueObjects;
using Xunit;

namespace KRT.UnitTests.Domain.BuildingBlocks;

public class ValueObjectTests
{
    [Fact] public void Cpf_Valid_ShouldCreate()       { Cpf.Create("12345678901").IsSuccess.Should().BeTrue(); }
    [Fact] public void Cpf_Empty_ShouldFail()          { Cpf.Create("").IsFailure.Should().BeTrue(); }
    [Fact] public void Cpf_Short_ShouldFail()           { Cpf.Create("123").IsFailure.Should().BeTrue(); }

    [Fact] public void Email_Valid_ShouldCreate()      { Email.Create("a@b.com").IsSuccess.Should().BeTrue(); }
    [Fact] public void Email_NoAt_ShouldFail()          { Email.Create("invalido").IsFailure.Should().BeTrue(); }
    [Fact] public void Email_Empty_ShouldFail()         { Email.Create("").IsFailure.Should().BeTrue(); }

    [Fact] public void Phone_Valid_ShouldCreate()      { Phone.Create("11999887766").IsSuccess.Should().BeTrue(); }
    [Fact] public void Phone_Empty_ShouldFail()         { Phone.Create("").IsFailure.Should().BeTrue(); }

    [Fact] public void Money_ShouldRound()             { Money.Create(10.556m).Amount.Should().Be(10.56m); }
    [Fact] public void Money_Add_ShouldWork()          { (Money.Create(100) + Money.Create(50)).Amount.Should().Be(150); }
    [Fact] public void Money_Sub_ShouldWork()          { (Money.Create(100) - Money.Create(30)).Amount.Should().Be(70); }
    [Fact] public void Money_DiffCurrency_ShouldThrow(){ FluentActions.Invoking(() => Money.Create(1,"BRL") + Money.Create(1,"USD")).Should().Throw<InvalidOperationException>(); }
    [Fact] public void Money_IsZero_ShouldWork()       { Money.Zero().IsZero().Should().BeTrue(); }
    [Fact] public void Money_Equality_ShouldWork()     { Money.Create(100).Should().Be(Money.Create(100)); }
    [Fact] public void Money_Inequality_ShouldWork()   { Money.Create(100).Should().NotBe(Money.Create(200)); }

    [Fact] public void Result_Ok_IsSuccess()           { Result.Ok().IsSuccess.Should().BeTrue(); }
    [Fact] public void Result_Fail_IsFailure()          { Result.Fail("err").IsFailure.Should().BeTrue(); }
    [Fact] public void ResultT_Ok_HasValue()            { Result.Ok(42).Value.Should().Be(42); }
    [Fact] public void ResultT_Fail_HasCode()           { Result.Fail<int>("err", "CODE").ErrorCode.Should().Be("CODE"); }
}
