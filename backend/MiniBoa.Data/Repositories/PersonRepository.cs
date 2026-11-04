using MiniBoa.Data.Entities;

namespace MiniBoa.Data.Repositories;

public sealed class PersonRepository(IUnitOfWork unitOfWork) : RepositoryBase(unitOfWork), IPersonRepository
{
    private const string SelectColumns = """
        SELECT id, identity_number, first_name, last_name, birth_date,
               gsm_country, gsm_area_code, gsm_number, email,
               city_id, county_id, address_line, created_at
        FROM cus.person
        """;

    public Task<PersonEntity?> GetByIdAsync(long id) =>
        QuerySingleOrDefaultAsync<PersonEntity>($"{SelectColumns} WHERE id = @Id", new { Id = id });

    public Task<PersonEntity?> GetByIdentityNumberAsync(string identityNumber) =>
        QuerySingleOrDefaultAsync<PersonEntity>(
            $"{SelectColumns} WHERE identity_number = @IdentityNumber",
            new { IdentityNumber = identityNumber });

    public Task<long> InsertAsync(PersonEntity person) =>
        ExecuteScalarAsync<long>(
            """
            INSERT INTO cus.person
                (identity_number, first_name, last_name, birth_date,
                 gsm_country, gsm_area_code, gsm_number, email,
                 city_id, county_id, address_line)
            VALUES
                (@IdentityNumber, @FirstName, @LastName, @BirthDate,
                 @GsmCountry, @GsmAreaCode, @GsmNumber, @Email,
                 @CityId, @CountyId, @AddressLine)
            RETURNING id
            """,
            person);

    public Task UpdateAddressAsync(long id, short cityId, int countyId, string addressLine, string? email) =>
        ExecuteAsync(
            """
            UPDATE cus.person
            SET city_id = @CityId, county_id = @CountyId, address_line = @AddressLine, email = @Email
            WHERE id = @Id
            """,
            new { Id = id, CityId = cityId, CountyId = countyId, AddressLine = addressLine, Email = email });
}
