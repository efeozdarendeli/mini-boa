using MiniBoa.Communication.Security;
using MiniBoa.Framework;

namespace MiniBoa.Communication.Middleware;

/// <summary>
/// HTTP isteğindeki bilgileri, alt katmanların kullandığı RequestContext'e aktarır.
/// Bu sayede Orchestration ve Business katmanları HTTP'den, header'lardan veya JWT'den hiç haberdar olmaz.
/// </summary>
public sealed class RequestContextMiddleware(RequestDelegate next)
{
    public const string TraceIdHeader = "X-Trace-Id";

    public async Task InvokeAsync(HttpContext httpContext, RequestContext context)
    {
        // Authentication middleware'i token'ı doğruladıysa müşteri no claim'i dolu gelir.
        var customerNoClaim = httpContext.User.FindFirst(TokenService.CustomerNoClaim)?.Value;
        if (long.TryParse(customerNoClaim, out var customerNo))
        {
            context.CustomerNo = customerNo;
        }

        var language = httpContext.Request.Headers.AcceptLanguage.ToString();
        context.Language = language.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en" : "tr";

        // Kullanıcı bir hata bildirdiğinde bu numarayla sunucu günlüklerinde istek bulunabilir.
        httpContext.Response.Headers[TraceIdHeader] = context.TraceId;

        await next(httpContext);
    }
}
