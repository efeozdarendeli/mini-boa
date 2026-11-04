import styles from './StepIndicator.module.css';

/** Çok adımlı akışlarda (Müşteri Ol) hangi adımda olunduğunu gösterir. */
export function StepIndicator({ steps, currentIndex }: { steps: string[]; currentIndex: number }) {
  return (
    <ol className={styles.steps}>
      {steps.map((step, index) => (
        <li
          key={step}
          className={[
            styles.step,
            index < currentIndex ? styles.done : '',
            index === currentIndex ? styles.current : '',
          ]
            .filter(Boolean)
            .join(' ')}
          aria-current={index === currentIndex ? 'step' : undefined}
        >
          <span className={styles.number}>{index + 1}</span>
          <span className={styles.label}>{step}</span>
        </li>
      ))}
    </ol>
  );
}
