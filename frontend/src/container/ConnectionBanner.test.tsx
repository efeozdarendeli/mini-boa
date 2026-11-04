import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { Routes } from '../app/routes';
import { setIsOnline } from '../proxy/connectionStatus';
import { renderApp } from '../test/renderApp';

describe('ConnectionBanner', () => {
  afterEach(() => setIsOnline(true));

  it('appears when the server cannot be reached and disappears once /health answers', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});
    const fetchMock = vi.fn(async (url: string) => {
      if (url.endsWith('/health')) {
        return new Response('Healthy', { status: 200 });
      }
      throw new TypeError('Failed to fetch');
    });
    vi.stubGlobal('fetch', fetchMock);

    renderApp(Routes.Welcome);

    // Karşılama ekranı il listesini ister; sunucuya ulaşılamaz.
    expect(await screen.findByText(/Sunucuya bağlanılamıyor/)).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Tekrar dene' }));

    expect(screen.queryByText(/Sunucuya bağlanılamıyor/)).not.toBeInTheDocument();
  });
});
