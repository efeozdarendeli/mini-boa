import { useState } from 'react';
import { useLocation, useNavigate } from 'react-router';
import { Routes } from '../../../app/routes';
import { DialogTypes, useContainerManager } from '../../../container/containerContext';
import { useMessaging, type Translate } from '../../../messaging/useMessaging';
import { Executer } from '../../../proxy/executer';
import { getErrorMessage } from '../../../proxy/responseHelpers';
import { hasExpiredNotice, startSession } from '../../../proxy/session';
import { hasErrors, useFormState, type FormErrors } from '../../../shared/useFormState';
import { isDigits, isValidIdentityNumber, onlyDigits } from '../../../shared/validation';

/** Başka ekranlardan giriş ekranına gelirken taşınan bilgi (örn. Müşteri Ol → "zaten müşterisiniz"). */
export interface LoginLocationState {
  identityNumber?: string;
  notice?: 'AlreadyCustomer' | 'CustomerCreated' | 'PasswordChanged';
}

type LoginForm = { identityNumber: string; password: string };

function validate(values: LoginForm, t: Translate): FormErrors<LoginForm> {
  return {
    identityNumber: isValidIdentityNumber(values.identityNumber) ? undefined : t('Validation', 'IdentityNumber'),
    password: isDigits(values.password, 6) ? undefined : t('Validation', 'Password'),
  };
}

/** Giriş ekranının mantığı. */
export function useLoginManager() {
  const navigate = useNavigate();
  const location = useLocation();
  const containerManager = useContainerManager();
  const { getMessage } = useMessaging();

  const locationState = (location.state ?? {}) as LoginLocationState;
  const form = useFormState<LoginForm>({ identityNumber: locationState.identityNumber ?? '', password: '' });
  const [isSubmitting, setIsSubmitting] = useState(false);

  const notice = hasExpiredNotice()
    ? { tone: 'warning' as const, text: getMessage('Xtm', 'SessionExpired') }
    : locationState.notice
      ? { tone: 'info' as const, text: getMessage('Login', locationState.notice) }
      : null;

  const submit = async () => {
    const errors = validate(form.values, getMessage);
    if (hasErrors(errors)) {
      form.setErrors(errors);
      return;
    }

    setIsSubmitting(true);
    const response = await Executer.login(form.values);
    setIsSubmitting(false);

    if (!response.success || !response.value?.token) {
      form.setValue('password', '');
      containerManager.showDialog(getErrorMessage(response), DialogTypes.Warning);
      return;
    }

    const { token, customerNo, firstName, lastName } = response.value;
    startSession({ token, customerNo, firstName, lastName });
    navigate(Routes.Dashboard, { replace: true });
  };

  return {
    form,
    notice,
    isSubmitting,
    setIdentityNumber: (value: string) => form.setValue('identityNumber', onlyDigits(value)),
    setPassword: (value: string) => form.setValue('password', onlyDigits(value)),
    submit,
    goToBecomeACustomer: () => navigate(Routes.BecomeACustomer),
    goToForgotPassword: () => navigate(Routes.ForgotPassword),
  };
}
