using Dapper;
using MiniBoa.Data;
using MiniBoa.Data.Entities;
using MiniBoa.Data.Repositories;
using MiniBoa.Types;
using Npgsql;

namespace MiniBoa.Tests.Data;

[Collection(DatabaseCollection.Name)]
[Trait("Category", "Integration")]
public class UnitOfWorkTests(DatabaseFixture fixture)
{
    [Fact]
    public async Task Rollback_DiscardsChanges()
    {
        var identityNumber = NewIdentityNumber();

        await using (var unitOfWork = fixture.CreateUnitOfWork())
        {
            await unitOfWork.BeginAsync();
            await new PersonRepository(unitOfWork).InsertAsync(NewPerson(identityNumber));
            await unitOfWork.RollbackAsync();
        }

        Assert.Null(await FindPersonAsync(identityNumber));
    }

    [Fact]
    public async Task Commit_PersistsChanges()
    {
        var identityNumber = NewIdentityNumber();

        try
        {
            await using (var unitOfWork = fixture.CreateUnitOfWork())
            {
                await unitOfWork.BeginAsync();
                await new PersonRepository(unitOfWork).InsertAsync(NewPerson(identityNumber));
                await unitOfWork.CommitAsync();
            }

            Assert.NotNull(await FindPersonAsync(identityNumber));
        }
        finally
        {
            await DeletePersonAsync(identityNumber);
        }
    }

    [Fact]
    public async Task ExecuteInTransaction_FailedResponse_RollsBack()
    {
        var identityNumber = NewIdentityNumber();

        await using (var unitOfWork = fixture.CreateUnitOfWork())
        {
            var response = await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await new PersonRepository(unitOfWork).InsertAsync(NewPerson(identityNumber));
                return GenericResponse<bool>.Fail(ErrorCodes.BusinessRule, "simulated business failure");
            });

            Assert.False(response.Success);
            Assert.False(unitOfWork.HasActiveTransaction);
        }

        Assert.Null(await FindPersonAsync(identityNumber));
    }

    [Fact]
    public async Task ExecuteInTransaction_Exception_RollsBackAndRethrows()
    {
        var identityNumber = NewIdentityNumber();

        await using (var unitOfWork = fixture.CreateUnitOfWork())
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                unitOfWork.ExecuteInTransactionAsync<bool>(async () =>
                {
                    await new PersonRepository(unitOfWork).InsertAsync(NewPerson(identityNumber));
                    throw new InvalidOperationException("boom");
                }));
        }

        Assert.Null(await FindPersonAsync(identityNumber));
    }

    [Fact]
    public async Task GetForUpdate_WithoutTransaction_Throws()
    {
        await using var unitOfWork = fixture.CreateUnitOfWork();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new AccountRepository(unitOfWork).GetForUpdateAsync(1));
    }

    [Fact]
    public async Task NegativeBalance_IsRejectedByDatabase()
    {
        await using var unitOfWork = fixture.CreateUnitOfWork();
        await unitOfWork.BeginAsync();

        var exception = await Assert.ThrowsAsync<PostgresException>(() =>
            new AccountRepository(unitOfWork).UpdateBalanceAsync(1, -1m));

        Assert.Equal(PostgresErrorCodes.CheckViolation, exception.SqlState);
        await unitOfWork.RollbackAsync();
    }

    private static string NewIdentityNumber() => "99" + Random.Shared.NextInt64(100_000_000, 999_999_999);

    private static PersonEntity NewPerson(string identityNumber) => new()
    {
        IdentityNumber = identityNumber,
        FirstName = "Test",
        LastName = "Person",
        BirthDate = new DateOnly(2000, 1, 1),
        GsmAreaCode = "500",
        GsmNumber = "0000000"
    };

    private async Task<PersonEntity?> FindPersonAsync(string identityNumber)
    {
        await using var unitOfWork = fixture.CreateUnitOfWork();
        return await new PersonRepository(unitOfWork).GetByIdentityNumberAsync(identityNumber);
    }

    private async Task DeletePersonAsync(string identityNumber)
    {
        await using var connection = await fixture.DataSource.OpenConnectionAsync();
        await connection.ExecuteAsync(
            "DELETE FROM cus.person WHERE identity_number = @IdentityNumber",
            new { IdentityNumber = identityNumber });
    }
}
