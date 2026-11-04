using MiniBoa.Data.Entities;
using MiniBoa.Data.Repositories;
using MiniBoa.Framework.Messaging;
using MiniBoa.Types;

namespace MiniBoa.Business.Kernel;

/// <summary>
/// İl, ilçe ve kur gibi parametreler. Şu an sadece okuma yapar, ama üst katmanlar
/// repository'ye doğrudan değil, hep bir iş nesnesine gider; kural eklenecekse yeri burasıdır.
/// </summary>
public sealed class Parameter(IParameterRepository parameters, IMessagingHelper messaging)
    : BusinessObjectBase(messaging)
{
    public async Task<GenericResponse<IReadOnlyList<CityEntity>>> GetCitiesAsync() =>
        GenericResponse<IReadOnlyList<CityEntity>>.Ok(await parameters.GetCitiesAsync());

    public async Task<GenericResponse<IReadOnlyList<CountyEntity>>> GetCountiesAsync(short cityId) =>
        GenericResponse<IReadOnlyList<CountyEntity>>.Ok(await parameters.GetCountiesAsync(cityId));

    public async Task<GenericResponse<IReadOnlyList<CurrencyRateEntity>>> GetRatesAsync() =>
        GenericResponse<IReadOnlyList<CurrencyRateEntity>>.Ok(await parameters.GetLatestRatesAsync());
}
