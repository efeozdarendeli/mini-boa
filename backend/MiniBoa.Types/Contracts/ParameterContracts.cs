namespace MiniBoa.Types.Contracts;

/// <summary>
/// Liste ekranları için genel id/ad çifti (BOA'daki GetCountyList'in döndüğü ParameterContract karşılığı).
/// </summary>
public sealed class ParameterContract
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

public sealed class CountyListInputContract
{
    public short CityId { get; init; }
}

public sealed class CurrencyRateContract
{
    public string CurrencyCode { get; init; } = string.Empty;
    public string CurrencyName { get; init; } = string.Empty;
    public decimal BuyRate { get; init; }
    public decimal SellRate { get; init; }
    public DateOnly RateDate { get; init; }
}
