using MiniBoa.Business;
using MiniBoa.Communication;
using MiniBoa.Communication.Middleware;
using MiniBoa.Data;
using MiniBoa.Framework;
using MiniBoa.Orchestration;

var builder = WebApplication.CreateBuilder(args);

// Katmanlar alttan üste doğru kaydedilir; her katman kendi servislerini kendi uzantı metodunda tanımlar.
builder.Services
    .AddMiniBoaFramework()
    .AddMiniBoaData(builder.Configuration.GetConnectionString("MiniBoa")
                    ?? throw new InvalidOperationException("Connection string 'MiniBoa' is not configured."))
    .AddMiniBoaBusiness()
    .AddMiniBoaOrchestration()
    .AddMiniBoaCommunication(builder.Configuration);

var app = builder.Build();

// Middleware sırası önemlidir: her istek bu sırayla içeri girer, cevap ters sırayla çıkar.
app.UseExceptionHandler();                         // 1. Aşağıda fırlayan her hatayı yakala
app.UseCors(CommunicationServiceExtensions.CorsPolicy);  // 2. React dev sunucusuna izin ver
app.UseAuthentication();                           // 3. JWT varsa doğrula, kullanıcıyı belirle
app.UseMiddleware<RequestContextMiddleware>();     // 4. Müşteri no + dil -> RequestContext

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.MapHealthChecks("/health");   // "Healthy" (200) veya "Unhealthy" (503)

app.Run();

/// <summary>Entegrasyon testlerindeki WebApplicationFactory&lt;Program&gt; için.</summary>
public partial class Program;
