import { fireEvent, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { Routes } from '../../../app/routes';
import { failure, mockApi, success } from '../../../test/apiMock';
import { renderApp } from '../../../test/renderApp';

async function fillIdentity() {
  await userEvent.type(screen.getByLabelText('T.C. Kimlik Numarası'), '12345678950');
  await userEvent.type(screen.getByLabelText('Cep telefonu'), '05321112233');
  fireEvent.change(screen.getByLabelText('Doğum tarihi'), { target: { value: '1990-05-14' } });
  await userEvent.click(screen.getByRole('button', { name: 'Devam' }));
}

describe('ForgotPasswordView', () => {
  it('verifies identity, sets the new password and returns to login', async () => {
    const api = mockApi({
      'PasswordReset.VerifyIdentity': () => success(true),
      'PasswordReset.ResetPassword': () => success(true),
    });
    const { router } = renderApp(Routes.ForgotPassword);

    await fillIdentity();
    await userEvent.type(await screen.findByLabelText('Yeni şifre (6 hane)'), '975310');
    await userEvent.type(screen.getByLabelText('Yeni şifre tekrar'), '975310');
    await userEvent.click(screen.getByRole('button', { name: 'Şifremi Değiştir' }));

    expect(await screen.findByText(/Şifreniz değiştirildi/)).toBeInTheDocument();
    expect(router.state.location.pathname).toBe(Routes.Login);
    expect(screen.getByLabelText('T.C. Kimlik Numarası')).toHaveValue('12345678950');

    // Telefon backend'e alan kodu + numara olarak gider; kimlik 2. adımda tekrar gönderilir.
    expect(api.callsTo('PasswordReset.ResetPassword')[0]).toEqual({
      identityNumber: '12345678950',
      gsmAreaCode: '532',
      gsmNumber: '1112233',
      birthDate: '1990-05-14',
      newPassword: '975310',
    });
  });

  it('stays on the identity step when the details do not match', async () => {
    mockApi({
      'PasswordReset.VerifyIdentity': () => failure('Girdiğiniz bilgiler kayıtlarımızla eşleşmedi.', 'SEC-0001'),
    });
    renderApp(Routes.ForgotPassword);

    await fillIdentity();

    expect(await screen.findByRole('alertdialog')).toHaveTextContent('kayıtlarımızla eşleşmedi');
    expect(screen.queryByLabelText('Yeni şifre (6 hane)')).not.toBeInTheDocument();
  });

  it('checks that the new passwords match before calling the server', async () => {
    const api = mockApi({ 'PasswordReset.VerifyIdentity': () => success(true) });
    renderApp(Routes.ForgotPassword);

    await fillIdentity();
    await userEvent.type(await screen.findByLabelText('Yeni şifre (6 hane)'), '975310');
    await userEvent.type(screen.getByLabelText('Yeni şifre tekrar'), '975311');
    await userEvent.click(screen.getByRole('button', { name: 'Şifremi Değiştir' }));

    expect(screen.getByText('Şifreler eşleşmiyor.')).toBeInTheDocument();
    expect(api.callsTo('PasswordReset.ResetPassword')).toHaveLength(0);
  });
});
