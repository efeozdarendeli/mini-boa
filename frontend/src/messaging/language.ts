// Seçili dilin tek kaynağı. React dışındaki kod (executer) da buradan okur;
// React bileşenleri useMessaging() üzerinden abone olup dil değişince yeniden çizilir.

export type Language = 'tr' | 'en';

const STORAGE_KEY = 'miniboa.language';
const listeners = new Set<() => void>();

let currentLanguage: Language = readStoredLanguage();
applyToDocument(currentLanguage);

export function getLanguage(): Language {
  return currentLanguage;
}

export function setLanguage(language: Language): void {
  if (language === currentLanguage) {
    return;
  }

  currentLanguage = language;
  applyToDocument(language);

  try {
    localStorage.setItem(STORAGE_KEY, language);
  } catch {
    // Gizli sekme vb. durumlarda depolama kapalı olabilir; dil seçimi yine de bu oturumda çalışır.
  }

  listeners.forEach((listener) => listener());
}

export function subscribeToLanguage(listener: () => void): () => void {
  listeners.add(listener);
  return () => listeners.delete(listener);
}

function readStoredLanguage(): Language {
  try {
    return localStorage.getItem(STORAGE_KEY) === 'en' ? 'en' : 'tr';
  } catch {
    return 'tr';
  }
}

/** Ekran okuyucular doğru telaffuz etsin diye <html lang="..."> güncellenir. */
function applyToDocument(language: Language): void {
  document.documentElement.lang = language;
}
