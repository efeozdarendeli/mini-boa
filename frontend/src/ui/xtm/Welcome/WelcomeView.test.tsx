import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { createMemoryRouter, RouterProvider } from 'react-router';
import { describe, expect, it } from 'vitest';
import { RootLayout } from '../../../app/RootLayout';
import { Routes } from '../../../app/routes';
import { mockFetchResponse } from '../../../test/fetchMock';
import { WelcomeView } from './WelcomeView';

function renderWelcome() {
  // Ekran açılınca il listesini önceden ister; sahte sunucu boş liste döner.
  const fetchMock = mockFetchResponse({ value: [], results: [], success: true });

  const router = createMemoryRouter(
    [
      {
        element: <RootLayout />,
        children: [
          { path: Routes.Welcome, element: <WelcomeView /> },
          { path: Routes.Login, element: <p>login screen</p> },
          { path: Routes.BecomeACustomer, element: <p>become a customer screen</p> },
        ],
      },
    ],
    { initialEntries: [Routes.Welcome] },
  );

  render(
    <QueryClientProvider client={new QueryClient()}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  );

  return { fetchMock };
}

describe('WelcomeView', () => {
  it('shows both cards in Turkish by default', () => {
    renderWelcome();

    expect(screen.getByRole('heading', { name: 'MiniBoa Müşterisiyim' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Müşteri Olmak İstiyorum' })).toBeInTheDocument();
  });

  it('switches every text to English', async () => {
    renderWelcome();

    await userEvent.click(screen.getByRole('button', { name: 'ENGLISH' }));

    expect(screen.getByRole('heading', { name: "I'm a MiniBoa Customer" })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'ENGLISH' })).toHaveAttribute('aria-pressed', 'true');
  });

  it('goes to the login screen', async () => {
    renderWelcome();

    await userEvent.click(screen.getByRole('button', { name: 'Giriş' }));

    expect(screen.getByText('login screen')).toBeInTheDocument();
  });

  it('goes to the become-a-customer screen', async () => {
    renderWelcome();

    await userEvent.click(screen.getByRole('button', { name: 'Müşteri Olmak İstiyorum' }));

    expect(screen.getByText('become a customer screen')).toBeInTheDocument();
  });

  it('prefetches the city list for the address step', () => {
    const { fetchMock } = renderWelcome();

    expect(fetchMock).toHaveBeenCalledOnce();
  });
});
