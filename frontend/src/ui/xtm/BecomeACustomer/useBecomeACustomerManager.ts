import { useState } from 'react';
import { useNavigate } from 'react-router';
import { Routes } from '../../../app/routes';
import { DialogTypes, useContainerManager } from '../../../container/containerContext';
import { useMessaging, type Translate } from '../../../messaging/useMessaging';
import { Executer } from '../../../proxy/executer';
import { getErrorMessage } from '../../../proxy/responseHelpers';
import { toTitleCase } from '../../../shared/format';
import { traceUtil } from '../../../shared/traceUtil';
import { hasErrors, useFormState, type FormErrors } from '../../../shared/useFormState';
import { useLocationOptions } from '../../../shared/useLocationOptions';
import {
  isDigits,
  isValidEmail,
  isValidIdentityNumber,
  isValidMobilePhone,
  isValidPastDate,
  lettersOnly,
  onlyDigits,
  splitMobilePhone,
} from '../../../shared/validation';
import type { CreateCustomerResultContract } from '../../../types/contracts';
import type { LoginLocationState } from '../Login/useLoginManager';

export type BecomeACustomerStep = 'identity' | 'details' | 'completed';

type IdentityForm = { identityNumber: string; phone: string };

type DetailsForm = {
  firstName: string;
  lastName: string;
  birthDate: string;
  email: string;
  cityId: string;
  countyId: string;
  addressLine: string;
  password: string;
  passwordRepeat: string;
};

const emptyIdentity: IdentityForm = { identityNumber: '', phone: '' };

const emptyDetails: DetailsForm = {
  firstName: '',
  lastName: '',
  birthDate: '',
  email: '',
  cityId: '',
  countyId: '',
  addressLine: '',
  password: '',
  passwordRepeat: '',
};

function validateIdentity(values: IdentityForm, t: Translate): FormErrors<IdentityForm> {
  return {
    identityNumber: isValidIdentityNumber(values.identityNumber) ? undefined : t('Validation', 'IdentityNumber'),
    phone: isValidMobilePhone(values.phone) ? undefined : t('Validation', 'MobilePhone'),
  };
}

function validateDetails(values: DetailsForm, t: Translate): FormErrors<DetailsForm> {
  const required = (value: string) => (value.trim() ? undefined : t('Validation', 'Required'));

  return {
    firstName: required(values.firstName),
    lastName: required(values.lastName),
    birthDate: isValidPastDate(values.birthDate) ? undefined : t('Validation', 'BirthDate'),
    email: !values.email || isValidEmail(values.email) ? undefined : t('Validation', 'Email'),
    cityId: required(values.cityId),
    countyId: required(values.countyId),
    addressLine: required(values.addressLine),
    password: isDigits(values.password, 6) ? undefined : t('Validation', 'Password'),
    passwordRepeat: values.password === values.passwordRepeat ? undefined : t('Validation', 'PasswordMismatch'),
  };
}

/**
 * "Müşteri Ol" akışının mantığı (BOA'daki Xtm.UI.BecomeACustomer.BecomeACustomerManager karşılığı).
 * 1. adım: TCKN + GSM → backend karar verir (GoCardLogin / NewCustomer / ContinueApplication).
 * 2. adım: kişisel bilgi, adres, şifre → müşteri ve ilk hesap oluşturulur.
 */
export function useBecomeACustomerManager() {
  const navigate = useNavigate();
  const containerManager = useContainerManager();
  const { getMessage, language } = useMessaging();

  const [step, setStep] = useState<BecomeACustomerStep>('identity');
  const [application, setApplication] = useState<{ id: number; isContinuation: boolean } | null>(null);
  const [result, setResult] = useState<CreateCustomerResultContract | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const identityForm = useFormState<IdentityForm>(emptyIdentity);
  const detailsForm = useFormState<DetailsForm>(emptyDetails);

  // İl ve ilçe listeleri önbellekten gelir (Client Caching); il seçilmeden ilçe istenmez.
  const { cityOptions, countyOptions } = useLocationOptions(Number(detailsForm.values.cityId));

  const submitIdentity = async () => {
    const errors = validateIdentity(identityForm.values, getMessage);
    if (hasErrors(errors)) {
      identityForm.setErrors(errors);
      return;
    }

    setIsSubmitting(true);
    // Ekranda tek alan ("0532 111 22 33"); backend'in contract'ı alan kodu + numara bekliyor.
    const response = await Executer.customerDecisionByTaxNumber({
      identityNumber: identityForm.values.identityNumber,
      ...splitMobilePhone(identityForm.values.phone),
    });
    setIsSubmitting(false);

    if (!response.success || !response.value) {
      containerManager.showDialog(getErrorMessage(response), DialogTypes.Warning);
      return;
    }

    const { kycProcessType, applicationId } = response.value;
    traceUtil.trace(`KYCProcessType ${kycProcessType}`);

    // BOA: if (kYCProcessType == KYCCustomerInfoContract.KYCProcessTypeEnum.GoCardLogin)
    if (kycProcessType === 'GoCardLogin') {
      const state: LoginLocationState = {
        identityNumber: identityForm.values.identityNumber,
        notice: 'AlreadyCustomer',
      };
      navigate(Routes.Login, { state });
      return;
    }

    setApplication({ id: applicationId!, isContinuation: kycProcessType === 'ContinueApplication' });
    setStep('details');
  };

  const submitDetails = async () => {
    const errors = validateDetails(detailsForm.values, getMessage);
    if (hasErrors(errors) || !application) {
      detailsForm.setErrors(errors);
      return;
    }

    const values = detailsForm.values;
    setIsSubmitting(true);
    const response = await Executer.createCustomer({
      applicationId: application.id,
      identityNumber: identityForm.values.identityNumber,
      firstName: toTitleCase(values.firstName, language),
      lastName: toTitleCase(values.lastName, language),
      birthDate: values.birthDate,
      email: values.email.trim() || undefined,
      cityId: Number(values.cityId),
      countyId: Number(values.countyId),
      addressLine: values.addressLine.trim(),
      password: values.password,
    });
    setIsSubmitting(false);

    if (!response.success || !response.value) {
      containerManager.showDialog(getErrorMessage(response), DialogTypes.Warning);
      return;
    }

    setResult(response.value);
    setStep('completed');
  };

  const selectCity = (value: string) => {
    detailsForm.setValue('cityId', value);
    detailsForm.setValue('countyId', '');
  };

  /** Ad/soyad: yazarken sadece harf kabul edilir, alandan çıkınca baş harfler büyütülür. */
  const setName = (name: 'firstName' | 'lastName', value: string) => detailsForm.setValue(name, lettersOnly(value));
  const formatName = (name: 'firstName' | 'lastName') =>
    detailsForm.setValue(name, toTitleCase(detailsForm.values[name], language));

  const goToLogin = () => {
    const state: LoginLocationState = { identityNumber: identityForm.values.identityNumber, notice: 'CustomerCreated' };
    navigate(Routes.Login, { state });
  };

  const back = () => (step === 'details' ? setStep('identity') : navigate(Routes.Welcome));

  return {
    step,
    stepIndex: ['identity', 'details', 'completed'].indexOf(step),
    application,
    result,
    isSubmitting,
    identityForm,
    detailsForm,
    cityOptions,
    countyOptions,
    setIdentityNumber: (value: string) => identityForm.setValue('identityNumber', onlyDigits(value)),
    setPhone: (digits: string) => identityForm.setValue('phone', digits),
    setName,
    formatName,
    selectCity,
    submitIdentity,
    submitDetails,
    goToLogin,
    back,
  };
}

export type BecomeACustomerManager = ReturnType<typeof useBecomeACustomerManager>;
