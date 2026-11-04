// Backend'deki MiniBoa.Types.GenericResponse<T> ve Result'ın birebir karşılığı.

export type Severity = 'Info' | 'Warning' | 'Error';

export interface Result {
  errorCode: string;
  message: string;
  severity: Severity;
}

export interface GenericResponse<T> {
  value: T | null;
  results: Result[];
  success: boolean;
}

/** Backend'deki ErrorCodes sabitleri + istemcinin kendi ürettiği ağ hatası kodu. */
export const ErrorCodes = {
  Unexpected: 'SYS-0001',
  MethodNotFound: 'SYS-0002',
  Unauthorized: 'SEC-0001',
  Validation: 'VAL-0001',
  NotFound: 'BUS-0001',
  BusinessRule: 'BUS-0002',
  Network: 'NET-0001',
} as const;
