using MiniBoa.Data.Entities;
using MiniBoa.Types;

namespace MiniBoa.Data.Repositories;

public sealed class ApplicationRepository(IUnitOfWork unitOfWork) : RepositoryBase(unitOfWork), IApplicationRepository
{
    private const string SelectColumns = """
        SELECT id, identity_number, gsm_country, gsm_area_code, gsm_number,
               status, person_id, created_at, updated_at
        FROM cus.application
        """;

    public Task<ApplicationEntity?> GetByIdAsync(long id) =>
        QuerySingleOrDefaultAsync<ApplicationEntity>($"{SelectColumns} WHERE id = @Id", new { Id = id });

    public Task<ApplicationEntity?> GetIncompleteByIdentityNumberAsync(string identityNumber) =>
        QuerySingleOrDefaultAsync<ApplicationEntity>(
            $"""
            {SelectColumns}
            WHERE identity_number = @IdentityNumber
              AND status IN ('Started', 'PersonCreated')
            ORDER BY created_at DESC
            LIMIT 1
            """,
            new { IdentityNumber = identityNumber });

    public Task<long> InsertAsync(ApplicationEntity application) =>
        ExecuteScalarAsync<long>(
            """
            INSERT INTO cus.application
                (identity_number, gsm_country, gsm_area_code, gsm_number, status, person_id)
            VALUES
                (@IdentityNumber, @GsmCountry, @GsmAreaCode, @GsmNumber, @Status, @PersonId)
            RETURNING id
            """,
            new
            {
                application.IdentityNumber,
                application.GsmCountry,
                application.GsmAreaCode,
                application.GsmNumber,
                Status = application.Status.ToString(),
                application.PersonId
            });

    public Task UpdateStatusAsync(long id, ApplicationStatus status, long? personId) =>
        ExecuteAsync(
            """
            UPDATE cus.application
            SET status = @Status,
                person_id = COALESCE(@PersonId, person_id),
                updated_at = now()
            WHERE id = @Id
            """,
            new { Id = id, Status = status.ToString(), PersonId = personId });
}
