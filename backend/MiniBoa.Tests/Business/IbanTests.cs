using MiniBoa.Business.Kernel;

namespace MiniBoa.Tests.Business;

public class IbanTests
{
    [Theory]
    [InlineData(10000001, 1, "TR550099900000100000010001")]
    [InlineData(10000001, 3, "TR980099900000100000010003")]
    [InlineData(10000002, 1, "TR060099900000100000020001")]
    public void Generate_MatchesSeedIbans(long customerNo, short suffix, string expected) =>
        Assert.Equal(expected, Iban.Generate(customerNo, suffix));

    [Fact]
    public void GeneratedIban_IsValid() =>
        Assert.True(Iban.IsValid(Iban.Generate(12345678, 7)));

    [Theory]
    [InlineData("TR560099900000100000010001")]   // kontrol hanesi yanlış
    [InlineData("TR55009990000010000001000")]    // eksik hane
    [InlineData("DE550099900000100000010001")]   // ülke kodu
    [InlineData("TR55009990000010000001000X")]   // harf
    [InlineData(null)]
    public void InvalidIbans_Fail(string? iban) =>
        Assert.False(Iban.IsValid(iban));
}
