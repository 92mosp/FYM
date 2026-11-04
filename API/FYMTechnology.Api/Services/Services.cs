using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FYMTechnology.Api.Contracts;
using FYMTechnology.Api.Data;
using FYMTechnology.Api.Domain;
using FYMTechnology.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FYMTechnology.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}

public interface IUserService
{
    Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<UserResponse?> AssignRolesAsync(int id, AssignRolesRequest request, CancellationToken cancellationToken);
}

public interface IRoleService
{
    Task<IReadOnlyCollection<RoleResponse>> GetAllAsync(CancellationToken cancellationToken);
}

public class AuthService(AppDbContext db, IOptions<JwtOptions> jwtOptions) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var normalized = request.Username.Trim().ToUpperInvariant();
        var user = await db.Users.AsNoTracking().Include(x => x.UserRoles).ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.NormalizedUsername == normalized, cancellationToken);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var options = jwtOptions.Value;
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(options.ExpiryMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(user.UserRoles.Select(x => new Claim(ClaimTypes.Role, x.Role.Name)));
        var token = new JwtSecurityToken(
            options.Issuer,
            options.Audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)), SecurityAlgorithms.HmacSha256));
        var response = Map(user);
        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt, response);
    }

    private static UserResponse Map(User user) => new(user.Id, user.Username, user.UserRoles.Select(x => x.Role.Name).Order().ToArray(), user.CreatedAtUtc);
}

public class UserService(AppDbContext db) : IUserService
{
    public async Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.Users.AsNoTracking().Include(x => x.UserRoles).ThenInclude(x => x.Role).OrderBy(x => x.Username)
            .Select(x => new UserResponse(x.Id, x.Username, x.UserRoles.Select(r => r.Role.Name).Order().ToArray(), x.CreatedAtUtc))
            .ToArrayAsync(cancellationToken);

    public async Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        await db.Users.AsNoTracking().Include(x => x.UserRoles).ThenInclude(x => x.Role).Where(x => x.Id == id)
            .Select(x => new UserResponse(x.Id, x.Username, x.UserRoles.Select(r => r.Role.Name).Order().ToArray(), x.CreatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var normalized = request.Username.Trim().ToUpperInvariant();
        if (await db.Users.AnyAsync(x => x.NormalizedUsername == normalized, cancellationToken))
        {
            throw new ConflictException("Username is already registered.");
        }
        var roleIds = request.RoleIds.Distinct().ToArray();
        var roles = await db.Roles.Where(x => roleIds.Contains(x.Id)).ToArrayAsync(cancellationToken);
        if (roles.Length != roleIds.Length)
        {
            throw new ValidationException("One or more roles do not exist.");
        }
        var user = new User
        {
            Username = request.Username.Trim(),
            NormalizedUsername = normalized,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12),
            UserRoles = roles.Select(role => new UserRole { Role = role }).ToList()
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return new UserResponse(user.Id, user.Username, roles.Select(x => x.Name).Order().ToArray(), user.CreatedAtUtc);
    }

    public async Task<UserResponse?> AssignRolesAsync(int id, AssignRolesRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null) return null;
        var roleIds = request.RoleIds.Distinct().ToArray();
        var roles = await db.Roles.Where(x => roleIds.Contains(x.Id)).ToArrayAsync(cancellationToken);
        if (roles.Length != roleIds.Length) throw new ValidationException("One or more roles do not exist.");
        user.UserRoles.Clear();
        foreach (var role in roles) user.UserRoles.Add(new UserRole { User = user, Role = role });
        await db.SaveChangesAsync(cancellationToken);
        return new UserResponse(user.Id, user.Username, roles.Select(x => x.Name).Order().ToArray(), user.CreatedAtUtc);
    }
}

public class RoleService(AppDbContext db) : IRoleService
{
    public async Task<IReadOnlyCollection<RoleResponse>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.Roles.AsNoTracking().OrderBy(x => x.Name).Select(x => new RoleResponse(x.Id, x.Name)).ToArrayAsync(cancellationToken);
}

public class ConflictException(string message) : Exception(message);
public class ValidationException(string message) : Exception(message);
