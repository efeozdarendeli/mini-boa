using MiniBoa.Types;

namespace MiniBoa.Data.Entities;

/// <summary>Bir IBAN'ın sahibi; transferde alıcının adını (maskeli) göstermek için.</summary>
public sealed class AccountHolderEntity
{
    public long AccountId { get; init; }
    public AccountStatus Status { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}
