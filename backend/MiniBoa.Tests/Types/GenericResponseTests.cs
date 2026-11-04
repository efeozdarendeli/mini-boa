using MiniBoa.Types;

namespace MiniBoa.Tests.Types;

public class GenericResponseTests
{
    [Fact]
    public void Ok_IsSuccessful_AndCarriesValue()
    {
        var response = GenericResponse<int>.Ok(42);

        Assert.True(response.Success);
        Assert.Equal(42, response.Value);
    }

    [Fact]
    public void AddError_MakesResponseUnsuccessful()
    {
        var response = GenericResponse<int>.Fail(ErrorCodes.Validation, "invalid");

        Assert.False(response.Success);
        Assert.Single(response.Results);
    }

    [Fact]
    public void Warning_DoesNotBreakSuccess()
    {
        var response = GenericResponse<int>.Ok(1);
        response.Results.Add(new Result("W-1", "just a warning", Severity.Warning));

        Assert.True(response.Success);
    }

    [Fact]
    public void AddResults_CopiesErrorsFromInnerResponse()
    {
        var inner = GenericResponse<string>.Fail(ErrorCodes.NotFound, "not found");

        var outer = new GenericResponse<int>().AddResults(inner);

        Assert.False(outer.Success);
        Assert.Equal(ErrorCodes.NotFound, outer.Results[0].ErrorCode);
    }
}
