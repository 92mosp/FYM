using FYMTechnology.Api.Domain;
using FYMTechnology.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FYMTechnology.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IHostEnvironment environment)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var db = provider.GetRequiredService<AppDbContext>();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DatabaseInitializer));

        if (provider.GetRequiredService<IOptions<DatabaseOptions>>().Value.ApplyMigrationsOnStartup)
        {
            await db.Database.MigrateAsync();
        }

        var superAdminRole = await EnsureRoleAsync(db, RoleNames.SuperAdmin);
        await EnsureRoleAsync(db, RoleNames.User);

        var seed = provider.GetRequiredService<IOptions<SeedOptions>>().Value;
        if (string.IsNullOrWhiteSpace(seed.SuperAdminUsername) || string.IsNullOrWhiteSpace(seed.SuperAdminPassword))
        {
            if (!environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "Seed:SuperAdminUsername and Seed:SuperAdminPassword are required. Set them as Seed__SuperAdminUsername and Seed__SuperAdminPassword app settings.");
            }
            logger.LogWarning(
                "SuperAdmin seed skipped: set Seed:SuperAdminUsername and Seed:SuperAdminPassword with dotnet user-secrets to create it.");
            await db.SaveChangesAsync();
            return;
        }

        var normalizedUsername = seed.SuperAdminUsername.Trim().ToUpperInvariant();
        var administrator = await db.Users.Include(x => x.UserRoles).SingleOrDefaultAsync(x => x.NormalizedUsername == normalizedUsername);
        if (administrator is null)
        {
            administrator = new User
            {
                Username = seed.SuperAdminUsername.Trim(),
                NormalizedUsername = normalizedUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seed.SuperAdminPassword, 12)
            };
            db.Users.Add(administrator);
        }

        if (!administrator.UserRoles.Any(x => x.RoleId == superAdminRole.Id))
        {
            administrator.UserRoles.Add(new UserRole { User = administrator, Role = superAdminRole });
        }

        await db.SaveChangesAsync();
    }

    private static async Task<Role> EnsureRoleAsync(AppDbContext db, string name)
    {
        var normalized = name.ToUpperInvariant();
        var role = await db.Roles.SingleOrDefaultAsync(x => x.NormalizedName == normalized);
        if (role is null)
        {
            role = new Role { Name = name, NormalizedName = normalized };
            db.Roles.Add(role);
        }
        return role;
    }
}
