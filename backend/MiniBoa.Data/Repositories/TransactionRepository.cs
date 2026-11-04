using MiniBoa.Data.Entities;
using MiniBoa.Types;

namespace MiniBoa.Data.Repositories;

public sealed class TransactionRepository(IUnitOfWork unitOfWork) : RepositoryBase(unitOfWork), ITransactionRepository
{
    public Task<IReadOnlyList<TransactionEntity>> GetByAccountIdAsync(
        long accountId, DateTime? since = null, TransactionDirection? direction = null, int limit = 100) =>
        // Filtre verilmediyse (NULL) o koşul devre dışı kalır; tek sorgu bütün kombinasyonları karşılar.
        QueryAsync<TransactionEntity>(
            """
            SELECT id, account_id, reference_id, direction, amount, balance_after,
                   description, channel, created_at
            FROM txn.transaction
            WHERE account_id = @AccountId
              AND (@Since::timestamptz IS NULL OR created_at >= @Since)
              AND (@Direction::varchar IS NULL OR direction = @Direction)
            ORDER BY created_at DESC, id DESC
            LIMIT @Limit
            """,
            new { AccountId = accountId, Since = since, Direction = direction?.ToString(), Limit = limit });

    public Task<IReadOnlyList<RecentTransactionEntity>> GetRecentByCustomerNoAsync(long customerNo, int limit) =>
        // Bir transfer iki satırdır (Debit + Credit, aynı reference_id). Karşı tarafı bulmak için
        // aynı reference_id'li diğer satırın hesabından müşteriye ve kişiye gidilir.
        // LEFT JOIN: karşı bacağı olmayan hareketler (örn. dışarıdan para yatırma) de listede kalır.
        QueryAsync<RecentTransactionEntity>(
            """
            SELECT t.id AS transaction_id, t.reference_id, a.customer_no, a.account_suffix, a.currency_code,
                   t.direction, t.amount, t.balance_after, t.description, t.created_at,
                   cp.first_name AS counterparty_first_name, cp.last_name AS counterparty_last_name
            FROM txn.transaction t
            JOIN acc.account a ON a.id = t.account_id
            LEFT JOIN txn.transaction other ON other.reference_id = t.reference_id AND other.id <> t.id
            LEFT JOIN acc.account other_account ON other_account.id = other.account_id
            LEFT JOIN cus.customer other_customer ON other_customer.customer_no = other_account.customer_no
            LEFT JOIN cus.person cp ON cp.id = other_customer.person_id
            WHERE a.customer_no = @CustomerNo
            ORDER BY t.created_at DESC, t.id DESC
            LIMIT @Limit
            """,
            new { CustomerNo = customerNo, Limit = limit });

    public Task<decimal> GetDebitTotalSinceAsync(long accountId, DateTime since) =>
        ExecuteScalarAsync<decimal>(
            """
            SELECT COALESCE(SUM(amount), 0)
            FROM txn.transaction
            WHERE account_id = @AccountId AND direction = 'Debit' AND created_at >= @Since
            """,
            new { AccountId = accountId, Since = since });

    public Task<long> InsertAsync(TransactionEntity transaction) =>
        ExecuteScalarAsync<long>(
            """
            INSERT INTO txn.transaction
                (account_id, reference_id, direction, amount, balance_after, description, channel)
            VALUES
                (@AccountId, @ReferenceId, @Direction, @Amount, @BalanceAfter, @Description, @Channel)
            RETURNING id
            """,
            new
            {
                transaction.AccountId,
                transaction.ReferenceId,
                Direction = transaction.Direction.ToString(),
                transaction.Amount,
                transaction.BalanceAfter,
                transaction.Description,
                transaction.Channel
            });
}
