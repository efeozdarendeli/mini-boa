import { useId, type InputHTMLAttributes } from 'react';
import styles from './TextField.module.css';

interface TextFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
  hint?: string;
}

/** Etiketli giriş alanı; hata mesajı ekran okuyuculara da bağlanır (aria-describedby). */
export function TextField({ label, error, hint, id, className, ...rest }: TextFieldProps) {
  const generatedId = useId();
  const inputId = id ?? generatedId;
  const messageId = `${inputId}-message`;
  const message = error ?? hint;

  return (
    <div className={[styles.field, className ?? ''].filter(Boolean).join(' ')}>
      <label htmlFor={inputId} className={styles.label}>
        {label}
      </label>
      <input
        id={inputId}
        className={[styles.input, error ? styles.invalid : ''].filter(Boolean).join(' ')}
        aria-invalid={Boolean(error)}
        aria-describedby={message ? messageId : undefined}
        {...rest}
      />
      {message && (
        <span id={messageId} className={error ? styles.error : styles.hint}>
          {message}
        </span>
      )}
    </div>
  );
}
