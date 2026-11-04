import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { Routes } from '../../../app/routes';
import { mockApi, success } from '../../../test/apiMock';
import { renderApp, signIn } from '../../../test/renderApp';
import type { ProfileContract } from '../../../types/contracts';

const profile: ProfileContract = {
  customerNo: 10000001,
  firstName: 'Abdullah Hakan',
  lastName: 'Demir',
  maskedIdentityNumber: '123******50',
  gsmNumber: '05321112233',
  email: 'hakan.demir@example.com',
  cityId: 34,
  countyId: 6,
  addressLine: 'Atatürk Mah. Örnek Sok. No:1',
};

const parameters = {
  'Parameter.GetCityList': () => success([{ id: 34, name: 'İstanbul' }]),
  'Parameter.GetCountyList': () =>
    success([
      { id: 6, name: 'Ataşehir' },
      { id: 8, name: 'Kadıköy' },
    ]),
};

describe('ProfileView', () => {
  it('shows identity details read-only with a masked identity number', async () => {
    signIn();
    mockApi({ ...parameters, 'Profile.GetProfile': () => success(profile) });
    renderApp(Routes.Profile);

    expect(await screen.findByText('123******50')).toBeInTheDocument();
    expect(screen.getByText('0532 111 22 33')).toBeInTheDocument();
    expect(screen.getByLabelText(/E-posta/)).toHaveValue('hakan.demir@example.com');
  });

  it('saves the new address and shows a confirmation', async () => {
    signIn();
    const api = mockApi({
      ...parameters,
      'Profile.GetProfile': () => success(profile),
      'Profile.UpdateProfile': () => success({ ...profile, countyId: 8, addressLine: 'Yeni Adres No:2' }),
    });
    renderApp(Routes.Profile);

    await userEvent.selectOptions(await screen.findByLabelText('İlçe'), await screen.findByRole('option', { name: 'Kadıköy' }));
    const address = screen.getByLabelText('Açık adres');
    await userEvent.clear(address);
    await userEvent.type(address, 'Yeni Adres No:2');
    await userEvent.click(screen.getByRole('button', { name: 'Kaydet' }));

    expect(await screen.findByText('Bilgileriniz güncellendi.')).toBeInTheDocument();
    expect(api.callsTo('Profile.UpdateProfile')[0]).toEqual({
      email: 'hakan.demir@example.com',
      cityId: 34,
      countyId: 8,
      addressLine: 'Yeni Adres No:2',
    });
  });
});
