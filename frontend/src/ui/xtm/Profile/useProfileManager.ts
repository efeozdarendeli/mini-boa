import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useNavigate } from 'react-router';
import { Routes } from '../../../app/routes';
import { DialogTypes, useContainerManager } from '../../../container/containerContext';
import { useMessaging, type Translate } from '../../../messaging/useMessaging';
import { accountQueries } from '../../../proxy/accountQueries';
import { Executer } from '../../../proxy/executer';
import { getErrorMessage } from '../../../proxy/responseHelpers';
import { hasErrors, useFormState, type FormErrors } from '../../../shared/useFormState';
import { useLocationOptions } from '../../../shared/useLocationOptions';
import { isValidEmail } from '../../../shared/validation';
import type { ProfileContract } from '../../../types/contracts';

type ContactForm = { email: string; cityId: string; countyId: string; addressLine: string };

function toForm(profile: ProfileContract): ContactForm {
  return {
    email: profile.email ?? '',
    cityId: profile.cityId ? String(profile.cityId) : '',
    countyId: profile.countyId ? String(profile.countyId) : '',
    addressLine: profile.addressLine ?? '',
  };
}

function validate(values: ContactForm, t: Translate): FormErrors<ContactForm> {
  const required = (value: string) => (value.trim() ? undefined : t('Validation', 'Required'));

  return {
    email: !values.email || isValidEmail(values.email) ? undefined : t('Validation', 'Email'),
    cityId: required(values.cityId),
    countyId: required(values.countyId),
    addressLine: required(values.addressLine),
  };
}

/**
 * Profilim ekranının mantığı. Form ilk açılışta sunucudaki bilgilerle doldurulur;
 * kullanıcı bir alana dokununca o andan itibaren kendi girdiği değerler kullanılır.
 */
export function useProfileManager() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const containerManager = useContainerManager();
  const { getMessage } = useMessaging();

  const profileQuery = useQuery(accountQueries.profile());
  const form = useFormState<ContactForm>({ email: '', cityId: '', countyId: '', addressLine: '' });
  const [isEdited, setIsEdited] = useState(false);
  const [isSaved, setIsSaved] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Düzenleme başlamadıysa sunucudaki değerler gösterilir; effect ile kopyalamaya gerek kalmaz.
  const values = isEdited || !profileQuery.data ? form.values : toForm(profileQuery.data);
  const { cityOptions, countyOptions } = useLocationOptions(Number(values.cityId));

  const setField = (name: keyof ContactForm, value: string) => {
    if (!isEdited) {
      form.setValues(values);
      setIsEdited(true);
    }
    form.setValue(name, value);
    if (name === 'cityId') {
      form.setValue('countyId', '');
    }
    setIsSaved(false);
  };

  const save = async () => {
    const errors = validate(values, getMessage);
    if (hasErrors(errors)) {
      form.setErrors(errors);
      return;
    }

    setIsSubmitting(true);
    const response = await Executer.updateProfile({
      email: values.email.trim() || undefined,
      cityId: Number(values.cityId),
      countyId: Number(values.countyId),
      addressLine: values.addressLine.trim(),
    });
    setIsSubmitting(false);

    if (!response.success || !response.value) {
      containerManager.showDialog(getErrorMessage(response), DialogTypes.Warning);
      return;
    }

    queryClient.setQueryData(accountQueries.profile().queryKey, response.value);
    setIsEdited(false);
    setIsSaved(true);
  };

  return {
    profileQuery,
    values,
    errors: form.errors,
    cityOptions,
    countyOptions,
    isSaved,
    isSubmitting,
    setField,
    save,
    goBack: () => navigate(Routes.Dashboard),
  };
}
