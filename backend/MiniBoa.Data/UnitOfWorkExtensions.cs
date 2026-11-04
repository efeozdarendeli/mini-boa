using MiniBoa.Types;

namespace MiniBoa.Data;

public static class UnitOfWorkExtensions
{
    /// <summary>
    /// Verilen işi tek bir transaction içinde çalıştırır:
    /// cevap başarılıysa commit, başarısızsa veya exception olursa rollback.
    /// Zaten açık bir transaction varsa ona katılır; böylece bir orchestration metodu
    /// başka bir transaction'lı metodu çağırdığında iç içe transaction açılmaz.
    /// </summary>
    public static async Task<GenericResponse<T>> ExecuteInTransactionAsync<T>(
        this IUnitOfWork unitOfWork,
        Func<Task<GenericResponse<T>>> work)
    {
        if (unitOfWork.HasActiveTransaction)
        {
            return await work();
        }

        await unitOfWork.BeginAsync();
        try
        {
            var response = await work();

            if (response.Success)
            {
                await unitOfWork.CommitAsync();
            }
            else
            {
                await unitOfWork.RollbackAsync();
            }

            return response;
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}
