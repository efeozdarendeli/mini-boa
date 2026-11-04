using MiniBoa.Business.Kernel;
using MiniBoa.Business.Security;
using MiniBoa.Data.Entities;
using MiniBoa.Data.Repositories;
using MiniBoa.Types;
using NSubstitute;

namespace MiniBoa.Tests.Business;

public class CustomerTests
{
    private const string IdentityNumber = "12345678950";
    private const string Password = "246810";
    private static readonly DateTimeOffset Now = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly ICustomerRepository _customers = Substitute.For<ICustomerRepository>();
    private readonly IPersonRepository _persons = Substitute.For<IPersonRepository>();
    private readonly BcryptPasswordHasher _hasher = new();
    private readonly Customer _customer;

    public CustomerTests()
    {
        _customer = new Customer(_customers, _persons, _hasher, new FixedTimeProvider(Now), TestHelpers.Messaging());
    }

    [Theory]
    [InlineData("111111")]
    [InlineData("123456")]
    [InlineData("654321")]
    public void ValidatePassword_RepeatedOrSequential_IsWeak(string password)
    {
        var response = _customer.ValidatePassword(password);

        Assert.False(response.Success);
        Assert.Contains("ardışık", response.Results[0].Message);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("12a456")]
    public void ValidatePassword_WrongFormat_Fails(string password) =>
        Assert.False(_customer.ValidatePassword(password).Success);

    [Fact]
    public void ValidatePassword_Strong_Passes() =>
        Assert.True(_customer.ValidatePassword(Password).Success);

    [Fact]
    public async Task Create_StoresHashNotPlainPassword()
    {
        _customers.InsertAsync(Arg.Any<long>(), Arg.Any<string>()).Returns(10000099L);

        var response = await _customer.CreateAsync(personId: 5, Password);

        Assert.True(response.Success);
        await _customers.Received(1).InsertAsync(5, Arg.Is<string>(h => h != Password && h.StartsWith("$2")));
    }

    [Fact]
    public async Task Authenticate_CorrectPassword_RecordsLogin()
    {
        GivenCustomer();

        var response = await _customer.AuthenticateAsync(IdentityNumber, Password);

        Assert.True(response.Success);
        Assert.Equal(10000001, response.Value!.CustomerNo);
        await _customers.Received(1).RegisterSuccessfulLoginAsync(10000001, Now.UtcDateTime);
    }

    [Fact]
    public async Task Authenticate_WrongPassword_AndUnknownPerson_ReturnSameError()
    {
        GivenCustomer();

        var wrongPassword = await _customer.AuthenticateAsync(IdentityNumber, "999999");
        var unknownPerson = await _customer.AuthenticateAsync("23456789138", Password);

        Assert.Equal(ErrorCodes.Unauthorized, wrongPassword.Results[0].ErrorCode);
        Assert.Equal(wrongPassword.Results[0], unknownPerson.Results[0]);
    }

    [Fact]
    public async Task Authenticate_WrongPassword_IncrementsFailedCount()
    {
        GivenCustomer(failedLoginCount: 1);

        await _customer.AuthenticateAsync(IdentityNumber, "999999");

        await _customers.Received(1).UpdateLoginFailureAsync(10000001, 2, null);
    }

    [Fact]
    public async Task Authenticate_ThirdWrongPassword_LocksAccountForFiveMinutes()
    {
        GivenCustomer(failedLoginCount: 2);

        var response = await _customer.AuthenticateAsync(IdentityNumber, "999999");

        Assert.False(response.Success);
        Assert.Contains("5 dakika", response.Results[0].Message);
        await _customers.Received(1).UpdateLoginFailureAsync(10000001, 0, Now.UtcDateTime.AddMinutes(5));
    }

    [Fact]
    public async Task Authenticate_LockedAccount_RejectsEvenCorrectPassword()
    {
        GivenCustomer(lockedUntil: Now.UtcDateTime.AddMinutes(3));

        var response = await _customer.AuthenticateAsync(IdentityNumber, Password);

        Assert.False(response.Success);
        Assert.Contains("3 dakika", response.Results[0].Message);
        await _customers.DidNotReceiveWithAnyArgs().RegisterSuccessfulLoginAsync(default, default);
    }

    [Fact]
    public async Task Authenticate_ExpiredLock_AllowsLogin()
    {
        GivenCustomer(lockedUntil: Now.UtcDateTime.AddMinutes(-1));

        var response = await _customer.AuthenticateAsync(IdentityNumber, Password);

        Assert.True(response.Success);
    }

    [Fact]
    public async Task Authenticate_PassiveCustomer_Fails()
    {
        GivenCustomer(status: CustomerStatus.Passive);

        var response = await _customer.AuthenticateAsync(IdentityNumber, Password);

        Assert.False(response.Success);
        Assert.Equal(ErrorCodes.BusinessRule, response.Results[0].ErrorCode);
    }

    [Fact]
    public async Task VerifyIdentity_AllDetailsMatch_Passes()
    {
        GivenCustomer();

        var response = await _customer.VerifyIdentityForPasswordResetAsync(IdentityNumber, "532", "1112233", new DateOnly(1990, 5, 14));

        Assert.True(response.Success);
    }

    [Theory]
    [InlineData("532", "9999999", 1990)]   // telefon yanlış
    [InlineData("532", "1112233", 1991)]   // doğum tarihi yanlış
    public async Task VerifyIdentity_AnyMismatch_FailsWithoutSayingWhich(string area, string number, int birthYear)
    {
        GivenCustomer();

        var response = await _customer.VerifyIdentityForPasswordResetAsync(IdentityNumber, area, number, new DateOnly(birthYear, 5, 14));

        Assert.False(response.Success);
        Assert.Equal("Girdiğiniz bilgiler kayıtlarımızla eşleşmedi.", response.Results[0].Message);
        await _customers.Received(1).UpdateLoginFailureAsync(10000001, 1, null);
    }

    [Fact]
    public async Task ResetPassword_SameAsOld_Fails()
    {
        var customer = GivenCustomer();

        var response = await _customer.ResetPasswordAsync(customer, Password);

        Assert.False(response.Success);
        await _customers.DidNotReceiveWithAnyArgs().UpdatePasswordAsync(default, default!);
    }

    [Fact]
    public async Task ResetPassword_NewStrongPassword_StoresNewHash()
    {
        var customer = GivenCustomer();

        var response = await _customer.ResetPasswordAsync(customer, "975310");

        Assert.True(response.Success);
        await _customers.Received(1).UpdatePasswordAsync(10000001, Arg.Is<string>(h => _hasher.Verify("975310", h)));
    }

    private CustomerEntity GivenCustomer(
        CustomerStatus status = CustomerStatus.Active, short failedLoginCount = 0, DateTime? lockedUntil = null)
    {
        _persons.GetByIdentityNumberAsync(IdentityNumber).Returns(new PersonEntity
        {
            Id = 1,
            IdentityNumber = IdentityNumber,
            GsmAreaCode = "532",
            GsmNumber = "1112233",
            BirthDate = new DateOnly(1990, 5, 14)
        });

        var customer = new CustomerEntity
        {
            CustomerNo = 10000001,
            PersonId = 1,
            Status = status,
            PasswordHash = _hasher.Hash(Password),
            FailedLoginCount = failedLoginCount,
            LockedUntil = lockedUntil
        };
        _customers.GetByPersonIdAsync(1).Returns(customer);
        return customer;
    }
}
