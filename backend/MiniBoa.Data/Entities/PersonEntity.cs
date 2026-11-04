namespace MiniBoa.Data.Entities;

/// <summary>cus.person satırı.</summary>
public sealed class PersonEntity
{
    public long Id { get; init; }
    public string IdentityNumber { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateOnly BirthDate { get; init; }
    public string GsmCountry { get; init; } = "90";
    public string GsmAreaCode { get; init; } = string.Empty;
    public string GsmNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public short? CityId { get; init; }
    public int? CountyId { get; init; }
    public string? AddressLine { get; init; }
    public DateTime CreatedAt { get; init; }
}
