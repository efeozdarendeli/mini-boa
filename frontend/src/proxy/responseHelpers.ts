import { getMessage } from '../messaging/messagingHelper';
import type { GenericResponse, Result } from '../types/genericResponse';

/** Kullanıcıya gösterilecek hata metni: sunucunun ilk mesajı, yoksa genel "işleminizi gerçekleştiremiyoruz". */
export function getErrorMessage(response: GenericResponse<unknown>): string {
  return response.results[0]?.message ?? getMessage('Xtm', 'WeCantMakeYourTranNowTryAgain');
}

/** Önbelleğe alınacak sorgularda başarısız cevap hata olarak fırlatılır; böylece hatalı cevap önbelleğe girmez. */
export class ResponseError extends Error {
  readonly results: Result[];

  constructor(response: GenericResponse<unknown>) {
    super(getErrorMessage(response));
    this.results = response.results;
  }
}

/** TanStack Query için: başarılıysa sadece değeri döner, değilse ResponseError fırlatır. */
export async function unwrap<T>(request: Promise<GenericResponse<T>>): Promise<T> {
  const response = await request;
  if (!response.success || response.value === null) {
    throw new ResponseError(response);
  }
  return response.value;
}
