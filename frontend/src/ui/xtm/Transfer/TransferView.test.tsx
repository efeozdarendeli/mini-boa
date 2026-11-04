import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { Routes } from '../../../app/routes';
import { failure, mockApi, success } from '../../../test/apiMock';
import { hakanSummary, renderApp, signIn } from '../../../test/renderApp';

const AYSE_IBAN = 'TR060099900000100000020001';

const accountsAndReceiver = {
  'Account.GetAccounts': () => success(hakanSummary.accounts),
  'Transfer.GetReceiverName': () => success({ maskedName: 'Ay** Ka**' }),
};

async function fillForm(iban: string, amount: string) {
  await userEvent.type(await screen.findByLabelText('Alıcı IBAN'), iban);
  await userEvent.type(screen.getByLabelText(/^Tutar/), amount);
  await userEvent.click(screen.getByRole('button', { name: 'Devam' }));
}

describe('TransferView', () => {
  it('validates IBAN and amount before the confirm step', async () => {
    signIn();
    const api = mockApi(accountsAndReceiver);
    renderApp(Routes.Transfer);

    await fillForm('TR000000000000000000000000', '999999');

    expect(screen.getByText(/geçerli bir IBAN/)).toBeInTheDocument();
    expect(screen.getByText('Tutar, hesap bakiyesinden büyük olamaz.')).toBeInTheDocument();
    expect(api.callsTo('Transfer.Transfer')).toHaveLength(0);
  });

  it('keeps TR fixed and groups the IBAN digits by four, even when pasted', async () => {
    signIn();
    mockApi(accountsAndReceiver);
    renderApp(Routes.Transfer);

    const ibanField = await screen.findByLabelText('Alıcı IBAN');
    expect(ibanField).toHaveValue('TR');

    await userEvent.click(ibanField);
    await userEvent.paste('tr06 0099 9000 0010 0000 0200 01');

    expect(ibanField).toHaveValue('TR06 0099 9000 0010 0000 0200 01');
  });

  it('shows the masked receiver name once the IBAN is complete', async () => {
    signIn();
    const api = mockApi(accountsAndReceiver);
    renderApp(Routes.Transfer);

    await userEvent.type(await screen.findByLabelText('Alıcı IBAN'), AYSE_IBAN);

    expect(await screen.findByText('Alıcı: Ay** Ka**')).toBeInTheDocument();
    expect(api.callsTo('Transfer.GetReceiverName')).toEqual([{ iban: AYSE_IBAN }]);
  });

  it('blocks the transfer when no account has that IBAN', async () => {
    signIn();
    const api = mockApi({
      'Account.GetAccounts': () => success(hakanSummary.accounts),
      'Transfer.GetReceiverName': () => failure('Alıcı hesap bulunamadı.', 'BUS-0001'),
    });
    renderApp(Routes.Transfer);

    await fillForm(AYSE_IBAN, '10');

    expect(await screen.findAllByText('Alıcı hesap bulunamadı.')).not.toHaveLength(0);
    expect(screen.queryByRole('heading', { name: 'Transfer bilgilerini kontrol ediniz' })).not.toBeInTheDocument();
    expect(api.callsTo('Transfer.Transfer')).toHaveLength(0);
  });

  it('fills the whole balance and formats the amount', async () => {
    signIn();
    mockApi(accountsAndReceiver);
    renderApp(Routes.Transfer);

    await userEvent.click(await screen.findByRole('button', { name: 'Tüm bakiye' }));

    expect(screen.getByLabelText(/^Tutar/)).toHaveValue('15.250,75');
  });

  it('asks for confirmation, then transfers and shows a printable receipt', async () => {
    signIn();
    const api = mockApi({
      ...accountsAndReceiver,
      'Transfer.Transfer': () =>
        success({ referenceId: 'ref-123', senderBalanceAfter: 15000.75, transactionDate: '2026-11-01T09:30:00Z' }),
    });
    const print = vi.spyOn(window, 'print').mockImplementation(() => {});
    renderApp(Routes.Transfer);

    await fillForm(AYSE_IBAN, '250');

    expect(screen.getByRole('heading', { name: 'Transfer bilgilerini kontrol ediniz' })).toBeInTheDocument();
    expect(screen.getByText('Ay** Ka**')).toBeInTheDocument();
    expect(api.callsTo('Transfer.Transfer')).toHaveLength(0);

    await userEvent.click(screen.getByRole('button', { name: 'Transferi Onayla' }));

    expect(await screen.findByText('ref-123')).toBeInTheDocument();
    expect(api.callsTo('Transfer.Transfer')[0]).toEqual({ fromAccountId: 1, toIban: AYSE_IBAN, amount: 250 });

    await userEvent.click(screen.getByRole('button', { name: 'Dekontu Yazdır' }));
    expect(print).toHaveBeenCalledOnce();
  });

  it('goes back to the form and shows the server message when the transfer fails', async () => {
    signIn();
    mockApi({
      ...accountsAndReceiver,
      'Transfer.Transfer': () => failure('Günlük transfer limitiniz (50.000,00 TRY) aşılıyor.'),
    });
    renderApp(Routes.Transfer);

    await fillForm(AYSE_IBAN, '10,5');
    await userEvent.click(screen.getByRole('button', { name: 'Transferi Onayla' }));

    expect(await screen.findByRole('alertdialog')).toHaveTextContent('Günlük transfer limitiniz');
    expect(screen.getByLabelText(/^Tutar/)).toHaveValue('10,50');
  });
});
