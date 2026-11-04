import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { useLocation, useNavigate } from 'react-router';
import { Routes } from '../../../app/routes';
import { accountQueries } from '../../../proxy/accountQueries';
import { traceUtil } from '../../../shared/traceUtil';
import type { TransactionDirection } from '../../../types/contracts';

/** Hesap açıldıktan sonra Hesaplarım ekranına taşınan bilgi. */
export interface AccountsLocationState {
  openedAccountNumber?: string;
}

/** Backend'deki Account.TransactionPeriods ile aynı dönemler. */
export const TRANSACTION_PERIODS = [7, 30, 90] as const;

export type DirectionFilter = 'All' | TransactionDirection;

const COPIED_FEEDBACK_MS = 2000;

/** Hesaplarım ekranının mantığı: hesap listesi, seçili hesabın hareketleri, IBAN kopyalama. */
export function useAccountsManager() {
  const navigate = useNavigate();
  const location = useLocation();
  const openedAccountNumber = (location.state as AccountsLocationState | null)?.openedAccountNumber;

  const accountsQuery = useQuery(accountQueries.accounts());
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [copiedIban, setCopiedIban] = useState<string | null>(null);
  const [days, setDays] = useState<number>(30);
  const [direction, setDirection] = useState<DirectionFilter>('All');

  // Kullanıcı seçim yapmadıysa ilk hesap seçili sayılır.
  const selectedAccountId = selectedId ?? accountsQuery.data?.[0]?.accountId ?? null;
  const selectedAccount = accountsQuery.data?.find((account) => account.accountId === selectedAccountId) ?? null;

  // Filtre sunucuda uygulanır; her filtre kombinasyonu önbellekte ayrı tutulur, geri dönünce anında gelir.
  const transactionsQuery = useQuery({
    ...accountQueries.transactions(selectedAccountId ?? 0, days, direction === 'All' ? undefined : direction),
    enabled: selectedAccountId !== null,
  });

  const copyIban = async (iban: string) => {
    try {
      await navigator.clipboard.writeText(iban);
      setCopiedIban(iban);
      setTimeout(() => setCopiedIban((current) => (current === iban ? null : current)), COPIED_FEEDBACK_MS);
    } catch (error) {
      // Panoya erişim izni yoksa (eski tarayıcı, http) IBAN ekranda zaten görünür.
      traceUtil.trace('Clipboard is not available', error);
    }
  };

  return {
    accountsQuery,
    transactionsQuery,
    selectedAccountId,
    selectedAccount,
    openedAccountNumber,
    copiedIban,
    days,
    direction,
    setDays,
    setDirection,
    selectAccount: setSelectedId,
    copyIban,
    goToOpenAccount: () => navigate(Routes.OpenAccount),
    goBack: () => navigate(Routes.Dashboard),
  };
}
