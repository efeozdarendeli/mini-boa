namespace MiniBoa.Types.Contracts;

/// <summary>Profil ekranı. TCKN maskeli gönderilir; tam hâline ekranın ihtiyacı yok.</summary>
public sealed class ProfileContract
{
    public long CustomerNo { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string MaskedIdentityNumber { get; init; } = string.Empty;
    public string GsmNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public short? CityId { get; init; }
    public int? CountyId { get; init; }
    public string? AddressLine { get; init; }
}

/// <summary>
/// Güncellenebilen iletişim bilgileri. Ad, TCKN ve telefon burada değiştirilemez;
/// gerçek bankada bunlar kimlik doğrulaması (şube, SMS) gerektirir.
/// </summary>
public sealed class UpdateProfileContract
{
    public string? Email { get; init; }
    public short CityId { get; init; }
    public int CountyId { get; init; }
    public string AddressLine { get; init; } = string.Empty;
}
