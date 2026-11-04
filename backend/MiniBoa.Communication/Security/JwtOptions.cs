namespace MiniBoa.Communication.Security;

/// <summary>appsettings.json'daki "Jwt" bölümü.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// Token'ı imzalayan gizli anahtar (HS256 için en az 32 karakter).
    /// Sadece Development ayarında durur; gerçek ortamda ortam değişkeni / secret store'dan gelmelidir.
    /// </summary>
    public string SigningKey { get; init; } = string.Empty;

    public int ExpiresMinutes { get; init; } = 30;
}
