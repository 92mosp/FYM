using FYMTechnology.Api.Contracts;

namespace FYMTechnology.Api.Services;

public interface IRoleService
{
    Task<IReadOnlyCollection<RoleResponse>> GetAllAsync(CancellationToken cancellationToken);
}
