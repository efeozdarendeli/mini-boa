// Giriş yapmış müşterinin oturumu (JWT + ekranda gösterilecek ad).
// sessionStorage kullanılır: sekme kapanınca oturum da biter; bankacılıkta localStorage'dan daha güvenlidir.

export interface SessionInfo {
  token: string;
  customerNo: number;
  firstName: string;
  lastName: string;
}

const STORAGE_KEY = 'miniboa.session';
const listeners = new Set<() => void>();

let currentSession: SessionInfo | null = readStoredSession();
let expiredNoticePending = false;

export function getSession(): SessionInfo | null {
  return currentSession;
}

export function startSession(session: SessionInfo): void {
  expiredNoticePending = false;
  update(session);
}

export function endSession(): void {
  expiredNoticePending = false;
  update(null);
}

/** Sunucu 401 döndüğünde çağrılır: oturum kapanır, giriş ekranı bir kez "süreniz doldu" uyarısı gösterir. */
export function expireSession(): void {
  if (currentSession) {
    expiredNoticePending = true;
  }
  update(null);
}

/** Giriş ekranı "oturum süresi doldu" uyarısı göstermeli mi? Yeni giriş yapılınca kendiliğinden temizlenir. */
export function hasExpiredNotice(): boolean {
  return expiredNoticePending;
}

export function subscribeToSession(listener: () => void): () => void {
  listeners.add(listener);
  return () => listeners.delete(listener);
}

function update(session: SessionInfo | null): void {
  currentSession = session;

  try {
    if (session) {
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    } else {
      sessionStorage.removeItem(STORAGE_KEY);
    }
  } catch {
    // Depolama kapalıysa oturum sadece bellekte yaşar; sayfa yenilenince tekrar giriş istenir.
  }

  listeners.forEach((listener) => listener());
}

function readStoredSession(): SessionInfo | null {
  try {
    const stored = sessionStorage.getItem(STORAGE_KEY);
    return stored ? (JSON.parse(stored) as SessionInfo) : null;
  } catch {
    return null;
  }
}
