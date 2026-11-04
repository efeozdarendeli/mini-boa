using MiniBoa.Business.Kernel;
using MiniBoa.Data.Entities;
using MiniBoa.Data.Repositories;
using MiniBoa.Types;
using NSubstitute;

namespace MiniBoa.Tests.Business;

public class PersonTests
{
    private static readonly DateTimeOffset Today = new(2026, 10, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IPersonRepository _persons = Substitute.For<IPersonRepository>();
    private readonly IParameterRepository _parameters = Substitute.For<IParameterRepository>();
    private readonly Person _person;

    public PersonTests()
    {
        _person = new Person(_persons, _parameters, new Mernis(), new FixedTimeProvider(Today), TestHelpers.Messaging());
    }

    [Fact]
    public async Task Create_ValidPerson_InsertsAndReturnsId()
    {
        _persons.InsertAsync(Arg.Any<PersonEntity>()).Returns(42L);

        var response = await _person.CreateAsync(ValidPerson());

        Assert.True(response.Success);
        Assert.Equal(42, response.Value);
    }

    [Fact]
    public async Task Create_InvalidIdentityNumber_Fails()
    {
        var response = await _person.CreateAsync(ValidPerson(identityNumber: "12345678951"));

        AssertFailed(response, ErrorCodes.Validation);
    }

    [Fact]
    public async Task Create_UnderAge_Fails()
    {
        // Bugün 18 yaşına bir gün kalmış.
        var response = await _person.CreateAsync(ValidPerson(birthDate: new DateOnly(2008, 10, 28)));

        AssertFailed(response, ErrorCodes.BusinessRule);
    }

    [Fact]
    public async Task Create_ExactlyEighteenToday_Passes()
    {
        var response = await _person.CreateAsync(ValidPerson(birthDate: new DateOnly(2008, 10, 27)));

        Assert.True(response.Success);
    }

    [Theory]
    [InlineData("432", "1234567")]   // 5 ile başlamıyor
    [InlineData("532", "123456")]    // numara 6 hane
    [InlineData("53a", "1234567")]
    public async Task Create_InvalidGsm_Fails(string areaCode, string number)
    {
        var response = await _person.CreateAsync(ValidPerson(gsmAreaCode: areaCode, gsmNumber: number));

        AssertFailed(response, ErrorCodes.Validation);
    }

    [Fact]
    public async Task Create_ExistingIdentityNumber_Fails()
    {
        _persons.GetByIdentityNumberAsync("12345678950").Returns(new PersonEntity { Id = 1 });

        var response = await _person.CreateAsync(ValidPerson());

        AssertFailed(response, ErrorCodes.BusinessRule);
        await _persons.DidNotReceive().InsertAsync(Arg.Any<PersonEntity>());
    }

    [Fact]
    public async Task UpdateAddress_CountyOfAnotherCity_Fails()
    {
        _parameters.GetCountiesAsync(34).Returns([new CountyEntity { Id = 1, CityId = 34, Name = "Kadıköy" }]);

        var response = await _person.UpdateAddressAsync(personId: 1, cityId: 34, countyId: 99, "Adres", email: null);

        AssertFailed(response, ErrorCodes.Validation);
        await _persons.DidNotReceiveWithAnyArgs().UpdateAddressAsync(default, default, default, default!, default);
    }

    private static PersonEntity ValidPerson(
        string identityNumber = "12345678950",
        DateOnly? birthDate = null,
        string gsmAreaCode = "532",
        string gsmNumber = "1234567") => new()
    {
        IdentityNumber = identityNumber,
        FirstName = "Test",
        LastName = "Kişi",
        BirthDate = birthDate ?? new DateOnly(1990, 1, 1),
        GsmAreaCode = gsmAreaCode,
        GsmNumber = gsmNumber
    };

    private static void AssertFailed<T>(GenericResponse<T> response, string errorCode)
    {
        Assert.False(response.Success);
        Assert.Equal(errorCode, response.Results[0].ErrorCode);
    }
}
