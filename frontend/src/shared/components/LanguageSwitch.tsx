import type { Language } from '../../messaging/language';
import { useMessaging } from '../../messaging/useMessaging';
import styles from './LanguageSwitch.module.css';

const options: { language: Language; label: string; shortLabel: string }[] = [
  { language: 'tr', label: 'TÜRKÇE', shortLabel: 'TR' },
  { language: 'en', label: 'ENGLISH', shortLabel: 'EN' },
];

/** Dil seçimi. Karşılama ekranında büyük, üst barda kısa (TR / EN) gösterilir. */
export function LanguageSwitch({ compact = false }: { compact?: boolean }) {
  const { language, setLanguage } = useMessaging();

  return (
    <div className={[styles.group, compact ? styles.compact : ''].filter(Boolean).join(' ')} role="group" aria-label="Dil / Language">
      {options.map((option) => (
        <button
          key={option.language}
          type="button"
          lang={option.language}
          aria-pressed={language === option.language}
          className={[styles.option, language === option.language ? styles.selected : ''].filter(Boolean).join(' ')}
          onClick={() => setLanguage(option.language)}
        >
          {compact ? option.shortLabel : option.label}
        </button>
      ))}
    </div>
  );
}
