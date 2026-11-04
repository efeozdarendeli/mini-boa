using MiniBoa.Types;

namespace MiniBoa.Data.Entities;

/// <summary>cus.application satırı: "Müşteri Ol" başvurusu.</summary>
public sealed class ApplicationEntity
{
    public long Id { get; init; }
    public string IdentityNumber { get; init; } = string.Empty;
    public string GsmCountry { get; init; } = "90";
    public string GsmAreaCode { get; init; } = string.Empty;
    public string GsmNumber { get; init; } = string.Empty;
    public ApplicationStatus Status { get; init; }
    public long? PersonId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
