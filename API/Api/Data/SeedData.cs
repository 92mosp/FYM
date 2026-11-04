public static class SeedData {
    public static void Initialize(AppDbContext context) {
        if (!context.Roles.Any()) {
            context.Roles.AddRange(
                new Role { Name = "SuperAdmin" },
                new Role { Name = "Admin" },
                new Role { Name = "User" }
            );
            context.SaveChanges();
        }

        if (!context.Users.Any()) {
            var superAdminRole = context.Roles.First(r => r.Name == "SuperAdmin");
            var adminUser = new User {
                Username = "superadmin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123*")
            };
            context.Users.Add(adminUser);
            context.SaveChanges();

            context.UserRoles.Add(new UserRole {
                UserId = adminUser.Id,
                RoleId = superAdminRole.Id
            });
            context.SaveChanges();
        }
    }
}
