import { useMessaging } from '../../../../messaging/useMessaging';
import { Button } from '../../../../shared/components/Button';
import layout from '../../../../shared/components/FormLayout.module.css';
import { PhoneField } from '../../../../shared/components/PhoneField';
import { TextField } from '../../../../shared/components/TextField';
import type { BecomeACustomerManager } from '../useBecomeACustomerManager';

/** 1. adım: TCKN ve cep telefonu (BOA'daki BecomeACustomerInputContract alanları). */
export function IdentityStep({ manager }: { manager: BecomeACustomerManager }) {
  const { getMessage } = useMessaging();
  const { values, errors } = manager.identityForm;

  return (
    <form
      className={layout.form}
      noValidate
      onSubmit={(event) => {
        event.preventDefault();
        void manager.submitIdentity();
      }}
    >
      <p className={layout.intro}>{getMessage('BecomeACustomer', 'IdentityIntro')}</p>

      <TextField
        label={getMessage('BecomeACustomer', 'IdentityNumber')}
        value={values.identityNumber}
        onChange={(event) => manager.setIdentityNumber(event.target.value)}
        error={errors.identityNumber}
        inputMode="numeric"
        maxLength={11}
        autoFocus
      />
      <PhoneField
        label={getMessage('BecomeACustomer', 'MobilePhone')}
        value={values.phone}
        onChange={manager.setPhone}
        error={errors.phone}
      />

      <div className={layout.actions}>
        <Button type="submit" variant="accent" isLoading={manager.isSubmitting}>
          {getMessage('Common', 'Continue')}
        </Button>
      </div>
    </form>
  );
}
