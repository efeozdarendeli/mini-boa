namespace MiniBoa.Types.Contracts;

public sealed class AccountContract
{
    public long AccountId { get; init; }
    public string AccountNumber { get; init; } = string.Empty;
    public string Iban { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public AccountType AccountType { get; init; }
    public decimal Balance { get; init; }
    public AccountStatus Status { get; init; }
}

/// <summary>Ana sayfadaki "Hoş geldin" ve "Hesaplarım" kartlarının verisi.</summary>
public sealed class CustomerSummaryContract
{
    public long CustomerNo { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public List<AccountContract> Accounts { get; init; } = [];
    public decimal TotalTryBalance { get; init; }

    /// <summary>Bir önceki girişin zamanı (UTC); ilk girişte null. "Son girişiniz" bilgisi için.</summary>
    public DateTime? PreviousLoginAt { get; init; }
}

public sealed class OpenAccountContract
{
    public string CurrencyCode { get; init; } = "TRY";
    public AccountType AccountType { get; init; } = AccountType.Current;
}

public sealed class AccountTransactionsInputContract
{
    public long AccountId { get; init; }

    /// <summary>Son kaç günün hareketleri: 7, 30 veya 90.</summary>
    public int Days { get; init; } = 30;

    /// <summary>Sadece gelen veya giden hareketler; null ise hepsi.</summary>
    public TransactionDirection? Direction { get; init; }
}

/// <summary>Ana sayfadaki "Son İşlemler" satırı.</summary>
public sealed class RecentTransactionContract
{
    public long TransactionId { get; init; }
    public Guid ReferenceId { get; init; }
    public string AccountNumber { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public TransactionDirection Direction { get; init; }
    public decimal Amount { get; init; }
    public decimal BalanceAfter { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }

    /// <summary>Karşı tarafın maskeli adı ("Ay** Ka**"); dışarıdan gelen hareketlerde null.</summary>
    public string? Counterparty { get; init; }
}

public sealed class TransactionContract
{
    public long TransactionId { get; init; }
    public Guid ReferenceId { get; init; }
    public TransactionDirection Direction { get; init; }
    public decimal Amount { get; init; }
    public decimal BalanceAfter { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}
