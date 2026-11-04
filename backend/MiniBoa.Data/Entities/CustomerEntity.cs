using MiniBoa.Types;

namespace MiniBoa.Data.Entities;

/// <summary>cus.customer satırı.</summary>
public sealed class CustomerEntity
{
    public long CustomerNo { get; init; }
    public long PersonId { get; init; }
    public CustomerStatus Status { get; init; }
    public string PasswordHash { get; init; } = string.Empty;
    public short FailedLoginCount { get; init; }
    public DateTime? LockedUntil { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime? PreviousLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
