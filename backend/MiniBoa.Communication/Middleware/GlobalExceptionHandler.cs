using Microsoft.AspNetCore.Diagnostics;
using MiniBoa.Framework;
using MiniBoa.Framework.Exceptions;
using MiniBoa.Framework.Messaging;
using MiniBoa.Types;

namespace MiniBoa.Communication.Middleware;

/// <summary>
/// Hiçbir katmanda yakalanmamış exception'ların son durağı (BOA'daki Exception Handling katmanı).
/// Kullanıcıya her zaman GenericResponse biçiminde, kendi dilinde bir mesaj döner;
/// hata ayrıntısı (stack trace) asla dışarı verilmez, sadece TraceId ile günlüğe yazılır.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Exception handler uygulama boyunca tektir (singleton); isteğe özel servisler isteğin kendisinden alınır.
        var context = httpContext.RequestServices.GetRequiredService<RequestContext>();
        var messaging = httpContext.RequestServices.GetRequiredService<IMessagingHelper>();

        var (statusCode, errorCode, message) = exception switch
        {
            BoaException { ErrorCode: ErrorCodes.Unauthorized } boa =>
                (StatusCodes.Status401Unauthorized, boa.ErrorCode, messaging.GetMessage("Xtm", boa.MessageKey)),
            BoaException boa =>
                (StatusCodes.Status400BadRequest, boa.ErrorCode, messaging.GetMessage("Business", boa.MessageKey)),
            _ =>
                (StatusCodes.Status500InternalServerError, ErrorCodes.Unexpected,
                    messaging.GetMessage("Xtm", "WeCantMakeYourTranNowTryAgain"))
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "[{TraceId}] Unhandled exception", context.TraceId);
        }
        else
        {
            logger.LogWarning("[{TraceId}] {ErrorCode}: {Message}", context.TraceId, errorCode, exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(GenericResponse<object>.Fail(errorCode, message), cancellationToken);
        return true;
    }
}
