import { useMessaging } from '../../../messaging/useMessaging';
import { Card } from '../../../shared/components/Card';
import layout from '../../../shared/components/FormLayout.module.css';
import { PageHeader } from '../../../shared/components/PageHeader';
import { StepIndicator } from '../../../shared/components/StepIndicator';
import { CompletedStep } from './steps/CompletedStep';
import { DetailsStep } from './steps/DetailsStep';
import { IdentityStep } from './steps/IdentityStep';
import { useBecomeACustomerManager } from './useBecomeACustomerManager';

/** "Müşteri Ol" sihirbazı: Kimlik → Bilgiler → Tamamlandı. Her adımın görünümü ayrı dosyada. */
export function BecomeACustomerView() {
  const { getMessage } = useMessaging();
  const manager = useBecomeACustomerManager();

  const steps = [
    getMessage('BecomeACustomer', 'StepIdentity'),
    getMessage('BecomeACustomer', 'StepDetails'),
    getMessage('BecomeACustomer', 'StepDone'),
  ];

  return (
    <div className={layout.wide}>
      <PageHeader
        title={getMessage('BecomeACustomer', 'Title')}
        onBack={manager.step === 'completed' ? undefined : manager.back}
      />
      <StepIndicator steps={steps} currentIndex={manager.stepIndex} />
      <Card>
        {manager.step === 'identity' && <IdentityStep manager={manager} />}
        {manager.step === 'details' && <DetailsStep manager={manager} />}
        {manager.step === 'completed' && <CompletedStep manager={manager} />}
      </Card>
    </div>
  );
}
