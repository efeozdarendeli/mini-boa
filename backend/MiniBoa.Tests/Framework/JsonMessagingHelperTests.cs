using MiniBoa.Framework;
using MiniBoa.Framework.Messaging;

namespace MiniBoa.Tests.Framework;

public class JsonMessagingHelperTests
{
    [Theory]
    [InlineData("tr", "Beklenmeyen bir hata oluştu.")]
    [InlineData("en", "An unexpected error occurred.")]
    public void GetMessage_ReturnsTextInContextLanguage(string language, string expected)
    {
        var helper = new JsonMessagingHelper(new RequestContext { Language = language });

        Assert.Equal(expected, helper.GetMessage("Xtm", "UnexpectedError"));
    }

    [Fact]
    public void GetMessage_UnknownLanguage_FallsBackToTurkish()
    {
        var helper = new JsonMessagingHelper(new RequestContext { Language = "de" });

        Assert.Equal("Beklenmeyen bir hata oluştu.", helper.GetMessage("Xtm", "UnexpectedError"));
    }

    [Fact]
    public void GetMessage_UnknownKey_ReturnsKeyItself()
    {
        var helper = new JsonMessagingHelper(new RequestContext());

        Assert.Equal("Xtm.NoSuchKey", helper.GetMessage("Xtm", "NoSuchKey"));
    }
}
