import { CircleAlert, CircleCheck, CircleX, Info } from 'lucide-react';
import { useEffect, useId, useRef } from 'react';
import { useMessaging } from '../messaging/useMessaging';
import { Button } from '../shared/components/Button';
import type { DialogType } from './containerContext';
import styles from './Dialog.module.css';

const icons: Record<DialogType, typeof Info> = {
  Info,
  Warning: CircleAlert,
  Error: CircleX,
  Success: CircleCheck,
};

interface DialogProps {
  message: string;
  type: DialogType;
  onClose: () => void;
}

/** Ekranın üstünde açılan mesaj kutusu. Escape ile veya butonla kapanır; açılınca odak butona gelir. */
export function Dialog({ message, type, onClose }: DialogProps) {
  const { getMessage } = useMessaging();
  const titleId = useId();
  const closeButtonRef = useRef<HTMLButtonElement>(null);
  const Icon = icons[type];

  useEffect(() => {
    closeButtonRef.current?.focus();

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        onClose();
      }
    };
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [onClose]);

  return (
    <div className={styles.backdrop} onClick={onClose}>
      <div
        role="alertdialog"
        aria-modal="true"
        aria-labelledby={titleId}
        className={styles.dialog}
        onClick={(event) => event.stopPropagation()}
      >
        <Icon className={[styles.icon, styles[type]].join(' ')} size={44} aria-hidden="true" />
        <h2 id={titleId} className={styles.title}>
          {getMessage('Dialog', type)}
        </h2>
        <p className={styles.message}>{message}</p>
        <Button ref={closeButtonRef} onClick={onClose} fullWidth>
          {getMessage('Common', 'Close')}
        </Button>
      </div>
    </div>
  );
}
