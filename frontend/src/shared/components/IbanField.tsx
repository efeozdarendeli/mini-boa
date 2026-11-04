import type { ComponentProps } from 'react';
import { formatIban } from '../format';
import { onlyDigits } from '../validation';
import { TextField } from './TextField';

interface IbanFieldProps extends Omit<ComponentProps<typeof TextField>, 'value' | 'onChange'> {
  /** Boşluksuz IBAN: "TR060099900000100000020001" */
  value: string;
  onChange: (iban: string) => void;
}

/**
 * IBAN alanı. "TR" her zaman başta durur, kullanıcı sadece rakamları yazar; rakamlar 4'erli gruplanır.
 * Başka bir yerden kopyalanan "tr06 0099 ..." gibi bir değer de yapıştırılınca doğru biçime getirilir.
 */
export function IbanField({ value, onChange, ...rest }: IbanFieldProps) {
  const handleChange = (input: string) => {
    // Yapıştırılan değer "TR" ile başlıyorsa onu atla; geri kalan rakamları al (en fazla 24).
    const digits = onlyDigits(input.replace(/^\s*tr/i, '')).slice(0, 24);
    onChange(`TR${digits}`);
  };

  return (
    <TextField
      {...rest}
      inputMode="numeric"
      autoComplete="off"
      spellCheck={false}
      placeholder="TR00 0000 0000 0000 0000 0000 00"
      value={formatIban(value || 'TR')}
      onChange={(event) => handleChange(event.target.value)}
    />
  );
}
