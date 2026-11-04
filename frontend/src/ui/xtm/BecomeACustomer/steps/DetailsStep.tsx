import { useMessaging } from '../../../../messaging/useMessaging';
import { Alert } from '../../../../shared/components/Alert';
import { Button } from '../../../../shared/components/Button';
import layout from '../../../../shared/components/FormLayout.module.css';
import { PasswordField } from '../../../../shared/components/PasswordField';
import { Select } from '../../../../shared/components/Select';
import { TextField } from '../../../../shared/components/TextField';
import { latestAdultBirthDate } from '../../../../shared/validation';
import type { BecomeACustomerManager } from '../useBecomeACustomerManager';

/** 2. adım: kişisel bilgiler, adres ve şifre (BOA'daki ContactInfo.xaml gibi bilgi ekranları). */
export function DetailsStep({ manager }: { manager: BecomeACustomerManager }) {
  const { getMessage } = useMessaging();
  const { values, errors, setValue } = manager.detailsForm;

  return (
    <form
      className={layout.form}
      noValidate
      onSubmit={(event) => {
        event.preventDefault();
        void manager.submitDetails();
      }}
    >
      {manager.application?.isContinuation && (
        <Alert tone="info">{getMessage('BecomeACustomer', 'ContinueApplication')}</Alert>
      )}

      <section className={layout.section}>
        <h2 className={layout.sectionTitle}>{getMessage('BecomeACustomer', 'PersonalInfo')}</h2>
        <div className={layout.grid}>
          <TextField
            label={getMessage('BecomeACustomer', 'FirstName')}
            value={values.firstName}
            onChange={(event) => manager.setName('firstName', event.target.value)}
            onBlur={() => manager.formatName('firstName')}
            error={errors.firstName}
            maxLength={50}
            autoComplete="given-name"
            autoFocus
          />
          <TextField
            label={getMessage('BecomeACustomer', 'LastName')}
            value={values.lastName}
            onChange={(event) => manager.setName('lastName', event.target.value)}
            onBlur={() => manager.formatName('lastName')}
            error={errors.lastName}
            maxLength={50}
            autoComplete="family-name"
          />
          <TextField
            label={getMessage('BecomeACustomer', 'BirthDate')}
            type="date"
            min="1900-01-01"
            max={latestAdultBirthDate()}
            value={values.birthDate}
            onChange={(event) => setValue('birthDate', event.target.value)}
            error={errors.birthDate}
            hint={getMessage('BecomeACustomer', 'BirthDateHint')}
            autoComplete="bday"
          />
          <TextField
            label={`${getMessage('BecomeACustomer', 'Email')} ${getMessage('Common', 'Optional')}`}
            type="email"
            value={values.email}
            onChange={(event) => setValue('email', event.target.value)}
            error={errors.email}
            autoComplete="email"
          />
        </div>
      </section>

      <section className={layout.section}>
        <h2 className={layout.sectionTitle}>{getMessage('BecomeACustomer', 'Address')}</h2>
        <div className={layout.grid}>
          <Select
            label={getMessage('BecomeACustomer', 'City')}
            value={values.cityId}
            onChange={(event) => manager.selectCity(event.target.value)}
            options={manager.cityOptions}
            placeholder={getMessage('Common', 'Select')}
            error={errors.cityId}
          />
          <Select
            label={getMessage('BecomeACustomer', 'County')}
            value={values.countyId}
            onChange={(event) => setValue('countyId', event.target.value)}
            options={manager.countyOptions}
            placeholder={getMessage('Common', 'Select')}
            error={errors.countyId}
            disabled={!values.cityId}
          />
        </div>
        <TextField
          label={getMessage('BecomeACustomer', 'AddressLine')}
          value={values.addressLine}
          onChange={(event) => setValue('addressLine', event.target.value)}
          error={errors.addressLine}
          maxLength={250}
          autoComplete="street-address"
        />
      </section>

      <section className={layout.section}>
        <h2 className={layout.sectionTitle}>{getMessage('BecomeACustomer', 'PasswordTitle')}</h2>
        <div className={layout.grid}>
          <PasswordField
            label={getMessage('BecomeACustomer', 'Password')}
            value={values.password}
            onChange={(digits) => setValue('password', digits)}
            error={errors.password}
            hint={getMessage('BecomeACustomer', 'PasswordHint')}
            autoComplete="new-password"
          />
          <PasswordField
            label={getMessage('BecomeACustomer', 'PasswordRepeat')}
            value={values.passwordRepeat}
            onChange={(digits) => setValue('passwordRepeat', digits)}
            error={errors.passwordRepeat}
            autoComplete="new-password"
          />
        </div>
      </section>

      <div className={layout.actions}>
        <Button type="submit" variant="accent" isLoading={manager.isSubmitting}>
          {getMessage('BecomeACustomer', 'Submit')}
        </Button>
      </div>
    </form>
  );
}
