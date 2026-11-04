import type { ComponentProps } from 'react';
import { formatMobilePhone } from '../format';
import { onlyDigits } from '../validation';
import { TextField } from './TextField';

interface PhoneFieldProps extends Omit<ComponentProps<typeof TextField>, 'value' | 'onChange'> {
  /** Sadece rakamlar: "05321112233" */
  value: string;
  onChange: (digits: string) => void;
}

/**
 * Cep telefonu alanı. Kullanıcı yazarken "0532 111 22 33" biçiminde gösterilir;
 * dışarıya sadece rakamlar verilir. "5" ile başlanırsa başına otomatik "0" eklenir.
 */
export function PhoneField({ value, onChange, ...rest }: PhoneFieldProps) {
  const handleChange = (input: string) => {
    let digits = onlyDigits(input);
    if (digits.startsWith('5')) {
      digits = `0${digits}`;
    }
    onChange(digits.slice(0, 11));
  };

  return (
    <TextField
      {...rest}
      type="tel"
      inputMode="numeric"
      autoComplete="tel-national"
      placeholder="05xx xxx xx xx"
      value={formatMobilePhone(value)}
      onChange={(event) => handleChange(event.target.value)}
    />
  );
}
