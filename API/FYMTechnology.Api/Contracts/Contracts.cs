using System.ComponentModel.DataAnnotations;

namespace FYMTechnology.Api.Contracts;

public record LoginRequest(
    [property: Required, StringLength(100, MinimumLength = 3)] string Username,
    [property: Required, StringLength(128, MinimumLength = 3)] string Password);

public record LoginResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, UserResponse User);

public record CreateUserRequest(
    [property: Required, StringLength(100, MinimumLength = 3)] string Username,
    [property: Required, StringLength(128, MinimumLength = 3)] string Password,
    [property: MinLength(1)] IReadOnlyCollection<int> RoleIds);

public record AssignRolesRequest([property: MinLength(1)] IReadOnlyCollection<int> RoleIds);
public record UserResponse(int Id, string Username, IReadOnlyCollection<string> Roles, DateTimeOffset CreatedAtUtc);
public record RoleResponse(int Id, string Name);
