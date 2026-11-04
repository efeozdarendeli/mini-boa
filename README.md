# MiniBoa

BOA bankacılık altyapısının **katmanlı mimarisini** örnek alan, sadeleştirilmiş bir self-servis bankacılık demosu. Staj projesi olarak hazırlandı.

Müşteri olma, giriş, şifremi unuttum, hesapları ve hareketleri görüntüleme, hesap açma, para transferi (dekontlu), profil güncelleme ve döviz kurları akışları uçtan uca çalışır.

> Bu bir eğitim demosudur. Kişiler, hesaplar, banka kodu ve kurlar hayalidir. Kurum içi kod kopyalanmamıştır; yalnızca mimari kalıplar (katman ayrımı, `GenericResponse`, `MethodName` ile yönlendirme, View + Manager ayrımı) örnek alınmıştır.

---

## Mimari

```
┌──────────────────────────────────────────────────────────────┐
│  Declarative UI          React ekranları (View + Manager)    │  frontend/src/ui
│  Client Caching          TanStack Query                      │  frontend/src/proxy
│  Proxy / Executer        executer.ts → POST /api/execute     │
├──────────────────────────────────────────────────────────────┤
│  Communication           ASP.NET Core, tek iş kapısı         │  MiniBoa.Communication
│  Messaging & Routing     methodName → orchestration metodu   │
│  Authentication/Authz.   JWT + [AnonymousAccess]             │
├──────────────────────────────────────────────────────────────┤
│  Orchestration           Senaryolar, transaction sınırı      │  MiniBoa.Orchestration
├──────────────────────────────────────────────────────────────┤
│  Business Objects        İş kuralları (Person, Account...)   │  MiniBoa.Business
├──────────────────────────────────────────────────────────────┤
│  Transaction Mgmt.       UnitOfWork                          │  MiniBoa.Data
│  Data Layer              Dapper + SQL                        │
├──────────────────────────────────────────────────────────────┤
│  PostgreSQL 16 (Docker)                                      │  database/
└──────────────────────────────────────────────────────────────┘
  Her katmanı kesen: Tracing & Logging, Exception Handling, Messaging (TR/EN) → MiniBoa.Framework
```

Bağımlılıklar yalnızca **yukarıdan aşağıya** doğrudur; bu kural `.csproj` referanslarıyla derleme seviyesinde zorlanır. Ayrıntılar: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)

## Teknolojiler

| Katman | Teknoloji |
|---|---|
| Önyüz | React 19, TypeScript, Vite, React Router, TanStack Query |
| Backend | C# / .NET 10, ASP.NET Core, Dapper, JWT, bcrypt |
| Veritabanı | PostgreSQL 16 (Docker) |
| Test | xUnit, NSubstitute, WebApplicationFactory · Vitest, Testing Library |

## Hızlı başlangıç

Gerekenler: Docker Desktop, .NET 10 SDK, Node.js 20+

**Tek tıkla (Windows):** proje kökündeki **`start.cmd`** dosyasına çift tıklayın. Veritabanı, API ve önyüz sırayla başlar, hazır olunca tarayıcı açılır. Kapatmak için **`stop.cmd`**.
- `start.cmd -Kiosk`: self-servis cihazı gibi tam ekran (Edge kiosk modu, çıkış Alt+F4)
- `stop.cmd -ResetData`: demo verisini de ilk hâline döndürür

**Elle:**

```bash
# 1. Veritabanı (şema, 81 il / 973 ilçe ve demo verisi otomatik yüklenir)
docker compose up -d --wait

# 2. Backend API → http://localhost:5080
cd backend
dotnet run --project MiniBoa.Communication

# 3. Önyüz → http://localhost:5173   (ayrı bir terminalde)
cd frontend
npm install
npm run dev
```

**Demo girişi:** TCKN `12345678950`, şifre `123456`

Diğer demo kişileri, adım adım 5 dakikalık demo akışı ve sorun giderme: [docs/SETUP.md](docs/SETUP.md)

## Proje yapısı

```
mini-boa/
├─ backend/
│  ├─ MiniBoa.Types/          GenericResponse, contract'lar, enum'lar
│  ├─ MiniBoa.Framework/      RequestContext, ObjectHelper, mesajlar, hatalar
│  ├─ MiniBoa.Data/           UnitOfWork, repository'ler (SQL)
│  ├─ MiniBoa.Business/       İş nesneleri (Kernel), Mernis, IBAN, şifre
│  ├─ MiniBoa.Orchestration/  Helper'lar: BecomeACustomer, Login, PasswordReset, Account, Transfer, Profile, Parameter
│  ├─ MiniBoa.Communication/  API, routing, JWT, middleware
│  └─ MiniBoa.Tests/          Birim, entegrasyon ve HTTP testleri
├─ frontend/src/
│  ├─ ui/xtm/                 Ekranlar (View + Manager)
│  ├─ proxy/                  Executer, oturum, önbellek sorguları
│  ├─ container/              ContainerManager (hata sayfası, dialog), çerçeve
│  ├─ messaging/              TR/EN metinler
│  └─ shared/                 Ortak bileşenler, doğrulama, biçimlendirme
├─ database/                  01_schema.sql, 02_locations.sql (81 il, 973 ilçe), 03_seed.sql
├─ docs/                      Mimari, faz notları, istek akışı, sorular
├─ scripts/                   start.ps1 / stop.ps1 (start.cmd ve stop.cmd bunları çalıştırır)
├─ .github/workflows/ci.yml   Her push'ta build + testler
└─ docker-compose.yml
```

## Testler

```bash
cd backend && dotnet test MiniBoa.slnx     # 127 test (veritabanı açık olmalı, API kapalı olmalı)
cd frontend && npm test                    # 86 test
```

| Test türü | Ne kanıtlıyor? |
|---|---|
| Birim (mock'lu) | İş kuralları: yaş, şifre, transfer kuralları, TCKN/IBAN algoritmaları |
| Entegrasyon (gerçek PostgreSQL) | SQL ve tip dönüşümleri, transaction/rollback, satır kilidi |
| Uçtan uca orchestration | "Müşteri Ol → Giriş → Ana sayfa", transfer; seed verisi korunur |
| HTTP (API) | 401/404/400 durumları, değiştirilmiş token, dil, TraceId |
| Ekran (frontend) | Gerçek ekran ağacı + sahte API: giriş, Müşteri Ol, ana sayfa, transfer |

## Belgeler

| Belge | İçerik |
|---|---|
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | BOA katmanları ↔ MiniBoa karşılıkları |
| [docs/REQUEST-FLOW.md](docs/REQUEST-FLOW.md) | Bir isteğin butondan veritabanına ve geri, dosya dosya yolculuğu |
| [docs/SETUP.md](docs/SETUP.md) | Kurulum, demo kullanıcıları, demo akışı |

## Bilinçli sadeleştirmeler

Gerçek bir bankacılık sisteminde olması gereken ama demo kapsamı dışında bırakılan konular:

| Konu | Demoda | Gerçek sistemde |
|---|---|---|
| Kimlik doğrulama (Mernis) | Sadece TCKN algoritması | NVİ servisi |
| Müşteri Ol doğrulaması | Başvuru no + TCKN eşleşmesi | Telefona SMS ile tek kullanımlık kod (OTP) |
| Şifremi unuttum | TCKN + kayıtlı telefon + doğum tarihi | Bunlara ek olarak SMS ile tek kullanımlık kod |
| Şifre denemeleri | 3 hatalı denemede 5 dk hesap kilidi | Ayrıca IP bazında istek sınırı (rate limiting) |
| Günlük transfer limiti | Hesap başına, hesabın döviz cinsinde 50.000 | Müşteri bazında, TL karşılığıyla, ayarlanabilir |
| Oturum | 30 dk JWT, `sessionStorage` | Yenileme token'ı, `HttpOnly` cookie, çıkışta token iptali |
| İletişim | HTTP (yerel) | Zorunlu HTTPS |
| Gizli anahtarlar | `appsettings.Development.json` | Ortam değişkeni / secret store |
| Kurlar | Seed'de sabit | Piyasa verisi servisi |
| Workflow, Code Generation | Yok | BOA mimarisinde ayrı katmanlar |
| Kiosk / donanım (kart okuyucu, kamera) | Yok; tarayıcıda çalışır | Masaüstü kabuk uygulaması |
