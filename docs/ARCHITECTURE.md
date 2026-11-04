# MiniBoa Mimarisi

MiniBoa, BOA bankacılık altyapısının katmanlı mimarisini örnek alan, sadeleştirilmiş bir self-servis bankacılık demosudur.

- **Backend:** C# / .NET 10
- **Frontend:** React + TypeScript
- **Veritabanı:** PostgreSQL

## BOA katmanları ve MiniBoa karşılıkları

| BOA katmanı | MiniBoa karşılığı |
|---|---|
| Declarative UI (XAML) | `frontend/src/ui/xtm/*View.tsx` (görünüm) + `use*Manager.ts` (ekran mantığı): Welcome, Login, ForgotPassword, BecomeACustomer, Dashboard, Accounts, OpenAccount, Transfer, Profile, Rates |
| Client Caching | TanStack Query (`frontend/src/proxy/parameterQueries.ts`, `accountQueries.ts`) |
| İstemci proxy'si (`BOA.SelfService.Proxy.Executer`) | `frontend/src/proxy/executer.ts` |
| `UIManagerFactory.ContainerManager` | `frontend/src/container/` (`showErrorPage`, `showDialog`, hareketsizlik koruması) |
| `MessagingHelper` + `ResourceTR.xaml` | `frontend/src/messaging/` (TR/EN) |
| Communication (WCF) | `MiniBoa.Communication`: tek iş kapısı `POST /api/execute`, `GET /health` |
| Authentication / Authorization | JWT (`TokenService`) + işlem bazında `[AnonymousAccess]` kontrolü; hatalı girişte hesap kilidi |
| Messaging & Routing | `OperationRegistry` + `RequestDispatcher` (`methodName` → helper metodu) |
| Orchestration | `MiniBoa.Orchestration`: BecomeACustomer, Login, PasswordReset, Account, Transfer, Profile, Parameter helper'ları |
| Business Objects / Business Kernel | `MiniBoa.Business/Kernel` (Person, Customer, CustomerApplication, Account, Mernis, Iban...) |
| Transaction Management | `UnitOfWork` + `ExecuteInTransactionAsync` |
| Data Layer | `MiniBoa.Data` (Dapper + PostgreSQL) |
| Tracing & Logging | `ObjectHelper` + dispatcher süre günlüğü + `X-Trace-Id` + `log.device_journal` |
| Exception Handling | `GenericResponse` + `GlobalExceptionHandler` |
| Configuration & Management | `appsettings.json` / `appsettings.Development.json` |
| Ortak tipler (`BOA.Types`) | `MiniBoa.Types` |
| Workflow, Code Generation | Kapsam dışı |

## Projeler arası bağımlılık yönü

Bağımlılık her zaman **yukarıdan aşağıya** doğrudur. Alt katman üst katmanı tanımaz.

```
MiniBoa.Communication   (API: dış dünyaya açılan kapı)
        │
MiniBoa.Orchestration   (iş akışları: "Müşteri Ol", "Transfer")
        │
MiniBoa.Business        (iş nesneleri ve kuralları: Person, Account)
        │
MiniBoa.Data            (SQL ve veritabanı erişimi)
        │
MiniBoa.Framework       (bağlam, loglama, mesajlar, hatalar: herkes kullanır)
        │
MiniBoa.Types           (ortak tipler: GenericResponse, contract'lar)
```

Örneğin Data katmanı hiçbir zaman Orchestration'ı çağıramaz. Bu kural `.csproj` dosyalarındaki `ProjectReference`'larla derleme seviyesinde zorlanır: yanlış yöne bir referans verilirse kod derlenmez.
