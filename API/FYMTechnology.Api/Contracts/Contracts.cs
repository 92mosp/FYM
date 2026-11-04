namespace FYMTechnology.Api.Contracts;

public record LoginRequest(
    string Username,
    string Password);

public record LoginResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, UserResponse User);

public record CreateUserRequest(
    string Username,
    string Password,
    int RoleId);

public record AssignRolesRequest(IReadOnlyCollection<int> RoleIds);
public record UserResponse(int Id, string Username, IReadOnlyCollection<string> Roles, DateTimeOffset CreatedAtUtc);
public record RoleResponse(int Id, string Name);
