using System.Globalization;
using MiniBoa.Data.Entities;
using MiniBoa.Data.Repositories;
using MiniBoa.Framework;
using MiniBoa.Framework.Messaging;
using MiniBoa.Types;

namespace MiniBoa.Business.Kernel;

/// <summary>
/// Hesap iş nesnesi (BOA'daki Business.Kernel.Account.Account karşılığı):
/// hesap listeleme, hesap açma ve para transferi kuralları.
/// </summary>
public sealed class Account(
    IAccountRepository accounts,
    ITransactionRepository transactions,
    RequestContext context,
    TimeProvider timeProvider,
    IMessagingHelper messaging) : BusinessObjectBase(messaging)
{
    public const int MaxAccountsPerCustomer = 10;

    /// <summary>
    /// Bir hesaptan bir günde gönderilebilecek en yüksek tutar (hesabın kendi döviz cinsinde).
    /// Demo sadeleştirmesi: gerçek bankalarda limit müşteri bazında ve TL karşılığıyla hesaplanır.
    /// </summary>
    public const decimal DailyTransferLimit = 50_000m;

    /// <summary>Hareket listesinde seçilebilecek dönemler (gün).</summary>
    public static readonly int[] TransactionPeriods = [7, 30, 90];

    private static readonly HashSet<string> SupportedCurrencies = ["TRY", "USD", "EUR", "GBP"];

    public async Task<GenericResponse<IReadOnlyList<AccountEntity>>> GetAccountsAsync(long customerNo) =>
        GenericResponse<IReadOnlyList<AccountEntity>>.Ok(await accounts.GetByCustomerNoAsync(customerNo));

    /// <summary>Hesap müşteriye ait değilse "bulunamadı" döner; başkasının hesabının varlığı sızdırılmaz.</summary>
    public async Task<GenericResponse<AccountEntity>> GetOwnedAccountAsync(long customerNo, long accountId)
    {
        var account = await accounts.GetByIdAsync(accountId);

        return account is null || account.CustomerNo != customerNo
            ? Fail<AccountEntity>(ErrorCodes.NotFound, "AccountNotFound")
            : GenericResponse<AccountEntity>.Ok(account);
    }

    /// <param name="days">Son kaç günün hareketleri: 7, 30 veya 90.</param>
    /// <param name="direction">Sadece gelen (Credit) veya giden (Debit); null ise hepsi.</param>
    public async Task<GenericResponse<IReadOnlyList<TransactionEntity>>> GetTransactionsAsync(
        long customerNo, long accountId, int days, TransactionDirection? direction)
    {
        if (!TransactionPeriods.Contains(days))
        {
            return Fail<IReadOnlyList<TransactionEntity>>(ErrorCodes.Validation, "InvalidPeriod");
        }

        var account = await GetOwnedAccountAsync(customerNo, accountId);
        if (!account.Success)
        {
            return new GenericResponse<IReadOnlyList<TransactionEntity>>().AddResults(account);
        }

        var since = timeProvider.GetUtcNow().UtcDateTime.AddDays(-days);
        return GenericResponse<IReadOnlyList<TransactionEntity>>.Ok(
            await transactions.GetByAccountIdAsync(accountId, since, direction));
    }

    /// <summary>Ana sayfadaki "Son İşlemler": müşterinin bütün hesaplarındaki en son hareketler.</summary>
    public const int RecentTransactionCount = 10;

    public async Task<GenericResponse<IReadOnlyList<RecentTransactionEntity>>> GetRecentTransactionsAsync(long customerNo) =>
        GenericResponse<IReadOnlyList<RecentTransactionEntity>>.Ok(
            await transactions.GetRecentByCustomerNoAsync(customerNo, RecentTransactionCount));

    /// <summary>
    /// Transfer ekranında IBAN girilince alıcının adını maskeli döner ("Ay** Ka**").
    /// Kullanıcı yanlış kişiye göndermediğini görür, ama alıcının tam adı açığa çıkmaz.
    /// </summary>
    public async Task<GenericResponse<string>> GetReceiverNameAsync(string iban)
    {
        if (!Iban.IsValid(iban))
        {
            return Fail<string>(ErrorCodes.Validation, "InvalidIban");
        }

        var holder = await accounts.GetHolderByIbanAsync(iban);
        if (holder is null || holder.Status != AccountStatus.Active)
        {
            return Fail<string>(ErrorCodes.NotFound, "ReceiverNotFound");
        }

        return GenericResponse<string>.Ok(MaskName($"{holder.FirstName} {holder.LastName}"));
    }

    /// <summary>Her kelimenin ilk iki harfi görünür, gerisi yıldız: "Ayşe Kaya" → "Ay** Ka**".</summary>
    public static string MaskName(string fullName) =>
        string.Join(' ', fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.Length <= 2 ? word[..1] + "*" : word[..2] + new string('*', word.Length - 2)));

    public async Task<GenericResponse<AccountEntity>> OpenAccountAsync(
        long customerNo, string currencyCode, AccountType accountType)
    {
        if (!SupportedCurrencies.Contains(currencyCode))
        {
            return Fail<AccountEntity>(ErrorCodes.Validation, "UnsupportedCurrency");
        }

        var existing = await accounts.GetByCustomerNoAsync(customerNo);
        if (existing.Count >= MaxAccountsPerCustomer)
        {
            return Fail<AccountEntity>(ErrorCodes.BusinessRule, "AccountLimitReached");
        }

        // BOA'daki GetAccountSuffix: müşterinin bir sonraki hesap ek numarası.
        // Aynı anda iki hesap açılırsa UNIQUE (customer_no, account_suffix) kısıtı ikincisini reddeder.
        var suffix = await accounts.GetNextSuffixAsync(customerNo);

        var id = await accounts.InsertAsync(new AccountEntity
        {
            CustomerNo = customerNo,
            AccountSuffix = suffix,
            BranchCode = 1,
            CurrencyCode = currencyCode,
            AccountType = accountType,
            Iban = Iban.Generate(customerNo, suffix),
            Balance = 0m
        });

        return GenericResponse<AccountEntity>.Ok((await accounts.GetByIdAsync(id))!);
    }

    /// <summary>
    /// Para transferi. Açık bir transaction içinde çağrılmalıdır (Orchestration açar):
    /// iki hesap kilitlenir, iki bakiye güncellenir ve iki hareket satırı yazılır; ya hepsi olur ya hiçbiri.
    /// Tüm kontroller yazma işlemlerinden ÖNCE yapılır.
    /// </summary>
    public async Task<GenericResponse<TransferResult>> TransferAsync(
        long customerNo, long fromAccountId, string toIban, decimal amount, string? description)
    {
        if (amount <= 0 || decimal.Round(amount, 2) != amount)
        {
            return Fail<TransferResult>(ErrorCodes.Validation, "InvalidAmount");
        }

        if (!Iban.IsValid(toIban))
        {
            return Fail<TransferResult>(ErrorCodes.Validation, "InvalidIban");
        }

        var receiver = await accounts.GetByIbanAsync(toIban);
        if (receiver is null)
        {
            return Fail<TransferResult>(ErrorCodes.NotFound, "ReceiverNotFound");
        }

        if (receiver.Id == fromAccountId)
        {
            return Fail<TransferResult>(ErrorCodes.BusinessRule, "SameAccount");
        }

        // Kilitledikten sonra güncel bakiyeyle çalışılır; bu noktadan sonra kimse bu iki hesabı değiştiremez.
        var locked = await accounts.GetForUpdateAsync(fromAccountId, receiver.Id);
        var from = locked.SingleOrDefault(a => a.Id == fromAccountId);
        var to = locked.Single(a => a.Id == receiver.Id);

        var check = CheckTransferRules(customerNo, from, to, amount);
        if (!check.Success)
        {
            return check;
        }

        // Limit de kilitten sonra kontrol edilir: aynı anda gelen iki transfer limiti birlikte aşamaz.
        var limitCheck = await CheckDailyLimitAsync(from!, amount);
        if (!limitCheck.Success)
        {
            return limitCheck;
        }

        var referenceId = Guid.NewGuid();
        var senderBalance = from!.Balance - amount;
        var receiverBalance = to.Balance + amount;

        await accounts.UpdateBalanceAsync(from.Id, senderBalance);
        await accounts.UpdateBalanceAsync(to.Id, receiverBalance);
        await transactions.InsertAsync(NewTransaction(from.Id, referenceId, TransactionDirection.Debit, amount, senderBalance, description));
        await transactions.InsertAsync(NewTransaction(to.Id, referenceId, TransactionDirection.Credit, amount, receiverBalance, description));

        return GenericResponse<TransferResult>.Ok(
            new TransferResult(referenceId, senderBalance, timeProvider.GetUtcNow().UtcDateTime));
    }

    private async Task<GenericResponse<TransferResult>> CheckDailyLimitAsync(AccountEntity from, decimal amount)
    {
        // "Bugün" müşterinin yerel saatine göre gece yarısından itibaren sayılır.
        var localNow = timeProvider.GetLocalNow();
        var startOfToday = new DateTimeOffset(localNow.Date, localNow.Offset).UtcDateTime;

        var sentToday = await transactions.GetDebitTotalSinceAsync(from.Id, startOfToday);
        if (sentToday + amount <= DailyTransferLimit)
        {
            return new GenericResponse<TransferResult>();
        }

        var remaining = Math.Max(DailyTransferLimit - sentToday, 0m);
        return Fail<TransferResult>(
            ErrorCodes.BusinessRule,
            "DailyLimitExceeded",
            FormatAmount(DailyTransferLimit, from.CurrencyCode),
            FormatAmount(remaining, from.CurrencyCode));
    }

    private string FormatAmount(decimal amount, string currencyCode)
    {
        var culture = CultureInfo.GetCultureInfo(context.Language == "en" ? "en-GB" : "tr-TR");
        return $"{amount.ToString("N2", culture)} {currencyCode}";
    }

    private GenericResponse<TransferResult> CheckTransferRules(
        long customerNo, AccountEntity? from, AccountEntity to, decimal amount)
    {
        if (from is null || from.CustomerNo != customerNo)
        {
            return Fail<TransferResult>(ErrorCodes.NotFound, "AccountNotFound");
        }

        if (from.Status != AccountStatus.Active || to.Status != AccountStatus.Active)
        {
            return Fail<TransferResult>(ErrorCodes.BusinessRule, "AccountNotActive");
        }

        if (from.CurrencyCode != to.CurrencyCode)
        {
            return Fail<TransferResult>(ErrorCodes.BusinessRule, "CurrencyMismatch");
        }

        if (from.Balance < amount)
        {
            return Fail<TransferResult>(ErrorCodes.BusinessRule, "InsufficientBalance");
        }

        return new GenericResponse<TransferResult>();
    }

    private TransactionEntity NewTransaction(
        long accountId, Guid referenceId, TransactionDirection direction,
        decimal amount, decimal balanceAfter, string? description) => new()
    {
        AccountId = accountId,
        ReferenceId = referenceId,
        Direction = direction,
        Amount = amount,
        BalanceAfter = balanceAfter,
        Description = description,
        Channel = context.Channel
    };
}
