using Dapper;
using Microsoft.Extensions.DependencyInjection;
using MiniBoa.Data.Repositories;
using Npgsql;

namespace MiniBoa.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMiniBoaData(this IServiceCollection services, string connectionString)
    {
        ConfigureDapper();

        // NpgsqlDataSource bağlantı havuzunu (connection pool) yönetir; uygulama boyunca tek olmalı.
        services.AddSingleton(NpgsqlDataSource.Create(connectionString));

        // UnitOfWork ve repository'ler scoped: aynı istekteki herkes aynı bağlantıyı ve transaction'ı paylaşır.
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IParameterRepository, ParameterRepository>();
        services.AddScoped<IJournalRepository, JournalRepository>();

        return services;
    }

    /// <summary>
    /// Kolon adı customer_no ↔ özellik adı CustomerNo eşlemesini açar;
    /// böylece SQL'de her kolona "AS CustomerNo" yazmak gerekmez.
    /// </summary>
    public static void ConfigureDapper() => DefaultTypeMap.MatchNamesWithUnderscores = true;
}
