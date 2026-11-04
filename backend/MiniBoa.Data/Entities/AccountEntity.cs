using MiniBoa.Types;

namespace MiniBoa.Data.Entities;

/// <summary>acc.account satırı.</summary>
public sealed class AccountEntity
{
    public long Id { get; init; }
    public long CustomerNo { get; init; }
    public short AccountSuffix { get; init; }
    public short BranchCode { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public AccountType AccountType { get; init; }
    public string Iban { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public AccountStatus Status { get; init; }
    public DateTime OpenedAt { get; init; }

    /// <summary>Ekranda gösterilen hesap numarası: müşteri no + ek no (örn. 10000001-1).</summary>
    public string AccountNumber => $"{CustomerNo}-{AccountSuffix}";
}
