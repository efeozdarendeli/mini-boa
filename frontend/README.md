# MiniBoa – Önyüz

MiniBoa'nın React + TypeScript önyüzü. Genel bakış ve kurulum için kök dizindeki [README](../README.md) dosyasına bakın.

## Komutlar

| Komut | Ne yapar? |
|---|---|
| `npm run dev` | Geliştirme sunucusu: `http://localhost:5173` (backend `http://localhost:5080`'de açık olmalı) |
| `npm test` | Bütün testleri bir kez çalıştırır (Vitest) |
| `npm run test:watch` | Dosya değiştikçe testleri yeniden çalıştırır |
| `npm run build` | Tip kontrolü + üretim derlemesi (`dist/`) |
| `npm run lint` | Kod denetimi (oxlint) |

Backend adresi farklıysa `.env.example` dosyasını `.env.local` adıyla kopyalayıp `VITE_API_BASE_URL` değerini değiştirin.

## Yapı

| Klasör | BOA'daki karşılığı | İçerik |
|---|---|---|
| `src/ui/xtm/` | UI / XTM | Ekranlar: `XView.tsx` (görünüm) + `useXManager.ts` (mantık) |
| `src/proxy/` | `BOA.SelfService.Proxy` | `executer.ts`, oturum, önbellek sorguları |
| `src/container/` | `UIManagerFactory.ContainerManager` | Hata sayfası, dialog, ortak çerçeve |
| `src/messaging/` | `MessagingHelper` + `ResourceTR.xaml` | TR/EN metinler |
| `src/types/` | `BOA.Types.SelfService` | Backend contract'larının TypeScript karşılıkları |
| `src/shared/` | — | Ortak bileşenler, doğrulama, biçimlendirme, `traceUtil` |
| `src/app/` | Container | Router, önbellek, giriş kontrolü |
| `src/test/` | — | Test yardımcıları (sahte API, ekran açma) |


## Yeni bir ekran eklemek

1. `src/ui/xtm/YeniEkran/` altında `YeniEkranView.tsx` ve `useYeniEkranManager.ts` oluşturun.
2. Metinleri `src/messaging/resources/tr.ts` ve `en.ts` dosyalarına ekleyin (eksik anahtar derleme hatası verir).
3. Backend işlemi gerekiyorsa `src/proxy/executer.ts` içindeki `Executer` nesnesine tipli bir fonksiyon ekleyin.
4. Adresi `src/app/routes.ts` dosyasına, ekranı `src/app/router.tsx` dosyasına ekleyin; giriş gerektiriyorsa `RequireAuth` grubuna koyun.
5. `YeniEkranView.test.tsx` yazın (`renderApp` + `mockApi`).
