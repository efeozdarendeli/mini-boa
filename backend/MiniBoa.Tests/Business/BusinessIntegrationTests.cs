using MiniBoa.Business.Kernel;
using MiniBoa.Business.Security;
using MiniBoa.Data;
using MiniBoa.Data.Repositories;
using MiniBoa.Framework;
using MiniBoa.Tests.Data;

namespace MiniBoa.Tests.Business;

/// <summary>İş nesnelerinin gerçek veritabanıyla birlikte çalıştığını doğrular. Yapılan her değişiklik geri alınır.</summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Integration")]
public class BusinessIntegrationTests(DatabaseFixture fixture)
{
    private const long Hakan = 10000001;
    private const string HakanTryIban = "TR550099900000100000010001";
    private const string AyseTryIban = "TR060099900000100000020001";

    [Fact]
    public async Task Authenticate_SeedCustomer_WithPgcryptoHash()
    {
        await using var unitOfWork = fixture.CreateUnitOfWork();
        var customer = new Customer(
            new CustomerRepository(unitOfWork), new PersonRepository(unitOfWork),
            new BcryptPasswordHasher(), TimeProvider.System, TestHelpers.Messaging());

        var response = await customer.AuthenticateAsync("12345678950", "123456");

        Assert.True(response.Success);
        Assert.Equal(Hakan, response.Value!.CustomerNo);
    }

    [Fact]
    public async Task Transfer_InTransaction_MovesMoneyThenRollsBack()
    {
        await using var unitOfWork = fixture.CreateUnitOfWork();
        var accounts = new AccountRepository(unitOfWork);
        var account = new Account(
            accounts, new TransactionRepository(unitOfWork), new RequestContext(), TimeProvider.System, TestHelpers.Messaging());

        var sender = (await accounts.GetByIbanAsync(HakanTryIban))!;
        var receiver = (await accounts.GetByIbanAsync(AyseTryIban))!;

        await unitOfWork.BeginAsync();
        var response = await account.TransferAsync(Hakan, sender.Id, AyseTryIban, 100m, "Test");

        Assert.True(response.Success);
        Assert.Equal(sender.Balance - 100m, (await accounts.GetByIdAsync(sender.Id))!.Balance);
        Assert.Equal(receiver.Balance + 100m, (await accounts.GetByIdAsync(receiver.Id))!.Balance);

        await unitOfWork.RollbackAsync();

        Assert.Equal(sender.Balance, (await accounts.GetByIdAsync(sender.Id))!.Balance);
    }
}
