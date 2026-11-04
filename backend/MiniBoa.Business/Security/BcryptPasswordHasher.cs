namespace MiniBoa.Business.Security;

/// <summary>
/// bcrypt: her şifreye rastgele "salt" ekler ve bilerek yavaş çalışır; böylece aynı şifreler
/// farklı hash'lenir ve kaba kuvvet denemeleri pahalılaşır. Seed'deki pgcrypto hash'leriyle uyumludur.
/// </summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
