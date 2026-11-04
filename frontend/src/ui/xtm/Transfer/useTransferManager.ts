import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useNavigate } from 'react-router';
import { Routes } from '../../../app/routes';
import { DialogTypes, useContainerManager } from '../../../container/containerContext';
import { useMessaging, type Translate } from '../../../messaging/useMessaging';
import { accountQueries, accountQueryKey } from '../../../proxy/accountQueries';
import { Executer } from '../../../proxy/executer';
import { getErrorMessage } from '../../../proxy/responseHelpers';
import { formatAmountInput } from '../../../shared/format';
import { hasErrors, useFormState, type FormErrors } from '../../../shared/useFormState';
import { isValidIban, normalizeIban, parseAmount } from '../../../shared/validation';
import type { AccountContract, TransferResultContract } from '../../../types/contracts';

export type TransferStep = 'form' | 'confirm' | 'done';

type TransferForm = { fromAccountId: string; toIban: string; amount: string; description: string };

const emptyForm: TransferForm = { fromAccountId: '', toIban: '', amount: '', description: '' };

function validate(
  values: TransferForm,
  fromAccount: AccountContract | null,
  receiverError: string | null,
  t: Translate,
): FormErrors<TransferForm> {
  const amount = parseAmount(values.amount);

  return {
    fromAccountId: fromAccount ? undefined : t('Validation', 'Required'),
    toIban: !isValidIban(values.toIban) ? t('Validation', 'Iban') : (receiverError ?? undefined),
    amount:
      amount === null
        ? t('Validation', 'Amount')
        : fromAccount && amount > fromAccount.balance
          ? t('Validation', 'AmountExceedsBalance')
          : undefined,
  };
}

/**
 * Para Transferi ekranının mantığı: Form → Onay → Sonuç.
 * Onay adımı bankacılıkta standarttır; kullanıcı parayı göndermeden önce bilgileri bir kez daha görür.
 */
export function useTransferManager() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const containerManager = useContainerManager();
  const { getMessage, language } = useMessaging();

  const accountsQuery = useQuery(accountQueries.accounts());
  const activeAccounts = (accountsQuery.data ?? []).filter((account) => account.status === 'Active');

  const form = useFormState<TransferForm>(emptyForm);
  const [step, setStep] = useState<TransferStep>('form');
  const [confirmedAmount, setConfirmedAmount] = useState(0);
  const [result, setResult] = useState<TransferResultContract | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Kullanıcı seçmediyse ilk aktif hesap gönderen hesap olarak seçili gelir.
  const fromAccountId = form.values.fromAccountId || String(activeAccounts[0]?.accountId ?? '');
  const fromAccount = activeAccounts.find((account) => String(account.accountId) === fromAccountId) ?? null;

  // IBAN tamamlanınca (26 karakter, geçerli) alıcının maskeli adı sorulur; yarım IBAN için istek atılmaz.
  const iban = normalizeIban(form.values.toIban);
  const receiverQuery = useQuery({ ...accountQueries.receiverName(iban), enabled: isValidIban(iban) });
  const receiverName = receiverQuery.data?.maskedName ?? null;
  const receiverError = receiverQuery.isError ? receiverQuery.error.message : null;

  const continueToConfirm = () => {
    const errors = validate({ ...form.values, fromAccountId }, fromAccount, receiverError, getMessage);
    if (hasErrors(errors)) {
      form.setErrors(errors);
      return;
    }

    setConfirmedAmount(parseAmount(form.values.amount)!);
    setStep('confirm');
  };

  const confirm = async () => {
    if (!fromAccount) {
      return;
    }

    setIsSubmitting(true);
    const response = await Executer.transfer({
      fromAccountId: fromAccount.accountId,
      toIban: iban,
      amount: confirmedAmount,
      description: form.values.description.trim() || undefined,
    });
    setIsSubmitting(false);

    if (!response.success || !response.value) {
      setStep('form');
      containerManager.showDialog(getErrorMessage(response), DialogTypes.Warning);
      return;
    }

    // Bakiyeler değişti: ana sayfa, hesap listesi ve hareketler bir sonraki gösterimde yeniden alınır.
    await queryClient.invalidateQueries({ queryKey: accountQueryKey });
    setResult(response.value);
    setStep('done');
  };

  const newTransfer = () => {
    form.setValues(emptyForm);
    setResult(null);
    setStep('form');
  };

  return {
    step,
    accountsQuery,
    activeAccounts,
    fromAccount,
    fromAccountId,
    form,
    receiverName,
    isReceiverLoading: receiverQuery.isFetching,
    receiverError,
    confirmedAmount,
    result,
    isSubmitting,
    fillWholeBalance: () =>
      fromAccount && form.setValue('amount', formatAmountInput(fromAccount.balance, language)),
    continueToConfirm,
    confirm,
    edit: () => setStep('form'),
    newTransfer,
    printReceipt: () => window.print(),
    goHome: () => navigate(Routes.Dashboard),
  };
}

export type TransferManager = ReturnType<typeof useTransferManager>;
