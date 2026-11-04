namespace MiniBoa.Framework;

/// <summary>
/// Bir isteğin başından sonuna kadar tüm katmanlarda taşınan bağlam bilgisi
/// (BOA'daki objectHelper.Context karşılığı). Her HTTP isteği için bir tane oluşur (scoped).
/// </summary>
public sealed class RequestContext
{
    public string TraceId { get; } = Guid.NewGuid().ToString("N");

    public string Channel { get; init; } = "SelfService";

    public string Language { get; set; } = "tr";

    /// <summary>Giriş yapmış müşterinin numarası; anonim isteklerde null.</summary>
    public long? CustomerNo { get; set; }

    public bool IsAuthenticated => CustomerNo.HasValue;
}
