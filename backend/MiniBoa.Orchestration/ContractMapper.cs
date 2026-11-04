using MiniBoa.Business.Kernel;
using MiniBoa.Data.Entities;
using MiniBoa.Types.Contracts;

namespace MiniBoa.Orchestration;

/// <summary>
/// Veritabanı entity'lerini UI'a giden contract'lara çevirir. Entity'ler dışarı hiç çıkmaz;
/// böylece örneğin şifre hash'i veya iç id'ler yanlışlıkla API cevabına sızmaz.
/// </summary>
internal static class ContractMapper
{
    public static AccountContract ToContract(this AccountEntity account) => new()
    {
        AccountId = account.Id,
        AccountNumber = account.AccountNumber,
        Iban = account.Iban,
        CurrencyCode = account.CurrencyCode,
        AccountType = account.AccountType,
        Balance = account.Balance,
        Status = account.Status
    };

    public static TransactionContract ToContract(this TransactionEntity transaction) => new()
    {
        TransactionId = transaction.Id,
        ReferenceId = transaction.ReferenceId,
        Direction = transaction.Direction,
        Amount = transaction.Amount,
        BalanceAfter = transaction.BalanceAfter,
        Description = transaction.Description,
        CreatedAt = transaction.CreatedAt
    };

    public static RecentTransactionContract ToContract(this RecentTransactionEntity transaction) => new()
    {
        TransactionId = transaction.TransactionId,
        ReferenceId = transaction.ReferenceId,
        AccountNumber = transaction.AccountNumber,
        CurrencyCode = transaction.CurrencyCode,
        Direction = transaction.Direction,
        Amount = transaction.Amount,
        BalanceAfter = transaction.BalanceAfter,
        Description = transaction.Description,
        CreatedAt = transaction.CreatedAt,
        // Karşı tarafın tam adı ekrana gitmez; transfer ekranındaki alıcı adıyla aynı maske kuralı.
        Counterparty = transaction.CounterpartyFirstName is null
            ? null
            : Account.MaskName($"{transaction.CounterpartyFirstName} {transaction.CounterpartyLastName}")
    };

    public static ParameterContract ToContract(this CityEntity city) => new() { Id = city.Id, Name = city.Name };

    public static ParameterContract ToContract(this CountyEntity county) => new() { Id = county.Id, Name = county.Name };

    public static CurrencyRateContract ToContract(this CurrencyRateEntity rate) => new()
    {
        CurrencyCode = rate.CurrencyCode,
        CurrencyName = rate.CurrencyName,
        BuyRate = rate.BuyRate,
        SellRate = rate.SellRate,
        RateDate = rate.RateDate
    };
}
