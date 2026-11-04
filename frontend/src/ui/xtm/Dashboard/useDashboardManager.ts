import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { useNavigate } from 'react-router';
import { Routes } from '../../../app/routes';
import { accountQueries } from '../../../proxy/accountQueries';
import type { AccountType } from '../../../types/contracts';

/** Ana sayfanın mantığı: müşteri özeti, bakiye gizle/göster ve kutucukların nereye gideceği. */
export function useDashboardManager() {
  const navigate = useNavigate();

  const summaryQuery = useQuery(accountQueries.summary());
  const recentTransactionsQuery = useQuery(accountQueries.recentTransactions());

  // Bakiye varsayılan olarak gizli: ekran başkalarının görebileceği bir yerde (şube, ATM) açık olabilir.
  const [isBalanceVisible, setIsBalanceVisible] = useState(false);

  return {
    summaryQuery,
    recentTransactionsQuery,
    isBalanceVisible,
    toggleBalance: () => setIsBalanceVisible((visible) => !visible),
    goToAccounts: () => navigate(Routes.Accounts),
    goToTransfer: () => navigate(Routes.Transfer),
    goToRates: () => navigate(Routes.Rates),
    goToProfile: () => navigate(Routes.Profile),
    openAccount: (accountType: AccountType) => navigate(`${Routes.OpenAccount}?type=${accountType}`),
  };
}
