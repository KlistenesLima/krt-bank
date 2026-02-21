using FluentAssertions;
using KRT.Payments.Api.Controllers;
using KRT.Payments.Api.Data;
using KRT.Payments.Api.Services;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KRT.UnitTests.Domain.Payments;

public class CardBillingTests : IDisposable
{
    private readonly PaymentsDbContext _db;
    private readonly VirtualCardsController _cardsController;
    private readonly CardChargesController _chargesController;

    // IDs fixos para testes
    private static readonly Guid AccountId = Guid.Parse("a1b2c3d4-0000-0000-0000-aabbccddeeff");
    private static readonly Guid MerchantId = ChargePaymentService.MerchantAccountId;

    public CardBillingTests()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new PaymentsDbContext(options);

        // Seed accounts
        _db.BankAccounts.Add(new BankAccount
        {
            Id = AccountId,
            CustomerName = "Teste Usuario",
            Document = "12345678900",
            Email = "test@krt.com",
            Balance = 300_000m,
            Status = "Active",
            RowVersion = Guid.NewGuid().ToByteArray()
        });

        _db.BankAccounts.Add(new BankAccount
        {
            Id = MerchantId,
            CustomerName = "AUREA Maison",
            Document = "99999999000199",
            Email = "aurea@krt.com",
            Balance = 0m,
            Status = "Active",
            RowVersion = Guid.NewGuid().ToByteArray()
        });

        _db.SaveChanges();

        var logger = Mock.Of<ILogger<VirtualCardsController>>();
        var paymentService = new ChargePaymentService(_db);
        _cardsController = new VirtualCardsController(_db, logger);
        _chargesController = new CardChargesController(_db, paymentService);
    }

    public void Dispose() => _db.Dispose();

    private VirtualCard CreateAndSaveCard()
    {
        var card = VirtualCard.Create(AccountId, "TESTE USUARIO");
        _db.VirtualCards.Add(card);
        _db.SaveChanges();
        return card;
    }

    private async Task<CardCharge> CreateAndSettleCharge(Guid cardId, decimal amount, string description = "Compra teste")
    {
        var card = await _db.VirtualCards.FindAsync(cardId);
        card!.AddSpending(amount);

        var charge = new CardCharge
        {
            Id = Guid.NewGuid(),
            CardId = cardId,
            ExternalId = "",
            Amount = amount,
            Description = description,
            Status = CardChargeStatus.Settled,
            AuthorizationCode = $"AUTH{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            Installments = 1,
            InstallmentAmount = amount,
            CreatedAt = DateTime.UtcNow
        };
        _db.CardCharges.Add(charge);
        await _db.SaveChangesAsync();
        return charge;
    }

    // ==========================================
    // TESTES PAY-BILL
    // ==========================================

    [Fact]
    public async Task PayBill_ShouldDebitAccountAndReduceSpending()
    {
        var card = CreateAndSaveCard();
        await CreateAndSettleCharge(card.Id, 2000m);

        var initialBalance = (await _db.BankAccounts.FindAsync(AccountId))!.Balance;

        var result = await _cardsController.PayBill(card.Id,
            new PayBillRequest(2000m, AccountId), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = ok.Value!;
        var amountPaid = (decimal)value.GetType().GetProperty("amountPaid")!.GetValue(value)!;
        amountPaid.Should().Be(2000m);

        // Verificar que o saldo diminuiu
        var account = await _db.BankAccounts.FindAsync(AccountId);
        account!.Balance.Should().Be(initialBalance - 2000m);

        // Verificar que SpentThisMonth zerou
        var updatedCard = await _db.VirtualCards.FindAsync(card.Id);
        updatedCard!.SpentThisMonth.Should().Be(0m);
    }

    [Fact]
    public async Task PayBill_PartialPayment_ShouldReduceSpending()
    {
        var card = CreateAndSaveCard();
        await CreateAndSettleCharge(card.Id, 3000m);

        var result = await _cardsController.PayBill(card.Id,
            new PayBillRequest(1500m, AccountId), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();

        var updatedCard = await _db.VirtualCards.FindAsync(card.Id);
        updatedCard!.SpentThisMonth.Should().Be(1500m);
    }

    [Fact]
    public async Task PayBill_InsufficientBalance_ShouldFail()
    {
        var card = CreateAndSaveCard();
        await CreateAndSettleCharge(card.Id, 2000m);

        // Zerar saldo
        var account = await _db.BankAccounts.FindAsync(AccountId);
        account!.Balance = 100m;
        await _db.SaveChangesAsync();

        var result = await _cardsController.PayBill(card.Id,
            new PayBillRequest(2000m, AccountId), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var value = badRequest.Value!;
        var error = (string)value.GetType().GetProperty("error")!.GetValue(value)!;
        error.Should().Contain("Saldo insuficiente");
    }

    [Fact]
    public async Task PayBill_ExceedsDebt_ShouldFail()
    {
        var card = CreateAndSaveCard();
        await CreateAndSettleCharge(card.Id, 1000m);

        var result = await _cardsController.PayBill(card.Id,
            new PayBillRequest(1500m, AccountId), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var value = badRequest.Value!;
        var error = (string)value.GetType().GetProperty("error")!.GetValue(value)!;
        error.Should().Contain("excede fatura");
    }

    [Fact]
    public async Task PayBill_BelowMinimum_ShouldFail()
    {
        var card = CreateAndSaveCard();
        await CreateAndSettleCharge(card.Id, 5000m);

        // Minimo = 10% de 5000 = 500
        // Tentar pagar 400 (abaixo do minimo, e nao e pagamento total)
        var result = await _cardsController.PayBill(card.Id,
            new PayBillRequest(400m, AccountId), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var value = badRequest.Value!;
        var error = (string)value.GetType().GetProperty("error")!.GetValue(value)!;
        error.Should().Contain("minimo");
    }

    [Fact]
    public async Task PayBill_ExactMinimum_ShouldSucceed()
    {
        var card = CreateAndSaveCard();
        await CreateAndSettleCharge(card.Id, 5000m);

        // Minimo = 10% de 5000 = 500
        var result = await _cardsController.PayBill(card.Id,
            new PayBillRequest(500m, AccountId), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();

        var updatedCard = await _db.VirtualCards.FindAsync(card.Id);
        updatedCard!.SpentThisMonth.Should().Be(4500m);
    }

    [Fact]
    public async Task PayBill_NoBill_ShouldFail()
    {
        var card = CreateAndSaveCard();
        // Sem fatura pendente

        var result = await _cardsController.PayBill(card.Id,
            new PayBillRequest(100m, AccountId), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PayBill_ShouldCreateStatementEntry()
    {
        var card = CreateAndSaveCard();
        await CreateAndSettleCharge(card.Id, 2000m);

        await _cardsController.PayBill(card.Id,
            new PayBillRequest(2000m, AccountId), CancellationToken.None);

        var statement = await _db.StatementEntries
            .Where(s => s.AccountId == AccountId && s.Type == "PagamentoFatura")
            .FirstOrDefaultAsync();

        statement.Should().NotBeNull();
        statement!.Amount.Should().Be(2000m);
        statement.IsCredit.Should().BeFalse();
        statement.Description.Should().Contain("fatura");
    }

    // ==========================================
    // TESTES GET-BILL
    // ==========================================

    [Fact]
    public async Task GetBill_ShouldReturnCurrentMonthCharges()
    {
        var card = CreateAndSaveCard();
        await CreateAndSettleCharge(card.Id, 2500m, "Colar Riviera");
        await CreateAndSettleCharge(card.Id, 900m, "Restaurante");

        var result = await _cardsController.GetBill(card.Id, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = ok.Value!;

        var currentBill = (decimal)value.GetType().GetProperty("currentBill")!.GetValue(value)!;
        currentBill.Should().Be(3400m);

        var minimumPayment = (decimal)value.GetType().GetProperty("minimumPayment")!.GetValue(value)!;
        minimumPayment.Should().Be(340m); // 10% de 3400
    }

    [Fact]
    public async Task GetBill_EmptyBill_ShouldReturnZero()
    {
        var card = CreateAndSaveCard();

        var result = await _cardsController.GetBill(card.Id, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = ok.Value!;

        var currentBill = (decimal)value.GetType().GetProperty("currentBill")!.GetValue(value)!;
        currentBill.Should().Be(0m);

        var minimumPayment = (decimal)value.GetType().GetProperty("minimumPayment")!.GetValue(value)!;
        minimumPayment.Should().Be(0m);
    }

    [Fact]
    public async Task GetBill_ShouldHaveCorrectDueDate()
    {
        var card = CreateAndSaveCard();

        var result = await _cardsController.GetBill(card.Id, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = ok.Value!;

        var now = DateTime.UtcNow;
        var expectedClosing = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
        var expectedDue = expectedClosing.AddDays(10);

        var closingDate = (string)value.GetType().GetProperty("closingDate")!.GetValue(value)!;
        closingDate.Should().Be(expectedClosing.ToString("yyyy-MM-dd"));

        var dueDate = (string)value.GetType().GetProperty("dueDate")!.GetValue(value)!;
        dueDate.Should().Be(expectedDue.ToString("yyyy-MM-dd"));
    }

    [Fact]
    public async Task GetBill_CardNotFound_ShouldReturn404()
    {
        var result = await _cardsController.GetBill(Guid.NewGuid(), CancellationToken.None);
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ==========================================
    // TESTES SIMULATE-PAYMENT (Credit Card)
    // ==========================================

    [Fact]
    public async Task SimulatePayment_CreditCard_ShouldNotDebitAccount()
    {
        var card = CreateAndSaveCard();

        // Criar charge aprovado
        var createResult = await _chargesController.CreateCharge(
            new CreateCardChargeRequest(2000m, card.Id, Description: "Teste credito"),
            CancellationToken.None);

        var created = (createResult as CreatedResult)!;
        var chargeId = (Guid)created.Value!.GetType().GetProperty("chargeId")!.GetValue(created.Value)!;

        // Salvar saldo antes
        var accountBefore = await _db.BankAccounts.FindAsync(AccountId);
        var balanceBefore = accountBefore!.Balance;

        // Simular pagamento (settle)
        var settleResult = await _chargesController.SimulatePayment(
            chargeId, null, CancellationToken.None);

        settleResult.Should().BeOfType<OkObjectResult>();

        // Saldo do cliente NÃO deve ter mudado
        var accountAfter = await _db.BankAccounts.FindAsync(AccountId);
        accountAfter!.Balance.Should().Be(balanceBefore);

        // Merchant DEVE ter recebido o crédito
        var merchant = await _db.BankAccounts.FindAsync(MerchantId);
        merchant!.Balance.Should().Be(2000m);

        // SpentThisMonth DEVE ter aumentado (feito no CreateCharge)
        var updatedCard = await _db.VirtualCards.FindAsync(card.Id);
        updatedCard!.SpentThisMonth.Should().Be(2000m);
    }

    [Fact]
    public async Task SimulatePayment_AlreadySettled_ShouldFail()
    {
        var card = CreateAndSaveCard();

        var createResult = await _chargesController.CreateCharge(
            new CreateCardChargeRequest(1000m, card.Id, Description: "Teste"),
            CancellationToken.None);

        var chargeId = (Guid)(createResult as CreatedResult)!.Value!.GetType().GetProperty("chargeId")!.GetValue((createResult as CreatedResult)!.Value!)!;

        // Settle uma vez
        await _chargesController.SimulatePayment(chargeId, null, CancellationToken.None);

        // Tentar settle novamente
        var result = await _chargesController.SimulatePayment(chargeId, null, CancellationToken.None);
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
