using MiniBoa.Business.Kernel;
using MiniBoa.Data.Entities;
using MiniBoa.Data.Repositories;
using MiniBoa.Framework;
using MiniBoa.Types;
using NSubstitute;

namespace MiniBoa.Tests.Business;

/// <summary>Transfer kuralları; veritabanı yerine sahte repository'lerle.</summary>
public class AccountTransferTests
{
    private const long Owner = 10000001;
    private const long Other = 10000002;

    private readonly IAccountRepository _accounts = Substitute.For<IAccountRepository>();
    private readonly ITransactionRepository _transactions = Substitute.For<ITransactionRepository>();
    private readonly Account _account;

    private readonly AccountEntity _sender = NewAccount(id: 1, Owner, suffix: 1, balance: 1000m);
    private readonly AccountEntity _receiver = NewAccount(id: 2, Other, suffix: 1, balance: 50m);

    private static readonly DateTimeOffset Now = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);

    public AccountTransferTests()
    {
        _account = new Account(
            _accounts, _transactions, new RequestContext(), new FixedTimeProvider(Now), TestHelpers.Messaging());
        GivenAccounts(_sender, _receiver);
    }

    [Fact]
    public async Task Transfer_OverDailyLimit_FailsWithRemainingLimit()
    {
        var richSender = NewAccount(id: 1, Owner, suffix: 1, balance: 100_000m);
        GivenAccounts(richSender, _receiver);
        _transactions.GetDebitTotalSinceAsync(1, Arg.Any<DateTime>()).Returns(45_000m);

        var response = await _account.TransferAsync(Owner, richSender.Id, _receiver.Iban, 6_000m, null);

        AssertFailedWithoutWrites(response, ErrorCodes.BusinessRule);
        Assert.Contains("5.000,00 TRY", response.Results[0].Message);
    }

    [Fact]
    public async Task Transfer_ExactlyAtDailyLimit_Passes()
    {
        var richSender = NewAccount(id: 1, Owner, suffix: 1, balance: 100_000m);
        GivenAccounts(richSender, _receiver);
        _transactions.GetDebitTotalSinceAsync(1, Arg.Any<DateTime>()).Returns(45_000m);

        var response = await _account.TransferAsync(Owner, richSender.Id, _receiver.Iban, 5_000m, null);

        Assert.True(response.Success);
    }

    [Fact]
    public async Task Transfer_DailyLimit_CountsFromLocalMidnight()
    {
        await _account.TransferAsync(Owner, _sender.Id, _receiver.Iban, 10m, null);

        // FixedTimeProvider yerel saat dilimi olarak UTC kullanıyor: gün başı 1 Kasım 00:00.
        await _transactions.Received(1).GetDebitTotalSinceAsync(1, new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Theory]
    [InlineData("Ayşe Kaya", "Ay** Ka**")]
    [InlineData("Abdullah Hakan Demir", "Ab****** Ha*** De***")]
    [InlineData("Ali Su", "Al* S*")]
    public void MaskName_KeepsFirstTwoLetters(string fullName, string expected) =>
        Assert.Equal(expected, Account.MaskName(fullName));

    [Fact]
    public async Task ReceiverName_IsMasked()
    {
        _accounts.GetHolderByIbanAsync(_receiver.Iban).Returns(new AccountHolderEntity
        {
            AccountId = 2, Status = AccountStatus.Active, FirstName = "Ayşe", LastName = "Kaya"
        });

        var response = await _account.GetReceiverNameAsync(_receiver.Iban);

        Assert.Equal("Ay** Ka**", response.Value);
    }

    [Fact]
    public async Task ReceiverName_UnknownIban_Fails()
    {
        var response = await _account.GetReceiverNameAsync(Iban.Generate(99999999, 1));

        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.NotFound, response.Results[0].ErrorCode);
    }

    [Fact]
    public async Task Transactions_InvalidPeriod_Fails()
    {
        var response = await _account.GetTransactionsAsync(Owner, _sender.Id, days: 365, direction: null);

        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.Validation, response.Results[0].ErrorCode);
    }

    [Fact]
    public async Task Transactions_PassesPeriodAndDirectionToRepository()
    {
        _accounts.GetByIdAsync(_sender.Id).Returns(_sender);

        await _account.GetTransactionsAsync(Owner, _sender.Id, days: 7, TransactionDirection.Credit);

        await _transactions.Received(1).GetByAccountIdAsync(
            _sender.Id, Now.UtcDateTime.AddDays(-7), TransactionDirection.Credit, Arg.Any<int>());
    }

    [Fact]
    public async Task Transfer_Valid_UpdatesBothBalancesAndWritesTwoTransactions()
    {
        var response = await _account.TransferAsync(Owner, _sender.Id, _receiver.Iban, 250m, "Kira");

        Assert.True(response.Success);
        Assert.Equal(750m, response.Value!.SenderBalanceAfter);

        await _accounts.Received(1).UpdateBalanceAsync(1, 750m);
        await _accounts.Received(1).UpdateBalanceAsync(2, 300m);
        await _transactions.Received(1).InsertAsync(Arg.Is<TransactionEntity>(t =>
            t.AccountId == 1 && t.Direction == TransactionDirection.Debit && t.ReferenceId == response.Value.ReferenceId));
        await _transactions.Received(1).InsertAsync(Arg.Is<TransactionEntity>(t =>
            t.AccountId == 2 && t.Direction == TransactionDirection.Credit && t.ReferenceId == response.Value.ReferenceId));
    }

    [Fact]
    public async Task Transfer_InsufficientBalance_FailsWithoutWriting()
    {
        var response = await _account.TransferAsync(Owner, _sender.Id, _receiver.Iban, 1000.01m, null);

        AssertFailedWithoutWrites(response, ErrorCodes.BusinessRule);
    }

    [Fact]
    public async Task Transfer_FromSomeoneElsesAccount_FailsAsNotFound()
    {
        var response = await _account.TransferAsync(Other, _sender.Id, _receiver.Iban, 10m, null);

        AssertFailedWithoutWrites(response, ErrorCodes.NotFound);
    }

    [Fact]
    public async Task Transfer_ToSameAccount_Fails()
    {
        var response = await _account.TransferAsync(Owner, _sender.Id, _sender.Iban, 10m, null);

        AssertFailedWithoutWrites(response, ErrorCodes.BusinessRule);
    }

    [Fact]
    public async Task Transfer_DifferentCurrency_Fails()
    {
        var usdReceiver = NewAccount(id: 3, Other, suffix: 2, balance: 0m, currency: "USD");
        GivenAccounts(_sender, usdReceiver);

        var response = await _account.TransferAsync(Owner, _sender.Id, usdReceiver.Iban, 10m, null);

        AssertFailedWithoutWrites(response, ErrorCodes.BusinessRule);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(10.555)]
    public async Task Transfer_InvalidAmount_Fails(decimal amount)
    {
        var response = await _account.TransferAsync(Owner, _sender.Id, _receiver.Iban, amount, null);

        AssertFailedWithoutWrites(response, ErrorCodes.Validation);
    }

    [Fact]
    public async Task Transfer_InvalidIban_Fails()
    {
        var response = await _account.TransferAsync(Owner, _sender.Id, "TR000000000000000000000000", 10m, null);

        AssertFailedWithoutWrites(response, ErrorCodes.Validation);
    }

    [Fact]
    public async Task OpenAccount_LimitReached_Fails()
    {
        var existing = Enumerable.Range(1, Account.MaxAccountsPerCustomer)
            .Select(i => NewAccount(i, Owner, (short)i, 0m))
            .ToList();
        _accounts.GetByCustomerNoAsync(Owner).Returns(existing);

        var response = await _account.OpenAccountAsync(Owner, "TRY", AccountType.Current);

        Assert.False(response.Success);
        await _accounts.DidNotReceive().InsertAsync(Arg.Any<AccountEntity>());
    }

    [Fact]
    public async Task OpenAccount_GeneratesIbanFromNextSuffix()
    {
        _accounts.GetByCustomerNoAsync(Owner).Returns([_sender]);
        _accounts.GetNextSuffixAsync(Owner).Returns((short)2);
        _accounts.InsertAsync(Arg.Any<AccountEntity>()).Returns(77L);
        _accounts.GetByIdAsync(77).Returns(new AccountEntity { Id = 77 });

        var response = await _account.OpenAccountAsync(Owner, "EUR", AccountType.Current);

        Assert.True(response.Success);
        await _accounts.Received(1).InsertAsync(Arg.Is<AccountEntity>(a =>
            a.AccountSuffix == 2 && a.Iban == Iban.Generate(Owner, 2) && a.Balance == 0m && a.CurrencyCode == "EUR"));
    }

    private void GivenAccounts(AccountEntity sender, AccountEntity receiver)
    {
        _accounts.GetByIbanAsync(receiver.Iban).Returns(receiver);
        _accounts.GetByIbanAsync(sender.Iban).Returns(sender);
        _accounts.GetForUpdateAsync(Arg.Any<long[]>())
            .Returns(call => new[] { sender, receiver }.Where(a => call.Arg<long[]>().Contains(a.Id)).ToList());
    }

    private void AssertFailedWithoutWrites<T>(GenericResponse<T> response, string errorCode)
    {
        Assert.False(response.Success);
        Assert.Equal(errorCode, response.Results[0].ErrorCode);
        _accounts.DidNotReceiveWithAnyArgs().UpdateBalanceAsync(default, default);
        _transactions.DidNotReceiveWithAnyArgs().InsertAsync(default!);
    }

    private static AccountEntity NewAccount(long id, long customerNo, short suffix, decimal balance, string currency = "TRY") =>
        new()
        {
            Id = id,
            CustomerNo = customerNo,
            AccountSuffix = suffix,
            CurrencyCode = currency,
            AccountType = AccountType.Current,
            Iban = Iban.Generate(customerNo, suffix),
            Balance = balance,
            Status = AccountStatus.Active
        };
}
