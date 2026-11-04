import type { ReactNode } from 'react';
import styles from './InfoList.module.css';

/** "Etiket: değer" satırları; onay ve sonuç ekranlarında kullanılır. */
export function InfoList({ items }: { items: { label: string; value: ReactNode }[] }) {
  return (
    <dl className={styles.list}>
      {items.map((item) => (
        <div key={item.label} className={styles.row}>
          <dt className={styles.label}>{item.label}</dt>
          <dd className={styles.value}>{item.value}</dd>
        </div>
      ))}
    </dl>
  );
}
