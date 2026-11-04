import { useId, type ComponentProps } from 'react';
import styles from './TextField.module.css';

export interface SelectOption {
  value: string;
  label: string;
}

interface SelectProps extends ComponentProps<'select'> {
  label: string;
  options: SelectOption[];
  placeholder?: string;
  error?: string;
}

/** Etiketli açılır liste; TextField ile aynı görünümü kullanır. */
export function Select({ label, options, placeholder, error, id, className, ...rest }: SelectProps) {
  const generatedId = useId();
  const selectId = id ?? generatedId;
  const errorId = `${selectId}-error`;

  return (
    <div className={[styles.field, className ?? ''].filter(Boolean).join(' ')}>
      <label htmlFor={selectId} className={styles.label}>
        {label}
      </label>
      <select
        id={selectId}
        className={[styles.input, error ? styles.invalid : ''].filter(Boolean).join(' ')}
        aria-invalid={Boolean(error)}
        aria-describedby={error ? errorId : undefined}
        {...rest}
      >
        {placeholder !== undefined && <option value="">{placeholder}</option>}
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
      {error && (
        <span id={errorId} className={styles.error}>
          {error}
        </span>
      )}
    </div>
  );
}
