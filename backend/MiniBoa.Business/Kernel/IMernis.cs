namespace MiniBoa.Business.Kernel;

/// <summary>
/// Kimlik doğrulama servisi (BOA'daki Business.Kernel.Customer.Mernis karşılığı).
/// Gerçekte Nüfus ve Vatandaşlık İşleri'nin (NVİ) servisine gidilir; interface sayesinde
/// gerçek servis ile demo/sahte servis birbirinin yerine kullanılabilir.
/// </summary>
public interface IMernis
{
    bool IsValidIdentityNumber(string identityNumber);

    Task<bool> VerifyIdentityAsync(string identityNumber, string firstName, string lastName, DateOnly birthDate);
}
