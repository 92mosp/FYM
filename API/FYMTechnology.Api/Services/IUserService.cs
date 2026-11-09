using FYMTechnology.Api.Contracts;

namespace FYMTechnology.Api.Services;

public interface IUserService
{
    Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<UserResponse?> AssignRolesAsync(int id, AssignRolesRequest request, int currentUserId, CancellationToken cancellationToken);
}
