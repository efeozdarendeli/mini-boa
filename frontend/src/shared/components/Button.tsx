import type { ComponentProps, ReactNode } from 'react';
import styles from './Button.module.css';

type ButtonVariant = 'primary' | 'accent' | 'outline' | 'ghost';

// ComponentProps<'button'> React 19'da `ref`'i de içerir; Dialog açılınca odağı butona vermek için gerekli.
interface ButtonProps extends ComponentProps<'button'> {
  variant?: ButtonVariant;
  isLoading?: boolean;
  icon?: ReactNode;
  fullWidth?: boolean;
}

export function Button({
  variant = 'primary',
  isLoading = false,
  icon,
  fullWidth = false,
  disabled,
  children,
  className,
  type = 'button',
  ...rest
}: ButtonProps) {
  const classes = [styles.button, styles[variant], fullWidth ? styles.fullWidth : '', className ?? '']
    .filter(Boolean)
    .join(' ');

  return (
    <button type={type} className={classes} disabled={disabled || isLoading} aria-busy={isLoading} {...rest}>
      {isLoading ? <span className={styles.spinner} aria-hidden="true" /> : icon}
      {children !== undefined && <span>{children}</span>}
    </button>
  );
}
