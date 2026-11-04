import { CircleAlert, CircleCheck, CircleX, Info } from 'lucide-react';
import type { ReactNode } from 'react';
import styles from './Alert.module.css';

type AlertTone = 'info' | 'warning' | 'success' | 'error';

const icons = { info: Info, warning: CircleAlert, success: CircleCheck, error: CircleX };

interface AlertProps {
  tone: AlertTone;
  children: ReactNode;
  action?: ReactNode;
}

/** Ekranın içinde gösterilen bilgi/uyarı şeridi. Uyarı ve hatalar ekran okuyucuya hemen okunur (role="alert"). */
export function Alert({ tone, children, action }: AlertProps) {
  const Icon = icons[tone];

  return (
    <div className={[styles.alert, styles[tone]].join(' ')} role={tone === 'error' || tone === 'warning' ? 'alert' : 'status'}>
      <Icon size={22} className={styles.icon} aria-hidden="true" />
      <div className={styles.content}>{children}</div>
      {action}
    </div>
  );
}
