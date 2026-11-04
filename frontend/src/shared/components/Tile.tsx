import type { ReactNode } from 'react';
import styles from './Tile.module.css';

type TileTone = 'primary' | 'accent' | 'blue' | 'red';

interface TileProps {
  label: string;
  icon: ReactNode;
  tone?: TileTone;
  onClick: () => void;
}

/** Ana sayfadaki işlem kutucukları (Para Transferi, Kurlar...). */
export function Tile({ label, icon, tone = 'primary', onClick }: TileProps) {
  return (
    <button type="button" className={styles.tile} onClick={onClick}>
      <span className={[styles.icon, styles[tone]].join(' ')} aria-hidden="true">
        {icon}
      </span>
      <span className={styles.label}>{label}</span>
    </button>
  );
}
