using Npgsql;

namespace MiniBoa.Data;

/// <summary>
/// Bir istek boyunca kullanılan tek veritabanı bağlantısı ve (varsa) açık transaction
/// (BOA'daki Transaction Management katmanı). Tüm repository'ler aynı UnitOfWork'ü paylaşır;
/// böylece transaction başlatıldığında aradaki her SQL otomatik olarak aynı transaction'a girer.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    NpgsqlTransaction? Transaction { get; }

    bool HasActiveTransaction { get; }

    Task<NpgsqlConnection> GetConnectionAsync();

    Task BeginAsync();

    Task CommitAsync();

    Task RollbackAsync();
}
