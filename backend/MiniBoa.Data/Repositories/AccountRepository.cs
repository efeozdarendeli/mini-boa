using MiniBoa.Data.Entities;

namespace MiniBoa.Data.Repositories;

public sealed class AccountRepository(IUnitOfWork unitOfWork) : RepositoryBase(unitOfWork), IAccountRepository
{
    private const string SelectColumns = """
        SELECT id, customer_no, account_suffix, branch_code, currency_code,
               account_type, iban, balance, status, opened_at
        FROM acc.account
        """;

    public Task<IReadOnlyList<AccountEntity>> GetByCustomerNoAsync(long customerNo) =>
        QueryAsync<AccountEntity>(
            $"{SelectColumns} WHERE customer_no = @CustomerNo ORDER BY account_suffix",
            new { CustomerNo = customerNo });

    public Task<AccountEntity?> GetByIdAsync(long id) =>
        QuerySingleOrDefaultAsync<AccountEntity>($"{SelectColumns} WHERE id = @Id", new { Id = id });

    public Task<AccountEntity?> GetByIbanAsync(string iban) =>
        QuerySingleOrDefaultAsync<AccountEntity>($"{SelectColumns} WHERE iban = @Iban", new { Iban = iban });

    public Task<AccountHolderEntity?> GetHolderByIbanAsync(string iban) =>
        QuerySingleOrDefaultAsync<AccountHolderEntity>(
            """
            SELECT a.id AS account_id, a.status, p.first_name, p.last_name
            FROM acc.account a
            JOIN cus.customer c ON c.customer_no = a.customer_no
            JOIN cus.person p ON p.id = c.person_id
            WHERE a.iban = @Iban
            """,
            new { Iban = iban });

    public Task<IReadOnlyList<AccountEntity>> GetForUpdateAsync(params long[] ids)
    {
        if (!UnitOfWork.HasActiveTransaction)
        {
            // Transaction yoksa kilit sorgu biter bitmez bırakılır ve hiçbir şeyi korumaz.
            throw new InvalidOperationException("GetForUpdateAsync requires an active transaction.");
        }

        // ORDER BY id: iki transfer aynı iki hesabı ters sırada kilitlemeye çalışırsa
        // birbirini sonsuza kadar bekler (deadlock). Hep aynı sırayla kilitlemek bunu önler.
        return QueryAsync<AccountEntity>(
            $"{SelectColumns} WHERE id = ANY(@Ids) ORDER BY id FOR UPDATE",
            new { Ids = ids });
    }

    public Task<short> GetNextSuffixAsync(long customerNo) =>
        ExecuteScalarAsync<short>(
            "SELECT (COALESCE(MAX(account_suffix), 0) + 1)::smallint FROM acc.account WHERE customer_no = @CustomerNo",
            new { CustomerNo = customerNo });

    public Task<long> InsertAsync(AccountEntity account) =>
        ExecuteScalarAsync<long>(
            """
            INSERT INTO acc.account
                (customer_no, account_suffix, branch_code, currency_code, account_type, iban, balance)
            VALUES
                (@CustomerNo, @AccountSuffix, @BranchCode, @CurrencyCode, @AccountType, @Iban, @Balance)
            RETURNING id
            """,
            new
            {
                account.CustomerNo,
                account.AccountSuffix,
                account.BranchCode,
                account.CurrencyCode,
                AccountType = account.AccountType.ToString(),
                account.Iban,
                account.Balance
            });

    public Task UpdateBalanceAsync(long id, decimal newBalance) =>
        ExecuteAsync(
            "UPDATE acc.account SET balance = @Balance WHERE id = @Id",
            new { Id = id, Balance = newBalance });
}
