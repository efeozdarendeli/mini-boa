using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using MiniBoa.Tests.Data;

namespace MiniBoa.Tests.Communication;

/// <summary>API'yi gerçek bir HTTP sunucusu açmadan, bellekte ayağa kaldırır (Development ayarlarıyla).</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:MiniBoa", DatabaseFixture.ConnectionString);
    }
}
