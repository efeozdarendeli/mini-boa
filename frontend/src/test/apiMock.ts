import { vi } from 'vitest';
import type { GenericResponse } from '../types/genericResponse';

type Handler = (data: unknown) => GenericResponse<unknown>;

export const success = <T,>(value: T): GenericResponse<T> => ({ value, results: [], success: true });

export const failure = (message: string, errorCode = 'BUS-0002'): GenericResponse<never> => ({
  value: null,
  success: false,
  results: [{ errorCode, message, severity: 'Error' }],
});

/**
 * Backend'in yerine geçen sahte /api/execute: gelen methodName'e göre ilgili handler'ın cevabını döner.
 * Tanımlanmamış bir işlem çağrılırsa test, gerçek backend'deki gibi 404 ile başarısız olur.
 */
export function mockApi(handlers: Record<string, Handler>) {
  const fetchMock = vi.fn(async (_url: string, init: RequestInit) => {
    const { methodName, data } = JSON.parse(init.body as string) as { methodName: string; data?: unknown };
    const handler = handlers[methodName];

    if (!handler) {
      return new Response(JSON.stringify(failure(`No mock for ${methodName}`, 'SYS-0002')), { status: 404 });
    }

    return new Response(JSON.stringify(handler(data)), { status: 200 });
  });

  vi.stubGlobal('fetch', fetchMock);

  /** Belirli bir işleme gönderilen girdiler (data). */
  const callsTo = (methodName: string) =>
    fetchMock.mock.calls
      .map(([, init]) => JSON.parse(init.body as string) as { methodName: string; data?: unknown })
      .filter((request) => request.methodName === methodName)
      .map((request) => request.data);

  return { fetchMock, callsTo };
}
