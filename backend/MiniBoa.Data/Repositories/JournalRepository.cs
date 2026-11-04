using Dapper;
using Npgsql;

namespace MiniBoa.Data.Repositories;

/// <summary>
/// Diğer repository'lerden farklı olarak UnitOfWork'ü değil, kendi bağlantısını kullanır:
/// işlem hata alıp rollback olsa bile "ne denendi" bilgisinin günlükte kalması gerekir.
/// </summary>
public sealed class JournalRepository(NpgsqlDataSource dataSource) : IJournalRepository
{
    public async Task AppendAsync(string traceId, long? customerNo, string methodName, string message)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await connection.ExecuteAsync(
            """
            INSERT INTO log.device_journal (trace_id, customer_no, method_name, message)
            VALUES (@TraceId, @CustomerNo, @MethodName, @Message)
            """,
            new { TraceId = traceId, CustomerNo = customerNo, MethodName = methodName, Message = message });
    }
}
