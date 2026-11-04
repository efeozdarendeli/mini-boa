namespace MiniBoa.Types.Contracts;

// "Müşteri Ol" akışının contract'ları (BOA'daki BOA.Types.SelfService karşılığı).
// Contract'lar UI ile backend arasında taşınan veridir; içlerinde kural veya mantık bulunmaz.

/// <summary>Müşteri Ol 1. adım: TCKN ve cep telefonu.</summary>
public sealed class BecomeACustomerInputContract
{
    public string IdentityNumber { get; init; } = string.Empty;
    public string GsmCountry { get; init; } = "90";
    public string GsmAreaCode { get; init; } = string.Empty;
    public string GsmNumber { get; init; } = string.Empty;
}

/// <summary>Müşteri Ol 1. adımın sonucu: UI hangi ekrana gideceğine buna göre karar verir.</summary>
public sealed class KYCCustomerInfoContract
{
    public enum KYCProcessTypeEnum
    {
        /// <summary>Yeni başvuru açıldı; kişisel bilgi ekranına devam.</summary>
        NewCustomer,

        /// <summary>Kişi zaten müşteri; giriş ekranına yönlendir.</summary>
        GoCardLogin,

        /// <summary>Yarım kalmış başvuru var; kaldığı yerden devam.</summary>
        ContinueApplication
    }

    public KYCProcessTypeEnum KYCProcessType { get; init; }

    public long? ApplicationId { get; init; }
}

/// <summary>Müşteri Ol 2. adım: kişisel bilgiler, adres ve şifre.</summary>
public sealed class CreateCustomerContract
{
    public long ApplicationId { get; init; }

    /// <summary>Başvuru sahibinin TCKN'si; başvurudakiyle eşleşmelidir.</summary>
    public string IdentityNumber { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateOnly BirthDate { get; init; }
    public string? Email { get; init; }
    public short CityId { get; init; }
    public int CountyId { get; init; }
    public string AddressLine { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public sealed class CreateCustomerResultContract
{
    public long CustomerNo { get; init; }
    public string AccountNumber { get; init; } = string.Empty;
    public string Iban { get; init; } = string.Empty;
}
