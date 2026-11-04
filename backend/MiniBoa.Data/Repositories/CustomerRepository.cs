using MiniBoa.Data.Entities;

namespace MiniBoa.Data.Repositories;

public sealed class CustomerRepository(IUnitOfWork unitOfWork) : RepositoryBase(unitOfWork), ICustomerRepository
{
    private const string SelectColumns = """
        SELECT customer_no, person_id, status, password_hash,
               failed_login_count, locked_until, last_login_at, previous_login_at, created_at
        FROM cus.customer
        """;

    public Task<CustomerEntity?> GetByCustomerNoAsync(long customerNo) =>
        QuerySingleOrDefaultAsync<CustomerEntity>(
            $"{SelectColumns} WHERE customer_no = @CustomerNo",
            new { CustomerNo = customerNo });

    public Task<CustomerEntity?> GetByPersonIdAsync(long personId) =>
        QuerySingleOrDefaultAsync<CustomerEntity>(
            $"{SelectColumns} WHERE person_id = @PersonId",
            new { PersonId = personId });

    /// <returns>Veritabanının verdiği yeni müşteri numarası.</returns>
    public Task<long> InsertAsync(long personId, string passwordHash) =>
        ExecuteScalarAsync<long>(
            """
            INSERT INTO cus.customer (person_id, password_hash)
            VALUES (@PersonId, @PasswordHash)
            RETURNING customer_no
            """,
            new { PersonId = personId, PasswordHash = passwordHash });

    public Task UpdateLoginFailureAsync(long customerNo, short failedLoginCount, DateTime? lockedUntil) =>
        ExecuteAsync(
            """
            UPDATE cus.customer
            SET failed_login_count = @FailedLoginCount, locked_until = @LockedUntil
            WHERE customer_no = @CustomerNo
            """,
            new { CustomerNo = customerNo, FailedLoginCount = failedLoginCount, LockedUntil = lockedUntil });

    public Task RegisterSuccessfulLoginAsync(long customerNo, DateTime loginAt) =>
        ExecuteAsync(
            """
            UPDATE cus.customer
            SET failed_login_count = 0,
                locked_until = NULL,
                previous_login_at = last_login_at,
                last_login_at = @LoginAt
            WHERE customer_no = @CustomerNo
            """,
            new { CustomerNo = customerNo, LoginAt = loginAt });

    public Task UpdatePasswordAsync(long customerNo, string passwordHash) =>
        ExecuteAsync(
            """
            UPDATE cus.customer
            SET password_hash = @PasswordHash, failed_login_count = 0, locked_until = NULL
            WHERE customer_no = @CustomerNo
            """,
            new { CustomerNo = customerNo, PasswordHash = passwordHash });
}
