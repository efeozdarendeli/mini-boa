import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/react';
import { afterEach, vi } from 'vitest';
import { setLanguage } from '../messaging/language';
import { endSession } from '../proxy/session';

// Her testten sonra ekran, mock'lar, dil ve oturum sıfırlanır; testler birbirini etkilemez.
afterEach(() => {
  cleanup();
  vi.unstubAllGlobals();
  vi.restoreAllMocks();
  setLanguage('tr');
  endSession();
});
