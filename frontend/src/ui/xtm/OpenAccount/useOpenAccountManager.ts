import { useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router';
import { Routes } from '../../../app/routes';
import { DialogTypes, useContainerManager } from '../../../container/containerContext';
import { accountQueryKey } from '../../../proxy/accountQueries';
import { Executer } from '../../../proxy/executer';
import { getErrorMessage } from '../../../proxy/responseHelpers';
import type { AccountType } from '../../../types/contracts';
import type { AccountsLocationState } from '../Accounts/useAccountsManager';

/** Backend'deki Account.SupportedCurrencies ile aynı liste. */
export const SUPPORTED_CURRENCIES = ['TRY', 'USD', 'EUR', 'GBP'] as const;

export const ACCOUNT_TYPES: AccountType[] = ['Current', 'Participation'];

/** Hesap Aç ekranının mantığı. Ana sayfadaki kutucuğa göre hesap türü önceden seçili gelir (?type=Participation). */
export function useOpenAccountManager() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const containerManager = useContainerManager();
  const [searchParams] = useSearchParams();

  const [accountType, setAccountType] = useState<AccountType>(
    searchParams.get('type') === 'Participation' ? 'Participation' : 'Current',
  );
  const [currencyCode, setCurrencyCode] = useState<string>('TRY');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const submit = async () => {
    setIsSubmitting(true);
    const response = await Executer.openAccount({ accountType, currencyCode });
    setIsSubmitting(false);

    if (!response.success || !response.value) {
      containerManager.showDialog(getErrorMessage(response), DialogTypes.Warning);
      return;
    }

    // Yeni hesap; ana sayfa özeti ve hesap listesi önbellekte eski kalmasın.
    await queryClient.invalidateQueries({ queryKey: accountQueryKey });

    const state: AccountsLocationState = { openedAccountNumber: response.value.accountNumber };
    navigate(Routes.Accounts, { state });
  };

  return {
    accountType,
    currencyCode,
    isSubmitting,
    setAccountType,
    setCurrencyCode,
    submit,
    goBack: () => navigate(Routes.Dashboard),
  };
}
