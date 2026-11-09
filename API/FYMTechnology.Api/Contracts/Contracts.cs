using System.ComponentModel.DataAnnotations;

namespace FYMTechnology.Api.Contracts;

public record LoginRequest(
    [Required, MaxLength(100)] string Username,
    [Required, MaxLength(128)] string Password);

public record LoginResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, UserResponse User);

public record CreateUserRequest(
    [Required, StringLength(100, MinimumLength = 3)] string Username,
    [Required, StringLength(128, MinimumLength = 12)] string Password,
    [Required, MinLength(1)] IReadOnlyCollection<int> RoleIds);

public record AssignRolesRequest(
    [Required, MinLength(1)] IReadOnlyCollection<int> RoleIds);

public record UserResponse(int Id, string Username, IReadOnlyCollection<string> Roles, DateTimeOffset CreatedAtUtc);

public record RoleResponse(int Id, string Name);
