using MiniBoa.Data.Entities;

namespace MiniBoa.Data.Repositories;

public interface ICustomerRepository
{
    Task<CustomerEntity?> GetByCustomerNoAsync(long customerNo);

    Task<CustomerEntity?> GetByPersonIdAsync(long personId);

    Task<long> InsertAsync(long personId, string passwordHash);

    /// <summary>Hatalı giriş sayacını ve (gerekiyorsa) kilit bitiş zamanını yazar.</summary>
    Task UpdateLoginFailureAsync(long customerNo, short failedLoginCount, DateTime? lockedUntil);

    /// <summary>Sayacı ve kilidi sıfırlar; önceki giriş zamanını saklayıp yenisini yazar.</summary>
    Task RegisterSuccessfulLoginAsync(long customerNo, DateTime loginAt);

    /// <summary>Yeni şifre hash'ini yazar; hatalı deneme sayacı ve kilit de sıfırlanır.</summary>
    Task UpdatePasswordAsync(long customerNo, string passwordHash);
}
