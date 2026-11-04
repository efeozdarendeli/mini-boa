import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { Routes } from '../../../app/routes';
import { getSession } from '../../../proxy/session';
import { failure, mockApi, success } from '../../../test/apiMock';
import { hakanSummary, renderApp, signIn } from '../../../test/renderApp';

const recentTransactions = [
  {
    transactionId: 9,
    referenceId: 'ref-2',
    accountNumber: '10000001-2',
    currencyCode: 'TRY',
    direction: 'Credit',
    amount: 1500,
    balanceAfter: 51500,
    description: null,
    createdAt: '2026-11-01T10:00:00Z',
    counterparty: 'Ay** Ka**',
  },
  {
    transactionId: 1,
    referenceId: 'ref-1',
    accountNumber: '10000001-1',
    currencyCode: 'TRY',
    direction: 'Debit',
    amount: 250,
    balanceAfter: 15250.75,
    description: "Ayşe Kaya'ya transfer",
    createdAt: '2026-10-31T09:00:00Z',
    counterparty: 'Ay** Ka**',
  },
];

describe('DashboardView', () => {
  it('lists recent transactions with account, counterparty and amount', async () => {
    signIn();
    mockApi({
      'Account.GetCustomerSummary': () => success(hakanSummary),
      'Account.GetRecentTransactions': () => success(recentTransactions),
    });
    renderApp(Routes.Dashboard);

    const list = await screen.findByRole('list', { name: 'Son İşlemler' });
    const rows = within(list).getAllByRole('listitem');
    expect(rows).toHaveLength(2);
    expect(rows[0]).toHaveTextContent('Gelen transfer');
    expect(rows[0]).toHaveTextContent('10000001-2 · Gönderen: Ay** Ka**');
    expect(rows[1]).toHaveTextContent("Ayşe Kaya'ya transfer");
    expect(rows[1]).toHaveTextContent('Alıcı: Ay** Ka**');

    // Tutarlar bakiye gibi gizli başlar; göz ikonuyla görünür.
    expect(rows[0]).toHaveTextContent('****');
    await userEvent.click(screen.getByRole('button', { name: 'Bakiyeyi göster' }));
    expect(rows[0]).toHaveTextContent(/\+₺1\.500,00/);
    expect(rows[1]).toHaveTextContent(/−₺250,00/);
  });

  it('says so when there are no transactions yet', async () => {
    signIn();
    mockApi({
      'Account.GetCustomerSummary': () => success(hakanSummary),
      'Account.GetRecentTransactions': () => success([]),
    });
    renderApp(Routes.Dashboard);

    expect(await screen.findByText('Henüz bir işleminiz yok.')).toBeInTheDocument();
  });

  it('greets the customer and hides the balance until asked', async () => {
    signIn();
    mockApi({ 'Account.GetCustomerSummary': () => success(hakanSummary) });
    renderApp(Routes.Dashboard);

    expect(await screen.findByRole('heading', { name: 'ABDULLAH HAKAN' })).toBeInTheDocument();
    expect(screen.getByTestId('total-balance')).toHaveTextContent('****');

    await userEvent.click(screen.getByRole('button', { name: 'Bakiyeyi göster' }));

    expect(screen.getByTestId('total-balance')).toHaveTextContent(/65\.250,75/);
  });

  it('shows the previous login time, or a first-login message', async () => {
    signIn();
    mockApi({ 'Account.GetCustomerSummary': () => success(hakanSummary) });
    renderApp(Routes.Dashboard);

    expect(await screen.findByText(/^Son girişiniz: 30 Eki 2026/)).toBeInTheDocument();
  });

  it('shows a first-login message when there is no previous login', async () => {
    signIn();
    mockApi({ 'Account.GetCustomerSummary': () => success({ ...hakanSummary, previousLoginAt: null }) });
    renderApp(Routes.Dashboard);

    expect(await screen.findByText(/İlk girişiniz/)).toBeInTheDocument();
  });

  it('has only working operations, no "coming soon" tiles', async () => {
    signIn();
    mockApi({ 'Account.GetCustomerSummary': () => success(hakanSummary) });
    renderApp(Routes.Dashboard);

    const operations = await screen.findByRole('region', { name: 'İşlemler' });
    expect(operations.querySelectorAll('button')).toHaveLength(6);
    expect(screen.queryByText('Yakında')).not.toBeInTheDocument();
  });

  it('opens the participation account form from its tile', async () => {
    signIn();
    mockApi({ 'Account.GetCustomerSummary': () => success(hakanSummary) });
    const { router } = renderApp(Routes.Dashboard);

    await userEvent.click(screen.getByRole('button', { name: 'Katılım Hesap' }));

    expect(router.state.location.pathname).toBe(Routes.OpenAccount);
    expect(screen.getByLabelText('Hesap türü')).toHaveValue('Participation');
  });

  it('opens the profile from "Profili Düzenle"', async () => {
    signIn();
    mockApi({ 'Account.GetCustomerSummary': () => success(hakanSummary), 'Profile.GetProfile': () => failure('x') });
    const { router } = renderApp(Routes.Dashboard);

    await userEvent.click(await screen.findByRole('button', { name: 'Profili Düzenle' }));

    expect(router.state.location.pathname).toBe(Routes.Profile);
  });

  it('signs out to the welcome screen, not the login screen', async () => {
    signIn();
    mockApi({
      'Account.GetCustomerSummary': () => success(hakanSummary),
      'Parameter.GetCityList': () => success([]),
    });
    const { router } = renderApp(Routes.Dashboard);

    await userEvent.click(await screen.findByRole('button', { name: 'Çıkış' }));

    expect(router.state.location.pathname).toBe(Routes.Welcome);
    expect(getSession()).toBeNull();
  });

  it('offers a retry when the summary cannot be loaded', async () => {
    signIn();
    mockApi({
      'Account.GetCustomerSummary': () => failure('İşleminizi şu anda gerçekleştiremiyoruz.'),
      'Account.GetRecentTransactions': () => success([]),
    });
    renderApp(Routes.Dashboard);

    // Özet yüklenemese de son işlemler kartı kendi başına çalışır; her kart kendi hatasını gösterir.
    expect(await screen.findByRole('button', { name: 'Tekrar dene' })).toBeInTheDocument();
    expect(await screen.findByText('Henüz bir işleminiz yok.')).toBeInTheDocument();
  });
});
