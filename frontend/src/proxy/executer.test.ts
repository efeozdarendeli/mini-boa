import { describe, expect, it, vi } from 'vitest';
import { setLanguage } from '../messaging/language';
import { mockFetchResponse, readRequest } from '../test/fetchMock';
import { ErrorCodes } from '../types/genericResponse';
import { Executer } from './executer';
import { getSession, hasExpiredNotice, startSession } from './session';

const okResponse = { value: [], results: [], success: true };

describe('Executer', () => {
  it('sends the method name and input contract to /api/execute', async () => {
    const fetchMock = mockFetchResponse(okResponse);

    await Executer.getCountyList(34);

    const request = readRequest(fetchMock);
    expect(request.url).toBe('http://localhost:5080/api/execute');
    expect(request.body).toEqual({ methodName: 'Parameter.GetCountyList', data: { cityId: 34 } });
  });

  it('sends the selected language and, when signed in, the token', async () => {
    const fetchMock = mockFetchResponse(okResponse);
    setLanguage('en');
    startSession({ token: 'abc', customerNo: 1, firstName: 'A', lastName: 'B' });

    await Executer.getAccounts();

    const { headers } = readRequest(fetchMock);
    expect(headers['Accept-Language']).toBe('en');
    expect(headers.Authorization).toBe('Bearer abc');
  });

  it('does not send a token when there is no session', async () => {
    const fetchMock = mockFetchResponse(okResponse);

    await Executer.getCityList();

    expect(readRequest(fetchMock).headers.Authorization).toBeUndefined();
  });

  it('returns the business failure as-is (no exception)', async () => {
    const failure = {
      value: null,
      success: false,
      results: [{ errorCode: 'BUS-0002', message: 'Hesap bakiyesi yetersiz.', severity: 'Error' }],
    };
    mockFetchResponse(failure);

    const response = await Executer.transfer({ fromAccountId: 1, toIban: 'TR00', amount: 5 });

    expect(response).toEqual(failure);
  });

  it('ends the session and leaves an "expired" notice on 401', async () => {
    startSession({ token: 'old', customerNo: 1, firstName: 'A', lastName: 'B' });
    mockFetchResponse({ value: null, success: false, results: [] }, 401);

    await Executer.getCustomerSummary();

    expect(getSession()).toBeNull();
    expect(hasExpiredNotice()).toBe(true);

    startSession({ token: 'new', customerNo: 1, firstName: 'A', lastName: 'B' });
    expect(hasExpiredNotice()).toBe(false);
  });

  it('returns a translated failure when the server cannot be reached', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => Promise.reject(new TypeError('Failed to fetch'))));
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const response = await Executer.getCityList();

    expect(response.success).toBe(false);
    expect(response.results[0].errorCode).toBe(ErrorCodes.Network);
    expect(response.results[0].message).toContain('İşleminizi şu anda gerçekleştiremiyoruz');
  });
});
