import { QueryClient } from '@tanstack/react-query';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // Başarısız sorgu bir kez daha denenir; daha fazlası kullanıcıyı gereksiz bekletir.
      retry: 1,
      // Sekmeye geri dönünce bankacılık verisi kendiliğinden yenilenmesin; yenileme ekranların kararı.
      refetchOnWindowFocus: false,
    },
  },
});
