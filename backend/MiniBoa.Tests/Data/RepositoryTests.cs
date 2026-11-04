using MiniBoa.Data.Repositories;
using MiniBoa.Types;

namespace MiniBoa.Tests.Data;

/// <summary>Repository'lerin seed verisini doğru okuyup C# tiplerine çevirdiğini doğrular.</summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Integration")]
public class RepositoryTests(DatabaseFixture fixture)
{
    private const long HakanCustomerNo = 10000001;

    [Fact]
    public async Task Person_IsReadByIdentityNumber()
    {
        await using var unitOfWork = fixture.CreateUnitOfWork();

        var person = await new PersonRepository(unitOfWork).GetByIdentityNumberAsync("12345678950");

        Assert.NotNull(person);
        Assert.Equal("Abdullah Hakan", person.FirstName);
        Assert.Equal(new DateOnly(1990, 5, 14), person.BirthDate);
    }

    [Fact]
    public async Task Customer_IsReadWithStatusAsEnum()
    {
        await using var unitOfWork = fixture.CreateUnitOfWork();

        var customer = await new CustomerRepository(unitOfWork).GetByCustomerNoAsync(HakanCustomerNo);

        Assert.NotNull(customer);
        Assert.Equal(CustomerStatus.Active, customer.Status);
        Assert.StartsWith("$2", customer.PasswordHash);   // bcrypt formatı
    }

    [Fact]
    public async Task Accounts_AreReadForCustomer()
    {
        await using var unitOfWork = fixture.CreateUnitOfWork();

        var accounts = await new AccountRepository(unitOfWork).GetByCustomerNoAsync(HakanCustomerNo);

        Assert.Equal(3, accounts.Count);
        Assert.Equal("10000001-1", accounts[0].AccountNumber);
        Assert.Equal(AccountType.Participation, accounts[1].AccountType);
        Assert.Equal("USD", accounts[2].CurrencyCode);
    }

    [Fact]
    public async Task NextSuffix_IsOneMoreThanLast()
    {
        await using var unitOfWork = fixture.CreateUnitOfWork();

        var suffix = await new AccountRepository(unitOfWork).GetNextSuffixAsync(HakanCustomerNo);

        Assert.Equal(4, suffix);
    }

    [Fact]
    public async Task IncompleteApplication_IsFound()
    {
        await using var unitOfWork = fixture.CreateUnitOfWork();

        var application = await new ApplicationRepository(unitOfWork)
            .GetIncompleteByIdentityNumberAsync("34567891238");

        Assert.NotNull(application);
        Assert.Equal(ApplicationStatus.PersonCreated, application.Status);
    }

    [Fact]
    public async Task Transactions_AreReadWithDirection()
    {
        await using var unitOfWork = fixture.CreateUnitOfWork();
        var account = await new AccountRepository(unitOfWork).GetByIbanAsync("TR550099900000100000010001");

        var transactions = await new TransactionRepository(unitOfWork).GetByAccountIdAsync(account!.Id);

        var transfer = Assert.Single(transactions);
        Assert.Equal(TransactionDirection.Debit, transfer.Direction);
        Assert.Equal(250m, transfer.Amount);
    }

    [Fact]
    public async Task Transactions_CanBeFilteredByDirectionAndDate()
    {
        await using var unitOfWork = fixture.CreateUnitOfWork();
        var account = await new AccountRepository(unitOfWork).GetByIbanAsync("TR550099900000100000010001");
        var repository = new TransactionRepository(unitOfWork);

        var credits = await repository.GetByAccountIdAsync(account!.Id, direction: TransactionDirection.Credit);
        var debitsSinceTomorrow = await repository.GetByAccountIdAsync(
            account.Id, since: DateTime.UtcNow.AddDays(1), direction: TransactionDirection.Debit);
        var debitTotal = await repository.GetDebitTotalSinceAsync(account.Id, DateTime.UtcNow.AddDays(-1));

        Assert.Empty(credits);
        Assert.Empty(debitsSinceTomorrow);
        Assert.Equal(250m, debitTotal);
    }

    [Fact]
    public async Task RecentTransactions_IncludeAccountAndCounterparty()
    {
        await using var unitOfWork = fixture.CreateUnitOfWork();
        var repository = new TransactionRepository(unitOfWork);

        var hakan = await repository.GetRecentByCustomerNoAsync(HakanCustomerNo, 100);
        var ayse = await repository.GetRecentByCustomerNoAsync(10000002, 100);

        // Seed'deki Hakan → Ayşe transferi (demo kullanıldıkça başka hareketler de eklenebilir).
        var seedTransfer = Guid.Parse("7f3c2a9e-1b4d-4c8a-9e21-5d6f7a8b9c01");

        var sent = Assert.Single(hakan, t => t.ReferenceId == seedTransfer);
        Assert.Equal(TransactionDirection.Debit, sent.Direction);
        Assert.Equal("10000001-1", sent.AccountNumber);
        Assert.Equal("Ayşe", sent.CounterpartyFirstName);

        var received = Assert.Single(ayse, t => t.ReferenceId == seedTransfer);
        Assert.Equal(TransactionDirection.Credit, received.Direction);
        Assert.Equal("Abdullah Hakan", received.CounterpartyFirstName);

        // En yeni hareket en üstte.
        Assert.Equal(hakan.OrderByDescending(t => t.CreatedAt).ThenByDescending(t => t.TransactionId), hakan);
    }

    [Fact]
    public async Task AccountHolder_IsReadByIban()
    {
        await using var unitOfWork = fixture.CreateUnitOfWork();

        var holder = await new AccountRepository(unitOfWork).GetHolderByIbanAsync("TR060099900000100000020001");

        Assert.Equal("Ayşe", holder!.FirstName);
        Assert.Equal("Kaya", holder.LastName);
    }

    [Fact]
    public async Task Parameters_AreRead()
    {
        await using var unitOfWork = fixture.CreateUnitOfWork();
        var repository = new ParameterRepository(unitOfWork);

        var cities = await repository.GetCitiesAsync();
        var counties = await repository.GetCountiesAsync(34);
        var rates = await repository.GetLatestRatesAsync();

        Assert.Equal(81, cities.Count);
        Assert.Equal(39, counties.Count);
        Assert.Equal(4, rates.Count);

        // Türkçe sıralama: İstanbul ve İzmir, Konya'dan önce gelir.
        var names = cities.Select(c => c.Name).ToList();
        Assert.True(names.IndexOf("İzmir") < names.IndexOf("Konya"));
        Assert.Equal("Adalar", counties[0].Name);
        Assert.Contains(rates, r => r.CurrencyCode == "USD" && r.CurrencyName == "Amerikan Doları");
    }
}
