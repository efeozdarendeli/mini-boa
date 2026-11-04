using System.Text.Json;

namespace MiniBoa.Communication.Routing;

/// <summary>
/// /api/execute'a gelen her isteğin zarfı (BOA'daki BecomeACustomerHelperRequest { MethodName = "..." } karşılığı).
/// </summary>
/// <param name="MethodName">"Helper.Metod" biçiminde işlem adı; örn. "BecomeACustomer.CustomerDecisionByTaxNumber".</param>
/// <param name="Data">İşlemin girdi contract'ı; girdisiz işlemlerde boş bırakılır.</param>
public sealed record ExecuteRequest(string MethodName, JsonElement? Data);
