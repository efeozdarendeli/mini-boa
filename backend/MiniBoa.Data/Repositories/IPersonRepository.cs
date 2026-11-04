using MiniBoa.Data.Entities;

namespace MiniBoa.Data.Repositories;

public interface IPersonRepository
{
    Task<PersonEntity?> GetByIdAsync(long id);

    Task<PersonEntity?> GetByIdentityNumberAsync(string identityNumber);

    Task<long> InsertAsync(PersonEntity person);

    Task UpdateAddressAsync(long id, short cityId, int countyId, string addressLine, string? email);
}
