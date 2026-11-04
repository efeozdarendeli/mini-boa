using Microsoft.Extensions.Logging;
using MiniBoa.Types;

namespace MiniBoa.Framework;

/// <summary>
/// Orchestration metodlarına verilen yardımcı nesne (BOA'daki ObjectHelper karşılığı).
/// Bağlamı taşır ve her metodun başında cevabı izlenebilir şekilde başlatır.
/// </summary>
public sealed class ObjectHelper(RequestContext context, ILogger<ObjectHelper> logger)
{
    public RequestContext Context => context;

    public GenericResponse<T> InitializeGenericResponse<T>(string methodFullName)
    {
        logger.LogInformation("[{TraceId}] {Method} started", context.TraceId, methodFullName);
        return new GenericResponse<T>();
    }
}
