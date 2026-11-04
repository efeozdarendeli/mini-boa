using MiniBoa.Business.Kernel;
using MiniBoa.Data.Repositories;
using MiniBoa.Framework;
using MiniBoa.Types;
using MiniBoa.Types.Contracts;

namespace MiniBoa.Orchestration;

/// <summary>"Müşterisiyim → Giriş" akışı.</summary>
public sealed class LoginHelper(
    ObjectHelper objectHelper,
    IJournalRepository journal,
    Customer customer,
    Person person) : OrchestrationBase(objectHelper, journal)
{
    [AnonymousAccess]
    public async Task<GenericResponse<LoginResultContract>> Login(LoginContract contract)
    {
        var returnObject = InitializeGenericResponse<LoginResultContract>();

        var authentication = await customer.AuthenticateAsync(contract.IdentityNumber, contract.Password);
        if (!authentication.Success)
        {
            // Günlüğe TCKN veya şifre yazılmaz; kişisel veri günlüklerde tutulmamalı.
            await AppendToJournalAsync($"Login failed: {authentication.Results[0].ErrorCode}");
            return returnObject.AddResults(authentication);
        }

        var customerEntity = authentication.Value!;
        var personResponse = await person.GetByIdAsync(customerEntity.PersonId);
        if (!personResponse.Success)
        {
            return returnObject.AddResults(personResponse);
        }

        await AppendToJournalAsync($"Login succeeded: {customerEntity.CustomerNo}");

        returnObject.Value = new LoginResultContract
        {
            CustomerNo = customerEntity.CustomerNo,
            FirstName = personResponse.Value!.FirstName,
            LastName = personResponse.Value.LastName
        };
        return returnObject;
    }
}
