import type { ComponentProps, ReactNode } from 'react';
import type { Language } from '../../messaging/language';
import { formatAmountInput } from '../format';
import { parseAmount } from '../validation';
import { TextField } from './TextField';
import styles from './AmountField.module.css';

interface AmountFieldProps extends Omit<ComponentProps<typeof TextField>, 'value' | 'onChange'> {
  value: string;
  onChange: (value: string) => void;
  language: Language;
  /** Alanın sağında gösterilecek buton (örn. "Tüm bakiye"). */
  action?: ReactNode;
}

/**
 * Tutar alanı. Kullanıcı serbestçe yazar ("1250,5"); alandan çıkınca okunaklı biçime getirilir ("1.250,50").
 * Yazarken biçimlendirmek imleci zıplattığı için biçim alandan çıkınca uygulanır.
 */
export function AmountField({ value, onChange, language, action, ...rest }: AmountFieldProps) {
  const handleBlur = () => {
    const amount = parseAmount(value);
    if (amount !== null) {
      onChange(formatAmountInput(amount, language));
    }
  };

  return (
    <div className={styles.wrapper}>
      <TextField
        {...rest}
        inputMode="decimal"
        placeholder={language === 'tr' ? '0,00' : '0.00'}
        value={value}
        onChange={(event) => onChange(event.target.value.replace(/[^\d.,]/g, ''))}
        onBlur={handleBlur}
        className={styles.field}
      />
      {action && <div className={styles.action}>{action}</div>}
    </div>
  );
}
