import { Headset, UserRound } from 'lucide-react';
import { useMessaging } from '../../../messaging/useMessaging';
import { BrandLogo } from '../../../shared/components/BrandLogo';
import { Button } from '../../../shared/components/Button';
import { Card } from '../../../shared/components/Card';
import { LanguageSwitch } from '../../../shared/components/LanguageSwitch';
import { useWelcomeManager } from './useWelcomeManager';
import styles from './WelcomeView.module.css';

/** Karşılama ekranı: dil seçimi, "Müşterisiyim → Giriş" ve "Müşteri Ol" kartları. */
export function WelcomeView() {
  const { getMessage } = useMessaging();
  const manager = useWelcomeManager();

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <BrandLogo subtitle={getMessage('Welcome', 'Greeting')} />
        <img src="/favicon.svg" alt="" className={styles.mark} />
        <div className={styles.headerActions}>
          <LanguageSwitch />
        </div>
      </header>

      <section className={styles.cards}>
        <Card className={styles.card}>
          <h2 className={styles.cardTitle}>{getMessage('Welcome', 'ExistingCustomerTitle')}</h2>
          <span className={[styles.avatar, styles.avatarPrimary].join(' ')} aria-hidden="true">
            <UserRound size={44} />
          </span>
          <p className={styles.description}>{getMessage('Welcome', 'ExistingCustomerDescription')}</p>
          <Button fullWidth onClick={manager.goToLogin}>
            {getMessage('Welcome', 'Login')}
          </Button>
        </Card>

        <Card className={styles.card}>
          <h2 className={styles.cardTitle}>{getMessage('Welcome', 'NewCustomerTitle')}</h2>
          <span className={[styles.avatar, styles.avatarAccent].join(' ')} aria-hidden="true">
            <Headset size={44} />
          </span>
          <p className={styles.description}>{getMessage('Welcome', 'NewCustomerDescription')}</p>
          <Button variant="accent" fullWidth onClick={manager.goToBecomeACustomer}>
            {getMessage('Welcome', 'BecomeACustomer')}
          </Button>
        </Card>
      </section>
    </main>
  );
}
