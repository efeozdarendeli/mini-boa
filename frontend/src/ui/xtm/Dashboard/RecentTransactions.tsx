import type { UseQueryResult } from '@tanstack/react-query';
import { ArrowDownLeft, ArrowUpRight } from 'lucide-react';
import { formatMessage } from '../../../messaging/messagingHelper';
import { useMessaging } from '../../../messaging/useMessaging';
import { Card } from '../../../shared/components/Card';
import { QueryBoundary } from '../../../shared/components/QueryBoundary';
import { formatDateTime, formatMoney } from '../../../shared/format';
import type { RecentTransactionContract } from '../../../types/contracts';
import styles from './RecentTransactions.module.css';

interface RecentTransactionsProps {
  query: UseQueryResult<RecentTransactionContract[]>;
  /** Tutarlar, ana sayfadaki bakiye gibi göz ikonuyla gösterilir/gizlenir. */
  isAmountVisible: boolean;
  maskedAmount: string;
  onShowAll: () => void;
}

/** Ana sayfadaki "Son İşlemler" kartı: bütün hesaplardaki son 10 hareket. */
export function RecentTransactions({ query, isAmountVisible, maskedAmount, onShowAll }: RecentTransactionsProps) {
  const { getMessage, language } = useMessaging();

  return (
    <Card className={styles.card}>
      <h2 className={styles.title}>{getMessage('Dashboard', 'RecentTransactions')}</h2>
      <QueryBoundary query={query}>
        {(transactions) =>
          transactions.length === 0 ? (
            <p className={styles.empty}>{getMessage('Dashboard', 'NoRecentTransactions')}</p>
          ) : (
            <ul className={styles.list} aria-label={getMessage('Dashboard', 'RecentTransactions')}>
              {transactions.map((transaction) => {
                const isIncoming = transaction.direction === 'Credit';
                const Icon = isIncoming ? ArrowDownLeft : ArrowUpRight;
                const counterparty = transaction.counterparty
                  ? formatMessage(getMessage('Dashboard', isIncoming ? 'From' : 'To'), transaction.counterparty)
                  : null;

                return (
                  <li key={transaction.transactionId} className={styles.row}>
                    <span className={[styles.icon, isIncoming ? styles.incoming : styles.outgoing].join(' ')}>
                      <Icon size={18} aria-hidden="true" />
                    </span>
                    <span className={styles.details}>
                      <span className={styles.description}>
                        {transaction.description ??
                          getMessage('Dashboard', isIncoming ? 'IncomingTransfer' : 'OutgoingTransfer')}
                      </span>
                      <small className={styles.meta}>
                        {[transaction.accountNumber, counterparty, formatDateTime(transaction.createdAt, language)]
                          .filter(Boolean)
                          .join(' · ')}
                      </small>
                    </span>
                    <strong className={[styles.amount, isIncoming ? styles.incoming : styles.outgoing].join(' ')}>
                      {isAmountVisible
                        ? `${isIncoming ? '+' : '−'}${formatMoney(transaction.amount, transaction.currencyCode, language)}`
                        : maskedAmount}
                    </strong>
                  </li>
                );
              })}
            </ul>
          )
        }
      </QueryBoundary>
      <button type="button" className={styles.showAll} onClick={onShowAll}>
        {getMessage('Dashboard', 'AllTransactions')}
      </button>
    </Card>
  );
}
