using MiniBoa.Business.Kernel;

namespace MiniBoa.Tests.Business;

public class MernisTests
{
    private readonly Mernis _mernis = new();

    [Theory]
    [InlineData("12345678950")]
    [InlineData("23456789138")]
    [InlineData("10000000146")]
    public void ValidIdentityNumbers_Pass(string identityNumber) =>
        Assert.True(_mernis.IsValidIdentityNumber(identityNumber));

    [Theory]
    [InlineData("12345678951")]   // 11. hane yanlış
    [InlineData("12345678940")]   // 10. hane yanlış
    [InlineData("02345678950")]   // 0 ile başlıyor
    [InlineData("1234567895")]    // 10 hane
    [InlineData("1234567895A")]   // rakam olmayan karakter
    [InlineData("")]
    public void InvalidIdentityNumbers_Fail(string identityNumber) =>
        Assert.False(_mernis.IsValidIdentityNumber(identityNumber));
}
