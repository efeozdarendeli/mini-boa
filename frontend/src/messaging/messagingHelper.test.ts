import { describe, expect, it } from 'vitest';
import { getLanguage, setLanguage } from './language';
import { getMessage } from './messagingHelper';

describe('MessagingHelper', () => {
  it('returns the text in the current language', () => {
    expect(getMessage('Welcome', 'Login')).toBe('Giriş');

    setLanguage('en');

    expect(getMessage('Welcome', 'Login')).toBe('Sign in');
  });

  it('can be asked for a specific language', () => {
    expect(getMessage('Common', 'Logout', 'en')).toBe('Sign out');
    expect(getLanguage()).toBe('tr');
  });

  it('remembers the selected language and updates <html lang>', () => {
    setLanguage('en');

    expect(localStorage.getItem('miniboa.language')).toBe('en');
    expect(document.documentElement.lang).toBe('en');
  });
});
