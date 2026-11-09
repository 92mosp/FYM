using FYMTechnology.Api.Contracts;
using FYMTechnology.Api.Data;
using FYMTechnology.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace FYMTechnology.Api.Services;

public class UserService(AppDbContext db) : IUserService
{
    public async Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.Users.AsNoTracking()
            .OrderBy(x => x.Username)
            .Select(x => new UserResponse(
                x.Id,
                x.Username,
                x.UserRoles.OrderBy(r => r.Role.Name).Select(r => r.Role.Name).ToArray(),
                x.CreatedAtUtc))
            .ToArrayAsync(cancellationToken);

    public async Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        await db.Users.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new UserResponse(
                x.Id,
                x.Username,
                x.UserRoles.OrderBy(r => r.Role.Name).Select(r => r.Role.Name).ToArray(),
                x.CreatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var normalized = request.Username.Trim().ToUpperInvariant();
        if (await db.Users.AnyAsync(x => x.NormalizedUsername == normalized, cancellationToken))
        {
            throw new ConflictException("Username is already registered.");
        }
        var roles = await LoadRolesAsync(request.RoleIds, cancellationToken);
        var user = new User
        {
            Username = request.Username.Trim(),
            NormalizedUsername = normalized,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12),
            UserRoles = roles.Select(role => new UserRole { Role = role }).ToList()
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(user, roles);
    }

    public async Task<UserResponse?> AssignRolesAsync(int id, AssignRolesRequest request, int currentUserId, CancellationToken cancellationToken)
    {
        var user = await db.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return null;
        }
        var roles = await LoadRolesAsync(request.RoleIds, cancellationToken);
        var wasSuperAdmin = user.UserRoles.Any(x => x.Role.Name == RoleNames.SuperAdmin);
        var staysSuperAdmin = roles.Any(x => x.Name == RoleNames.SuperAdmin);
        if (user.Id == currentUserId && wasSuperAdmin && !staysSuperAdmin)
        {
            throw new ConflictException("You cannot remove your own SuperAdmin role.");
        }
        user.UserRoles.Clear();
        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole { User = user, Role = role });
        }
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(user, roles);
    }

    private async Task<Role[]> LoadRolesAsync(IReadOnlyCollection<int>? requestedRoleIds, CancellationToken cancellationToken)
    {
        var roleIds = (requestedRoleIds ?? []).Distinct().ToArray();
        if (roleIds.Length == 0)
        {
            throw new ValidationException("At least one role is required.");
        }
        var roles = await db.Roles.Where(x => roleIds.Contains(x.Id)).ToArrayAsync(cancellationToken);
        if (roles.Length != roleIds.Length)
        {
            throw new ValidationException("One or more roles do not exist.");
        }
        return roles;
    }

    private static UserResponse ToResponse(User user, IEnumerable<Role> roles) =>
        new(user.Id, user.Username, roles.Select(x => x.Name).Order().ToArray(), user.CreatedAtUtc);
}
