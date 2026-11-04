import { useSyncExternalStore } from 'react';
import { getSession, subscribeToSession } from './session';

/** Oturum değişince (giriş, çıkış, süre dolması) bileşeni yeniden çizer. */
export function useSession() {
  return useSyncExternalStore(subscribeToSession, getSession);
}
