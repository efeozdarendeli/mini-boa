-- =============================================================================
-- MiniBoa veritabanı şeması
-- Tablolar iş alanına göre şemalara ayrılmıştır (BOA'daki modül ayrımına benzer):
--   prm : parametreler (il, ilçe, döviz, kur)
--   cus : müşteri (kişi, müşteri, başvuru)
--   acc : hesap
--   txn : hesap hareketleri
--   log : cihaz / işlem günlüğü
-- =============================================================================

CREATE EXTENSION IF NOT EXISTS pgcrypto;   -- seed'deki şifre hash'leri (bcrypt) için

CREATE SCHEMA prm;
CREATE SCHEMA cus;
CREATE SCHEMA acc;
CREATE SCHEMA txn;
CREATE SCHEMA log;

-- -----------------------------------------------------------------------------
-- PRM: Parametreler
-- -----------------------------------------------------------------------------
CREATE TABLE prm.city (
    id      smallint     PRIMARY KEY,          -- plaka kodu
    name    varchar(50)  NOT NULL
);

CREATE TABLE prm.county (
    id       integer      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    city_id  smallint     NOT NULL REFERENCES prm.city (id),
    name     varchar(50)  NOT NULL,
    UNIQUE (city_id, name)
);

CREATE TABLE prm.currency (
    code    char(3)      PRIMARY KEY,          -- ISO 4217 (TRY, USD...); altın için XAU
    name    varchar(50)  NOT NULL
);

CREATE TABLE prm.currency_rate (
    currency_code  char(3)        NOT NULL REFERENCES prm.currency (code),
    rate_date      date           NOT NULL,
    buy_rate       numeric(18, 4) NOT NULL CHECK (buy_rate > 0),   -- bankanın alış kuru
    sell_rate      numeric(18, 4) NOT NULL CHECK (sell_rate > 0),  -- bankanın satış kuru
    PRIMARY KEY (currency_code, rate_date)
);

-- -----------------------------------------------------------------------------
-- CUS: Müşteri
-- Kişi (person) ile müşteri (customer) ayrıdır: bir kişi kaydı müşteri olmadan
-- da var olabilir (yarım kalmış başvuru). BOA'daki Person / Customer ayrımı.
-- -----------------------------------------------------------------------------
CREATE TABLE cus.person (
    id               bigint        GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    identity_number  char(11)      NOT NULL UNIQUE,   -- TCKN
    first_name       varchar(50)   NOT NULL,
    last_name        varchar(50)   NOT NULL,
    birth_date       date          NOT NULL,
    gsm_country      varchar(4)    NOT NULL DEFAULT '90',
    gsm_area_code    varchar(4)    NOT NULL,
    gsm_number       varchar(8)    NOT NULL,
    email            varchar(100),
    city_id          smallint      REFERENCES prm.city (id),
    county_id        integer       REFERENCES prm.county (id),
    address_line     varchar(250),
    created_at       timestamptz   NOT NULL DEFAULT now()
);

CREATE TABLE cus.customer (
    customer_no    bigint        GENERATED ALWAYS AS IDENTITY (START WITH 10000001) PRIMARY KEY,
    person_id      bigint        NOT NULL UNIQUE REFERENCES cus.person (id),
    status         varchar(10)   NOT NULL DEFAULT 'Active' CHECK (status IN ('Active', 'Passive')),
    password_hash  text          NOT NULL,          -- bcrypt; düz şifre asla saklanmaz
    -- Kaba kuvvet koruması: art arda hatalı denemeler sayılır, sınıra gelince hesap geçici kilitlenir.
    failed_login_count  smallint     NOT NULL DEFAULT 0,
    locked_until        timestamptz,
    -- "Son girişiniz" bilgisi: yeni girişte last_login_at, previous_login_at'e kaydırılır.
    last_login_at       timestamptz,
    previous_login_at   timestamptz,
    created_at     timestamptz   NOT NULL DEFAULT now()
);

-- Self-servis "Müşteri Ol" başvuruları. Yarım kalan başvuru tekrar devam ettirilebilir.
CREATE TABLE cus.application (
    id               bigint        GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    identity_number  char(11)      NOT NULL,
    gsm_country      varchar(4)    NOT NULL DEFAULT '90',
    gsm_area_code    varchar(4)    NOT NULL,
    gsm_number       varchar(8)    NOT NULL,
    status           varchar(20)   NOT NULL DEFAULT 'Started'
                     CHECK (status IN ('Started', 'PersonCreated', 'Completed', 'Cancelled')),
    person_id        bigint        REFERENCES cus.person (id),
    created_at       timestamptz   NOT NULL DEFAULT now(),
    updated_at       timestamptz   NOT NULL DEFAULT now()
);

CREATE INDEX ix_application_identity_number ON cus.application (identity_number);

-- -----------------------------------------------------------------------------
-- ACC: Hesap
-- Hesap numarası = müşteri no + ek no (suffix). Örn: 10000001-1, 10000001-2
-- -----------------------------------------------------------------------------
CREATE TABLE acc.account (
    id              bigint          GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    customer_no     bigint          NOT NULL REFERENCES cus.customer (customer_no),
    account_suffix  smallint        NOT NULL,
    branch_code     smallint        NOT NULL DEFAULT 1,
    currency_code   char(3)         NOT NULL REFERENCES prm.currency (code),
    account_type    varchar(20)     NOT NULL CHECK (account_type IN ('Current', 'Participation')),
    iban            char(26)        NOT NULL UNIQUE,
    balance         numeric(18, 2)  NOT NULL DEFAULT 0 CHECK (balance >= 0),  -- eksi bakiyeye DB de izin vermez
    status          varchar(10)     NOT NULL DEFAULT 'Active' CHECK (status IN ('Active', 'Closed')),
    opened_at       timestamptz     NOT NULL DEFAULT now(),
    UNIQUE (customer_no, account_suffix)
);

-- -----------------------------------------------------------------------------
-- TXN: Hesap hareketleri
-- Bir transfer iki hareket üretir (gönderende 'Debit' borç, alıcıda 'Credit' alacak);
-- ikisi aynı reference_id ile birbirine bağlanır.
-- -----------------------------------------------------------------------------
CREATE TABLE txn.transaction (
    id             bigint          GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    account_id     bigint          NOT NULL REFERENCES acc.account (id),
    reference_id   uuid            NOT NULL,
    direction      varchar(6)      NOT NULL CHECK (direction IN ('Debit', 'Credit')),  -- Debit: borç (çıkış), Credit: alacak (giriş)
    amount         numeric(18, 2)  NOT NULL CHECK (amount > 0),
    balance_after  numeric(18, 2)  NOT NULL,
    description    varchar(200),
    channel        varchar(20)     NOT NULL DEFAULT 'SelfService',
    created_at     timestamptz     NOT NULL DEFAULT now()
);

CREATE INDEX ix_transaction_account_created ON txn.transaction (account_id, created_at DESC);
CREATE INDEX ix_transaction_reference ON txn.transaction (reference_id);

-- -----------------------------------------------------------------------------
-- LOG: Cihaz günlüğü (BOA'daki DeviceJournalManager.AppendLineToJournal karşılığı)
-- -----------------------------------------------------------------------------
CREATE TABLE log.device_journal (
    id           bigint        GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    trace_id     varchar(32)   NOT NULL,
    customer_no  bigint,
    method_name  varchar(100)  NOT NULL,
    message      text          NOT NULL,
    created_at   timestamptz   NOT NULL DEFAULT now()
);

CREATE INDEX ix_device_journal_trace ON log.device_journal (trace_id);
