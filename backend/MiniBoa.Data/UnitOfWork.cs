using Npgsql;

namespace MiniBoa.Data;

/// <summary>
/// Bağlantı ilk ihtiyaç anında açılır (lazy) ve istek bitince DI tarafından kapatılır.
/// Commit edilmeden kapatılan transaction PostgreSQL tarafından geri alınır.
/// </summary>
public sealed class UnitOfWork(NpgsqlDataSource dataSource) : IUnitOfWork
{
    private NpgsqlConnection? _connection;
    private NpgsqlTransaction? _transaction;

    public NpgsqlTransaction? Transaction => _transaction;

    public bool HasActiveTransaction => _transaction is not null;

    public async Task<NpgsqlConnection> GetConnectionAsync()
    {
        _connection ??= await dataSource.OpenConnectionAsync();
        return _connection;
    }

    public async Task BeginAsync()
    {
        if (_transaction is not null)
        {
            throw new InvalidOperationException("A transaction is already active.");
        }

        var connection = await GetConnectionAsync();
        _transaction = await connection.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException("There is no active transaction to commit.");
        }

        await _transaction.CommitAsync();
        await EndTransactionAsync();
    }

    public async Task RollbackAsync()
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.RollbackAsync();
        await EndTransactionAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await EndTransactionAsync();

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    private async Task EndTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
