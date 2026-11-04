using MiniBoa.Business.Kernel;
using MiniBoa.Data;
using MiniBoa.Data.Entities;
using MiniBoa.Data.Repositories;
using MiniBoa.Framework;
using MiniBoa.Types;
using MiniBoa.Types.Contracts;

namespace MiniBoa.Orchestration;

/// <summary>"Profilim" ekranı: bilgileri gösterme ve iletişim/adres bilgisi güncelleme.</summary>
public sealed class ProfileHelper(
    ObjectHelper objectHelper,
    IJournalRepository journal,
    IUnitOfWork unitOfWork,
    Customer customer,
    Person person) : OrchestrationBase(objectHelper, journal)
{
    public async Task<GenericResponse<ProfileContract>> GetProfile()
    {
        var returnObject = InitializeGenericResponse<ProfileContract>();

        var personResponse = await GetCurrentPersonAsync();
        if (!personResponse.Success)
        {
            return returnObject.AddResults(personResponse);
        }

        returnObject.Value = ToContract(personResponse.Value!);
        return returnObject;
    }

    public async Task<GenericResponse<ProfileContract>> UpdateProfile(UpdateProfileContract contract)
    {
        var returnObject = InitializeGenericResponse<ProfileContract>();

        var personResponse = await GetCurrentPersonAsync();
        if (!personResponse.Success)
        {
            return returnObject.AddResults(personResponse);
        }

        var updated = await unitOfWork.ExecuteInTransactionAsync(() => person.UpdateAddressAsync(
            personResponse.Value!.Id, contract.CityId, contract.CountyId, contract.AddressLine, contract.Email));
        if (!updated.Success)
        {
            return returnObject.AddResults(updated);
        }

        await AppendToJournalAsync("Profile updated");

        // Kaydedilen hâli veritabanından tekrar okunur; ekran tam olarak saklanan bilgiyi gösterir.
        var refreshed = await person.GetByIdAsync(personResponse.Value!.Id);
        returnObject.Value = ToContract(refreshed.Value!);
        return returnObject;
    }

    private async Task<GenericResponse<PersonEntity>> GetCurrentPersonAsync()
    {
        var customerResponse = await customer.GetByCustomerNoAsync(CurrentCustomerNo);
        if (!customerResponse.Success)
        {
            return new GenericResponse<PersonEntity>().AddResults(customerResponse);
        }

        return await person.GetByIdAsync(customerResponse.Value!.PersonId);
    }

    private ProfileContract ToContract(PersonEntity entity) => new()
    {
        CustomerNo = CurrentCustomerNo,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        MaskedIdentityNumber = MaskIdentityNumber(entity.IdentityNumber),
        GsmNumber = $"0{entity.GsmAreaCode}{entity.GsmNumber}",
        Email = entity.Email,
        CityId = entity.CityId,
        CountyId = entity.CountyId,
        AddressLine = entity.AddressLine
    };

    /// <summary>"12345678950" → "123******50"</summary>
    private static string MaskIdentityNumber(string identityNumber) =>
        identityNumber[..3] + new string('*', identityNumber.Length - 5) + identityNumber[^2..];
}
