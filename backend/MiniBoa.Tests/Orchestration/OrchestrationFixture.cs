using Dapper;
using Microsoft.Extensions.DependencyInjection;
using MiniBoa.Business;
using MiniBoa.Data;
using MiniBoa.Framework;
using MiniBoa.Orchestration;
using MiniBoa.Tests.Data;
using Npgsql;

namespace MiniBoa.Tests.Orchestration;

/// <summary>
/// Uygulamadaki Dependency Injection kurulumunun aynısını kurar; testler helper'ları
/// tıpkı API'nin yapacağı gibi, her "istek" için ayrı bir scope içinden alır.
/// </summary>
public sealed class OrchestrationFixture : IDisposable
{
    private readonly ServiceProvider _services;

    public OrchestrationFixture()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMiniBoaFramework();
        services.AddMiniBoaData(DatabaseFixture.ConnectionString);
        services.AddMiniBoaBusiness();
        services.AddMiniBoaOrchestration();

        _services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    /// <summary>Bir HTTP isteğini taklit eder; customerNo verilirse "giriş yapmış" gibi davranır.</summary>
    public AsyncServiceScope CreateRequestScope(long? customerNo = null)
    {
        var scope = _services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<RequestContext>().CustomerNo = customerNo;
        return scope;
    }

    public async Task ExecuteSqlAsync(string sql, object? param = null)
    {
        await using var connection = await _services.GetRequiredService<NpgsqlDataSource>().OpenConnectionAsync();
        await connection.ExecuteAsync(sql, param);
    }

    public async Task<T> QuerySingleAsync<T>(string sql, object? param = null)
    {
        await using var connection = await _services.GetRequiredService<NpgsqlDataSource>().OpenConnectionAsync();
        return await connection.QuerySingleAsync<T>(sql, param);
    }

    /// <summary>Test sırasında oluşturulan kişiyi ve ona bağlı tüm kayıtları siler.</summary>
    public Task DeletePersonCascadeAsync(string identityNumber) =>
        ExecuteSqlAsync(
            """
            DELETE FROM acc.account WHERE customer_no IN
                (SELECT c.customer_no FROM cus.customer c JOIN cus.person p ON p.id = c.person_id
                 WHERE p.identity_number = @IdentityNumber);
            DELETE FROM cus.customer WHERE person_id IN
                (SELECT id FROM cus.person WHERE identity_number = @IdentityNumber);
            DELETE FROM cus.application WHERE identity_number = @IdentityNumber;
            DELETE FROM cus.person WHERE identity_number = @IdentityNumber;
            """,
            new { IdentityNumber = identityNumber });

    public void Dispose() => _services.Dispose();
}
