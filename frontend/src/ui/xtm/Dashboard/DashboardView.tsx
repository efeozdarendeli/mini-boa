import { ArrowRight, ChartLine, Eye, EyeOff, Landmark, ListOrdered, UserRound, Wallet } from 'lucide-react';
import { formatMessage } from '../../../messaging/messagingHelper';
import { useMessaging } from '../../../messaging/useMessaging';
import { Button } from '../../../shared/components/Button';
import { Card } from '../../../shared/components/Card';
import { QueryBoundary } from '../../../shared/components/QueryBoundary';
import { Tile } from '../../../shared/components/Tile';
import { formatDateTime, formatMoney, toUpperCase } from '../../../shared/format';
import styles from './DashboardView.module.css';
import { RecentTransactions } from './RecentTransactions';
import { useDashboardManager } from './useDashboardManager';

const MASKED_BALANCE = '****';

/** Ana sayfa: "Hoş geldin", "Hesaplarım" kartları ve işlem kutucukları. */
export function DashboardView() {
  const { getMessage, language } = useMessaging();
  const manager = useDashboardManager();

  return (
    <div className={styles.layout}>
      <QueryBoundary query={manager.summaryQuery}>
        {(summary) => (
          <div className={styles.column}>
            <Card className={styles.welcome}>
              <div>
                <p className={styles.hello}>{toUpperCase(getMessage('Dashboard', 'Hello'), language)}</p>
                <h1 className={styles.name}>{toUpperCase(summary.firstName, language)}</h1>
                <p className={styles.lastLogin}>
                  {summary.previousLoginAt
                    ? formatMessage(
                        getMessage('Dashboard', 'PreviousLogin'),
                        formatDateTime(summary.previousLoginAt, language),
                      )
                    : getMessage('Dashboard', 'FirstLogin')}
                </p>
                <button type="button" className={styles.link} onClick={manager.goToProfile}>
                  {getMessage('Dashboard', 'EditProfile')}
                </button>
              </div>
              <span className={styles.avatar} aria-hidden="true">
                <UserRound size={48} />
              </span>
            </Card>

            <Card className={styles.accounts}>
              <div className={styles.accountsHeader}>
                <h2>{toUpperCase(getMessage('Dashboard', 'MyAccounts'), language)}</h2>
                <Button
                  variant="ghost"
                  onClick={manager.toggleBalance}
                  aria-label={getMessage('Dashboard', manager.isBalanceVisible ? 'HideBalance' : 'ShowBalance')}
                  aria-pressed={manager.isBalanceVisible}
                  icon={manager.isBalanceVisible ? <EyeOff size={20} /> : <Eye size={20} />}
                />
              </div>

              <p className={styles.totalLabel}>{getMessage('Dashboard', 'TotalBalance')}</p>
              <p className={styles.total} data-testid="total-balance">
                {manager.isBalanceVisible ? formatMoney(summary.totalTryBalance, 'TRY', language) : MASKED_BALANCE}
              </p>

              <ul className={styles.accountList}>
                {summary.accounts.slice(0, 3).map((account) => (
                  <li key={account.accountId} className={styles.accountRow}>
                    <span>
                      {getMessage('Accounts', account.accountType)}
                      <small className={styles.accountNumber}>{account.accountNumber}</small>
                    </span>
                    <strong>
                      {manager.isBalanceVisible
                        ? formatMoney(account.balance, account.currencyCode, language)
                        : MASKED_BALANCE}
                    </strong>
                  </li>
                ))}
              </ul>

              <button type="button" className={styles.allAccounts} onClick={manager.goToAccounts}>
                {getMessage('Dashboard', 'AllAccounts')}
              </button>
            </Card>
          </div>
        )}
      </QueryBoundary>

      <div className={styles.column}>
        <section aria-label={getMessage('Dashboard', 'Operations')} className={styles.tiles}>
          <Tile
            label={getMessage('Dashboard', 'MoneyTransfer')}
            icon={<ArrowRight />}
            tone="blue"
            onClick={manager.goToTransfer}
          />
          <Tile label={getMessage('Dashboard', 'Accounts')} icon={<ListOrdered />} onClick={manager.goToAccounts} />
          <Tile
            label={getMessage('Dashboard', 'Rates')}
            icon={<ChartLine />}
            tone="accent"
            onClick={manager.goToRates}
          />
          <Tile
            label={getMessage('Dashboard', 'OpenCurrentAccount')}
            icon={<Wallet />}
            onClick={() => manager.openAccount('Current')}
          />
          <Tile
            label={getMessage('Dashboard', 'ParticipationAccount')}
            icon={<Landmark />}
            onClick={() => manager.openAccount('Participation')}
          />
          <Tile
            label={getMessage('Dashboard', 'Profile')}
            icon={<UserRound />}
            tone="red"
            onClick={manager.goToProfile}
          />
        </section>

        <RecentTransactions
          query={manager.recentTransactionsQuery}
          isAmountVisible={manager.isBalanceVisible}
          maskedAmount={MASKED_BALANCE}
          onShowAll={manager.goToAccounts}
        />
      </div>
    </div>
  );
}
