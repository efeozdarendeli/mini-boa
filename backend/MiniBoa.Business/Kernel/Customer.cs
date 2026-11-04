using MiniBoa.Business.Security;
using MiniBoa.Data.Entities;
using MiniBoa.Data.Repositories;
using MiniBoa.Framework.Messaging;
using MiniBoa.Types;

namespace MiniBoa.Business.Kernel;

/// <summary>
/// Müşteri iş nesnesi (BOA'daki Business.Kernel.Customer.Customer karşılığı):
/// müşteri kaydı, şifre kuralları, giriş kontrolü, hatalı deneme kilidi ve şifre sıfırlama.
/// </summary>
public sealed class Customer(
    ICustomerRepository customers,
    IPersonRepository persons,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider,
    IMessagingHelper messaging) : BusinessObjectBase(messaging)
{
    public const int PasswordLength = 6;
    public const int MaxFailedAttempts = 3;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);

    public async Task<GenericResponse<CustomerEntity>> GetByCustomerNoAsync(long customerNo)
    {
        var customer = await customers.GetByCustomerNoAsync(customerNo);

        return customer is null
            ? Fail<CustomerEntity>(ErrorCodes.NotFound, "CustomerNotFound")
            : GenericResponse<CustomerEntity>.Ok(customer);
    }

    /// <summary>Kişinin müşteri kaydı varsa döner; yoksa Value null'dır (bu bir hata değildir).</summary>
    public async Task<GenericResponse<CustomerEntity?>> FindByPersonIdAsync(long personId) =>
        GenericResponse<CustomerEntity?>.Ok(await customers.GetByPersonIdAsync(personId));

    /// <returns>Yeni müşteri numarası.</returns>
    public async Task<GenericResponse<long>> CreateAsync(long personId, string password)
    {
        var passwordCheck = ValidatePassword(password);
        if (!passwordCheck.Success)
        {
            return new GenericResponse<long>().AddResults(passwordCheck);
        }

        if (await customers.GetByPersonIdAsync(personId) is not null)
        {
            return Fail<long>(ErrorCodes.BusinessRule, "CustomerAlreadyExists");
        }

        var customerNo = await customers.InsertAsync(personId, passwordHasher.Hash(password));
        return GenericResponse<long>.Ok(customerNo);
    }

    /// <summary>
    /// TCKN + şifre ile giriş kontrolü. Kişi yok, müşteri değil veya şifre yanlış durumlarının hepsinde
    /// aynı mesaj döner; böylece dışarıdan "bu TCKN müşteri mi?" bilgisi sızdırılmaz.
    /// Art arda <see cref="MaxFailedAttempts"/> hatalı denemede hesap <see cref="LockoutDuration"/> kilitlenir.
    /// </summary>
    public async Task<GenericResponse<CustomerEntity>> AuthenticateAsync(string identityNumber, string password)
    {
        var person = await persons.GetByIdentityNumberAsync(identityNumber);
        var customer = person is null ? null : await customers.GetByPersonIdAsync(person.Id);
        if (customer is null)
        {
            return Fail<CustomerEntity>(ErrorCodes.Unauthorized, "InvalidCredentials");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (IsLocked(customer, now))
        {
            return LockedResponse<CustomerEntity>(customer.LockedUntil!.Value, now);
        }

        if (!passwordHasher.Verify(password, customer.PasswordHash))
        {
            return await RegisterFailureAsync<CustomerEntity>(customer, now, "InvalidCredentials");
        }

        if (customer.Status != CustomerStatus.Active)
        {
            return Fail<CustomerEntity>(ErrorCodes.BusinessRule, "CustomerPassive");
        }

        await customers.RegisterSuccessfulLoginAsync(customer.CustomerNo, now);
        return GenericResponse<CustomerEntity>.Ok(customer);
    }

    /// <summary>
    /// "Şifremi unuttum" kimlik doğrulaması: TCKN, kayıtlı cep telefonu ve doğum tarihi birlikte eşleşmeli.
    /// Hangi bilginin yanlış olduğu söylenmez; yanlış denemeler giriş kilidi sayacına eklenir.
    /// </summary>
    public async Task<GenericResponse<CustomerEntity>> VerifyIdentityForPasswordResetAsync(
        string identityNumber, string gsmAreaCode, string gsmNumber, DateOnly birthDate)
    {
        var person = await persons.GetByIdentityNumberAsync(identityNumber);
        var customer = person is null ? null : await customers.GetByPersonIdAsync(person.Id);
        if (person is null || customer is null)
        {
            return Fail<CustomerEntity>(ErrorCodes.Unauthorized, "IdentityVerificationFailed");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (IsLocked(customer, now))
        {
            return LockedResponse<CustomerEntity>(customer.LockedUntil!.Value, now);
        }

        var matches = person.GsmAreaCode == gsmAreaCode
                      && person.GsmNumber == gsmNumber
                      && person.BirthDate == birthDate;

        return matches
            ? GenericResponse<CustomerEntity>.Ok(customer)
            : await RegisterFailureAsync<CustomerEntity>(customer, now, "IdentityVerificationFailed");
    }

    /// <summary>Kimliği doğrulanmış müşterinin şifresini değiştirir; eski şifre tekrar kullanılamaz.</summary>
    public async Task<GenericResponse<bool>> ResetPasswordAsync(CustomerEntity customer, string newPassword)
    {
        var passwordCheck = ValidatePassword(newPassword);
        if (!passwordCheck.Success)
        {
            return passwordCheck;
        }

        if (passwordHasher.Verify(newPassword, customer.PasswordHash))
        {
            return Fail<bool>(ErrorCodes.Validation, "SameAsOldPassword");
        }

        await customers.UpdatePasswordAsync(customer.CustomerNo, passwordHasher.Hash(newPassword));
        return GenericResponse<bool>.Ok(true);
    }

    /// <summary>Şifre: 6 rakam; hepsi aynı (111111) veya ardışık (123456, 654321) olamaz.</summary>
    public GenericResponse<bool> ValidatePassword(string password)
    {
        if (password is not { Length: PasswordLength } || !password.All(char.IsAsciiDigit))
        {
            return Fail<bool>(ErrorCodes.Validation, "InvalidPasswordFormat");
        }

        var differences = password.Zip(password.Skip(1), (a, b) => b - a).Distinct().ToArray();
        var isRepeatedOrSequential = differences is [0] or [1] or [-1];

        return isRepeatedOrSequential
            ? Fail<bool>(ErrorCodes.Validation, "WeakPassword")
            : GenericResponse<bool>.Ok(true);
    }

    private static bool IsLocked(CustomerEntity customer, DateTime now) =>
        customer.LockedUntil is { } lockedUntil && lockedUntil > now;

    /// <summary>
    /// Hatalı denemeyi sayar. Sınıra ulaşılırsa hesap kilitlenir ve sayaç sıfırlanır;
    /// kilit bitince müşteri yeniden <see cref="MaxFailedAttempts"/> hak kazanır.
    /// </summary>
    private async Task<GenericResponse<T>> RegisterFailureAsync<T>(CustomerEntity customer, DateTime now, string messageKey)
    {
        var failedCount = customer.FailedLoginCount + 1;

        if (failedCount >= MaxFailedAttempts)
        {
            var lockedUntil = now.Add(LockoutDuration);
            await customers.UpdateLoginFailureAsync(customer.CustomerNo, 0, lockedUntil);
            return LockedResponse<T>(lockedUntil, now);
        }

        await customers.UpdateLoginFailureAsync(customer.CustomerNo, (short)failedCount, null);
        return Fail<T>(ErrorCodes.Unauthorized, messageKey);
    }

    private GenericResponse<T> LockedResponse<T>(DateTime lockedUntil, DateTime now)
    {
        var minutesLeft = (int)Math.Ceiling((lockedUntil - now).TotalMinutes);
        return Fail<T>(ErrorCodes.Unauthorized, "AccountLocked", minutesLeft);
    }
}
