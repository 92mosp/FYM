using System.ComponentModel.DataAnnotations;

namespace FYMTechnology.Api.Contracts;

public sealed record LoginRequest(
    [property: Required, StringLength(100, MinimumLength = 3)] string Username,
    [property: Required, StringLength(128, MinimumLength = 8)] string Password);

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, UserResponse User);

public sealed record CreateUserRequest(
    [property: Required, StringLength(100, MinimumLength = 3)] string Username,
    [property: Required, StringLength(128, MinimumLength = 12)] string Password,
    [property: MinLength(1)] IReadOnlyCollection<int> RoleIds);

public sealed record AssignRolesRequest([property: MinLength(1)] IReadOnlyCollection<int> RoleIds);
public sealed record UserResponse(int Id, string Username, IReadOnlyCollection<string> Roles, DateTimeOffset CreatedAtUtc);
public sealed record RoleResponse(int Id, string Name);
