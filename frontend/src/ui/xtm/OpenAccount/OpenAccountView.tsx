import { useMessaging } from '../../../messaging/useMessaging';
import { Button } from '../../../shared/components/Button';
import { Card } from '../../../shared/components/Card';
import layout from '../../../shared/components/FormLayout.module.css';
import { PageHeader } from '../../../shared/components/PageHeader';
import { Select } from '../../../shared/components/Select';
import { formatCurrencyName } from '../../../shared/format';
import type { AccountType } from '../../../types/contracts';
import { ACCOUNT_TYPES, SUPPORTED_CURRENCIES, useOpenAccountManager } from './useOpenAccountManager';

/** Hesap Aç: hesap türü + döviz cinsi. */
export function OpenAccountView() {
  const { getMessage, language } = useMessaging();
  const manager = useOpenAccountManager();

  return (
    <div className={layout.narrow}>
      <PageHeader title={getMessage('OpenAccount', 'Title')} onBack={manager.goBack} />
      <Card>
        <form
          className={layout.form}
          onSubmit={(event) => {
            event.preventDefault();
            void manager.submit();
          }}
        >
          <p className={layout.intro}>{getMessage('OpenAccount', 'Intro')}</p>

          <Select
            label={getMessage('OpenAccount', 'AccountType')}
            value={manager.accountType}
            onChange={(event) => manager.setAccountType(event.target.value as AccountType)}
            options={ACCOUNT_TYPES.map((type) => ({ value: type, label: getMessage('Accounts', type) }))}
          />
          <Select
            label={getMessage('OpenAccount', 'Currency')}
            value={manager.currencyCode}
            onChange={(event) => manager.setCurrencyCode(event.target.value)}
            options={SUPPORTED_CURRENCIES.map((code) => ({
              value: code,
              label: `${code} · ${formatCurrencyName(code, language)}`,
            }))}
          />

          <div className={layout.actions}>
            <Button type="submit" isLoading={manager.isSubmitting}>
              {getMessage('OpenAccount', 'Submit')}
            </Button>
          </div>
        </form>
      </Card>
    </div>
  );
}
