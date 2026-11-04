import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router';
import { Routes } from '../../../app/routes';
import { parameterQueries } from '../../../proxy/parameterQueries';
import { useSession } from '../../../proxy/useSession';

/** Kurlar ekranının mantığı. Kurlar giriş gerektirmez; geri butonu kullanıcının durumuna göre döner. */
export function useRatesManager() {
  const navigate = useNavigate();
  const session = useSession();

  return {
    ratesQuery: useQuery(parameterQueries.currencyRates()),
    goBack: () => navigate(session ? Routes.Dashboard : Routes.Welcome),
  };
}
