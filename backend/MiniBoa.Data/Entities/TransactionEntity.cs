using MiniBoa.Types;

namespace MiniBoa.Data.Entities;

/// <summary>txn.transaction satırı: tek bir hesap hareketi.</summary>
public sealed class TransactionEntity
{
    public long Id { get; init; }
    public long AccountId { get; init; }
    public Guid ReferenceId { get; init; }
    public TransactionDirection Direction { get; init; }
    public decimal Amount { get; init; }
    public decimal BalanceAfter { get; init; }
    public string? Description { get; init; }
    public string Channel { get; init; } = "SelfService";
    public DateTime CreatedAt { get; init; }
}
