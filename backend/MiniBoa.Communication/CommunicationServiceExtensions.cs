using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MiniBoa.Communication.Health;
using MiniBoa.Communication.Middleware;
using MiniBoa.Communication.Routing;
using MiniBoa.Communication.Security;
using MiniBoa.Framework.Messaging;
using MiniBoa.Types;

namespace MiniBoa.Communication;

public static class CommunicationServiceExtensions
{
    public const string CorsPolicy = "Frontend";

    public static IServiceCollection AddMiniBoaCommunication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
                // Enum'lar JSON'da sayı (0, 1) yerine isim ("GoCardLogin") olarak gider; UI tarafı okunaklı kalır.
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .ConfigureApiBehaviorOptions(options =>
                // Gövdesi bozuk istekler de ASP.NET'in varsayılan hata biçimi yerine GenericResponse ile cevaplanır.
                options.InvalidModelStateResponseFactory = context =>
                {
                    var messaging = context.HttpContext.RequestServices.GetRequiredService<IMessagingHelper>();
                    return new BadRequestObjectResult(GenericResponse<object>.Fail(
                        ErrorCodes.Validation, messaging.GetMessage("Xtm", "InvalidRequest")));
                });

        services.AddOpenApi();
        services.AddProblemDetails();
        services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddSingleton(OperationRegistry.FromOrchestrationAssembly());
        services.AddScoped<RequestDispatcher>();

        AddJwtAuthentication(services, configuration);
        AddCors(services, configuration);

        return services;
    }

    private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwt = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

        // Uygulama eksik/zayıf bir anahtarla hiç açılmasın; sessizce güvensiz çalışmaktan iyidir.
        if (jwt.SigningKey.Length < 32)
        {
            throw new InvalidOperationException("Jwt:SigningKey must be configured and at least 32 characters long.");
        }

        services.Configure<JwtOptions>(jwtSection);
        services.AddSingleton<TokenService>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // "sub" gibi standart claim adları .NET'in uzun claim adlarına çevrilmesin.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = TokenService.CreateSigningKey(jwt),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });
    }

    private static void AddCors(IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options => options.AddPolicy(CorsPolicy, policy => policy
            .WithOrigins(origins)
            .WithHeaders("Content-Type", "Authorization", "Accept-Language")
            .WithMethods("GET", "POST")
            .WithExposedHeaders(RequestContextMiddleware.TraceIdHeader)));
    }
}
