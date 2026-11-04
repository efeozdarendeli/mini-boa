using Microsoft.AspNetCore.Mvc;
using MiniBoa.Communication.Routing;

namespace MiniBoa.Communication.Controllers;

/// <summary>
/// Uygulamanın dışarıya açılan tek iş kapısı (BOA'daki WCF Communication katmanı).
/// Ekran sayısı ne kadar artarsa artsın yeni endpoint açılmaz; yeni orchestration metodu yazmak yeterlidir.
/// </summary>
[ApiController]
[Route("api")]
public sealed class ExecuteController(RequestDispatcher dispatcher, OperationRegistry registry) : ControllerBase
{
    [HttpPost("execute")]
    public async Task<IActionResult> Execute(ExecuteRequest request)
    {
        var result = await dispatcher.DispatchAsync(request);
        return StatusCode(result.StatusCode, result.Body);
    }

    /// <summary>Çağrılabilir işlemlerin listesi; geliştirirken ve demoda hangi metodların var olduğunu görmek için.</summary>
    [HttpGet("operations")]
    public IActionResult Operations() =>
        Ok(registry.All.Select(o => new
        {
            o.Name,
            Input = o.InputType?.Name,
            o.AllowAnonymous
        }));
}
