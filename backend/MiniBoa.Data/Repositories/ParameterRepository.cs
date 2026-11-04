using MiniBoa.Data.Entities;

namespace MiniBoa.Data.Repositories;

public sealed class ParameterRepository(IUnitOfWork unitOfWork) : RepositoryBase(unitOfWork), IParameterRepository
{
    // Varsayılan sıralama Türkçe harfleri bilmez ("İstanbul" "Konya"dan sonra gelir);
    // ICU'nun Türkçe kuralları ile Ç, Ğ, İ, Ö, Ş, Ü doğru yerine oturur.
    private const string TurkishOrder = "COLLATE \"tr-TR-x-icu\"";

    public Task<IReadOnlyList<CityEntity>> GetCitiesAsync() =>
        QueryAsync<CityEntity>($"SELECT id, name FROM prm.city ORDER BY name {TurkishOrder}");

    public Task<IReadOnlyList<CountyEntity>> GetCountiesAsync(short cityId) =>
        QueryAsync<CountyEntity>(
            $"SELECT id, city_id, name FROM prm.county WHERE city_id = @CityId ORDER BY name {TurkishOrder}",
            new { CityId = cityId });

    public Task<IReadOnlyList<CurrencyRateEntity>> GetLatestRatesAsync() =>
        // DISTINCT ON: her currency_code için, ORDER BY'a göre ilk satırı (en yeni tarih) alır.
        QueryAsync<CurrencyRateEntity>(
            """
            SELECT DISTINCT ON (r.currency_code)
                   r.currency_code, c.name AS currency_name, r.rate_date, r.buy_rate, r.sell_rate
            FROM prm.currency_rate r
            JOIN prm.currency c ON c.code = r.currency_code
            ORDER BY r.currency_code, r.rate_date DESC
            """);
}
