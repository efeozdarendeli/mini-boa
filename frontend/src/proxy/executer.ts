import { getLanguage } from '../messaging/language';
import { getMessage } from '../messaging/messagingHelper';
import { traceUtil } from '../shared/traceUtil';
import type {
  AccountContract,
  AccountTransactionsInputContract,
  BecomeACustomerInputContract,
  CreateCustomerContract,
  CreateCustomerResultContract,
  CurrencyRateContract,
  CustomerSummaryContract,
  KYCCustomerInfoContract,
  LoginContract,
  LoginResultContract,
  OpenAccountContract,
  ParameterContract,
  PasswordResetIdentityContract,
  ProfileContract,
  ReceiverNameContract,
  RecentTransactionContract,
  ResetPasswordContract,
  TransactionContract,
  TransferContract,
  TransferResultContract,
  UpdateProfileContract,
} from '../types/contracts';
import { ErrorCodes, type GenericResponse } from '../types/genericResponse';
import { setIsOnline } from './connectionStatus';
import { expireSession, getSession } from './session';

export const API_BASE_URL: string = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5080';

/**
 * Backend'in tek iş kapısına (POST /api/execute) istek gönderir (BOA'daki AtmExecute<TReq, TRes>).
 * Ne olursa olsun bir GenericResponse döner, exception fırlatmaz; ekranlar her zaman
 * aynı şekilde `if (!response.success)` kontrolü yapar.
 */
export async function execute<TOut>(methodName: string, data?: unknown): Promise<GenericResponse<TOut>> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    'Accept-Language': getLanguage(),
  };

  const session = getSession();
  if (session) {
    headers.Authorization = `Bearer ${session.token}`;
  }

  let response: Response;
  try {
    response = await fetch(`${API_BASE_URL}/api/execute`, {
      method: 'POST',
      headers,
      body: JSON.stringify({ methodName, data }),
    });
  } catch (error) {
    // Sunucuya hiç ulaşılamadı (kapalı, ağ yok, CORS).
    traceUtil.error(`${methodName} could not reach the server`, error);
    setIsOnline(false);
    return failure(ErrorCodes.Network);
  }

  setIsOnline(true);
  const traceId = response.headers.get('X-Trace-Id');

  if (response.status === 401) {
    expireSession();
  }

  const body = (await response.json().catch(() => null)) as GenericResponse<TOut> | null;
  if (!body) {
    traceUtil.error(`${methodName} returned an unreadable response`, { status: response.status, traceId });
    return failure(ErrorCodes.Unexpected);
  }

  if (!body.success) {
    traceUtil.trace(`${methodName} failed`, { traceId, results: body.results });
  }

  return body;
}

/** "Bağlantı yok" şeridindeki "Tekrar dene" için: API ve veritabanı ayakta mı? */
export async function checkHealth(): Promise<boolean> {
  try {
    const response = await fetch(`${API_BASE_URL}/health`);
    setIsOnline(response.ok);
    return response.ok;
  } catch {
    setIsOnline(false);
    return false;
  }
}

function failure<TOut>(errorCode: string): GenericResponse<TOut> {
  return {
    value: null,
    success: false,
    results: [{ errorCode, message: getMessage('Xtm', 'WeCantMakeYourTranNowTryAgain'), severity: 'Error' }],
  };
}

/**
 * Backend'deki her işlemin tipli karşılığı (BOA'daki BOA.SelfService.Proxy.Executer).
 * Ekranlar methodName metnini hiç yazmaz; yanlış girdi tipi gönderilirse kod derlenmez.
 */
export const Executer = {
  customerDecisionByTaxNumber: (contract: BecomeACustomerInputContract) =>
    execute<KYCCustomerInfoContract>('BecomeACustomer.CustomerDecisionByTaxNumber', contract),

  createCustomer: (contract: CreateCustomerContract) =>
    execute<CreateCustomerResultContract>('BecomeACustomer.CreateCustomer', contract),

  login: (contract: LoginContract) => execute<LoginResultContract>('Login.Login', contract),

  verifyIdentityForPasswordReset: (contract: PasswordResetIdentityContract) =>
    execute<boolean>('PasswordReset.VerifyIdentity', contract),

  resetPassword: (contract: ResetPasswordContract) => execute<boolean>('PasswordReset.ResetPassword', contract),

  getCustomerSummary: () => execute<CustomerSummaryContract>('Account.GetCustomerSummary'),

  getAccounts: () => execute<AccountContract[]>('Account.GetAccounts'),

  getRecentTransactions: () => execute<RecentTransactionContract[]>('Account.GetRecentTransactions'),

  getAccountTransactions: (contract: AccountTransactionsInputContract) =>
    execute<TransactionContract[]>('Account.GetAccountTransactions', contract),

  openAccount: (contract: OpenAccountContract) => execute<AccountContract>('Account.OpenAccount', contract),

  transfer: (contract: TransferContract) => execute<TransferResultContract>('Transfer.Transfer', contract),

  getReceiverName: (iban: string) => execute<ReceiverNameContract>('Transfer.GetReceiverName', { iban }),

  getProfile: () => execute<ProfileContract>('Profile.GetProfile'),

  updateProfile: (contract: UpdateProfileContract) => execute<ProfileContract>('Profile.UpdateProfile', contract),

  getCityList: () => execute<ParameterContract[]>('Parameter.GetCityList'),

  /** BOA'daki Executer.GetCountyList(int cityId) karşılığı. */
  getCountyList: (cityId: number) => execute<ParameterContract[]>('Parameter.GetCountyList', { cityId }),

  getCurrencyRates: () => execute<CurrencyRateContract[]>('Parameter.GetCurrencyRates'),
};
