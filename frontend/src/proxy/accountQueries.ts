import { queryOptions } from '@tanstack/react-query';
import type { TransactionDirection } from '../types/contracts';
import { Executer } from './executer';
import { unwrap } from './responseHelpers';

/** Hesapla ilgili tüm sorguların ortak anahtarı; para hareketinden sonra hepsi birlikte yenilenir. */
export const accountQueryKey = ['account'] as const;

/**
 * Müşteriye özel veriler. Parametrelerin aksine kısa süre taze sayılır: bakiye her an değişebilir.
 * Transfer veya hesap açma sonrası `invalidateQueries({ queryKey: accountQueryKey })` ile hemen yenilenir.
 */
export const accountQueries = {
  summary: () =>
    queryOptions({
      queryKey: [...accountQueryKey, 'summary'],
      queryFn: () => unwrap(Executer.getCustomerSummary()),
      staleTime: 10_000,
    }),

  accounts: () =>
    queryOptions({
      queryKey: [...accountQueryKey, 'list'],
      queryFn: () => unwrap(Executer.getAccounts()),
      staleTime: 10_000,
    }),

  /** Ana sayfadaki son 10 işlem; transferden sonra accountQueryKey ile birlikte yenilenir. */
  recentTransactions: () =>
    queryOptions({
      queryKey: [...accountQueryKey, 'recentTransactions'],
      queryFn: () => unwrap(Executer.getRecentTransactions()),
      staleTime: 10_000,
    }),

  transactions: (accountId: number, days: number, direction?: TransactionDirection) =>
    queryOptions({
      queryKey: [...accountQueryKey, 'transactions', accountId, days, direction ?? 'all'],
      queryFn: () => unwrap(Executer.getAccountTransactions({ accountId, days, direction })),
      staleTime: 10_000,
    }),

  profile: () =>
    queryOptions({
      queryKey: [...accountQueryKey, 'profile'],
      queryFn: () => unwrap(Executer.getProfile()),
      staleTime: 60_000,
    }),

  /** Aynı IBAN tekrar yazılırsa sunucuya tekrar gidilmez. */
  receiverName: (iban: string) =>
    queryOptions({
      queryKey: ['transfer', 'receiverName', iban],
      queryFn: () => unwrap(Executer.getReceiverName(iban)),
      staleTime: 5 * 60_000,
      retry: false,
    }),
};
