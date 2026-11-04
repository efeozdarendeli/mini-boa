import type { Language } from '../messaging/language';
import { normalizeIban } from './validation';

const locales: Record<Language, string> = { tr: 'tr-TR', en: 'en-GB' };

/** 15250.75, "TRY" → "₺15.250,75" (tr) / "TRY 15,250.75" (en). */
export function formatMoney(amount: number, currencyCode: string, language: Language): string {
  return new Intl.NumberFormat(locales[language], {
    style: 'currency',
    currency: currencyCode,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount);
}

/** Kur değerleri 4 ondalıkla gösterilir. */
export function formatRate(value: number, language: Language): string {
  return new Intl.NumberFormat(locales[language], { minimumFractionDigits: 4, maximumFractionDigits: 4 }).format(value);
}

/** "USD" → "ABD Doları" / "US Dollar" (tarayıcının kendi döviz adları). */
export function formatCurrencyName(currencyCode: string, language: Language): string {
  return new Intl.DisplayNames([locales[language]], { type: 'currency' }).of(currencyCode) ?? currencyCode;
}

/** "TR550099900000100000010001" → "TR55 0099 9000 0010 0000 0100 01" (okunabilir gruplar). */
export function formatIban(iban: string): string {
  return normalizeIban(iban).replace(/(.{4})/g, '$1 ').trim();
}

/** Backend'den gelen DateTime (ISO, UTC) → yerel tarih ve saat. */
export function formatDateTime(isoDateTime: string, language: Language): string {
  return new Intl.DateTimeFormat(locales[language], { dateStyle: 'medium', timeStyle: 'short' }).format(
    new Date(isoDateTime),
  );
}

/** Backend'den gelen DateOnly ("YYYY-MM-DD") → yerel tarih. Saat dilimi kaymasın diye yerel gün olarak okunur. */
export function formatDate(dateOnly: string, language: Language): string {
  const [year, month, day] = dateOnly.split('-').map(Number);
  return new Intl.DateTimeFormat(locales[language], { dateStyle: 'medium' }).format(new Date(year, month - 1, day));
}

/** "05321112233" → "0532 111 22 33" */
export function formatMobilePhone(digits: string): string {
  const parts = [digits.slice(0, 4), digits.slice(4, 7), digits.slice(7, 9), digits.slice(9, 11)];
  return parts.filter(Boolean).join(' ');
}

/** Tutar alanı için: 1250.5 → "1.250,50" (tr) / "1,250.50" (en). Para birimi simgesi olmadan. */
export function formatAmountInput(amount: number, language: Language): string {
  return new Intl.NumberFormat(locales[language], { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(
    amount,
  );
}

/** Türkçe kurallarla her kelimenin baş harfi büyük: "ibrahim ÖZTÜRK" → "İbrahim Öztürk". */
export function toTitleCase(text: string, language: Language): string {
  return text
    .trim()
    .split(/\s+/)
    .filter(Boolean)
    .map((word) => word.charAt(0).toLocaleUpperCase(locales[language]) + word.slice(1).toLocaleLowerCase(locales[language]))
    .join(' ');
}

/** Listeleri dilin alfabe sırasına göre sıralar (Türkçede "İzmir", "Konya"dan önce gelir). */
export function sortByName<T extends { name: string }>(items: T[], language: Language): T[] {
  const collator = new Intl.Collator(locales[language]);
  return [...items].sort((a, b) => collator.compare(a.name, b.name));
}

/** Türkçede "i" → "İ" olsun diye büyük harfe dile göre çevrilir ("Hakan" → "HAKAN", "ibrahim" → "İBRAHİM"). */
export function toUpperCase(text: string, language: Language): string {
  return text.toLocaleUpperCase(locales[language]);
}
