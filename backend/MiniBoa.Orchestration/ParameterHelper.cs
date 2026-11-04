using MiniBoa.Business.Kernel;
using MiniBoa.Data.Repositories;
using MiniBoa.Framework;
using MiniBoa.Types;
using MiniBoa.Types.Contracts;

namespace MiniBoa.Orchestration;

/// <summary>Adres ekranındaki il/ilçe listeleri ve "Kurlar" ekranı. Giriş gerektirmez.</summary>
public sealed class ParameterHelper(
    ObjectHelper objectHelper,
    IJournalRepository journal,
    Parameter parameter) : OrchestrationBase(objectHelper, journal)
{
    [AnonymousAccess]
    public async Task<GenericResponse<List<ParameterContract>>> GetCityList()
    {
        var returnObject = InitializeGenericResponse<List<ParameterContract>>();

        var cities = await parameter.GetCitiesAsync();
        returnObject.Value = cities.Value!.Select(c => c.ToContract()).ToList();
        return returnObject;
    }

    /// <summary>BOA'daki Executer.GetCountyList(cityId) karşılığı.</summary>
    [AnonymousAccess]
    public async Task<GenericResponse<List<ParameterContract>>> GetCountyList(CountyListInputContract contract)
    {
        var returnObject = InitializeGenericResponse<List<ParameterContract>>();

        var counties = await parameter.GetCountiesAsync(contract.CityId);
        returnObject.Value = counties.Value!.Select(c => c.ToContract()).ToList();
        return returnObject;
    }

    [AnonymousAccess]
    public async Task<GenericResponse<List<CurrencyRateContract>>> GetCurrencyRates()
    {
        var returnObject = InitializeGenericResponse<List<CurrencyRateContract>>();

        var rates = await parameter.GetRatesAsync();
        returnObject.Value = rates.Value!.Select(r => r.ToContract()).ToList();
        return returnObject;
    }
}
