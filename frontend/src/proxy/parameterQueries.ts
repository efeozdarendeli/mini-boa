import { queryOptions } from '@tanstack/react-query';
import { Executer } from './executer';
import { unwrap } from './responseHelpers';

/**
 * Client Caching katmanı (BOA mimarisindeki "Client Caching" kutusu).
 * İl/ilçe listesi oturum boyunca değişmez: bir kez istenir, sonraki her kullanımda bellekten gelir.
 * Kurlar değişebildiği için 1 dakika taze sayılır, sonra arka planda yenilenir.
 */
export const parameterQueries = {
  cities: () =>
    queryOptions({
      queryKey: ['parameters', 'cities'],
      queryFn: () => unwrap(Executer.getCityList()),
      staleTime: Infinity,
    }),

  counties: (cityId: number) =>
    queryOptions({
      queryKey: ['parameters', 'counties', cityId],
      queryFn: () => unwrap(Executer.getCountyList(cityId)),
      staleTime: Infinity,
    }),

  currencyRates: () =>
    queryOptions({
      queryKey: ['parameters', 'currencyRates'],
      queryFn: () => unwrap(Executer.getCurrencyRates()),
      staleTime: 60_000,
    }),
};
