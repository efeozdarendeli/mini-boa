using System.Text.RegularExpressions;
using MiniBoa.Data.Entities;
using MiniBoa.Data.Repositories;
using MiniBoa.Framework.Messaging;
using MiniBoa.Types;

namespace MiniBoa.Business.Kernel;

/// <summary>
/// Kişi iş nesnesi (BOA'daki Business.Kernel.Customer.Person karşılığı).
/// Kişi kaydına ait tüm kurallar burada: TCKN, ad, yaş, GSM, adres, e-posta.
/// </summary>
public sealed partial class Person(
    IPersonRepository persons,
    IParameterRepository parameters,
    IMernis mernis,
    TimeProvider timeProvider,
    IMessagingHelper messaging) : BusinessObjectBase(messaging)
{
    public const int MinimumAge = 18;

    public async Task<GenericResponse<PersonEntity>> GetByIdentityNumberAsync(string identityNumber)
    {
        var person = await persons.GetByIdentityNumberAsync(identityNumber);

        return person is null
            ? Fail<PersonEntity>(ErrorCodes.NotFound, "PersonNotFound")
            : GenericResponse<PersonEntity>.Ok(person);
    }

    /// <summary>Kişi kaydı varsa döner; yoksa Value null'dır (bu bir hata değildir).</summary>
    public async Task<GenericResponse<PersonEntity?>> FindByIdentityNumberAsync(string identityNumber) =>
        GenericResponse<PersonEntity?>.Ok(await persons.GetByIdentityNumberAsync(identityNumber));

    public async Task<GenericResponse<PersonEntity>> GetByIdAsync(long id)
    {
        var person = await persons.GetByIdAsync(id);

        return person is null
            ? Fail<PersonEntity>(ErrorCodes.NotFound, "PersonNotFound")
            : GenericResponse<PersonEntity>.Ok(person);
    }

    /// <returns>Yeni kişinin id'si.</returns>
    public async Task<GenericResponse<long>> CreateAsync(PersonEntity person)
    {
        var validation = Validate(person);
        if (!validation.Success)
        {
            return validation;
        }

        if (await persons.GetByIdentityNumberAsync(person.IdentityNumber) is not null)
        {
            return Fail<long>(ErrorCodes.BusinessRule, "PersonAlreadyExists");
        }

        var verified = await mernis.VerifyIdentityAsync(
            person.IdentityNumber, person.FirstName, person.LastName, person.BirthDate);
        if (!verified)
        {
            return Fail<long>(ErrorCodes.Validation, "IdentityNotVerified");
        }

        return GenericResponse<long>.Ok(await persons.InsertAsync(person));
    }

    public async Task<GenericResponse<bool>> UpdateAddressAsync(
        long personId, short cityId, int countyId, string addressLine, string? email)
    {
        if (string.IsNullOrWhiteSpace(addressLine))
        {
            return Fail<bool>(ErrorCodes.Validation, "InvalidAddress");
        }

        if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
        {
            return Fail<bool>(ErrorCodes.Validation, "InvalidEmail");
        }

        var counties = await parameters.GetCountiesAsync(cityId);
        if (counties.All(c => c.Id != countyId))
        {
            return Fail<bool>(ErrorCodes.Validation, "CountyNotInCity");
        }

        await persons.UpdateAddressAsync(
            personId, cityId, countyId, addressLine.Trim(), string.IsNullOrWhiteSpace(email) ? null : email.Trim());
        return GenericResponse<bool>.Ok(true);
    }

    public bool IsAdult(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        return birthDate.AddYears(MinimumAge) <= today;
    }

    private GenericResponse<long> Validate(PersonEntity person)
    {
        if (!mernis.IsValidIdentityNumber(person.IdentityNumber))
        {
            return Fail<long>(ErrorCodes.Validation, "InvalidIdentityNumber");
        }

        if (!IsValidName(person.FirstName) || !IsValidName(person.LastName))
        {
            return Fail<long>(ErrorCodes.Validation, "InvalidName");
        }

        if (!IsValidGsm(person.GsmAreaCode, person.GsmNumber))
        {
            return Fail<long>(ErrorCodes.Validation, "InvalidGsm");
        }

        if (!IsAdult(person.BirthDate))
        {
            return Fail<long>(ErrorCodes.BusinessRule, "UnderAge");
        }

        return new GenericResponse<long>();
    }

    /// <summary>Ad/soyad: sadece harfler (Türkçe harfler dahil), kelimeler arasında tek boşluk, en fazla 50 karakter.</summary>
    public static bool IsValidName(string? name) =>
        name is { Length: > 0 and <= 50 } && NamePattern().IsMatch(name);

    public static bool IsValidEmail(string email) => EmailPattern().IsMatch(email.Trim());

    [GeneratedRegex(@"^\p{L}+( \p{L}+)*$")]
    private static partial Regex NamePattern();

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex EmailPattern();

    /// <summary>Türkiye cep telefonu: alan kodu 5 ile başlayan 3 hane, numara 7 hane.</summary>
    public static bool IsValidGsm(string areaCode, string number) =>
        areaCode is { Length: 3 } && areaCode[0] == '5' && areaCode.All(char.IsAsciiDigit)
        && number is { Length: 7 } && number.All(char.IsAsciiDigit);
}
