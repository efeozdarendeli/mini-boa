# Bir İsteğin Yolculuğu: "Müşteri Ol" 1. Adım

Bu belge tek bir isteği, ekrandaki butondan veritabanına ve geri, dosya dosya takip eder. Örnek olarak BOA ekran görüntülerindeki `CustomerDecisionByTaxNumber` metodu seçildi.

---

### 1. UI → API
Kullanıcı TCKN ve telefonunu girip "Devam"a basar.

| Sıra | Dosya | Ne olur? |
|---|---|---|
| 1.1 | `frontend/src/ui/xtm/BecomeACustomer/steps/IdentityStep.tsx` | Form gönderilince `manager.submitIdentity()` çağrılır |
| 1.2 | `frontend/src/ui/xtm/BecomeACustomer/useBecomeACustomerManager.ts` | TCKN/GSM biçimi kontrol edilir (hatalıysa sunucuya gidilmez), sonra `Executer.customerDecisionByTaxNumber(contract)` çağrılır |
| 1.3 | `frontend/src/proxy/executer.ts` | `Accept-Language` (ve varsa `Authorization`) başlığı eklenir, istek gönderilir |

Giden istek:
```http
POST /api/execute
Accept-Language: tr

{
  "methodName": "BecomeACustomer.CustomerDecisionByTaxNumber",
  "data": { "identityNumber": "12345678950", "gsmAreaCode": "532", "gsmNumber": "1112233" }
}
```
BOA'daki karşılığı: `BOA.SelfService.Proxy.Executer.CustomerDecisionByTaxNumber(contract)` → `AtmExecute<...>(request)` ve `MethodName = "CustomerDecisionByTaxNumber"`.

### 2. Communication katmanı
| Sıra | Dosya | Ne olur? |
|---|---|---|
| 2.1 | `Middleware/GlobalExceptionHandler.cs` | Aşağıda bir hata olursa yakalamak için bekler |
| 2.2 | `Program.cs` → `UseAuthentication` | Token yok; kullanıcı anonim |
| 2.3 | `Middleware/RequestContextMiddleware.cs` | `RequestContext.Language = "tr"`, yeni bir `TraceId` üretilir |
| 2.4 | `Controllers/ExecuteController.cs` | İsteği `RequestDispatcher`'a verir |
| 2.5 | `Routing/RequestDispatcher.cs` | `OperationRegistry`'de işlemi bulur; `[AnonymousAccess]` olduğu için giriş istemez; `data`'yı `BecomeACustomerInputContract`'a çevirir; DI'dan `BecomeACustomerHelper`'ı alıp metodu çağırır |

### 3. Orchestration katmanı: `BecomeACustomerHelper.CustomerDecisionByTaxNumber`
```csharp
var returnObject = InitializeGenericResponse<KYCCustomerInfoContract>();   // trace: "...started"
var validation = application.ValidateInput(...);                            // TCKN + GSM biçimi
await unitOfWork.ExecuteInTransactionAsync(() => DecideAsync(contract, returnObject));
await AppendToJournalAsync($"KYCProcessType {...}");                         // log.device_journal
return returnObject;
```
`DecideAsync` içinde, BOA'daki gibi sırayla:
1. `person.FindByIdentityNumberAsync`: kişi var mı?
2. `customer.FindByPersonIdAsync`: müşteri mi? Evet ise → `GoCardLogin`
3. `application.FindIncompleteAsync`: tamamlanmamış başvurusu var mı? Evet ise → `ContinueApplication`
4. `application.StartAsync`: yeni başvuru → `NewCustomer`

### 4. Business katmanı
| Dosya | Görevi |
|---|---|
| `Kernel/CustomerApplication.cs` | TCKN algoritması (`Mernis`) ve GSM biçimi; başvuruyu başlatır |
| `Kernel/Person.cs` | Kişiyi TCKN ile arar |
| `Kernel/Customer.cs` | Kişinin müşteri kaydını arar |

### 5. Data katmanı
| Dosya | SQL |
|---|---|
| `Repositories/PersonRepository.cs` | `SELECT ... FROM cus.person WHERE identity_number = @IdentityNumber` |
| `Repositories/CustomerRepository.cs` | `SELECT ... FROM cus.customer WHERE person_id = @PersonId` |
| `Repositories/ApplicationRepository.cs` | `SELECT ... WHERE status IN ('Started','PersonCreated')`, gerekirse `INSERT INTO cus.application` |
| `UnitOfWork.cs` | Hepsi aynı bağlantı ve aynı transaction üzerinde çalışır; sonunda `COMMIT` |
| `Repositories/JournalRepository.cs` | Ayrı bir bağlantıyla `INSERT INTO log.device_journal` |

### 6. Geri dönüş
```json
HTTP 200
X-Trace-Id: 0298dbdc4c1641a2aff3057a405c0087

{
  "value": { "kycProcessType": "GoCardLogin", "applicationId": null },
  "results": [],
  "success": true
}
```
`executer.ts` cevabı olduğu gibi manager'a verir. Manager, BOA'daki `if (kYCProcessType == KYCCustomerInfoContract.KYCProcessTypeEnum.GoCardLogin)` kontrolünün aynısını yapar ve kullanıcıyı giriş ekranına yönlendirir. `success: false` gelirse `useContainerManager().showErrorPage(...)` veya `showDialog(...)` ile mesaj gösterilir.

### Hata olsaydı?
| Durum | Nerede yakalanır? | Cevap |
|---|---|---|
| TCKN geçersiz | `CustomerApplication.ValidateInput` | 200, `success: false`, "T.C. kimlik numarası geçersiz." |
| `data` bozuk | `RequestDispatcher.TryReadInput` | 400, "İstek eksik veya hatalı." |
| Veritabanı kapalı | `GlobalExceptionHandler` | 500, "İşleminizi şu anda gerçekleştiremiyoruz..." + günlükte ayrıntı |
