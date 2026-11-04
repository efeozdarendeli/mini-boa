# Kurulum

## Gerekenler
- Docker Desktop
- .NET 10 SDK
- Node.js 20+ (frontend için)

## En kolay yol: tek tıkla başlatma (Windows)
Proje kökündeki **`start.cmd`** dosyasına çift tıklayın:
1. Docker kapalıysa açar ve hazır olmasını bekler
2. Veritabanını başlatır
3. API'yi ayrı bir pencerede başlatır ve `/health` "Healthy" diyene kadar bekler
4. Önyüzü ayrı bir pencerede başlatır (ilk seferde `npm install` dahil)
5. Tarayıcıda `http://localhost:5173` açar

Kapatmak için **`stop.cmd`**. Komut satırından seçeneklerle de çalıştırılabilir:
```bash
start.cmd -Kiosk
```
```bash
stop.cmd -ResetData
```
`-Kiosk` tarayıcıyı self-servis cihazı gibi adres çubuğu olmadan tam ekran açar (çıkış Alt+F4). `-ResetData` veritabanını siler; bir sonraki başlatmada demo verisi baştan yüklenir.

> `.cmd` dosyaları asıl script'leri (`scripts/start.ps1`, `scripts/stop.ps1`) `-ExecutionPolicy Bypass` ile çalıştırır. Bu sadece o çalıştırma için geçerlidir; bilgisayarın PowerShell güvenlik ayarı değişmez.

Aşağıdaki adımlar her şeyi elle başlatmak içindir.

## 1. Veritabanı
Proje kök klasöründe:
```bash
docker compose up -d --wait
```
PostgreSQL `localhost:5433` adresinde açılır.

| Ayar | Değer |
|---|---|
| Veritabanı | `miniboa` |
| Kullanıcı | `miniboa` |
| Şifre | `miniboa_dev` (yerel demo şifresi) |

Veritabanını sıfırlamak için (tüm veri silinir, seed yeniden çalışır):
```bash
docker compose down -v
docker compose up -d --wait
```

## 2. Backend testleri
```bash
cd backend
dotnet test MiniBoa.slnx
```
Testlerin bir kısmı gerçek veritabanına bağlandığı için önce 1. adım çalıştırılmış olmalı.

## 3. API'yi çalıştırma
```bash
cd backend
dotnet run --project MiniBoa.Communication
```
API `http://localhost:5080` adresinde açılır.
- Çağrılabilir işlemler: `http://localhost:5080/api/operations`
- Örnek istekler: `backend/MiniBoa.Communication/MiniBoa.Communication.http`

## 4. Frontend'i çalıştırma
API açıkken, ayrı bir terminalde:
```bash
cd frontend
npm install
npm run dev
```
PowerShell "running scripts is disabled" hatası verirse `npm` yerine `npm.cmd` yazın (`npm.cmd install`, `npm.cmd run dev`).

Tarayıcıda `http://localhost:5173` açılır. API adresi farklıysa `frontend/.env.example` dosyasını `.env.local` adıyla kopyalayıp `VITE_API_BASE_URL` değerini değiştirin.

Frontend testleri:
```bash
cd frontend
npm test
```

> **Not:** API çalışırken `dotnet test` derleme yapamaz (çalışan uygulama dll dosyalarını kilitler). Backend testlerinden önce API'yi durdurun.

## Sorun giderme

| Belirti | Sebep | Çözüm |
|---|---|---|
| `dotnet test` / `dotnet build`: "dosya ... tarafından kilitlendi (MiniBoa.Communication)" | API çalışıyor ve dll dosyalarını kullanıyor | API'yi durdurun (çalıştığı terminalde `Ctrl+C`), sonra tekrar deneyin |
| `npm ci` / `npm install`: `EPERM` hatası | `npm run dev` çalışıyor ve `node_modules` içindeki dosyaları kullanıyor | Geliştirme sunucusunu durdurun, sonra tekrar deneyin |
| Frontend açılıyor ama "İşleminizi şu anda gerçekleştiremiyoruz" | API kapalı veya farklı bir adreste | API'nin `http://localhost:5080`'de açık olduğunu kontrol edin |
| API açılmıyor: "Connection refused" / veritabanı hatası | PostgreSQL konteyneri kapalı | `docker compose up -d --wait` |
| `Port 5173 is already in use` | Başka bir Vite sunucusu açık | Diğer sunucuyu kapatın; port bilinçli olarak sabit (backend CORS ayarı bu adrese izin veriyor) |
| Demo verisi bozuldu / denemeler sonrası temiz başlamak istiyorum | — | `stop.cmd -ResetData`, sonra `start.cmd` |
| "Hesabınız geçici olarak kilitlendi" | 3 kez yanlış şifre / yanlış kimlik bilgisi girildi | 5 dakika bekleyin ya da `stop.cmd -ResetData` |
| Kullanıcı kendiliğinden çıkış yaptı | 2 dakika hareketsizlik + 30 saniye uyarı (kiosk güvenliği) | Bilinçli davranış; tekrar giriş yapın |
| Üstte "Sunucuya bağlanılamıyor" şeridi | API kapalı | API'yi başlatın, şeritteki "Tekrar dene"ye basın |
| `start.cmd`'de "API 120 saniye içinde hazır olmadı" | Veritabanı kapalı ya da build hatası | API penceresindeki hata mesajına bakın |

## Önerilen demo akışı (yaklaşık 7 dakika)
1. `start.cmd` → karşılama ekranı; ENGLISH ile dil değişimini gösterin, sonra TÜRKÇE'ye dönün.
2. **Müşteri Olmak İstiyorum** → TCKN `12345678950`, telefon `0532 111 22 33` (yazarken biçimlendiğini gösterin) → sistem "zaten müşterisiniz" deyip giriş ekranına yönlendirir (`GoCardLogin`).
3. Şifre `123456` (göz ikonuyla gösterilebilir) ile giriş → ana sayfa: "Son girişiniz" satırı; göz ikonuyla bakiyeyi gösterin.
4. **Para Transferi** → IBAN alanına `TR06 0099 9000 0010 0000 0200 01` yapıştırın → alıcı adı "Ay\*\* Ka\*\*" olarak görünür → tutar `100` → onay → dekont → **Dekontu Yazdır**.
5. **Hesaplarım** → yeni bakiye ve hareket; dönem ve Gelen/Giden filtresini gösterin.
6. **Cari Hesap Aç** → USD → yeni hesap listede.
7. **Profilim** → maskeli TCKN; ilçeyi değiştirip kaydedin (il listesinde 81 il).
8. Çıkış → **Giriş** → Ayşe'nin TCKN'si `23456789138` ile 3 kez yanlış şifre → hesap kilitlenir.
9. **Şifremi unuttum** → Hakan: TCKN `12345678950`, telefon `0532 111 22 33`, doğum tarihi `14.05.1990` → yeni şifre → yeni şifreyle giriş.
10. Çıkış → **Müşteri Olmak İstiyorum** → TCKN `34567891238`, telefon `0555 777 88 99` → "yarım kalmış başvuru" uyarısı (`ContinueApplication`).

Demo sonrası veriyi ilk hâline döndürmek için: `stop.cmd -ResetData`.

## Demo kullanıcıları
| Kişi | TCKN | Şifre | Telefon | Doğum tarihi |
|---|---|---|---|---|
| Abdullah Hakan Demir | 12345678950 | 123456 | 0532 111 22 33 | 14.05.1990 |
| Ayşe Kaya | 23456789138 | 123456 | 0541 444 55 66 | 02.11.1995 |
| Mehmet Öztürk | 34567891238 | — (yarım kalmış başvuru) | 0555 777 88 99 | 21.03.1988 |

Telefon ve doğum tarihi "Şifremi unuttum" ekranında kimlik doğrulamak için kullanılır.
