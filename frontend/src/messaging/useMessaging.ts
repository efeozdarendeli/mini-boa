import { useCallback, useSyncExternalStore } from 'react';
import { getLanguage, setLanguage, subscribeToLanguage } from './language';
import type { MessageCategory, MessageKey } from './messageCatalog';
import { getMessage } from './messagingHelper';

export type Translate = <Category extends MessageCategory>(category: Category, key: MessageKey<Category>) => string;

/** Bileşenlerin metin ve dil için kullandığı hook; dil değişince bileşen yeniden çizilir. */
export function useMessaging() {
  const language = useSyncExternalStore(subscribeToLanguage, getLanguage);

  const translate = useCallback<Translate>((category, key) => getMessage(category, key, language), [language]);

  return { language, setLanguage, getMessage: translate };
}
