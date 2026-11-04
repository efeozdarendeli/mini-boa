using MiniBoa.Data;
using Npgsql;

namespace MiniBoa.Tests.Data;

/// <summary>
/// docker-compose ile çalışan gerçek PostgreSQL'e bağlanan entegrasyon testleri için ortak kurulum.
/// Başka bir veritabanı kullanmak için MINIBOA_TEST_DB ortam değişkeni verilebilir.
/// </summary>
public sealed class DatabaseFixture : IDisposable
{
    public static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("MINIBOA_TEST_DB")
        ?? "Host=localhost;Port=5433;Database=miniboa;Username=miniboa;Password=miniboa_dev";

    public DatabaseFixture()
    {
        ServiceCollectionExtensions.ConfigureDapper();
        DataSource = NpgsqlDataSource.Create(ConnectionString);
    }

    public NpgsqlDataSource DataSource { get; }

    public UnitOfWork CreateUnitOfWork() => new(DataSource);

    /// <summary>
    /// Hatalı şifre deneyen testlerden sonra çağrılır: seed müşterilerinin hatalı deneme sayacı ve kilidi sıfırlanır,
    /// böylece sonraki testler kilitli bir hesapla karşılaşmaz.
    /// </summary>
    public async Task ResetLoginStateAsync()
    {
        await using var command = DataSource.CreateCommand(
            "UPDATE cus.customer SET failed_login_count = 0, locked_until = NULL");
        await command.ExecuteNonQueryAsync();
    }

    public void Dispose() => DataSource.Dispose();
}

/// <summary>
/// Veritabanına dokunan tüm testler bu koleksiyondadır ve sırayla (paralel değil) çalışır;
/// böylece bir testin geçici değişikliği diğerinin beklediği seed verisini bozmaz.
/// </summary>
[CollectionDefinition(Name)]
public sealed class DatabaseCollection
    : ICollectionFixture<DatabaseFixture>, ICollectionFixture<Orchestration.OrchestrationFixture>
{
    public const string Name = "Database";
}
