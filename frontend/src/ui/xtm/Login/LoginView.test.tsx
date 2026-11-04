import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { Routes } from '../../../app/routes';
import { getSession } from '../../../proxy/session';
import { failure, mockApi, success } from '../../../test/apiMock';
import { hakanSummary, renderApp } from '../../../test/renderApp';

describe('LoginView', () => {
  it('validates the form without calling the server', async () => {
    const { fetchMock } = mockApi({});
    renderApp(Routes.Login);

    await userEvent.type(screen.getByLabelText('T.C. Kimlik Numarası'), '12345678951');
    await userEvent.type(screen.getByLabelText('Şifre'), '123');
    await userEvent.click(screen.getByRole('button', { name: 'Giriş Yap' }));

    expect(screen.getByText('Geçerli bir T.C. kimlik numarası giriniz.')).toBeInTheDocument();
    expect(screen.getByText('6 haneli bir şifre giriniz.')).toBeInTheDocument();
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it('signs in and opens the dashboard', async () => {
    mockApi({
      'Login.Login': () =>
        success({ customerNo: 10000001, firstName: 'Abdullah Hakan', lastName: 'Demir', token: 'jwt' }),
      'Account.GetCustomerSummary': () => success(hakanSummary),
    });
    renderApp(Routes.Login);

    await userEvent.type(screen.getByLabelText('T.C. Kimlik Numarası'), '12345678950');
    await userEvent.type(screen.getByLabelText('Şifre'), '123456');
    await userEvent.click(screen.getByRole('button', { name: 'Giriş Yap' }));

    expect(await screen.findByRole('heading', { name: 'ABDULLAH HAKAN' })).toBeInTheDocument();
    expect(getSession()?.token).toBe('jwt');
  });

  it('shows the server message when the password is wrong', async () => {
    mockApi({ 'Login.Login': () => failure('T.C. kimlik numarası veya şifre hatalı.', 'SEC-0001') });
    renderApp(Routes.Login);

    await userEvent.type(screen.getByLabelText('T.C. Kimlik Numarası'), '12345678950');
    await userEvent.type(screen.getByLabelText('Şifre'), '000000');
    await userEvent.click(screen.getByRole('button', { name: 'Giriş Yap' }));

    expect(await screen.findByRole('alertdialog')).toHaveTextContent('T.C. kimlik numarası veya şifre hatalı.');
    expect(screen.getByLabelText('Şifre')).toHaveValue('');
    expect(getSession()).toBeNull();

    // Dialog Escape ile kapanır.
    await userEvent.keyboard('{Escape}');
    expect(screen.queryByRole('alertdialog')).not.toBeInTheDocument();
  });

  it('can show the typed password', async () => {
    mockApi({});
    renderApp(Routes.Login);

    await userEvent.type(screen.getByLabelText('Şifre'), '123456');
    expect(screen.getByLabelText('Şifre')).toHaveAttribute('type', 'password');

    await userEvent.click(screen.getByRole('button', { name: 'Şifreyi göster' }));

    expect(screen.getByLabelText('Şifre')).toHaveAttribute('type', 'text');
  });

  it('opens the forgot-password screen', async () => {
    mockApi({});
    const { router } = renderApp(Routes.Login);

    await userEvent.click(screen.getByRole('button', { name: 'Şifremi unuttum' }));

    expect(router.state.location.pathname).toBe(Routes.ForgotPassword);
  });

  it('accepts only digits', async () => {
    mockApi({});
    renderApp(Routes.Login);

    await userEvent.type(screen.getByLabelText('T.C. Kimlik Numarası'), '12a3-45');

    expect(screen.getByLabelText('T.C. Kimlik Numarası')).toHaveValue('12345');
  });

  it('redirects protected screens to login when signed out', () => {
    mockApi({});
    const { router } = renderApp(Routes.Transfer);

    expect(router.state.location.pathname).toBe(Routes.Login);
  });
});
