using Microsoft.Extensions.DependencyInjection;

namespace MiniBoa.Orchestration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMiniBoaOrchestration(this IServiceCollection services)
    {
        services.AddScoped<BecomeACustomerHelper>();
        services.AddScoped<LoginHelper>();
        services.AddScoped<AccountHelper>();
        services.AddScoped<TransferHelper>();
        services.AddScoped<ParameterHelper>();
        services.AddScoped<PasswordResetHelper>();
        services.AddScoped<ProfileHelper>();
        return services;
    }
}
