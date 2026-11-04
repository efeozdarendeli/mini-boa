import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render } from '@testing-library/react';
import { createMemoryRouter } from 'react-router';
import { RouterProvider } from 'react-router/dom';
import { routes } from '../app/router';
import { startSession } from '../proxy/session';
import type { CustomerSummaryContract } from '../types/contracts';

/** Uygulamanın gerçek ekran ağacını bellekte, verilen adresten başlatır. */
export function renderApp(path: string, options: { state?: unknown } = {}) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  const router = createMemoryRouter(routes, { initialEntries: [{ pathname: path, state: options.state }] });

  render(
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  );

  return { router };
}

/** Giriş yapılmış gibi davranmak için. */
export function signIn() {
  startSession({ token: 'test-token', customerNo: 10000001, firstName: 'Abdullah Hakan', lastName: 'Demir' });
}

export const hakanSummary: CustomerSummaryContract = {
  customerNo: 10000001,
  firstName: 'Abdullah Hakan',
  lastName: 'Demir',
  totalTryBalance: 65250.75,
  previousLoginAt: '2026-10-30T11:22:00Z',
  accounts: [
    {
      accountId: 1,
      accountNumber: '10000001-1',
      iban: 'TR550099900000100000010001',
      currencyCode: 'TRY',
      accountType: 'Current',
      balance: 15250.75,
      status: 'Active',
    },
    {
      accountId: 2,
      accountNumber: '10000001-2',
      iban: 'TR280099900000100000010002',
      currencyCode: 'TRY',
      accountType: 'Participation',
      balance: 50000,
      status: 'Active',
    },
  ],
};
