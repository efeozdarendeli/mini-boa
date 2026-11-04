using MiniBoa.Data.Entities;

namespace MiniBoa.Data.Repositories;

public interface IParameterRepository
{
    Task<IReadOnlyList<CityEntity>> GetCitiesAsync();

    Task<IReadOnlyList<CountyEntity>> GetCountiesAsync(short cityId);

    /// <summary>Her döviz için en güncel tarihli kuru getirir.</summary>
    Task<IReadOnlyList<CurrencyRateEntity>> GetLatestRatesAsync();
}
