namespace MiniBoa.Business.Kernel;

/// <summary>
/// Demo Mernis: dış servise gitmez, sadece TCKN kontrol algoritmasını uygular.
/// </summary>
public sealed class Mernis : IMernis
{
    /// <summary>
    /// TCKN kuralları: 11 hane, ilk hane 0 olamaz,
    /// 10. hane = ((tek sıradakiler toplamı * 7) - çift sıradakiler toplamı) mod 10,
    /// 11. hane = ilk 10 hanenin toplamı mod 10.
    /// </summary>
    public bool IsValidIdentityNumber(string identityNumber)
    {
        if (identityNumber is not { Length: 11 } || !identityNumber.All(char.IsAsciiDigit) || identityNumber[0] == '0')
        {
            return false;
        }

        var digits = identityNumber.Select(c => c - '0').ToArray();

        var oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        var evenSum = digits[1] + digits[3] + digits[5] + digits[7];
        var tenth = ((oddSum * 7 - evenSum) % 10 + 10) % 10;
        var eleventh = digits.Take(10).Sum() % 10;

        return digits[9] == tenth && digits[10] == eleventh;
    }

    public Task<bool> VerifyIdentityAsync(string identityNumber, string firstName, string lastName, DateOnly birthDate)
    {
        var verified = IsValidIdentityNumber(identityNumber)
                       && !string.IsNullOrWhiteSpace(firstName)
                       && !string.IsNullOrWhiteSpace(lastName);

        return Task.FromResult(verified);
    }
}
