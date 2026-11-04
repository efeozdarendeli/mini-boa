using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MiniBoa.Communication.Security;
using MiniBoa.Framework;
using MiniBoa.Framework.Messaging;
using MiniBoa.Types;
using MiniBoa.Types.Contracts;

namespace MiniBoa.Communication.Routing;

/// <param name="StatusCode">HTTP durum kodu.</param>
/// <param name="Body">Her durumda bir GenericResponse; UI hep aynı biçimi okur.</param>
public sealed record DispatchResult(int StatusCode, object Body);

/// <summary>
/// Gelen isteği doğru orchestration metoduna yönlendirir (BOA'daki AtmExecute&lt;TRequest, TResponse&gt;(request) karşılığı):
/// işlemi bulur, yetkiyi kontrol eder, girdiyi contract'a çevirir, metodu çağırır.
/// </summary>
public sealed class RequestDispatcher(
    OperationRegistry registry,
    IServiceProvider services,
    RequestContext context,
    IMessagingHelper messaging,
    TokenService tokenService,
    IOptions<JsonOptions> jsonOptions,
    ILogger<RequestDispatcher> logger)
{
    public async Task<DispatchResult> DispatchAsync(ExecuteRequest request)
    {
        if (!registry.TryGet(request.MethodName, out var operation))
        {
            return Error(StatusCodes.Status404NotFound, ErrorCodes.MethodNotFound, "MethodNotFound");
        }

        if (!operation.AllowAnonymous && !context.IsAuthenticated)
        {
            return Error(StatusCodes.Status401Unauthorized, ErrorCodes.Unauthorized, "Unauthorized");
        }

        if (!TryReadInput(operation, request.Data, out var input))
        {
            return Error(StatusCodes.Status400BadRequest, ErrorCodes.Validation, "InvalidRequest");
        }

        var stopwatch = Stopwatch.StartNew();
        var response = await InvokeAsync(operation, input);

        logger.LogInformation(
            "[{TraceId}] {Operation} finished in {ElapsedMs} ms, success: {Success}",
            context.TraceId, operation.Name, stopwatch.ElapsedMilliseconds, IsSuccess(response));

        AttachTokenIfLogin(response);
        return new DispatchResult(StatusCodes.Status200OK, response);
    }

    private bool TryReadInput(OperationDescriptor operation, JsonElement? data, out object? input)
    {
        input = null;
        if (operation.InputType is null)
        {
            return true;
        }

        if (data is not { ValueKind: JsonValueKind.Object } json)
        {
            return false;
        }

        try
        {
            input = json.Deserialize(operation.InputType, jsonOptions.Value.JsonSerializerOptions);
            return input is not null;
        }
        catch (JsonException)
        {
            // Örn. tutar alanına metin gönderilmesi: sunucu hatası değil, hatalı istek.
            return false;
        }
    }

    private async Task<object> InvokeAsync(OperationDescriptor operation, object? input)
    {
        var helper = services.GetRequiredService(operation.HelperType);
        var arguments = operation.InputType is null ? [] : new[] { input };

        var task = (Task)operation.Method.Invoke(helper, arguments)!;
        await task;

        // Task<GenericResponse<T>>'nin sonucu; T derleme anında bilinmediği için Result okunur.
        return task.GetType().GetProperty(nameof(Task<object>.Result))!.GetValue(task)!;
    }

    /// <summary>Token, kimlik doğrulama ile ilgili olduğu için Orchestration'da değil burada eklenir.</summary>
    private void AttachTokenIfLogin(object response)
    {
        if (response is GenericResponse<LoginResultContract> { Success: true, Value: { } login })
        {
            login.Token = tokenService.CreateToken(login.CustomerNo);
        }
    }

    private static bool IsSuccess(object response) =>
        (bool)response.GetType().GetProperty(nameof(GenericResponse<object>.Success))!.GetValue(response)!;

    private DispatchResult Error(int statusCode, string errorCode, string messageKey) =>
        new(statusCode, GenericResponse<object>.Fail(errorCode, messaging.GetMessage("Xtm", messageKey)));
}
