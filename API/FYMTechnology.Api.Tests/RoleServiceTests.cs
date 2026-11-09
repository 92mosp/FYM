using FluentAssertions;
using FYMTechnology.Api.Services;
using Xunit;

namespace FYMTechnology.Api.Tests;

public sealed class RoleServiceTests : IDisposable
{
    private readonly TestDatabase database = new();

    [Fact]
    public async Task GetAllAsyncReturnsRolesOrderedByName()
    {
        var (superAdmin, user) = await database.SeedRolesAsync();

        var result = await new RoleService(database.CreateContext()).GetAllAsync(CancellationToken.None);

        result.Select(x => (x.Id, x.Name)).Should().Equal((superAdmin.Id, "SuperAdmin"), (user.Id, "User"));
    }

    public void Dispose() => database.Dispose();
}
