import { getLanguage, type Language } from './language';
import type { MessageCatalog, MessageCategory, MessageKey } from './messageCatalog';
import { en } from './resources/en';
import { tr } from './resources/tr';

const catalogs: Record<Language, MessageCatalog> = { tr, en };

/**
 * Seçili dilde metni getirir (BOA'daki MessagingHelper.GetMessage("Xtm", key) karşılığı).
 * Kategori ve anahtar tip kontrollüdür: olmayan bir anahtar yazılırsa kod derlenmez.
 */
export function getMessage<Category extends MessageCategory>(
  category: Category,
  key: MessageKey<Category>,
  language: Language = getLanguage(),
): string {
  return catalogs[language][category][key] as string;
}

/** "Yeni hesabınız açıldı: {0}" + "10000001-4" → "Yeni hesabınız açıldı: 10000001-4" (BOA'daki String.Format kullanımı). */
export function formatMessage(template: string, ...values: (string | number)[]): string {
  return template.replace(/\{(\d+)\}/g, (placeholder, index: string) => String(values[Number(index)] ?? placeholder));
}
