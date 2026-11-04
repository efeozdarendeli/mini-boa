import { vi } from 'vitest';

/** fetch'i sahte bir sunucu cevabıyla değiştirir ve gönderilen istekleri incelemek için mock'u döner. */
export function mockFetchResponse(body: unknown, status = 200, headers: Record<string, string> = {}) {
  const fetchMock = vi.fn(async () => new Response(JSON.stringify(body), { status, headers }));
  vi.stubGlobal('fetch', fetchMock);
  return fetchMock;
}

/** Gönderilen isteğin gövdesini ve başlıklarını okunabilir hâle getirir. */
export function readRequest(fetchMock: ReturnType<typeof vi.fn>, callIndex = 0) {
  const [url, init] = fetchMock.mock.calls[callIndex] as [string, RequestInit];
  return {
    url,
    body: JSON.parse(init.body as string) as { methodName: string; data?: unknown },
    headers: init.headers as Record<string, string>,
  };
}
