using MiniBoa.Data.Entities;
using MiniBoa.Types;

namespace MiniBoa.Data.Repositories;

public interface IApplicationRepository
{
    Task<ApplicationEntity?> GetByIdAsync(long id);

    /// <summary>Tamamlanmamış (Started / PersonCreated) en son başvuruyu getirir.</summary>
    Task<ApplicationEntity?> GetIncompleteByIdentityNumberAsync(string identityNumber);

    Task<long> InsertAsync(ApplicationEntity application);

    Task UpdateStatusAsync(long id, ApplicationStatus status, long? personId);
}
