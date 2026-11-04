using Microsoft.Extensions.DependencyInjection;
using MiniBoa.Framework.Messaging;

namespace MiniBoa.Framework;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Framework servislerini kaydeder. Hepsi scoped: her HTTP isteği kendi bağlamını alır.
    /// </summary>
    public static IServiceCollection AddMiniBoaFramework(this IServiceCollection services)
    {
        services.AddScoped<RequestContext>();
        services.AddScoped<ObjectHelper>();
        services.AddScoped<IMessagingHelper, JsonMessagingHelper>();
        return services;
    }
}
