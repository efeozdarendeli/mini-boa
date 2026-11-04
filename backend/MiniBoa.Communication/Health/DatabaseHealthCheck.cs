using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace MiniBoa.Communication.Health;

/// <summary>
/// /health uç noktası için: API ayakta mı ve veritabanına ulaşabiliyor mu?
/// Başlatma script'i ve izleme araçları uygulamanın gerçekten hazır olduğunu buradan anlar.
/// </summary>
public sealed class DatabaseHealthCheck(NpgsqlDataSource dataSource) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var command = dataSource.CreateCommand("SELECT 1");
            await command.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Database is not reachable.", exception);
        }
    }
}
