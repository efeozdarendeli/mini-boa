// Backend'deki MiniBoa.Types.Contracts sınıflarının TypeScript karşılıkları (BOA.Types.SelfService).
// Alan adları JSON'daki gibi camelCase; enum'lar isim olarak gelir ("GoCardLogin").
// Tarihler "YYYY-MM-DD" (DateOnly) veya ISO metin (DateTime), tutarlar number olarak taşınır.

export type AccountType = 'Current' | 'Participation';
export type AccountStatus = 'Active' | 'Closed';
export type TransactionDirection = 'Debit' | 'Credit';

// ---- Müşteri Ol ----

export interface BecomeACustomerInputContract {
  identityNumber: string;
  gsmCountry?: string;
  gsmAreaCode: string;
  gsmNumber: string;
}

export type KYCProcessType = 'NewCustomer' | 'GoCardLogin' | 'ContinueApplication';

export interface KYCCustomerInfoContract {
  kycProcessType: KYCProcessType;
  applicationId: number | null;
}

export interface CreateCustomerContract {
  applicationId: number;
  identityNumber: string;
  firstName: string;
  lastName: string;
  birthDate: string;
  email?: string;
  cityId: number;
  countyId: number;
  addressLine: string;
  password: string;
}

export interface CreateCustomerResultContract {
  customerNo: number;
  accountNumber: string;
  iban: string;
}

// ---- Giriş ----

export interface LoginContract {
  identityNumber: string;
  password: string;
}

export interface LoginResultContract {
  customerNo: number;
  firstName: string;
  lastName: string;
  token: string | null;
}

// ---- Hesaplar ----

export interface AccountContract {
  accountId: number;
  accountNumber: string;
  iban: string;
  currencyCode: string;
  accountType: AccountType;
  balance: number;
  status: AccountStatus;
}

export interface CustomerSummaryContract {
  customerNo: number;
  firstName: string;
  lastName: string;
  accounts: AccountContract[];
  totalTryBalance: number;
  /** Bir önceki giriş (ISO, UTC); ilk girişte null. */
  previousLoginAt: string | null;
}

export interface AccountTransactionsInputContract {
  accountId: number;
  /** 7, 30 veya 90 */
  days: number;
  direction?: TransactionDirection;
}

export interface OpenAccountContract {
  currencyCode: string;
  accountType: AccountType;
}

/** Ana sayfadaki "Son İşlemler" satırı. */
export interface RecentTransactionContract {
  transactionId: number;
  referenceId: string;
  accountNumber: string;
  currencyCode: string;
  direction: TransactionDirection;
  amount: number;
  balanceAfter: number;
  description: string | null;
  createdAt: string;
  /** Karşı tarafın maskeli adı ("Ay** Ka**"); dışarıdan gelen hareketlerde null. */
  counterparty: string | null;
}

export interface TransactionContract {
  transactionId: number;
  referenceId: string;
  direction: TransactionDirection;
  amount: number;
  balanceAfter: number;
  description: string | null;
  createdAt: string;
}

// ---- Transfer ----

export interface TransferContract {
  fromAccountId: number;
  toIban: string;
  amount: number;
  description?: string;
}

export interface TransferResultContract {
  referenceId: string;
  senderBalanceAfter: number;
  transactionDate: string;
}

export interface ReceiverNameContract {
  /** Örn. "Ay** Ka**" */
  maskedName: string;
}

// ---- Şifremi unuttum ----

export interface PasswordResetIdentityContract {
  identityNumber: string;
  gsmAreaCode: string;
  gsmNumber: string;
  birthDate: string;
}

export interface ResetPasswordContract extends PasswordResetIdentityContract {
  newPassword: string;
}

// ---- Profil ----

export interface ProfileContract {
  customerNo: number;
  firstName: string;
  lastName: string;
  maskedIdentityNumber: string;
  /** "05321112233" */
  gsmNumber: string;
  email: string | null;
  cityId: number | null;
  countyId: number | null;
  addressLine: string | null;
}

export interface UpdateProfileContract {
  email?: string;
  cityId: number;
  countyId: number;
  addressLine: string;
}

// ---- Parametreler ----

export interface ParameterContract {
  id: number;
  name: string;
}

export interface CurrencyRateContract {
  currencyCode: string;
  currencyName: string;
  buyRate: number;
  sellRate: number;
  rateDate: string;
}
