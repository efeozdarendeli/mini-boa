import { useQuery } from '@tanstack/react-query';
import { useMessaging } from '../messaging/useMessaging';
import { parameterQueries } from '../proxy/parameterQueries';
import { sortByName } from './format';
import type { SelectOption } from './components/Select';

/**
 * İl ve ilçe seçim kutularının seçenekleri (Müşteri Ol ve Profilim ekranları ortak kullanır).
 * İl listesi bir kez, her ilin ilçeleri ilk seçildiğinde istenir; sonrası önbellekten gelir.
 */
export function useLocationOptions(cityId: number) {
  const { language } = useMessaging();
  const citiesQuery = useQuery(parameterQueries.cities());
  const countiesQuery = useQuery({ ...parameterQueries.counties(cityId), enabled: cityId > 0 });

  const toOptions = (items: { id: number; name: string }[]): SelectOption[] =>
    sortByName(items, language).map((item) => ({ value: String(item.id), label: item.name }));

  return {
    cityOptions: toOptions(citiesQuery.data ?? []),
    countyOptions: toOptions(countiesQuery.data ?? []),
  };
}
