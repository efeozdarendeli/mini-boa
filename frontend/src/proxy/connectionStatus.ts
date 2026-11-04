// Sunucuya ulaşılabiliyor mu? Executer her istekte günceller; üstteki "Bağlantı yok" şeridi buna abone olur.

const listeners = new Set<() => void>();
let isOnline = true;

export function getIsOnline(): boolean {
  return isOnline;
}

export function setIsOnline(online: boolean): void {
  if (online === isOnline) {
    return;
  }
  isOnline = online;
  listeners.forEach((listener) => listener());
}

export function subscribeToConnection(listener: () => void): () => void {
  listeners.add(listener);
  return () => listeners.delete(listener);
}
