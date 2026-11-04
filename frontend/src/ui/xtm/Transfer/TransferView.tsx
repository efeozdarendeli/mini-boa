import { CircleCheck, Printer } from 'lucide-react';
import { formatMessage } from '../../../messaging/messagingHelper';
import { useMessaging } from '../../../messaging/useMessaging';
import { Alert } from '../../../shared/components/Alert';
import { AmountField } from '../../../shared/components/AmountField';
import { Button } from '../../../shared/components/Button';
import { Card } from '../../../shared/components/Card';
import layout from '../../../shared/components/FormLayout.module.css';
import { IbanField } from '../../../shared/components/IbanField';
import { InfoList } from '../../../shared/components/InfoList';
import { PageHeader } from '../../../shared/components/PageHeader';
import { QueryBoundary } from '../../../shared/components/QueryBoundary';
import { Select } from '../../../shared/components/Select';
import { TextField } from '../../../shared/components/TextField';
import { formatDateTime, formatIban, formatMoney } from '../../../shared/format';
import styles from './TransferView.module.css';
import { useTransferManager, type TransferManager } from './useTransferManager';

/** Para Transferi: Form → Onay → Sonuç (yazdırılabilir dekont). */
export function TransferView() {
  const { getMessage } = useMessaging();
  const manager = useTransferManager();

  return (
    <div className={layout.narrow}>
      <div className="no-print">
        <PageHeader
          title={getMessage('Transfer', 'Title')}
          onBack={manager.step === 'confirm' ? manager.edit : manager.goHome}
        />
      </div>
      <Card>
        <QueryBoundary query={manager.accountsQuery}>
          {() => (
            <>
              {manager.step === 'form' && <TransferForm manager={manager} />}
              {manager.step === 'confirm' && <TransferConfirm manager={manager} />}
              {manager.step === 'done' && <TransferDone manager={manager} />}
            </>
          )}
        </QueryBoundary>
      </Card>
    </div>
  );
}

function TransferForm({ manager }: { manager: TransferManager }) {
  const { getMessage, language } = useMessaging();
  const { values, errors, setValue } = manager.form;

  if (manager.activeAccounts.length === 0) {
    return <Alert tone="info">{getMessage('Transfer', 'NoActiveAccount')}</Alert>;
  }

  const receiverHint = manager.isReceiverLoading
    ? getMessage('Transfer', 'ReceiverLookup')
    : manager.receiverName
      ? `${getMessage('Transfer', 'Receiver')}: ${manager.receiverName}`
      : getMessage('Transfer', 'DemoHint');

  return (
    <form
      className={layout.form}
      noValidate
      onSubmit={(event) => {
        event.preventDefault();
        manager.continueToConfirm();
      }}
    >
      <Select
        label={getMessage('Transfer', 'FromAccount')}
        value={manager.fromAccountId}
        onChange={(event) => setValue('fromAccountId', event.target.value)}
        options={manager.activeAccounts.map((account) => ({
          value: String(account.accountId),
          label: `${account.accountNumber} · ${formatMoney(account.balance, account.currencyCode, language)}`,
        }))}
        error={errors.fromAccountId}
      />
      <IbanField
        label={getMessage('Transfer', 'ToIban')}
        value={values.toIban}
        onChange={(iban) => setValue('toIban', iban)}
        error={errors.toIban ?? manager.receiverError ?? undefined}
        hint={receiverHint}
      />
      <AmountField
        label={
          manager.fromAccount
            ? `${getMessage('Transfer', 'Amount')} (${manager.fromAccount.currencyCode})`
            : getMessage('Transfer', 'Amount')
        }
        language={language}
        value={values.amount}
        onChange={(amount) => setValue('amount', amount)}
        error={errors.amount}
        hint={
          manager.fromAccount
            ? formatMessage(
                getMessage('Transfer', 'Available'),
                formatMoney(manager.fromAccount.balance, manager.fromAccount.currencyCode, language),
              )
            : undefined
        }
        action={
          <Button variant="outline" onClick={manager.fillWholeBalance}>
            {getMessage('Transfer', 'WholeBalance')}
          </Button>
        }
      />
      <TextField
        label={`${getMessage('Transfer', 'Description')} ${getMessage('Common', 'Optional')}`}
        value={values.description}
        onChange={(event) => setValue('description', event.target.value)}
        maxLength={200}
      />

      <div className={layout.actions}>
        <Button type="submit">{getMessage('Common', 'Continue')}</Button>
      </div>
    </form>
  );
}

function TransferConfirm({ manager }: { manager: TransferManager }) {
  const { getMessage, language } = useMessaging();
  const fromAccount = manager.fromAccount!;
  const { toIban, description } = manager.form.values;

  return (
    <div className={layout.form}>
      <h2 className={layout.sectionTitle}>{getMessage('Transfer', 'ConfirmTitle')}</h2>
      <InfoList
        items={[
          { label: getMessage('Transfer', 'FromAccount'), value: fromAccount.accountNumber },
          { label: getMessage('Transfer', 'ToIban'), value: formatIban(toIban) },
          ...(manager.receiverName ? [{ label: getMessage('Transfer', 'Receiver'), value: manager.receiverName }] : []),
          {
            label: getMessage('Transfer', 'Amount'),
            value: formatMoney(manager.confirmedAmount, fromAccount.currencyCode, language),
          },
          ...(description.trim() ? [{ label: getMessage('Transfer', 'Description'), value: description.trim() }] : []),
        ]}
      />
      <div className={layout.actions}>
        <Button variant="outline" onClick={manager.edit} disabled={manager.isSubmitting}>
          {getMessage('Transfer', 'Edit')}
        </Button>
        <Button variant="accent" onClick={() => void manager.confirm()} isLoading={manager.isSubmitting}>
          {getMessage('Transfer', 'Confirm')}
        </Button>
      </div>
    </div>
  );
}

function TransferDone({ manager }: { manager: TransferManager }) {
  const { getMessage, language } = useMessaging();
  const result = manager.result!;
  const fromAccount = manager.fromAccount;
  const currencyCode = fromAccount?.currencyCode ?? 'TRY';
  const { toIban, description } = manager.form.values;

  return (
    <div className={layout.center}>
      <CircleCheck size={56} className={layout.successIcon} aria-hidden="true" />
      <h2 className="no-print">{getMessage('Transfer', 'DoneTitle')}</h2>
      {/* Yazdırılınca sadece bu başlık ve tablo görünür (bkz. global.css @media print). */}
      <h2 className={styles.receiptTitle}>{getMessage('Transfer', 'ReceiptTitle')}</h2>
      <div style={{ width: '100%' }}>
        <InfoList
          items={[
            { label: getMessage('Transfer', 'ReferenceNo'), value: result.referenceId },
            { label: getMessage('Transfer', 'TransactionDate'), value: formatDateTime(result.transactionDate, language) },
            ...(fromAccount ? [{ label: getMessage('Transfer', 'FromAccount'), value: fromAccount.accountNumber }] : []),
            { label: getMessage('Transfer', 'ToIban'), value: formatIban(toIban) },
            ...(manager.receiverName ? [{ label: getMessage('Transfer', 'Receiver'), value: manager.receiverName }] : []),
            {
              label: getMessage('Transfer', 'Amount'),
              value: formatMoney(manager.confirmedAmount, currencyCode, language),
            },
            ...(description.trim() ? [{ label: getMessage('Transfer', 'Description'), value: description.trim() }] : []),
            {
              label: getMessage('Transfer', 'BalanceAfter'),
              value: formatMoney(result.senderBalanceAfter, currencyCode, language),
            },
          ]}
        />
      </div>
      <div className={[layout.actions, 'no-print'].join(' ')}>
        <Button variant="outline" icon={<Printer size={18} aria-hidden="true" />} onClick={manager.printReceipt}>
          {getMessage('Transfer', 'PrintReceipt')}
        </Button>
        <Button variant="outline" onClick={manager.newTransfer}>
          {getMessage('Transfer', 'NewTransfer')}
        </Button>
        <Button onClick={manager.goHome}>{getMessage('Common', 'ReturnHome')}</Button>
      </div>
    </div>
  );
}
