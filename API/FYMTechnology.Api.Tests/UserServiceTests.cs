using FluentAssertions;
using FYMTechnology.Api.Contracts;
using FYMTechnology.Api.Data;
using FYMTechnology.Api.Domain;
using FYMTechnology.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FYMTechnology.Api.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task CreateAsyncCreatesUserWithAssignedRole()
    {
        await using var db = CreateDatabase();
        db.Roles.Add(new Role { Id = 1, Name = "User", NormalizedName = "USER" });
        await db.SaveChangesAsync();
        var service = new UserService(db);

        var result = await service.CreateAsync(new CreateUserRequest("michael", "ExamplePassword123!", 1), CancellationToken.None);

        result.Username.Should().Be("michael");
        result.Roles.Should().ContainSingle().Which.Should().Be("User");
        BCrypt.Net.BCrypt.Verify("ExamplePassword123!", (await db.Users.SingleAsync()).PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsyncRejectsDuplicateUsernameIgnoringCase()
    {
        await using var db = CreateDatabase();
        db.Users.Add(new User { Username = "Michael", NormalizedUsername = "MICHAEL", PasswordHash = "hash" });
        db.Roles.Add(new Role { Id = 1, Name = "User", NormalizedName = "USER" });
        await db.SaveChangesAsync();
        var service = new UserService(db);

        var action = () => service.CreateAsync(new CreateUserRequest("michael", "ExamplePassword123!", 1), CancellationToken.None);

        await action.Should().ThrowAsync<ConflictException>();
    }

    private static AppDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(options);
    }
}
