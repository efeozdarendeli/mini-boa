using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MiniBoa.Tests.Data;

namespace MiniBoa.Tests.Communication;

/// <summary>HTTP seviyesinde uçtan uca: istek → routing → yetki → orchestration → veritabanı → JSON cevap.</summary>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Integration")]
public class ApiTests(ApiFactory factory, DatabaseFixture database) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task UnknownMethod_Returns404WithGenericResponse()
    {
        var (status, body) = await ExecuteAsync(new { methodName = "Account.DeleteEverything" });

        Assert.Equal(HttpStatusCode.NotFound, status);
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.Equal("SYS-0002", ErrorCode(body));
    }

    [Fact]
    public async Task InheritedMethod_IsNotCallable()
    {
        // object.ToString gibi taban metodlar listede yok; isimle çağrılamaz.
        var (status, _) = await ExecuteAsync(new { methodName = "Account.ToString" });

        Assert.Equal(HttpStatusCode.NotFound, status);
    }

    [Fact]
    public async Task AnonymousOperation_WorksWithoutToken()
    {
        var (status, body) = await ExecuteAsync(new { methodName = "Parameter.GetCountyList", data = new { cityId = 34 } });

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(39, body.GetProperty("value").GetArrayLength());
    }

    [Fact]
    public async Task ProtectedOperation_WithoutToken_Returns401()
    {
        var (status, body) = await ExecuteAsync(new { methodName = "Account.GetCustomerSummary" });

        Assert.Equal(HttpStatusCode.Unauthorized, status);
        Assert.Equal("SEC-0001", ErrorCode(body));
    }

    [Fact]
    public async Task ProtectedOperation_WithTamperedToken_Returns401()
    {
        var token = await LoginAsync();
        var tampered = token[..^2] + (token[^2] == 'A' ? "BB" : "AA");

        var (status, _) = await ExecuteAsync(new { methodName = "Account.GetCustomerSummary" }, tampered);

        Assert.Equal(HttpStatusCode.Unauthorized, status);
    }

    [Fact]
    public async Task Login_ThenSummary_WithToken()
    {
        var token = await LoginAsync();

        var (status, body) = await ExecuteAsync(new { methodName = "Account.GetCustomerSummary" }, token);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal("Abdullah Hakan", body.GetProperty("value").GetProperty("firstName").GetString());

        // Enum'lar JSON'da isim olarak gider.
        var firstAccount = body.GetProperty("value").GetProperty("accounts")[0];
        Assert.Equal("Current", firstAccount.GetProperty("accountType").GetString());
    }

    [Fact]
    public async Task KycDecision_ReturnsEnumAsName()
    {
        var (_, body) = await ExecuteAsync(new
        {
            methodName = "BecomeACustomer.CustomerDecisionByTaxNumber",
            data = new { identityNumber = "12345678950", gsmAreaCode = "532", gsmNumber = "1112233" }
        });

        Assert.Equal("GoCardLogin", body.GetProperty("value").GetProperty("kycProcessType").GetString());
    }

    [Fact]
    public async Task MalformedData_Returns400NotServerError()
    {
        var (status, body) = await ExecuteAsync(new
        {
            methodName = "Parameter.GetCountyList",
            data = new { cityId = "istanbul" }
        });

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Equal("VAL-0001", ErrorCode(body));
    }

    [Fact]
    public async Task MissingData_Returns400()
    {
        var (status, _) = await ExecuteAsync(new { methodName = "Parameter.GetCountyList" });

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }

    [Theory]
    [InlineData("tr", "T.C. kimlik numarası veya şifre hatalı.")]
    [InlineData("en", "Identity number or password is incorrect.")]
    public async Task ErrorMessage_FollowsAcceptLanguage(string language, string expected)
    {
        try
        {
            var (_, body) = await ExecuteAsync(
                new { methodName = "Login.Login", data = new { identityNumber = "12345678950", password = "000000" } },
                language: language);

            Assert.Equal(expected, body.GetProperty("results")[0].GetProperty("message").GetString());
        }
        finally
        {
            await database.ResetLoginStateAsync();
        }
    }

    [Fact]
    public async Task Health_ReportsHealthyWhenDatabaseIsUp()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Response_HasTraceIdHeader()
    {
        var response = await _client.PostAsJsonAsync("/api/execute", new { methodName = "Parameter.GetCityList" });

        Assert.True(response.Headers.TryGetValues("X-Trace-Id", out var values));
        Assert.Equal(32, values.Single().Length);
    }

    private async Task<string> LoginAsync()
    {
        var (status, body) = await ExecuteAsync(new
        {
            methodName = "Login.Login",
            data = new { identityNumber = "12345678950", password = "123456" }
        });

        Assert.Equal(HttpStatusCode.OK, status);
        return body.GetProperty("value").GetProperty("token").GetString()!;
    }

    private async Task<(HttpStatusCode Status, JsonElement Body)> ExecuteAsync(
        object request, string? token = null, string language = "tr")
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/execute")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(language));
        if (token is not null)
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await _client.SendAsync(message);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (response.StatusCode, body);
    }

    private static string? ErrorCode(JsonElement body) =>
        body.GetProperty("results")[0].GetProperty("errorCode").GetString();
}
