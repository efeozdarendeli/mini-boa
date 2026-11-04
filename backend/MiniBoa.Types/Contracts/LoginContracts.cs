namespace MiniBoa.Types.Contracts;

public sealed class LoginContract
{
    public string IdentityNumber { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

/// <summary>Giriş başarılıysa müşteri bilgisi. Token'ı Communication katmanı ekler.</summary>
public sealed class LoginResultContract
{
    public long CustomerNo { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Token { get; set; }
}
