import { useMessaging } from '../../../messaging/useMessaging';
import { Button } from '../../../shared/components/Button';
import { Card } from '../../../shared/components/Card';
import layout from '../../../shared/components/FormLayout.module.css';
import { PageHeader } from '../../../shared/components/PageHeader';
import { PasswordField } from '../../../shared/components/PasswordField';
import { PhoneField } from '../../../shared/components/PhoneField';
import { StepIndicator } from '../../../shared/components/StepIndicator';
import { TextField } from '../../../shared/components/TextField';
import { latestAdultBirthDate } from '../../../shared/validation';
import { useForgotPasswordManager } from './useForgotPasswordManager';

/** Şifremi Unuttum: kimlik doğrulama → yeni şifre. */
export function ForgotPasswordView() {
  const { getMessage } = useMessaging();
  const manager = useForgotPasswordManager();
  const identity = manager.identityForm;
  const password = manager.passwordForm;

  return (
    <div className={layout.narrow}>
      <PageHeader title={getMessage('ForgotPassword', 'Title')} onBack={manager.back} />
      <StepIndicator
        steps={[getMessage('ForgotPassword', 'StepIdentity'), getMessage('ForgotPassword', 'StepPassword')]}
        currentIndex={manager.stepIndex}
      />
      <Card>
        {manager.step === 'identity' ? (
          <form
            className={layout.form}
            noValidate
            onSubmit={(event) => {
              event.preventDefault();
              void manager.submitIdentity();
            }}
          >
            <p className={layout.intro}>{getMessage('ForgotPassword', 'IdentityIntro')}</p>
            <TextField
              label={getMessage('Login', 'IdentityNumber')}
              value={identity.values.identityNumber}
              onChange={(event) => manager.setIdentityNumber(event.target.value)}
              error={identity.errors.identityNumber}
              inputMode="numeric"
              maxLength={11}
              autoFocus
            />
            <PhoneField
              label={getMessage('BecomeACustomer', 'MobilePhone')}
              value={identity.values.phone}
              onChange={(digits) => identity.setValue('phone', digits)}
              error={identity.errors.phone}
            />
            <TextField
              label={getMessage('BecomeACustomer', 'BirthDate')}
              type="date"
              min="1900-01-01"
              max={latestAdultBirthDate()}
              value={identity.values.birthDate}
              onChange={(event) => identity.setValue('birthDate', event.target.value)}
              error={identity.errors.birthDate}
            />
            <div className={layout.actions}>
              <Button type="submit" isLoading={manager.isSubmitting}>
                {getMessage('Common', 'Continue')}
              </Button>
            </div>
          </form>
        ) : (
          <form
            className={layout.form}
            noValidate
            onSubmit={(event) => {
              event.preventDefault();
              void manager.submitPassword();
            }}
          >
            <p className={layout.intro}>{getMessage('ForgotPassword', 'PasswordIntro')}</p>
            <PasswordField
              label={getMessage('ForgotPassword', 'NewPassword')}
              value={password.values.newPassword}
              onChange={(digits) => password.setValue('newPassword', digits)}
              error={password.errors.newPassword}
              hint={getMessage('BecomeACustomer', 'PasswordHint')}
              autoComplete="new-password"
              autoFocus
            />
            <PasswordField
              label={getMessage('ForgotPassword', 'NewPasswordRepeat')}
              value={password.values.newPasswordRepeat}
              onChange={(digits) => password.setValue('newPasswordRepeat', digits)}
              error={password.errors.newPasswordRepeat}
              autoComplete="new-password"
            />
            <div className={layout.actions}>
              <Button type="submit" variant="accent" isLoading={manager.isSubmitting}>
                {getMessage('ForgotPassword', 'Submit')}
              </Button>
            </div>
          </form>
        )}
      </Card>
    </div>
  );
}
