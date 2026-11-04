import { useCallback, useState } from 'react';

export type FormErrors<T> = Partial<Record<keyof T, string>>;

export function hasErrors<T>(errors: FormErrors<T>): boolean {
  return Object.values(errors).some(Boolean);
}

/**
 * Form alanlarının değerleri ve hata mesajları. Bir alan değiştiğinde o alanın hatası silinir;
 * kullanıcı düzeltmeye başladığı anda kırmızı uyarı kaybolur.
 */
export function useFormState<T extends Record<string, string>>(initialValues: T) {
  const [values, setValues] = useState<T>(initialValues);
  const [errors, setErrors] = useState<FormErrors<T>>({});

  const setValue = useCallback(<K extends keyof T>(name: K, value: T[K]) => {
    setValues((current) => ({ ...current, [name]: value }));
    setErrors((current) => ({ ...current, [name]: undefined }));
  }, []);

  return { values, errors, setValue, setValues, setErrors };
}

export type FormState<T extends Record<string, string>> = ReturnType<typeof useFormState<T>>;
