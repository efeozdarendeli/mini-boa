namespace MiniBoa.Data.Entities;

// prm şemasındaki küçük, salt okunur parametre tabloları.

public sealed class CityEntity
{
    public short Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

public sealed class CountyEntity
{
    public int Id { get; init; }
    public short CityId { get; init; }
    public string Name { get; init; } = string.Empty;
}

public sealed class CurrencyRateEntity
{
    public string CurrencyCode { get; init; } = string.Empty;
    public string CurrencyName { get; init; } = string.Empty;
    public DateOnly RateDate { get; init; }
    public decimal BuyRate { get; init; }
    public decimal SellRate { get; init; }
}
