import { act, fireEvent, screen } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { Routes } from '../app/routes';
import { getSession } from '../proxy/session';
import { mockApi, success } from '../test/apiMock';
import { hakanSummary, renderApp, signIn } from '../test/renderApp';
import { IDLE_WARNING_MS } from './InactivityGuard';

/** Sahte saati ilerletir ve bekleyen promise'lerin (navigasyon, sorgular) tamamlanmasına izin verir. */
async function advance(ms: number) {
  await act(async () => {
    await vi.advanceTimersByTimeAsync(ms);
  });
}

describe('InactivityGuard', () => {
  beforeEach(() => {
    vi.useFakeTimers({ shouldAdvanceTime: true });
    signIn();
    mockApi({
      'Account.GetCustomerSummary': () => success(hakanSummary),
      'Parameter.GetCityList': () => success([]),
    });
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('warns after two idle minutes and signs out 30 seconds later', async () => {
    const { router } = renderApp(Routes.Dashboard);

    await advance(IDLE_WARNING_MS);
    expect(screen.getByRole('alertdialog')).toHaveTextContent('30 saniye');

    await advance(30_000);

    expect(router.state.location.pathname).toBe(Routes.Welcome);
    expect(getSession()).toBeNull();
  });

  it('keeps the session when the customer chooses to stay', async () => {
    const { router } = renderApp(Routes.Dashboard);

    await advance(IDLE_WARNING_MS);
    fireEvent.click(screen.getByRole('button', { name: 'Devam et' }));
    await advance(30_000);

    expect(screen.queryByRole('alertdialog')).not.toBeInTheDocument();
    expect(router.state.location.pathname).toBe(Routes.Dashboard);
    expect(getSession()).not.toBeNull();
  });

  it('restarts the idle timer on activity', async () => {
    renderApp(Routes.Dashboard);

    await advance(IDLE_WARNING_MS - 1_000);
    fireEvent.keyDown(window, { key: 'a' });
    await advance(2_000);

    expect(screen.queryByRole('alertdialog')).not.toBeInTheDocument();
  });
});
