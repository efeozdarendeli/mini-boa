import { WifiOff } from 'lucide-react';
import { useState, useSyncExternalStore } from 'react';
import { useMessaging } from '../messaging/useMessaging';
import { getIsOnline, subscribeToConnection } from '../proxy/connectionStatus';
import { checkHealth } from '../proxy/executer';
import { Button } from '../shared/components/Button';
import styles from './ConnectionBanner.module.css';

/**
 * Sunucuya ulaşılamadığında sayfanın en üstünde görünen şerit. Executer bir istekte sunucuya
 * ulaşamayınca açılır, sonraki başarılı istekte kendiliğinden kapanır. "Tekrar dene" /health'e sorar.
 */
export function ConnectionBanner() {
  const { getMessage } = useMessaging();
  const isOnline = useSyncExternalStore(subscribeToConnection, getIsOnline);
  const [isChecking, setIsChecking] = useState(false);

  if (isOnline) {
    return null;
  }

  const retry = async () => {
    setIsChecking(true);
    await checkHealth();
    setIsChecking(false);
  };

  return (
    <div className={[styles.banner, 'no-print'].join(' ')} role="alert">
      <WifiOff size={20} aria-hidden="true" />
      <span className={styles.text}>{getMessage('Xtm', 'Offline')}</span>
      <Button variant="outline" onClick={() => void retry()} isLoading={isChecking}>
        {getMessage('Common', 'Retry')}
      </Button>
    </div>
  );
}
