import { describe, expect, it } from 'vitest';
import {
  isValidIban,
  isValidIdentityNumber,
  isValidMobilePhone,
  latestAdultBirthDate,
  lettersOnly,
  normalizeIban,
  parseAmount,
  splitMobilePhone,
} from './validation';

describe('validation', () => {
  it.each(['12345678950', '23456789138', '10000000146'])('accepts valid identity number %s', (value) => {
    expect(isValidIdentityNumber(value)).toBe(true);
  });

  it.each(['12345678951', '02345678950', '1234567895', 'abcdefghijk'])('rejects identity number %s', (value) => {
    expect(isValidIdentityNumber(value)).toBe(false);
  });

  it('accepts IBANs with spaces and lower case', () => {
    expect(isValidIban('tr55 0099 9000 0010 0000 0100 01')).toBe(true);
    expect(normalizeIban('tr55 0099')).toBe('TR550099');
  });

  it('rejects an IBAN with a wrong check digit', () => {
    expect(isValidIban('TR560099900000100000010001')).toBe(false);
  });

  it.each([
    ['250', 250],
    ['250,5', 250.5],
    ['1.250,75', 1250.75],
    ['1250.75', 1250.75],
  ])('parses amount %s', (input, expected) => {
    expect(parseAmount(input)).toBe(expected);
  });

  it.each(['0', '-5', '10,555', 'abc', ''])('rejects amount %s', (input) => {
    expect(parseAmount(input)).toBeNull();
  });

  it.each([
    ['05321112233', true],
    ['5321112233', false],
    ['04321112233', false],
    ['0532111223', false],
  ])('mobile phone %s valid: %s', (input, expected) => {
    expect(isValidMobilePhone(input)).toBe(expected);
  });

  it('splits a mobile phone into area code and number', () => {
    expect(splitMobilePhone('05321112233')).toEqual({ gsmAreaCode: '532', gsmNumber: '1112233' });
  });

  it('keeps only letters (including Turkish) and single spaces in names', () => {
    expect(lettersOnly(' Ayşe3  Gül!')).toBe('Ayşe Gül');
  });

  it('gives the latest birth date of an 18-year-old', () => {
    expect(latestAdultBirthDate(new Date(2026, 10, 1))).toBe('2008-11-01');
  });
});
