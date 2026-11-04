import { ArrowLeft } from 'lucide-react';
import type { ReactNode } from 'react';
import { useMessaging } from '../../messaging/useMessaging';
import styles from './PageHeader.module.css';

interface PageHeaderProps {
  title: string;
  onBack?: () => void;
  actions?: ReactNode;
}

/** Ekran başlığı; solda geri butonu, sağda isteğe bağlı işlem butonları. */
export function PageHeader({ title, onBack, actions }: PageHeaderProps) {
  const { getMessage } = useMessaging();

  return (
    <div className={styles.header}>
      {onBack && (
        <button type="button" className={styles.back} onClick={onBack} aria-label={getMessage('Common', 'Back')}>
          <ArrowLeft size={22} aria-hidden="true" />
        </button>
      )}
      <h1 className={styles.title}>{title}</h1>
      {actions && <div className={styles.actions}>{actions}</div>}
    </div>
  );
}
