/**
 * İstemci tarafı izleme (BOA'daki KtmTraceUtil.Trace karşılığı).
 * Sadece geliştirme ortamında konsola yazar; sunucudan gelen TraceId ile birlikte
 * loglandığında bir hatanın sunucu günlüğündeki karşılığı bulunabilir.
 */
const isDevelopment = import.meta.env.DEV;

export const traceUtil = {
  trace(message: string, details?: unknown): void {
    if (isDevelopment) {
      console.debug(`[MiniBoa] ${message}`, details ?? '');
    }
  },

  error(message: string, details?: unknown): void {
    console.error(`[MiniBoa] ${message}`, details ?? '');
  },
};
