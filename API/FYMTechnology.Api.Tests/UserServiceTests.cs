using FluentAssertions;
using FYMTechnology.Api.Contracts;
using FYMTechnology.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FYMTechnology.Api.Tests;

public sealed class UserServiceTests : IDisposable
{
    private const string Password = "ExamplePassword123!";
    private readonly TestDatabase database = new();

    [Fact]
    public async Task CreateAsyncCreatesUserWithAllRequestedRoles()
    {
        var (superAdmin, user) = await database.SeedRolesAsync();

        var result = await CreateService().CreateAsync(new CreateUserRequest(" michael ", Password, [user.Id, superAdmin.Id, user.Id]), CancellationToken.None);

        result.Username.Should().Be("michael");
        result.Roles.Should().Equal("SuperAdmin", "User");
        await using var db = database.CreateContext();
        var stored = await db.Users.SingleAsync();
        stored.NormalizedUsername.Should().Be("MICHAEL");
        BCrypt.Net.BCrypt.Verify(Password, stored.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsyncRejectsDuplicateUsernameIgnoringCase()
    {
        var (_, user) = await database.SeedRolesAsync();
        await database.SeedUserAsync("Michael", Password, user);

        var action = () => CreateService().CreateAsync(new CreateUserRequest("MICHAEL", Password, [user.Id]), CancellationToken.None);

        await action.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateAsyncRejectsUnknownRole()
    {
        var (_, user) = await database.SeedRolesAsync();

        var action = () => CreateService().CreateAsync(new CreateUserRequest("michael", Password, [user.Id, 999]), CancellationToken.None);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("One or more roles do not exist.");
    }

    [Fact]
    public async Task CreateAsyncRejectsEmptyRoleList()
    {
        await database.SeedRolesAsync();

        var action = () => CreateService().CreateAsync(new CreateUserRequest("michael", Password, []), CancellationToken.None);

        await action.Should().ThrowAsync<ValidationException>().WithMessage("At least one role is required.");
    }

    [Fact]
    public async Task AssignRolesAsyncReplacesRoles()
    {
        var (superAdmin, user) = await database.SeedRolesAsync();
        var admin = await database.SeedUserAsync("admin", Password, superAdmin);
        var target = await database.SeedUserAsync("michael", Password, user);

        var result = await CreateService().AssignRolesAsync(target.Id, new AssignRolesRequest([superAdmin.Id]), admin.Id, CancellationToken.None);

        result!.Roles.Should().Equal("SuperAdmin");
        (await CreateService().GetByIdAsync(target.Id, CancellationToken.None))!.Roles.Should().Equal("SuperAdmin");
    }

    [Fact]
    public async Task AssignRolesAsyncRejectsEmptyRoleList()
    {
        var (superAdmin, user) = await database.SeedRolesAsync();
        var admin = await database.SeedUserAsync("admin", Password, superAdmin);
        var target = await database.SeedUserAsync("michael", Password, user);

        var action = () => CreateService().AssignRolesAsync(target.Id, new AssignRolesRequest([]), admin.Id, CancellationToken.None);

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task AssignRolesAsyncRejectsRemovingOwnSuperAdminRole()
    {
        var (superAdmin, user) = await database.SeedRolesAsync();
        var admin = await database.SeedUserAsync("admin", Password, superAdmin);

        var action = () => CreateService().AssignRolesAsync(admin.Id, new AssignRolesRequest([user.Id]), admin.Id, CancellationToken.None);

        await action.Should().ThrowAsync<ConflictException>().WithMessage("You cannot remove your own SuperAdmin role.");
    }

    [Fact]
    public async Task AssignRolesAsyncAllowsRemovingSuperAdminFromAnotherUser()
    {
        var (superAdmin, user) = await database.SeedRolesAsync();
        var admin = await database.SeedUserAsync("admin", Password, superAdmin);
        var other = await database.SeedUserAsync("other", Password, superAdmin);

        var result = await CreateService().AssignRolesAsync(other.Id, new AssignRolesRequest([user.Id]), admin.Id, CancellationToken.None);

        result!.Roles.Should().Equal("User");
    }

    [Fact]
    public async Task AssignRolesAsyncReturnsNullWhenUserDoesNotExist()
    {
        var (_, user) = await database.SeedRolesAsync();

        var result = await CreateService().AssignRolesAsync(404, new AssignRolesRequest([user.Id]), 1, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsyncReturnsUsersOrderedWithSortedRoles()
    {
        var (superAdmin, user) = await database.SeedRolesAsync();
        await database.SeedUserAsync("zoe", Password, user);
        await database.SeedUserAsync("ana", Password, user, superAdmin);

        var result = await CreateService().GetAllAsync(CancellationToken.None);

        result.Select(x => x.Username).Should().Equal("ana", "zoe");
        result.First().Roles.Should().Equal("SuperAdmin", "User");
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNullWhenUserDoesNotExist()
    {
        (await CreateService().GetByIdAsync(404, CancellationToken.None)).Should().BeNull();
    }

    private UserService CreateService() => new(database.CreateContext());

    public void Dispose() => database.Dispose();
}
