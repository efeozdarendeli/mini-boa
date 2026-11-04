import { useMessaging } from '../../../messaging/useMessaging';
import { Alert } from '../../../shared/components/Alert';
import { Button } from '../../../shared/components/Button';
import { Card } from '../../../shared/components/Card';
import layout from '../../../shared/components/FormLayout.module.css';
import { InfoList } from '../../../shared/components/InfoList';
import { PageHeader } from '../../../shared/components/PageHeader';
import { QueryBoundary } from '../../../shared/components/QueryBoundary';
import { Select } from '../../../shared/components/Select';
import { TextField } from '../../../shared/components/TextField';
import { formatMobilePhone } from '../../../shared/format';
import { useProfileManager } from './useProfileManager';

/** Profilim: kimlik bilgileri (salt okunur) + iletişim ve adres (güncellenebilir). */
export function ProfileView() {
  const { getMessage } = useMessaging();
  const manager = useProfileManager();
  const { values, errors } = manager;

  return (
    <div className={layout.wide}>
      <PageHeader title={getMessage('Profile', 'Title')} onBack={manager.goBack} />
      <QueryBoundary query={manager.profileQuery}>
        {(profile) => (
          <div className={layout.form}>
            <Card className={layout.section}>
              <h2 className={layout.sectionTitle}>{getMessage('Profile', 'PersonalInfo')}</h2>
              <InfoList
                items={[
                  { label: getMessage('Profile', 'FullName'), value: `${profile.firstName} ${profile.lastName}` },
                  { label: getMessage('Profile', 'IdentityNumber'), value: profile.maskedIdentityNumber },
                  { label: getMessage('Profile', 'CustomerNo'), value: profile.customerNo },
                  { label: getMessage('Profile', 'MobilePhone'), value: formatMobilePhone(profile.gsmNumber) },
                ]}
              />
              <p className={layout.hint}>{getMessage('Profile', 'ReadOnlyHint')}</p>
            </Card>

            <Card>
              <form
                className={layout.form}
                noValidate
                onSubmit={(event) => {
                  event.preventDefault();
                  void manager.save();
                }}
              >
                <h2 className={layout.sectionTitle}>{getMessage('Profile', 'ContactInfo')}</h2>
                {manager.isSaved && <Alert tone="success">{getMessage('Profile', 'Saved')}</Alert>}

                <TextField
                  label={`${getMessage('BecomeACustomer', 'Email')} ${getMessage('Common', 'Optional')}`}
                  type="email"
                  value={values.email}
                  onChange={(event) => manager.setField('email', event.target.value)}
                  error={errors.email}
                  autoComplete="email"
                />
                <div className={layout.grid}>
                  <Select
                    label={getMessage('BecomeACustomer', 'City')}
                    value={values.cityId}
                    onChange={(event) => manager.setField('cityId', event.target.value)}
                    options={manager.cityOptions}
                    placeholder={getMessage('Common', 'Select')}
                    error={errors.cityId}
                  />
                  <Select
                    label={getMessage('BecomeACustomer', 'County')}
                    value={values.countyId}
                    onChange={(event) => manager.setField('countyId', event.target.value)}
                    options={manager.countyOptions}
                    placeholder={getMessage('Common', 'Select')}
                    error={errors.countyId}
                    disabled={!values.cityId}
                  />
                </div>
                <TextField
                  label={getMessage('BecomeACustomer', 'AddressLine')}
                  value={values.addressLine}
                  onChange={(event) => manager.setField('addressLine', event.target.value)}
                  error={errors.addressLine}
                  maxLength={250}
                  autoComplete="street-address"
                />

                <div className={layout.actions}>
                  <Button type="submit" isLoading={manager.isSubmitting}>
                    {getMessage('Common', 'Save')}
                  </Button>
                </div>
              </form>
            </Card>
          </div>
        )}
      </QueryBoundary>
    </div>
  );
}
