namespace MiniBoa.Business.Kernel;

/// <summary>
/// Türkiye IBAN'ı üretir ve doğrular (ISO 13616, mod 97).
/// Biçim: TR + 2 kontrol hanesi + 5 banka kodu + 1 rezerv (0) + 16 hesap no (müşteri no 12 + ek no 4).
/// </summary>
public static class Iban
{
    public const string BankCode = "00999";   // demo için hayali banka kodu
    private const string CountryCode = "TR";
    private const int Length = 26;

    public static string Generate(long customerNo, short accountSuffix)
    {
        var bban = $"{BankCode}0{customerNo:D12}{accountSuffix:D4}";

        // Kontrol haneleri "00" kabul edilip ülke kodu sona taşınır; 98'den kalan çıkarılır.
        var checkDigits = 98 - Mod97(bban + CountryCode + "00");

        return $"{CountryCode}{checkDigits:D2}{bban}";
    }

    public static bool IsValid(string? iban)
    {
        if (iban is not { Length: Length } || !iban.StartsWith(CountryCode) || !iban[2..].All(char.IsAsciiDigit))
        {
            return false;
        }

        // İlk 4 karakter sona taşınınca kalan 1 çıkmalı.
        return Mod97(iban[4..] + iban[..4]) == 1;
    }

    /// <summary>
    /// Harfleri sayıya çevirerek (A=10 ... Z=35) çok uzun sayının 97'ye bölümünden kalanı,
    /// sayıyı hiç bütün olarak oluşturmadan, hane hane hesaplar.
    /// </summary>
    private static int Mod97(string value)
    {
        var remainder = 0;

        foreach (var c in value)
        {
            remainder = char.IsAsciiDigit(c)
                ? (remainder * 10 + (c - '0')) % 97
                : (remainder * 100 + (c - 'A' + 10)) % 97;
        }

        return remainder;
    }
}
