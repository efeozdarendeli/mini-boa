// Kullanıcı yazarken anında geri bildirim vermek için biçim kontrolleri.
// Asıl kural kontrolü her zaman backend'dedir (Business katmanı); buradakiler sadece
// açıkça hatalı bir değeri sunucuya göndermeden yakalar.

export function onlyDigits(value: string): string {
  return value.replace(/\D/g, '');
}

/** Ad/soyad alanı için: harf ve boşluk dışındaki her şeyi siler, art arda boşlukları teke indirir. */
export function lettersOnly(value: string): string {
  return value.replace(/[^\p{L} ]/gu, '').replace(/ {2,}/g, ' ').replace(/^ /, '');
}

/** Türkiye cep telefonu: "05" ile başlayan 11 hane (örn. 05321112233). */
export function isValidMobilePhone(digits: string): boolean {
  return /^05\d{9}$/.test(digits);
}

/** "05321112233" → backend'in beklediği { gsmAreaCode: "532", gsmNumber: "1112233" }. */
export function splitMobilePhone(digits: string): { gsmAreaCode: string; gsmNumber: string } {
  return { gsmAreaCode: digits.slice(1, 4), gsmNumber: digits.slice(4) };
}

export function isDigits(value: string, length: number): boolean {
  return value.length === length && /^\d+$/.test(value);
}

/** TCKN algoritması (backend'deki Mernis.IsValidIdentityNumber ile aynı kural). */
export function isValidIdentityNumber(value: string): boolean {
  if (!isDigits(value, 11) || value[0] === '0') {
    return false;
  }

  const digits = [...value].map(Number);
  const oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
  const evenSum = digits[1] + digits[3] + digits[5] + digits[7];
  const tenth = (((oddSum * 7 - evenSum) % 10) + 10) % 10;
  const eleventh = digits.slice(0, 10).reduce((sum, digit) => sum + digit, 0) % 10;

  return digits[9] === tenth && digits[10] === eleventh;
}

/** "tr55 0099 9000..." gibi girişleri "TR550099900..." biçimine getirir. */
export function normalizeIban(value: string): string {
  return value.replace(/\s/g, '').toUpperCase();
}

/** Türkiye IBAN'ı: TR + 24 rakam ve mod 97 kontrolü (backend'deki Iban.IsValid ile aynı kural). */
export function isValidIban(value: string): boolean {
  const iban = normalizeIban(value);
  if (!/^TR\d{24}$/.test(iban)) {
    return false;
  }

  // İlk 4 karakter sona taşınır, harfler sayıya çevrilir (T=29, R=27); 97'ye bölümünden kalan 1 olmalı.
  const rearranged = iban.slice(4) + iban.slice(0, 4);
  let remainder = 0;
  for (const char of rearranged) {
    const numeric = /\d/.test(char) ? char : String(char.charCodeAt(0) - 55);
    for (const digit of numeric) {
      remainder = (remainder * 10 + Number(digit)) % 97;
    }
  }

  return remainder === 1;
}

/**
 * Kullanıcının yazdığı tutarı sayıya çevirir. Türkçe yazım ("1.250,50") ve nokta ondalık ("1250.50") kabul edilir.
 * Sıfırdan büyük değilse veya 2'den fazla ondalık basamak varsa null döner.
 */
export function parseAmount(value: string): number | null {
  const compact = value.trim().replace(/\s/g, '');
  const normalized = compact.includes(',') ? compact.replace(/\./g, '').replace(',', '.') : compact;

  if (!/^\d+(\.\d{1,2})?$/.test(normalized)) {
    return null;
  }

  const amount = Number(normalized);
  return amount > 0 ? amount : null;
}

export function isValidEmail(value: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
}

/** 18 yaşını dolduranların en geç doğum tarihi ("YYYY-MM-DD"); tarih seçicinin üst sınırı. */
export function latestAdultBirthDate(today: Date = new Date()): string {
  const date = new Date(today.getFullYear() - 18, today.getMonth(), today.getDate());
  const pad = (value: number) => String(value).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

/** "YYYY-MM-DD" biçiminde, gerçek ve gelecekte olmayan bir tarih mi? */
export function isValidPastDate(value: string): boolean {
  if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) {
    return false;
  }

  const date = new Date(`${value}T00:00:00`);
  return !Number.isNaN(date.getTime()) && date.getTime() <= Date.now();
}
