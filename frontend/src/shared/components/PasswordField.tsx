import { Eye, EyeOff } from 'lucide-react';
import { useState, type ComponentProps } from 'react';
import { useMessaging } from '../../messaging/useMessaging';
import { onlyDigits } from '../validation';
import styles from './PasswordField.module.css';
import { TextField } from './TextField';

interface PasswordFieldProps extends Omit<ComponentProps<typeof TextField>, 'value' | 'onChange' | 'type'> {
  value: string;
  onChange: (digits: string) => void;
}

/** 6 haneli sayısal şifre alanı; göz ikonuyla yazılan şifre görülebilir. */
export function PasswordField({ value, onChange, ...rest }: PasswordFieldProps) {
  const { getMessage } = useMessaging();
  const [isVisible, setIsVisible] = useState(false);

  return (
    <div className={styles.wrapper}>
      <TextField
        {...rest}
        type={isVisible ? 'text' : 'password'}
        inputMode="numeric"
        maxLength={6}
        value={value}
        onChange={(event) => onChange(onlyDigits(event.target.value))}
      />
      <button
        type="button"
        className={styles.toggle}
        onClick={() => setIsVisible((visible) => !visible)}
        aria-label={getMessage('Common', isVisible ? 'HidePassword' : 'ShowPassword')}
        aria-pressed={isVisible}
      >
        {isVisible ? <EyeOff size={20} aria-hidden="true" /> : <Eye size={20} aria-hidden="true" />}
      </button>
    </div>
  );
}
