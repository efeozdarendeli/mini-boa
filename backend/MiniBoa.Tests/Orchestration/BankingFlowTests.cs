using Microsoft.Extensions.DependencyInjection;
using MiniBoa.Framework.Exceptions;
using MiniBoa.Orchestration;
using MiniBoa.Tests.Data;
using MiniBoa.Types;
using MiniBoa.Types.Contracts;

namespace MiniBoa.Tests.Orchestration;

/// <summary>Giriş, ana sayfa, transfer ve parametre akışları uçtan uca.</summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Integration")]
public class BankingFlowTests(OrchestrationFixture fixture, DatabaseFixture database)
{
    private const long Hakan = 10000001;
    private const string HakanIdentityNumber = "12345678950";
    private const string HakanTryIban = "TR550099900000100000010001";
    private const string AyseTryIban = "TR060099900000100000020001";

    [Fact]
    public async Task Login_WrongPassword_Fails()
    {
        try
        {
            var response = await LoginAsync("000000");

            Assert.False(response.Success);
            Assert.Equal(ErrorCodes.Unauthorized, response.Results[0].ErrorCode);
        }
        finally
        {
            await database.ResetLoginStateAsync();
        }
    }

    [Fact]
    public async Task Login_ThreeWrongPasswords_LockTheAccountEvenForTheRightPassword()
    {
        try
        {
            await LoginAsync("000000");
            await LoginAsync("000000");
            var third = await LoginAsync("000000");
            var withCorrectPassword = await LoginAsync("123456");

            Assert.Contains("kilitlendi", third.Results[0].Message);
            Assert.False(withCorrectPassword.Success);
            Assert.Contains("kilitlendi", withCorrectPassword.Results[0].Message);
        }
        finally
        {
            await database.ResetLoginStateAsync();
        }
    }

    [Fact]
    public async Task Summary_ShowsPreviousLogin_AfterSecondLogin()
    {
        await LoginAsync("123456");
        await LoginAsync("123456");

        await using var scope = fixture.CreateRequestScope(Hakan);
        var summary = await scope.ServiceProvider.GetRequiredService<AccountHelper>().GetCustomerSummary();

        Assert.NotNull(summary.Value!.PreviousLoginAt);
        Assert.True(summary.Value.PreviousLoginAt > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task PasswordReset_WithMatchingIdentity_ChangesPassword()
    {
        var identity = new ResetPasswordContract
        {
            IdentityNumber = HakanIdentityNumber,
            GsmAreaCode = "532",
            GsmNumber = "1112233",
            BirthDate = new DateOnly(1990, 5, 14),
            NewPassword = "975310"
        };

        try
        {
            await using (var scope = fixture.CreateRequestScope())
            {
                var helper = scope.ServiceProvider.GetRequiredService<PasswordResetHelper>();

                Assert.True((await helper.VerifyIdentity(identity)).Success);
                Assert.True((await helper.ResetPassword(identity)).Success);
            }

            Assert.False((await LoginAsync("123456")).Success);
            Assert.True((await LoginAsync("975310")).Success);
        }
        finally
        {
            // Seed şifresine geri dön (pgcrypto ile, seed'deki gibi).
            await fixture.ExecuteSqlAsync(
                "UPDATE cus.customer SET password_hash = crypt('123456', gen_salt('bf')) WHERE customer_no = @CustomerNo",
                new { CustomerNo = Hakan });
            await database.ResetLoginStateAsync();
        }
    }

    [Fact]
    public async Task PasswordReset_WithWrongBirthDate_Fails()
    {
        try
        {
            await using var scope = fixture.CreateRequestScope();

            var response = await scope.ServiceProvider.GetRequiredService<PasswordResetHelper>()
                .VerifyIdentity(new PasswordResetIdentityContract
                {
                    IdentityNumber = HakanIdentityNumber,
                    GsmAreaCode = "532",
                    GsmNumber = "1112233",
                    BirthDate = new DateOnly(1991, 1, 1)
                });

            Assert.False(response.Success);
        }
        finally
        {
            await database.ResetLoginStateAsync();
        }
    }

    [Fact]
    public async Task Profile_UpdatesAddressAndEmail()
    {
        await using var scope = fixture.CreateRequestScope(Hakan);
        var helper = scope.ServiceProvider.GetRequiredService<ProfileHelper>();
        var original = (await helper.GetProfile()).Value!;

        try
        {
            var kadikoy = await fixture.QuerySingleAsync<int>(
                "SELECT id FROM prm.county WHERE city_id = 34 AND name = 'Kadıköy'");

            var updated = await helper.UpdateProfile(new UpdateProfileContract
            {
                Email = "yeni@example.com", CityId = 34, CountyId = kadikoy, AddressLine = "Yeni Adres No:2"
            });

            Assert.True(updated.Success);
            Assert.Equal("yeni@example.com", updated.Value!.Email);
            Assert.Equal(kadikoy, updated.Value.CountyId);
            Assert.Equal("123******50", updated.Value.MaskedIdentityNumber);
        }
        finally
        {
            await helper.UpdateProfile(new UpdateProfileContract
            {
                Email = original.Email, CityId = original.CityId!.Value,
                CountyId = original.CountyId!.Value, AddressLine = original.AddressLine!
            });
        }
    }

    [Fact]
    public async Task RecentTransactions_ShowNewestFirstWithMaskedCounterparty()
    {
        var sender = await fixture.QuerySingleAsync<decimal>(
            "SELECT balance FROM acc.account WHERE iban = @Iban", new { Iban = HakanTryIban });
        var receiver = await fixture.QuerySingleAsync<decimal>(
            "SELECT balance FROM acc.account WHERE iban = @Iban", new { Iban = AyseTryIban });
        Guid? referenceId = null;

        try
        {
            await using (var scope = fixture.CreateRequestScope(Hakan))
            {
                var transfer = await TransferAsync(scope, HakanTryIban, AyseTryIban, 42m);
                referenceId = transfer.Value!.ReferenceId;
            }

            await using var summaryScope = fixture.CreateRequestScope(Hakan);
            var recent = await summaryScope.ServiceProvider.GetRequiredService<AccountHelper>().GetRecentTransactions();

            Assert.InRange(recent.Value!.Count, 2, 10);
            var newest = recent.Value[0];
            Assert.Equal(referenceId, newest.ReferenceId);
            Assert.Equal(42m, newest.Amount);
            Assert.Equal(TransactionDirection.Debit, newest.Direction);
            Assert.Equal("10000001-1", newest.AccountNumber);
            Assert.Equal("Ay** Ka**", newest.Counterparty);
        }
        finally
        {
            await RestoreAsync(sender, receiver, referenceId);
        }
    }

    [Fact]
    public async Task ReceiverName_IsMaskedForLoggedInCustomer()
    {
        await using var scope = fixture.CreateRequestScope(Hakan);

        var response = await scope.ServiceProvider.GetRequiredService<TransferHelper>()
            .GetReceiverName(new ReceiverNameInputContract { Iban = AyseTryIban });

        Assert.Equal("Ay** Ka**", response.Value!.MaskedName);
    }

    private async Task<GenericResponse<LoginResultContract>> LoginAsync(string password)
    {
        await using var scope = fixture.CreateRequestScope();
        return await scope.ServiceProvider.GetRequiredService<LoginHelper>()
            .Login(new LoginContract { IdentityNumber = HakanIdentityNumber, Password = password });
    }

    [Fact]
    public async Task Summary_ShowsNameAccountsAndTryTotal()
    {
        await using var scope = fixture.CreateRequestScope(Hakan);

        var summary = await scope.ServiceProvider.GetRequiredService<AccountHelper>().GetCustomerSummary();

        Assert.True(summary.Success);
        Assert.Equal("Abdullah Hakan", summary.Value!.FirstName);
        // Beklenen değerler veritabanından okunur; demo kullanıldıkça bakiyeler değişebilir.
        var expectedTryTotal = await fixture.QuerySingleAsync<decimal>(
            "SELECT sum(balance) FROM acc.account WHERE customer_no = @CustomerNo AND currency_code = 'TRY'",
            new { CustomerNo = Hakan });
        var expectedAccountCount = await fixture.QuerySingleAsync<long>(
            "SELECT count(*) FROM acc.account WHERE customer_no = @CustomerNo", new { CustomerNo = Hakan });

        Assert.Equal(expectedAccountCount, summary.Value.Accounts.Count);
        Assert.Equal(expectedTryTotal, summary.Value.TotalTryBalance);   // USD hesabı toplama dahil değil
        Assert.Contains(summary.Value.Accounts, a => a.CurrencyCode == "USD");
    }

    [Fact]
    public async Task AccountOperations_WithoutLogin_AreRejected()
    {
        await using var scope = fixture.CreateRequestScope(customerNo: null);

        var exception = await Assert.ThrowsAsync<BoaException>(() =>
            scope.ServiceProvider.GetRequiredService<AccountHelper>().GetAccounts());

        Assert.Equal(ErrorCodes.Unauthorized, exception.ErrorCode);
    }

    [Fact]
    public async Task Transfer_Success_MovesMoneyAndWritesJournal()
    {
        var sender = await BalanceAsync(HakanTryIban);
        var receiver = await BalanceAsync(AyseTryIban);
        Guid? referenceId = null;

        try
        {
            await using (var scope = fixture.CreateRequestScope(Hakan))
            {
                var response = await TransferAsync(scope, HakanTryIban, AyseTryIban, 125.50m);

                Assert.True(response.Success);
                referenceId = response.Value!.ReferenceId;
                Assert.Equal(sender - 125.50m, response.Value.SenderBalanceAfter);
            }

            Assert.Equal(sender - 125.50m, await BalanceAsync(HakanTryIban));
            Assert.Equal(receiver + 125.50m, await BalanceAsync(AyseTryIban));

            var journalCount = await fixture.QuerySingleAsync<long>(
                "SELECT count(*) FROM log.device_journal WHERE message LIKE @Pattern",
                new { Pattern = $"%{referenceId}%" });
            Assert.Equal(1, journalCount);
        }
        finally
        {
            await RestoreAsync(sender, receiver, referenceId);
        }
    }

    [Fact]
    public async Task Transfer_InsufficientBalance_ChangesNothing()
    {
        var sender = await BalanceAsync(HakanTryIban);

        await using var scope = fixture.CreateRequestScope(Hakan);
        var response = await TransferAsync(scope, HakanTryIban, AyseTryIban, sender + 1m);

        Assert.False(response.Success);
        Assert.Equal(sender, await BalanceAsync(HakanTryIban));
    }

    [Fact]
    public async Task Transfer_FromAnotherCustomersAccount_IsRejected()
    {
        // Ayşe giriş yapmış ama Hakan'ın hesabından göndermeye çalışıyor.
        await using var scope = fixture.CreateRequestScope(10000002);
        var response = await TransferAsync(scope, HakanTryIban, AyseTryIban, 1m);

        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.NotFound, response.Results[0].ErrorCode);
    }

    [Fact]
    public async Task CountyList_ReturnsCountiesOfCity()
    {
        await using var scope = fixture.CreateRequestScope();

        var response = await scope.ServiceProvider.GetRequiredService<ParameterHelper>()
            .GetCountyList(new CountyListInputContract { CityId = 34 });

        Assert.Equal(39, response.Value!.Count);
        Assert.Contains(response.Value, c => c.Name == "Kadıköy");
    }

    private async Task<GenericResponse<TransferResultContract>> TransferAsync(
        AsyncServiceScope scope, string fromIban, string toIban, decimal amount)
    {
        var fromAccountId = await fixture.QuerySingleAsync<long>(
            "SELECT id FROM acc.account WHERE iban = @Iban", new { Iban = fromIban });

        return await scope.ServiceProvider.GetRequiredService<TransferHelper>().Transfer(new TransferContract
        {
            FromAccountId = fromAccountId,
            ToIban = toIban,
            Amount = amount,
            Description = "Test transferi"
        });
    }

    private Task<decimal> BalanceAsync(string iban) =>
        fixture.QuerySingleAsync<decimal>("SELECT balance FROM acc.account WHERE iban = @Iban", new { Iban = iban });

    /// <summary>Seed verisini test öncesi hâline döndürür.</summary>
    private Task RestoreAsync(decimal senderBalance, decimal receiverBalance, Guid? referenceId) =>
        fixture.ExecuteSqlAsync(
            """
            UPDATE acc.account SET balance = @SenderBalance WHERE iban = @SenderIban;
            UPDATE acc.account SET balance = @ReceiverBalance WHERE iban = @ReceiverIban;
            DELETE FROM txn.transaction WHERE reference_id = @ReferenceId;
            """,
            new
            {
                SenderBalance = senderBalance,
                SenderIban = HakanTryIban,
                ReceiverBalance = receiverBalance,
                ReceiverIban = AyseTryIban,
                ReferenceId = referenceId ?? Guid.Empty
            });
}
