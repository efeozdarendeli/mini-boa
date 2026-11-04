import { useState } from 'react';
import { useNavigate } from 'react-router';
import { Routes } from '../../../app/routes';
import { DialogTypes, useContainerManager } from '../../../container/containerContext';
import { useMessaging, type Translate } from '../../../messaging/useMessaging';
import { Executer } from '../../../proxy/executer';
import { getErrorMessage } from '../../../proxy/responseHelpers';
import { hasErrors, useFormState, type FormErrors } from '../../../shared/useFormState';
import {
  isDigits,
  isValidIdentityNumber,
  isValidMobilePhone,
  isValidPastDate,
  onlyDigits,
  splitMobilePhone,
} from '../../../shared/validation';
import type { PasswordResetIdentityContract } from '../../../types/contracts';
import type { LoginLocationState } from '../Login/useLoginManager';

export type ForgotPasswordStep = 'identity' | 'password';

type IdentityForm = { identityNumber: string; phone: string; birthDate: string };
type PasswordForm = { newPassword: string; newPasswordRepeat: string };

function validateIdentity(values: IdentityForm, t: Translate): FormErrors<IdentityForm> {
  return {
    identityNumber: isValidIdentityNumber(values.identityNumber) ? undefined : t('Validation', 'IdentityNumber'),
    phone: isValidMobilePhone(values.phone) ? undefined : t('Validation', 'MobilePhone'),
    birthDate: isValidPastDate(values.birthDate) ? undefined : t('Validation', 'BirthDate'),
  };
}

function validatePassword(values: PasswordForm, t: Translate): FormErrors<PasswordForm> {
  return {
    newPassword: isDigits(values.newPassword, 6) ? undefined : t('Validation', 'Password'),
    newPasswordRepeat:
      values.newPassword === values.newPasswordRepeat ? undefined : t('Validation', 'PasswordMismatch'),
  };
}

/**
 * "Şifremi unuttum" akışı: kimlik doğrulama → yeni şifre → giriş ekranı.
 * Gerçek bankada araya SMS ile tek kullanımlık kod girer; demoda kimlik, kayıtlı telefon ve doğum tarihi
 * birlikte eşleşmeli. Yanlış denemeler giriş kilidine sayılır (3 denemede 5 dakika).
 */
export function useForgotPasswordManager() {
  const navigate = useNavigate();
  const containerManager = useContainerManager();
  const { getMessage } = useMessaging();

  const [step, setStep] = useState<ForgotPasswordStep>('identity');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const identityForm = useFormState<IdentityForm>({ identityNumber: '', phone: '', birthDate: '' });
  const passwordForm = useFormState<PasswordForm>({ newPassword: '', newPasswordRepeat: '' });

  const identityContract = (): PasswordResetIdentityContract => ({
    identityNumber: identityForm.values.identityNumber,
    ...splitMobilePhone(identityForm.values.phone),
    birthDate: identityForm.values.birthDate,
  });

  const submitIdentity = async () => {
    const errors = validateIdentity(identityForm.values, getMessage);
    if (hasErrors(errors)) {
      identityForm.setErrors(errors);
      return;
    }

    setIsSubmitting(true);
    const response = await Executer.verifyIdentityForPasswordReset(identityContract());
    setIsSubmitting(false);

    if (!response.success) {
      containerManager.showDialog(getErrorMessage(response), DialogTypes.Warning);
      return;
    }

    setStep('password');
  };

  const submitPassword = async () => {
    const errors = validatePassword(passwordForm.values, getMessage);
    if (hasErrors(errors)) {
      passwordForm.setErrors(errors);
      return;
    }

    setIsSubmitting(true);
    const response = await Executer.resetPassword({
      ...identityContract(),
      newPassword: passwordForm.values.newPassword,
    });
    setIsSubmitting(false);

    if (!response.success) {
      containerManager.showDialog(getErrorMessage(response), DialogTypes.Warning);
      return;
    }

    const state: LoginLocationState = {
      identityNumber: identityForm.values.identityNumber,
      notice: 'PasswordChanged',
    };
    navigate(Routes.Login, { state });
  };

  return {
    step,
    stepIndex: step === 'identity' ? 0 : 1,
    isSubmitting,
    identityForm,
    passwordForm,
    setIdentityNumber: (value: string) => identityForm.setValue('identityNumber', onlyDigits(value)),
    submitIdentity,
    submitPassword,
    back: () => (step === 'password' ? setStep('identity') : navigate(Routes.Login)),
  };
}
