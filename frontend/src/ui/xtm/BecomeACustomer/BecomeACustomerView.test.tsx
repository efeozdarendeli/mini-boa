import { fireEvent, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { Routes } from '../../../app/routes';
import { mockApi, success } from '../../../test/apiMock';
import { renderApp } from '../../../test/renderApp';

const parameters = {
  'Parameter.GetCityList': () => success([{ id: 34, name: 'İstanbul' }]),
  'Parameter.GetCountyList': () => success([{ id: 8, name: 'Kadıköy' }]),
};

async function fillIdentityStep(identityNumber = '12345678950', phone = '05321112233') {
  await userEvent.type(screen.getByLabelText('T.C. Kimlik Numarası'), identityNumber);
  await userEvent.type(screen.getByLabelText('Cep telefonu'), phone);
  await userEvent.click(screen.getByRole('button', { name: 'Devam' }));
}

describe('BecomeACustomerView', () => {
  it('formats the phone as typed and adds the leading 0 automatically', async () => {
    mockApi(parameters);
    renderApp(Routes.BecomeACustomer);

    await userEvent.type(screen.getByLabelText('Cep telefonu'), '5321112233');

    expect(screen.getByLabelText('Cep telefonu')).toHaveValue('0532 111 22 33');
  });

  it('rejects a phone number that does not start with 05', async () => {
    const api = mockApi(parameters);
    renderApp(Routes.BecomeACustomer);

    await fillIdentityStep('12345678950', '04321112233');

    expect(screen.getByText('05 ile başlayan 11 haneli cep telefonu numaranızı giriniz.')).toBeInTheDocument();
    expect(api.callsTo('BecomeACustomer.CustomerDecisionByTaxNumber')).toHaveLength(0);
  });

  it('accepts only letters in names and capitalizes them in Turkish', async () => {
    mockApi({
      ...parameters,
      'BecomeACustomer.CustomerDecisionByTaxNumber': () =>
        success({ kycProcessType: 'NewCustomer', applicationId: 42 }),
    });
    renderApp(Routes.BecomeACustomer);

    await fillIdentityStep();
    await userEvent.type(await screen.findByLabelText('Ad'), 'ibrahim3!');
    await userEvent.tab();

    expect(screen.getByLabelText('Ad')).toHaveValue('İbrahim');
  });

  it('sends an existing customer to login with the identity number filled in', async () => {
    const api = mockApi({
      ...parameters,
      'BecomeACustomer.CustomerDecisionByTaxNumber': () =>
        success({ kycProcessType: 'GoCardLogin', applicationId: null }),
    });
    const { router } = renderApp(Routes.BecomeACustomer);

    await fillIdentityStep();

    expect(await screen.findByText(/Zaten MiniBoa müşterisisiniz/)).toBeInTheDocument();
    expect(router.state.location.pathname).toBe(Routes.Login);
    expect(screen.getByLabelText('T.C. Kimlik Numarası')).toHaveValue('12345678950');
    expect(api.callsTo('BecomeACustomer.CustomerDecisionByTaxNumber')[0]).toEqual({
      identityNumber: '12345678950',
      gsmAreaCode: '532',
      gsmNumber: '1112233',
    });
  });

  it('tells the user they are continuing an unfinished application', async () => {
    mockApi({
      ...parameters,
      'BecomeACustomer.CustomerDecisionByTaxNumber': () =>
        success({ kycProcessType: 'ContinueApplication', applicationId: 7 }),
    });
    renderApp(Routes.BecomeACustomer);

    await fillIdentityStep('34567891238');

    expect(await screen.findByText(/Yarım kalmış başvurunuza/)).toBeInTheDocument();
  });

  it('creates a customer and shows the new IBAN', async () => {
    const api = mockApi({
      ...parameters,
      'BecomeACustomer.CustomerDecisionByTaxNumber': () =>
        success({ kycProcessType: 'NewCustomer', applicationId: 42 }),
      'BecomeACustomer.CreateCustomer': () =>
        success({ customerNo: 10000003, accountNumber: '10000003-1', iban: 'TR330099900000100000030001' }),
    });
    renderApp(Routes.BecomeACustomer);

    await fillIdentityStep();
    await userEvent.type(await screen.findByLabelText('Ad'), 'Deneme');
    await userEvent.type(screen.getByLabelText('Soyad'), 'Müşteri');
    fireEvent.change(screen.getByLabelText('Doğum tarihi'), { target: { value: '1995-06-15' } });
    await userEvent.selectOptions(screen.getByLabelText('İl'), '34');
    await userEvent.selectOptions(await screen.findByRole('combobox', { name: 'İlçe' }), await screen.findByRole('option', { name: 'Kadıköy' }));
    await userEvent.type(screen.getByLabelText('Açık adres'), 'Test Sok. No:1');
    await userEvent.type(screen.getByLabelText('Şifre (6 hane)'), '246810');
    await userEvent.type(screen.getByLabelText('Şifre tekrar'), '246810');
    await userEvent.click(screen.getByRole('button', { name: 'Müşteri Ol' }));

    expect(await screen.findByText('TR33 0099 9000 0010 0000 0300 01')).toBeInTheDocument();
    expect(api.callsTo('BecomeACustomer.CreateCustomer')[0]).toMatchObject({
      applicationId: 42,
      identityNumber: '12345678950',
      cityId: 34,
      countyId: 8,
      birthDate: '1995-06-15',
    });
  });

  it('does not submit when the passwords do not match', async () => {
    const api = mockApi({
      ...parameters,
      'BecomeACustomer.CustomerDecisionByTaxNumber': () =>
        success({ kycProcessType: 'NewCustomer', applicationId: 42 }),
    });
    renderApp(Routes.BecomeACustomer);

    await fillIdentityStep();
    await userEvent.type(await screen.findByLabelText('Şifre (6 hane)'), '246810');
    await userEvent.type(screen.getByLabelText('Şifre tekrar'), '246811');
    await userEvent.click(screen.getByRole('button', { name: 'Müşteri Ol' }));

    expect(screen.getByText('Şifreler eşleşmiyor.')).toBeInTheDocument();
    expect(api.callsTo('BecomeACustomer.CreateCustomer')).toHaveLength(0);
  });
});
