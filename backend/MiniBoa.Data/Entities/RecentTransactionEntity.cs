using MiniBoa.Types;

namespace MiniBoa.Data.Entities;

/// <summary>
/// Ana sayfadaki "Son İşlemler" satırı: hareket + hangi hesapta olduğu + karşı tarafın adı.
/// Karşı taraf, aynı reference_id'ye sahip diğer hareketin hesap sahibidir; yoksa (örn. dışarıdan yatırma) boştur.
/// </summary>
public sealed class RecentTransactionEntity
{
    public long TransactionId { get; init; }
    public Guid ReferenceId { get; init; }
    public long CustomerNo { get; init; }
    public short AccountSuffix { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public TransactionDirection Direction { get; init; }
    public decimal Amount { get; init; }
    public decimal BalanceAfter { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CounterpartyFirstName { get; init; }
    public string? CounterpartyLastName { get; init; }

    public string AccountNumber => $"{CustomerNo}-{AccountSuffix}";
}
