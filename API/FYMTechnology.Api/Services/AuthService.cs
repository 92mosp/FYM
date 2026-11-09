using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FYMTechnology.Api.Contracts;
using FYMTechnology.Api.Data;
using FYMTechnology.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FYMTechnology.Api.Services;

public class AuthService(AppDbContext db, IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private static readonly string DummyPasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString(), 12);

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var normalized = request.Username.Trim().ToUpperInvariant();
        var user = await db.Users.AsNoTracking().Include(x => x.UserRoles).ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.NormalizedUsername == normalized, cancellationToken);
        var passwordMatches = BCrypt.Net.BCrypt.Verify(request.Password, user?.PasswordHash ?? DummyPasswordHash);
        if (user is null || !passwordMatches)
        {
            return null;
        }

        var options = jwtOptions.Value;
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(options.ExpiryMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)),
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
        var response = new UserResponse(user.Id, user.Username, user.UserRoles.Select(x => x.Role.Name).Order().ToArray(), user.CreatedAtUtc);
        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt, response);
    }
}
