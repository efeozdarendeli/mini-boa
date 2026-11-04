using Microsoft.Extensions.DependencyInjection;
using MiniBoa.Orchestration;
using MiniBoa.Tests.Data;
using MiniBoa.Types;
using MiniBoa.Types.Contracts;
using static MiniBoa.Types.Contracts.KYCCustomerInfoContract;

namespace MiniBoa.Tests.Orchestration;

/// <summary>"Müşteri Ol" akışı uçtan uca: Orchestration → Business → Data → PostgreSQL.</summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Integration")]
public class BecomeACustomerFlowTests(OrchestrationFixture fixture)
{
    private const string NewPassword = "246810";

    [Fact]
    public async Task Decision_ExistingCustomer_GoesToLogin()
    {
        var response = await DecideAsync("12345678950");

        Assert.True(response.Success);
        Assert.Equal(KYCProcessTypeEnum.GoCardLogin, response.Value!.KYCProcessType);
        Assert.Null(response.Value.ApplicationId);
    }

    [Fact]
    public async Task Decision_IncompleteApplication_Continues()
    {
        var response = await DecideAsync("34567891238");

        Assert.True(response.Success);
        Assert.Equal(KYCProcessTypeEnum.ContinueApplication, response.Value!.KYCProcessType);
        Assert.NotNull(response.Value.ApplicationId);
    }

    [Fact]
    public async Task Decision_InvalidIdentityNumber_Fails()
    {
        var response = await DecideAsync("12345678951");

        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.Validation, response.Results[0].ErrorCode);
    }

    [Fact]
    public async Task FullFlow_NewCustomer_CanLoginAndSeeFirstAccount()
    {
        var identityNumber = TestData.NewValidIdentityNumber();

        try
        {
            // 1. adım: yeni kişi -> yeni başvuru
            var decision = await DecideAsync(identityNumber);
            Assert.Equal(KYCProcessTypeEnum.NewCustomer, decision.Value!.KYCProcessType);

            // Aynı TCKN ile tekrar gelirse yarım başvurusuna yönlendirilir
            var again = await DecideAsync(identityNumber);
            Assert.Equal(KYCProcessTypeEnum.ContinueApplication, again.Value!.KYCProcessType);
            Assert.Equal(decision.Value.ApplicationId, again.Value.ApplicationId);

            // 2. adım: müşteri oluştur
            var created = await CreateCustomerAsync(decision.Value.ApplicationId!.Value, identityNumber);
            Assert.True(created.Success, created.Results.FirstOrDefault()?.Message);
            Assert.EndsWith("-1", created.Value!.AccountNumber);

            // Artık müşteri: karar ekranı girişe yönlendirir
            var afterCreate = await DecideAsync(identityNumber);
            Assert.Equal(KYCProcessTypeEnum.GoCardLogin, afterCreate.Value!.KYCProcessType);

            // Giriş
            LoginResultContract login;
            await using (var scope = fixture.CreateRequestScope())
            {
                var response = await scope.ServiceProvider.GetRequiredService<LoginHelper>()
                    .Login(new LoginContract { IdentityNumber = identityNumber, Password = NewPassword });
                Assert.True(response.Success);
                login = response.Value!;
            }

            Assert.Equal(created.Value.CustomerNo, login.CustomerNo);

            // Ana sayfa özeti
            await using (var scope = fixture.CreateRequestScope(login.CustomerNo))
            {
                var summary = await scope.ServiceProvider.GetRequiredService<AccountHelper>().GetCustomerSummary();

                Assert.Equal("Deneme", summary.Value!.FirstName);
                var account = Assert.Single(summary.Value.Accounts);
                Assert.Equal(created.Value.Iban, account.Iban);
                Assert.Equal(0m, summary.Value.TotalTryBalance);
            }
        }
        finally
        {
            await fixture.DeletePersonCascadeAsync(identityNumber);
        }
    }

    [Fact]
    public async Task CreateCustomer_WrongIdentityForApplication_Fails()
    {
        var identityNumber = TestData.NewValidIdentityNumber();

        try
        {
            var decision = await DecideAsync(identityNumber);

            // Başvuru numarası doğru ama TCKN başkasının
            var created = await CreateCustomerAsync(decision.Value!.ApplicationId!.Value, "12345678950");

            Assert.False(created.Success);
            Assert.Equal(ErrorCodes.NotFound, created.Results[0].ErrorCode);
        }
        finally
        {
            await fixture.DeletePersonCascadeAsync(identityNumber);
        }
    }

    [Fact]
    public async Task CreateCustomer_FailureInLastSteps_RollsBackEverything()
    {
        var identityNumber = TestData.NewValidIdentityNumber();

        try
        {
            var decision = await DecideAsync(identityNumber);

            // Kişi oluşturulur ama şifre zayıf olduğu için müşteri oluşturma adımı başarısız olur.
            var created = await CreateCustomerAsync(decision.Value!.ApplicationId!.Value, identityNumber, password: "123456");

            Assert.False(created.Success);

            // Transaction geri alındığı için kişi kaydı da oluşmamış olmalı.
            var personCount = await fixture.QuerySingleAsync<long>(
                "SELECT count(*) FROM cus.person WHERE identity_number = @IdentityNumber",
                new { IdentityNumber = identityNumber });
            Assert.Equal(0, personCount);
        }
        finally
        {
            await fixture.DeletePersonCascadeAsync(identityNumber);
        }
    }

    private async Task<GenericResponse<KYCCustomerInfoContract>> DecideAsync(string identityNumber)
    {
        await using var scope = fixture.CreateRequestScope();
        return await scope.ServiceProvider.GetRequiredService<BecomeACustomerHelper>()
            .CustomerDecisionByTaxNumber(new BecomeACustomerInputContract
            {
                IdentityNumber = identityNumber,
                GsmAreaCode = "532",
                GsmNumber = "9998877"
            });
    }

    private async Task<GenericResponse<CreateCustomerResultContract>> CreateCustomerAsync(
        long applicationId, string identityNumber, string password = NewPassword)
    {
        await using var scope = fixture.CreateRequestScope();
        return await scope.ServiceProvider.GetRequiredService<BecomeACustomerHelper>()
            .CreateCustomer(new CreateCustomerContract
            {
                ApplicationId = applicationId,
                IdentityNumber = identityNumber,
                FirstName = "Deneme",
                LastName = "Müşteri",
                BirthDate = new DateOnly(1995, 6, 15),
                CityId = 34,
                CountyId = await fixture.QuerySingleAsync<int>(
                    "SELECT id FROM prm.county WHERE city_id = 34 AND name = 'Kadıköy'"),
                AddressLine = "Test Mah. Deneme Sok. No:1",
                Password = password
            });
    }
}
