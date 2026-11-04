import { useNavigate } from 'react-router';
import { Routes } from '../../../app/routes';
import { useMessaging } from '../../../messaging/useMessaging';
import { Alert } from '../../../shared/components/Alert';
import { Button } from '../../../shared/components/Button';
import { Card } from '../../../shared/components/Card';
import layout from '../../../shared/components/FormLayout.module.css';
import { PageHeader } from '../../../shared/components/PageHeader';
import { PasswordField } from '../../../shared/components/PasswordField';
import { TextField } from '../../../shared/components/TextField';
import { useLoginManager } from './useLoginManager';

/** Giriş ekranı: TCKN + 6 haneli şifre. */
export function LoginView() {
  const { getMessage } = useMessaging();
  const navigate = useNavigate();
  const manager = useLoginManager();
  const { values, errors } = manager.form;

  return (
    <div className={layout.narrow}>
      <PageHeader title={getMessage('Login', 'Title')} onBack={() => navigate(Routes.Welcome)} />
      <Card>
        <form
          className={layout.form}
          noValidate
          onSubmit={(event) => {
            event.preventDefault();
            void manager.submit();
          }}
        >
          {manager.notice && <Alert tone={manager.notice.tone}>{manager.notice.text}</Alert>}
          <p className={layout.intro}>{getMessage('Login', 'Intro')}</p>

          <TextField
            label={getMessage('Login', 'IdentityNumber')}
            value={values.identityNumber}
            onChange={(event) => manager.setIdentityNumber(event.target.value)}
            error={errors.identityNumber}
            inputMode="numeric"
            maxLength={11}
            autoComplete="username"
            autoFocus
          />
          <PasswordField
            label={getMessage('Login', 'Password')}
            value={values.password}
            onChange={manager.setPassword}
            error={errors.password}
            autoComplete="current-password"
          />

          <Button type="submit" fullWidth isLoading={manager.isSubmitting}>
            {getMessage('Login', 'Submit')}
          </Button>
          <div className={layout.split}>
            <span className={layout.hint}>{getMessage('Login', 'DemoHint')}</span>
            <Button variant="ghost" onClick={manager.goToForgotPassword}>
              {getMessage('Login', 'ForgotPassword')}
            </Button>
          </div>
        </form>
      </Card>

      <div className={layout.center} style={{ marginTop: 24 }}>
        <span className={layout.intro}>{getMessage('Login', 'NotCustomerYet')}</span>
        <Button variant="accent" onClick={manager.goToBecomeACustomer}>
          {getMessage('Login', 'BecomeACustomer')}
        </Button>
      </div>
    </div>
  );
}
