import { useQueryClient } from '@tanstack/react-query';
import { useEffect } from 'react';
import { useNavigate } from 'react-router';
import { Routes } from '../../../app/routes';
import { parameterQueries } from '../../../proxy/parameterQueries';

/**
 * Karşılama ekranının mantığı (BOA'daki *Manager.cs karşılığı).
 * Görünüm (WelcomeView) sadece bu hook'un verdiği değerleri çizer ve fonksiyonları çağırır;
 * yönlendirme ve önbellek gibi kararlar burada verilir.
 */
export function useWelcomeManager() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  // Kullanıcı "Müşteri Ol"a geçtiğinde adres adımı il listesini beklemeden bellekten alsın.
  useEffect(() => {
    void queryClient.prefetchQuery(parameterQueries.cities());
  }, [queryClient]);

  return {
    goToLogin: () => navigate(Routes.Login),
    goToBecomeACustomer: () => navigate(Routes.BecomeACustomer),
  };
}
