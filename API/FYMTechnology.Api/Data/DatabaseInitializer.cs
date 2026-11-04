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
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        var superAdminRole = await db.Roles.SingleOrDefaultAsync(x => x.NormalizedName == "SUPERADMIN");
        if (superAdminRole is null)
        {
            superAdminRole = new Role { Name = "SuperAdmin", NormalizedName = "SUPERADMIN" };
            db.Roles.Add(superAdminRole);
        }

        if (!await db.Roles.AnyAsync(x => x.NormalizedName == "USER"))
        {
            db.Roles.Add(new Role { Name = "User", NormalizedName = "USER" });
        }

        var seed = scope.ServiceProvider.GetRequiredService<IOptions<SeedOptions>>().Value;
        if (!environment.IsDevelopment() && (string.IsNullOrWhiteSpace(seed.SuperAdminUsername) || string.IsNullOrWhiteSpace(seed.SuperAdminPassword)))
        {
            throw new InvalidOperationException("Production super administrator seed credentials are required.");
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
}
