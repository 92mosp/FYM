using FYMTechnology.Api.Data;
using FYMTechnology.Api.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FYMTechnology.Api.Tests;

public sealed class TestDatabase : IDisposable
{
    public TestDatabase()
    {
        Connection = new SqliteConnection("DataSource=:memory:");
        Connection.Open();
        using var db = CreateContext();
        db.Database.EnsureCreated();
    }

    public SqliteConnection Connection { get; }

    public AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(Connection).Options);

    public async Task<(Role SuperAdmin, Role User)> SeedRolesAsync()
    {
        await using var db = CreateContext();
        var superAdmin = new Role { Name = RoleNames.SuperAdmin, NormalizedName = "SUPERADMIN" };
        var user = new Role { Name = RoleNames.User, NormalizedName = "USER" };
        db.Roles.AddRange(superAdmin, user);
        await db.SaveChangesAsync();
        return (superAdmin, user);
    }

    public async Task<User> SeedUserAsync(string username, string password, params Role[] roles)
    {
        await using var db = CreateContext();
        var user = new User
        {
            Username = username,
            NormalizedUsername = username.ToUpperInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, 4),
            UserRoles = roles.Select(role => new UserRole { RoleId = role.Id }).ToList()
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public void Dispose() => Connection.Dispose();
}
