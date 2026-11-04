using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MiniBoa.Business.Kernel;
using MiniBoa.Business.Security;

namespace MiniBoa.Business;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMiniBoaBusiness(this IServiceCollection services)
    {
        // Durum tutmayan servisler uygulama boyunca tek.
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IMernis, Mernis>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        // İş nesneleri isteğe özel bağlamı (RequestContext, UnitOfWork) kullandığı için scoped.
        services.AddScoped<Person>();
        services.AddScoped<Customer>();
        services.AddScoped<CustomerApplication>();
        services.AddScoped<Account>();
        services.AddScoped<Parameter>();

        return services;
    }
}
