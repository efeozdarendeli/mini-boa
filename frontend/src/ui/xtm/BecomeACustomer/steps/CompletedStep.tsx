import { PartyPopper } from 'lucide-react';
import { useMessaging } from '../../../../messaging/useMessaging';
import { Button } from '../../../../shared/components/Button';
import layout from '../../../../shared/components/FormLayout.module.css';
import { InfoList } from '../../../../shared/components/InfoList';
import { formatIban } from '../../../../shared/format';
import type { BecomeACustomerManager } from '../useBecomeACustomerManager';

/** 3. adım: oluşturulan müşteri numarası, ilk hesap ve IBAN. */
export function CompletedStep({ manager }: { manager: BecomeACustomerManager }) {
  const { getMessage } = useMessaging();
  const result = manager.result;

  if (!result) {
    return null;
  }

  return (
    <div className={layout.center}>
      <PartyPopper size={56} className={layout.successIcon} aria-hidden="true" />
      <h2>{getMessage('BecomeACustomer', 'DoneTitle')}</h2>
      <p className={layout.intro}>{getMessage('BecomeACustomer', 'DoneDescription')}</p>

      <div style={{ width: '100%' }}>
        <InfoList
          items={[
            { label: getMessage('BecomeACustomer', 'CustomerNo'), value: result.customerNo },
            { label: getMessage('BecomeACustomer', 'AccountNumber'), value: result.accountNumber },
            { label: getMessage('BecomeACustomer', 'Iban'), value: formatIban(result.iban) },
          ]}
        />
      </div>

      <Button onClick={manager.goToLogin}>{getMessage('BecomeACustomer', 'GoToLogin')}</Button>
    </div>
  );
}
