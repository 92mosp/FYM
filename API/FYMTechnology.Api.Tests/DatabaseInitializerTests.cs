using FluentAssertions;
using FYMTechnology.Api.Data;
using FYMTechnology.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace FYMTechnology.Api.Tests;

public sealed class DatabaseInitializerTests : IDisposable
{
    private readonly TestDatabase database = new();

    [Fact]
    public async Task InitializeAsyncIsIdempotent()
    {
        var services = BuildServices("admin", "Admin-Password-123!");

        await DatabaseInitializer.InitializeAsync(services, new FakeEnvironment(Environments.Production));
        await DatabaseInitializer.InitializeAsync(services, new FakeEnvironment(Environments.Production));

        await using var db = database.CreateContext();
        (await db.Roles.Select(x => x.Name).OrderBy(x => x).ToArrayAsync()).Should().Equal("SuperAdmin", "User");
        var admin = await db.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).SingleAsync();
        admin.Username.Should().Be("admin");
        admin.UserRoles.Select(x => x.Role.Name).Should().Equal("SuperAdmin");
        BCrypt.Net.BCrypt.Verify("Admin-Password-123!", admin.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task InitializeAsyncSkipsSuperAdminInDevelopmentWhenSeedIsMissing()
    {
        var services = BuildServices("", "");

        await DatabaseInitializer.InitializeAsync(services, new FakeEnvironment(Environments.Development));

        await using var db = database.CreateContext();
        (await db.Roles.CountAsync()).Should().Be(2);
        (await db.Users.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsyncFailsOutsideDevelopmentWhenSeedIsMissing()
    {
        var services = BuildServices("admin", " ");

        var action = () => DatabaseInitializer.InitializeAsync(services, new FakeEnvironment(Environments.Production));

        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Seed__SuperAdminUsername*");
    }

    private ServiceProvider BuildServices(string username, string password)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(database.Connection));
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeedOptions { SuperAdminUsername = username, SuperAdminPassword = password }));
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new DatabaseOptions { ApplyMigrationsOnStartup = false }));
        return services.BuildServiceProvider();
    }

    public void Dispose() => database.Dispose();

    private sealed class FakeEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "FYMTechnology.Api";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
