using MiniBoa.Business.Kernel;
using MiniBoa.Data;
using MiniBoa.Data.Repositories;
using MiniBoa.Framework;
using MiniBoa.Types;
using MiniBoa.Types.Contracts;

namespace MiniBoa.Orchestration;

/// <summary>
/// "Şifremi unuttum" akışı. İki adım: kimlik doğrulama → yeni şifre.
/// 2. adımda kimlik tekrar doğrulanır; 1. adımı atlayıp doğrudan şifre değiştirmeye çalışan bir istek işe yaramaz.
/// </summary>
public sealed class PasswordResetHelper(
    ObjectHelper objectHelper,
    IJournalRepository journal,
    IUnitOfWork unitOfWork,
    Customer customer) : OrchestrationBase(objectHelper, journal)
{
    [AnonymousAccess]
    public async Task<GenericResponse<bool>> VerifyIdentity(PasswordResetIdentityContract contract)
    {
        var returnObject = InitializeGenericResponse<bool>();

        var verification = await VerifyAsync(contract);
        if (!verification.Success)
        {
            await AppendToJournalAsync($"Password reset identity check failed: {verification.Results[0].ErrorCode}");
            return returnObject.AddResults(verification);
        }

        returnObject.Value = true;
        return returnObject;
    }

    [AnonymousAccess]
    public async Task<GenericResponse<bool>> ResetPassword(ResetPasswordContract contract)
    {
        var returnObject = InitializeGenericResponse<bool>();

        var verification = await VerifyAsync(contract);
        if (!verification.Success)
        {
            return returnObject.AddResults(verification);
        }

        var reset = await unitOfWork.ExecuteInTransactionAsync(() =>
            customer.ResetPasswordAsync(verification.Value!, contract.NewPassword));

        await AppendToJournalAsync(reset.Success
            ? $"Password reset succeeded: {verification.Value!.CustomerNo}"
            : $"Password reset failed: {reset.Results[0].ErrorCode}");

        if (!reset.Success)
        {
            return returnObject.AddResults(reset);
        }

        returnObject.Value = true;
        return returnObject;
    }

    private Task<GenericResponse<Data.Entities.CustomerEntity>> VerifyAsync(PasswordResetIdentityContract contract) =>
        customer.VerifyIdentityForPasswordResetAsync(
            contract.IdentityNumber, contract.GsmAreaCode, contract.GsmNumber, contract.BirthDate);
}
