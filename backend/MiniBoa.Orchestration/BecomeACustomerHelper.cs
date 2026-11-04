using MiniBoa.Business.Kernel;
using MiniBoa.Data;
using MiniBoa.Data.Entities;
using MiniBoa.Data.Repositories;
using MiniBoa.Framework;
using MiniBoa.Types;
using MiniBoa.Types.Contracts;
using static MiniBoa.Types.Contracts.KYCCustomerInfoContract;

namespace MiniBoa.Orchestration;

/// <summary>
/// "Müşteri Ol" akışı (BOA'daki BOA.Orchestration.SelfService.BecomeACustomerHelper karşılığı).
/// Kendi kuralı yoktur; Person, Customer, CustomerApplication ve Account nesnelerini doğru sırayla,
/// tek bir transaction içinde çağırır.
/// </summary>
public sealed class BecomeACustomerHelper(
    ObjectHelper objectHelper,
    IJournalRepository journal,
    IUnitOfWork unitOfWork,
    Person person,
    Customer customer,
    CustomerApplication application,
    Account account) : OrchestrationBase(objectHelper, journal)
{
    /// <summary>
    /// 1. adım: TCKN'ye göre kişinin durumuna karar verir.
    /// Müşteriyse girişe, yarım başvurusu varsa devama yönlendirir; yoksa yeni başvuru açar.
    /// </summary>
    [AnonymousAccess]
    public async Task<GenericResponse<KYCCustomerInfoContract>> CustomerDecisionByTaxNumber(
        BecomeACustomerInputContract contract)
    {
        var returnObject = InitializeGenericResponse<KYCCustomerInfoContract>();

        var validation = application.ValidateInput(contract.IdentityNumber, contract.GsmAreaCode, contract.GsmNumber);
        if (!validation.Success)
        {
            return returnObject.AddResults(validation);
        }

        await unitOfWork.ExecuteInTransactionAsync(() => DecideAsync(contract, returnObject));

        if (returnObject.Success)
        {
            await AppendToJournalAsync($"KYCProcessType {returnObject.Value!.KYCProcessType}");
        }

        return returnObject;
    }

    /// <summary>2. adım: kişi, adres, müşteri ve ilk TL vadesiz hesap tek transaction'da oluşturulur.</summary>
    [AnonymousAccess]
    public async Task<GenericResponse<CreateCustomerResultContract>> CreateCustomer(CreateCustomerContract contract)
    {
        var returnObject = InitializeGenericResponse<CreateCustomerResultContract>();

        await unitOfWork.ExecuteInTransactionAsync(() => CreateCustomerAsync(contract, returnObject));

        await AppendToJournalAsync(returnObject.Success
            ? $"Customer created: {returnObject.Value!.CustomerNo}"
            : $"Customer creation failed: {returnObject.Results[0].ErrorCode}");

        return returnObject;
    }

    private async Task<GenericResponse<KYCCustomerInfoContract>> DecideAsync(
        BecomeACustomerInputContract contract, GenericResponse<KYCCustomerInfoContract> returnObject)
    {
        var existingPerson = (await person.FindByIdentityNumberAsync(contract.IdentityNumber)).Value;

        if (existingPerson is not null
            && (await customer.FindByPersonIdAsync(existingPerson.Id)).Value is not null)
        {
            returnObject.Value = new KYCCustomerInfoContract { KYCProcessType = KYCProcessTypeEnum.GoCardLogin };
            return returnObject;
        }

        // Tamamlanmamış başvurusu var kontrolü
        var incomplete = (await application.FindIncompleteAsync(contract.IdentityNumber)).Value;
        if (incomplete is not null)
        {
            returnObject.Value = new KYCCustomerInfoContract
            {
                KYCProcessType = KYCProcessTypeEnum.ContinueApplication,
                ApplicationId = incomplete.Id
            };
            return returnObject;
        }

        var started = await application.StartAsync(contract.IdentityNumber, contract.GsmAreaCode, contract.GsmNumber);
        if (!started.Success)
        {
            return returnObject.AddResults(started);
        }

        // Kişi kaydı var ama müşteri değil: yeni başvuru o kişiye bağlanır, kişi bilgisi tekrar istenmez.
        if (existingPerson is not null)
        {
            await application.MarkPersonCreatedAsync(started.Value, existingPerson.Id);
        }

        returnObject.Value = new KYCCustomerInfoContract
        {
            KYCProcessType = existingPerson is null
                ? KYCProcessTypeEnum.NewCustomer
                : KYCProcessTypeEnum.ContinueApplication,
            ApplicationId = started.Value
        };
        return returnObject;
    }

    private async Task<GenericResponse<CreateCustomerResultContract>> CreateCustomerAsync(
        CreateCustomerContract contract, GenericResponse<CreateCustomerResultContract> returnObject)
    {
        var applicationResponse = await application.GetIncompleteAsync(contract.ApplicationId, contract.IdentityNumber);
        if (!applicationResponse.Success)
        {
            return returnObject.AddResults(applicationResponse);
        }

        var currentApplication = applicationResponse.Value!;

        var personId = currentApplication.PersonId;
        if (personId is null)
        {
            var created = await person.CreateAsync(new PersonEntity
            {
                IdentityNumber = currentApplication.IdentityNumber,
                FirstName = contract.FirstName.Trim(),
                LastName = contract.LastName.Trim(),
                BirthDate = contract.BirthDate,
                GsmCountry = currentApplication.GsmCountry,
                GsmAreaCode = currentApplication.GsmAreaCode,
                GsmNumber = currentApplication.GsmNumber,
                Email = contract.Email
            });
            if (!created.Success)
            {
                return returnObject.AddResults(created);
            }

            personId = created.Value;
            await application.MarkPersonCreatedAsync(currentApplication.Id, created.Value);
        }

        var address = await person.UpdateAddressAsync(
            personId.Value, contract.CityId, contract.CountyId, contract.AddressLine, contract.Email);
        if (!address.Success)
        {
            return returnObject.AddResults(address);
        }

        var customerNo = await customer.CreateAsync(personId.Value, contract.Password);
        if (!customerNo.Success)
        {
            return returnObject.AddResults(customerNo);
        }

        var firstAccount = await account.OpenAccountAsync(customerNo.Value, "TRY", AccountType.Current);
        if (!firstAccount.Success)
        {
            return returnObject.AddResults(firstAccount);
        }

        await application.CompleteAsync(currentApplication.Id);

        returnObject.Value = new CreateCustomerResultContract
        {
            CustomerNo = customerNo.Value,
            AccountNumber = firstAccount.Value!.AccountNumber,
            Iban = firstAccount.Value.Iban
        };
        return returnObject;
    }
}
