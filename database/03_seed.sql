-- =============================================================================
-- MiniBoa demo verisi
-- Tüm kişiler hayalidir. TCKN ve IBAN'lar algoritmaya göre geçerli üretilmiş
-- test değerleridir. Demo müşterilerin şifresi: 123456
-- İller ve ilçeler 02_locations.sql'de; bu dosya ondan sonra çalışır.
-- =============================================================================

-- -----------------------------------------------------------------------------
-- Dövizler ve kurlar
-- -----------------------------------------------------------------------------
INSERT INTO prm.currency (code, name) VALUES
    ('TRY', 'Türk Lirası'),
    ('USD', 'Amerikan Doları'),
    ('EUR', 'Euro'),
    ('GBP', 'İngiliz Sterlini'),
    ('XAU', 'Altın (gram)');

INSERT INTO prm.currency_rate (currency_code, rate_date, buy_rate, sell_rate) VALUES
    ('USD', CURRENT_DATE, 41.8500, 42.3500),
    ('EUR', CURRENT_DATE, 48.6000, 49.2000),
    ('GBP', CURRENT_DATE, 55.9000, 56.7000),
    ('XAU', CURRENT_DATE, 5420.0000, 5510.0000);

-- -----------------------------------------------------------------------------
-- Kişiler
--   1) Abdullah Hakan Demir : müşteri, 3 hesabı var
--   2) Ayşe Kaya            : müşteri, 1 hesabı var
--   3) Mehmet Öztürk        : başvurusu yarım kalmış (kişi kaydı var, müşteri değil)
-- -----------------------------------------------------------------------------
INSERT INTO cus.person
    (identity_number, first_name, last_name, birth_date, gsm_area_code, gsm_number, email, city_id, county_id, address_line)
VALUES
    ('12345678950', 'Abdullah Hakan', 'Demir', '1990-05-14', '532', '1112233', 'hakan.demir@example.com',
        34, (SELECT id FROM prm.county WHERE city_id = 34 AND name = 'Ataşehir'), 'Atatürk Mah. Örnek Sok. No:1'),
    ('23456789138', 'Ayşe', 'Kaya', '1995-11-02', '541', '4445566', 'ayse.kaya@example.com',
        6, (SELECT id FROM prm.county WHERE city_id = 6 AND name = 'Çankaya'), 'Kızılay Mah. Deneme Cad. No:5'),
    ('34567891238', 'Mehmet', 'Öztürk', '1988-03-21', '555', '7778899', NULL,
        NULL, NULL, NULL);

-- customer_no identity'den gelir: Hakan -> 10000001, Ayşe -> 10000002
INSERT INTO cus.customer (person_id, password_hash)
SELECT id, crypt('123456', gen_salt('bf'))
FROM cus.person
WHERE identity_number IN ('12345678950', '23456789138')
ORDER BY id;

INSERT INTO cus.application (identity_number, gsm_area_code, gsm_number, status, person_id)
SELECT identity_number, gsm_area_code, gsm_number, 'PersonCreated', id
FROM cus.person
WHERE identity_number = '34567891238';

-- -----------------------------------------------------------------------------
-- Hesaplar (IBAN: TR + kontrol + 00999 hayali banka kodu + 0 + müşteri no(12) + ek no(4))
-- -----------------------------------------------------------------------------
INSERT INTO acc.account (customer_no, account_suffix, currency_code, account_type, iban, balance) VALUES
    (10000001, 1, 'TRY', 'Current',       'TR550099900000100000010001', 15250.75),
    (10000001, 2, 'TRY', 'Participation', 'TR280099900000100000010002', 50000.00),
    (10000001, 3, 'USD', 'Current',       'TR980099900000100000010003',  1200.00),
    (10000002, 1, 'TRY', 'Current',       'TR060099900000100000020001',  3400.00);

-- -----------------------------------------------------------------------------
-- Örnek hareketler: Hakan'dan Ayşe'ye 250 TL transfer (iki bacak, aynı reference_id)
-- Bakiyeler yukarıda bu transfer sonrası hâliyle girilmiştir.
-- -----------------------------------------------------------------------------
INSERT INTO txn.transaction (account_id, reference_id, direction, amount, balance_after, description)
SELECT a.id, '7f3c2a9e-1b4d-4c8a-9e21-5d6f7a8b9c01', v.direction, 250.00, v.balance_after, v.description
FROM (VALUES
        ('TR550099900000100000010001', 'Debit',  15250.75, 'Ayşe Kaya''ya transfer'),
        ('TR060099900000100000020001', 'Credit',  3400.00, 'Abdullah Hakan Demir''den gelen transfer')
     ) AS v (iban, direction, balance_after, description)
JOIN acc.account a ON a.iban = v.iban;
