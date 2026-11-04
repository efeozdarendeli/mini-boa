using MiniBoa.Data.Entities;
using MiniBoa.Types;

namespace MiniBoa.Data.Repositories;

public interface ITransactionRepository
{
    /// <summary>Hesabın hareketleri, en yeniden eskiye. Tarih ve yön filtresi isteğe bağlıdır.</summary>
    Task<IReadOnlyList<TransactionEntity>> GetByAccountIdAsync(
        long accountId, DateTime? since = null, TransactionDirection? direction = null, int limit = 100);

    /// <summary>Müşterinin bütün hesaplarındaki en son hareketler, karşı tarafın adıyla birlikte.</summary>
    Task<IReadOnlyList<RecentTransactionEntity>> GetRecentByCustomerNoAsync(long customerNo, int limit);

    /// <summary>Verilen andan bu yana hesaptan çıkan (Debit) tutarların toplamı; günlük limit kontrolü için.</summary>
    Task<decimal> GetDebitTotalSinceAsync(long accountId, DateTime since);

    Task<long> InsertAsync(TransactionEntity transaction);
}
