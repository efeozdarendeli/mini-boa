using MiniBoa.Data.Entities;

namespace MiniBoa.Data.Repositories;

public interface IAccountRepository
{
    Task<IReadOnlyList<AccountEntity>> GetByCustomerNoAsync(long customerNo);

    Task<AccountEntity?> GetByIdAsync(long id);

    Task<AccountEntity?> GetByIbanAsync(string iban);

    Task<AccountHolderEntity?> GetHolderByIbanAsync(string iban);

    /// <summary>
    /// Hesapları satır kilidiyle (SELECT ... FOR UPDATE) okur. Transaction bitene kadar
    /// başka bir işlem bu hesapların bakiyesini değiştiremez. Açık bir transaction gerektirir.
    /// </summary>
    Task<IReadOnlyList<AccountEntity>> GetForUpdateAsync(params long[] ids);

    Task<short> GetNextSuffixAsync(long customerNo);

    Task<long> InsertAsync(AccountEntity account);

    Task UpdateBalanceAsync(long id, decimal newBalance);
}
