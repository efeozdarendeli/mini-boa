import { useQueryClient } from '@tanstack/react-query';
import { useCallback } from 'react';
import { useNavigate } from 'react-router';
import { Routes } from '../app/routes';
import { endSession } from '../proxy/session';

/** Çıkış: üst bardaki "Çıkış" butonu ve hareketsizlik zaman aşımı aynı yolu kullanır. */
export function useLogout() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  return useCallback(async () => {
    // Önce korumalı ekrandan çıkılır; oturum önce kapansaydı RequireAuth kullanıcıyı giriş ekranına atardı.
    // flushSync: geçiş varsayılan olarak düşük öncelikli (transition) yapılır ve oturum kapanmadan ekrana
    // yansımayabilir; flushSync ile karşılama ekranı oturum kapanmadan önce kesin olarak çizilir.
    await navigate(Routes.Welcome, { flushSync: true });
    endSession();
    // Önbellekteki kişisel veriler (hesaplar, bakiye) bir sonraki kullanıcıya kalmasın.
    queryClient.clear();
  }, [navigate, queryClient]);
}
