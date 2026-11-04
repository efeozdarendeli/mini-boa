import styles from './BrandLogo.module.css';

/** Demoya ait yazı logosu: kalın "Mini" + ince "Boa". */
export function BrandLogo({ subtitle }: { subtitle?: string }) {
  return (
    <div className={styles.brand}>
      <span className={styles.wordmark}>
        <strong>Mini</strong>Boa
      </span>
      {subtitle && <span className={styles.subtitle}>{subtitle}</span>}
    </div>
  );
}
