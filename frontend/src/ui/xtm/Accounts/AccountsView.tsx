import { ArrowDownLeft, ArrowUpRight, Copy, Plus } from 'lucide-react';
import { formatMessage } from '../../../messaging/messagingHelper';
import { useMessaging } from '../../../messaging/useMessaging';
import { Alert } from '../../../shared/components/Alert';
import { Button } from '../../../shared/components/Button';
import { Card } from '../../../shared/components/Card';
import { PageHeader } from '../../../shared/components/PageHeader';
import { QueryBoundary } from '../../../shared/components/QueryBoundary';
import { Select } from '../../../shared/components/Select';
import { formatDateTime, formatIban, formatMoney } from '../../../shared/format';
import styles from './AccountsView.module.css';
import { TRANSACTION_PERIODS, useAccountsManager, type DirectionFilter } from './useAccountsManager';

/** Hesaplarım: hesap kartları ve seçili hesabın hareketleri. */
export function AccountsView() {
  const { getMessage, language } = useMessaging();
  const manager = useAccountsManager();

  return (
    <div>
      <PageHeader
        title={getMessage('Accounts', 'Title')}
        onBack={manager.goBack}
        actions={
          <Button icon={<Plus size={18} aria-hidden="true" />} onClick={manager.goToOpenAccount}>
            {getMessage('Accounts', 'OpenNew')}
          </Button>
        }
      />

      {manager.openedAccountNumber && (
        <div className={styles.notice}>
          <Alert tone="success">
            {formatMessage(getMessage('Accounts', 'Opened'), manager.openedAccountNumber)}
          </Alert>
        </div>
      )}

      <div className={styles.layout}>
        <QueryBoundary query={manager.accountsQuery}>
          {(accounts) => (
            <ul className={styles.accountList}>
              {accounts.map((account) => {
                const isSelected = account.accountId === manager.selectedAccountId;
                return (
                  <li key={account.accountId}>
                    <Card className={[styles.account, isSelected ? styles.selected : ''].filter(Boolean).join(' ')}>
                      <button
                        type="button"
                        className={styles.accountButton}
                        aria-pressed={isSelected}
                        onClick={() => manager.selectAccount(account.accountId)}
                      >
                        <span className={styles.accountType}>
                          {getMessage('Accounts', account.accountType)} · {account.currencyCode}
                        </span>
                        <span className={styles.accountNumber}>{account.accountNumber}</span>
                        <span className={styles.balance}>
                          {formatMoney(account.balance, account.currencyCode, language)}
                        </span>
                      </button>
                      <div className={styles.ibanRow}>
                        <code className={styles.iban}>{formatIban(account.iban)}</code>
                        <Button
                          variant="ghost"
                          icon={<Copy size={16} aria-hidden="true" />}
                          onClick={() => void manager.copyIban(account.iban)}
                          aria-label={getMessage('Accounts', 'CopyIban')}
                        >
                          {manager.copiedIban === account.iban
                            ? getMessage('Common', 'Copied')
                            : getMessage('Common', 'Copy')}
                        </Button>
                      </div>
                    </Card>
                  </li>
                );
              })}
            </ul>
          )}
        </QueryBoundary>

        <Card className={styles.transactions}>
          <h2 className={styles.transactionsTitle}>{getMessage('Accounts', 'Transactions')}</h2>
          <div className={styles.filters}>
            <Select
              label={getMessage('Accounts', 'Period')}
              value={String(manager.days)}
              onChange={(event) => manager.setDays(Number(event.target.value))}
              options={TRANSACTION_PERIODS.map((days) => ({
                value: String(days),
                label: formatMessage(getMessage('Accounts', 'LastDays'), days),
              }))}
            />
            <Select
              label={getMessage('Accounts', 'Direction')}
              value={manager.direction}
              onChange={(event) => manager.setDirection(event.target.value as DirectionFilter)}
              options={[
                { value: 'All', label: getMessage('Accounts', 'AllDirections') },
                { value: 'Credit', label: getMessage('Accounts', 'Incoming') },
                { value: 'Debit', label: getMessage('Accounts', 'Outgoing') },
              ]}
            />
          </div>
          {manager.selectedAccount !== null && (
            <QueryBoundary query={manager.transactionsQuery}>
              {(transactions) =>
                transactions.length === 0 ? (
                  <p className={styles.empty}>{getMessage('Accounts', 'NoTransactions')}</p>
                ) : (
                  <ul className={styles.transactionList}>
                    {transactions.map((transaction) => {
                      const isDebit = transaction.direction === 'Debit';
                      const Icon = isDebit ? ArrowUpRight : ArrowDownLeft;
                      return (
                        <li key={transaction.transactionId} className={styles.transaction}>
                          <Icon
                            size={20}
                            className={isDebit ? styles.debit : styles.credit}
                            aria-hidden="true"
                          />
                          <span className={styles.transactionText}>
                            {transaction.description ?? '—'}
                            <small>{formatDateTime(transaction.createdAt, language)}</small>
                          </span>
                          <strong className={isDebit ? styles.debit : styles.credit}>
                            {isDebit ? '−' : '+'}
                            {formatMoney(transaction.amount, manager.selectedAccount!.currencyCode, language)}
                          </strong>
                        </li>
                      );
                    })}
                  </ul>
                )
              }
            </QueryBoundary>
          )}
        </Card>
      </div>
    </div>
  );
}
