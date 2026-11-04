import { describe, expect, it } from 'vitest';
import {
  formatAmountInput,
  formatIban,
  formatMobilePhone,
  formatMoney,
  sortByName,
  toTitleCase,
  toUpperCase,
} from './format';

describe('format', () => {
  it('formats money for the selected language', () => {
    expect(formatMoney(15250.75, 'TRY', 'tr')).toMatch(/15\.250,75/);
    expect(formatMoney(15250.75, 'USD', 'en')).toMatch(/15,250\.75/);
  });

  it('groups IBAN digits by four', () => {
    expect(formatIban('TR550099900000100000010001')).toBe('TR55 0099 9000 0010 0000 0100 01');
  });

  it('sorts names in Turkish alphabetical order', () => {
    const cities = [{ name: 'Konya' }, { name: 'İzmir' }, { name: 'Ankara' }, { name: 'İstanbul' }];

    expect(sortByName(cities, 'tr').map((city) => city.name)).toEqual(['Ankara', 'İstanbul', 'İzmir', 'Konya']);
  });

  it('formats a mobile phone in groups', () => {
    expect(formatMobilePhone('05321112233')).toBe('0532 111 22 33');
    expect(formatMobilePhone('0532')).toBe('0532');
  });

  it('formats amount inputs without a currency symbol', () => {
    expect(formatAmountInput(1250.5, 'tr')).toBe('1.250,50');
    expect(formatAmountInput(1250.5, 'en')).toBe('1,250.50');
  });

  it('capitalizes names with Turkish rules', () => {
    expect(toTitleCase('ibrahim  ÖZTÜRK', 'tr')).toBe('İbrahim Öztürk');
  });

  it('uses Turkish casing rules', () => {
    expect(toUpperCase('ibrahim', 'tr')).toBe('İBRAHİM');
    expect(toUpperCase('ibrahim', 'en')).toBe('IBRAHIM');
  });
});
