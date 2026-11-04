using MiniBoa.Data.Entities;
using MiniBoa.Data.Repositories;
using MiniBoa.Framework.Messaging;
using MiniBoa.Types;

namespace MiniBoa.Business.Kernel;

/// <summary>
/// "Müşteri Ol" başvurusunun durumunu yönetir: Started → PersonCreated → Completed.
/// BOA kodundaki "Tamamlanmamış başvurusu var kontrolü" bu nesne üzerinden yapılır.
/// </summary>
public sealed class CustomerApplication(
    IApplicationRepository applications,
    IMernis mernis,
    IMessagingHelper messaging) : BusinessObjectBase(messaging)
{
    /// <summary>Başvurunun ilk adımında girilen TCKN ve GSM'in biçim kontrolü.</summary>
    public GenericResponse<bool> ValidateInput(string identityNumber, string gsmAreaCode, string gsmNumber)
    {
        if (!mernis.IsValidIdentityNumber(identityNumber))
        {
            return Fail<bool>(ErrorCodes.Validation, "InvalidIdentityNumber");
        }

        return Person.IsValidGsm(gsmAreaCode, gsmNumber)
            ? GenericResponse<bool>.Ok(true)
            : Fail<bool>(ErrorCodes.Validation, "InvalidGsm");
    }

    /// <summary>Yarım kalmış başvuru varsa döner; yoksa Value null'dır.</summary>
    public async Task<GenericResponse<ApplicationEntity?>> FindIncompleteAsync(string identityNumber) =>
        GenericResponse<ApplicationEntity?>.Ok(await applications.GetIncompleteByIdentityNumberAsync(identityNumber));

    /// <summary>
    /// Devam eden başvuruyu getirir. Başvuru yoksa, tamamlanmışsa veya TCKN başvurununkiyle eşleşmiyorsa
    /// "bulunamadı" döner; böylece başkasının başvuru numarasıyla işlem yapılamaz.
    /// </summary>
    public async Task<GenericResponse<ApplicationEntity>> GetIncompleteAsync(long applicationId, string identityNumber)
    {
        var application = await applications.GetByIdAsync(applicationId);

        var isUsable = application is { Status: ApplicationStatus.Started or ApplicationStatus.PersonCreated }
                       && application.IdentityNumber == identityNumber;

        return isUsable
            ? GenericResponse<ApplicationEntity>.Ok(application!)
            : Fail<ApplicationEntity>(ErrorCodes.NotFound, "ApplicationNotFound");
    }

    /// <returns>Yeni başvurunun id'si.</returns>
    public async Task<GenericResponse<long>> StartAsync(string identityNumber, string gsmAreaCode, string gsmNumber)
    {
        var validation = ValidateInput(identityNumber, gsmAreaCode, gsmNumber);
        if (!validation.Success)
        {
            return new GenericResponse<long>().AddResults(validation);
        }

        var id = await applications.InsertAsync(new ApplicationEntity
        {
            IdentityNumber = identityNumber,
            GsmAreaCode = gsmAreaCode,
            GsmNumber = gsmNumber,
            Status = ApplicationStatus.Started
        });

        return GenericResponse<long>.Ok(id);
    }

    public async Task<GenericResponse<bool>> MarkPersonCreatedAsync(long applicationId, long personId)
    {
        await applications.UpdateStatusAsync(applicationId, ApplicationStatus.PersonCreated, personId);
        return GenericResponse<bool>.Ok(true);
    }

    public async Task<GenericResponse<bool>> CompleteAsync(long applicationId)
    {
        await applications.UpdateStatusAsync(applicationId, ApplicationStatus.Completed, personId: null);
        return GenericResponse<bool>.Ok(true);
    }
}
