using Dapper;

namespace MiniBoa.Data;

/// <summary>
/// Repository'lerin ortak tabanı. Her sorgu UnitOfWork'ün bağlantısı ve (varsa) transaction'ı ile çalışır;
/// alt sınıflar sadece SQL yazmakla ilgilenir.
/// </summary>
public abstract class RepositoryBase(IUnitOfWork unitOfWork)
{
    protected IUnitOfWork UnitOfWork => unitOfWork;

    protected async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null)
    {
        var connection = await unitOfWork.GetConnectionAsync();
        var rows = await connection.QueryAsync<T>(sql, param, unitOfWork.Transaction);
        return rows.AsList();
    }

    protected async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null)
    {
        var connection = await unitOfWork.GetConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<T>(sql, param, unitOfWork.Transaction);
    }

    protected async Task<T> ExecuteScalarAsync<T>(string sql, object? param = null)
    {
        var connection = await unitOfWork.GetConnectionAsync();
        return (await connection.ExecuteScalarAsync<T>(sql, param, unitOfWork.Transaction))!;
    }

    protected async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        var connection = await unitOfWork.GetConnectionAsync();
        return await connection.ExecuteAsync(sql, param, unitOfWork.Transaction);
    }
}
