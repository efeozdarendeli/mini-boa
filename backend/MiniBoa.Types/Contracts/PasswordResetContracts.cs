namespace MiniBoa.Types.Contracts;

// "Şifremi unuttum" akışı. Gerçek bankada bu adıma SMS ile tek kullanımlık kod eklenir;
// demoda kimlik, kayıtlı telefon ve doğum tarihinin birlikte eşleşmesiyle doğrulanır.

/// <summary>1. adım: kimliği doğrulamak için istenen bilgiler.</summary>
public class PasswordResetIdentityContract
{
    public string IdentityNumber { get; init; } = string.Empty;
    public string GsmAreaCode { get; init; } = string.Empty;
    public string GsmNumber { get; init; } = string.Empty;
    public DateOnly BirthDate { get; init; }
}

/// <summary>
/// 2. adım: yeni şifre. Kimlik bilgileri tekrar gönderilir ve sunucuda tekrar doğrulanır;
/// böylece sunucu iki adım arasında durum (oturum, bilet) tutmak zorunda kalmaz.
/// </summary>
public sealed class ResetPasswordContract : PasswordResetIdentityContract
{
    public string NewPassword { get; init; } = string.Empty;
}
