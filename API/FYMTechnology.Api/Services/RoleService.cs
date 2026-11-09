using FYMTechnology.Api.Contracts;
using FYMTechnology.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace FYMTechnology.Api.Services;

public class RoleService(AppDbContext db) : IRoleService
{
    public async Task<IReadOnlyCollection<RoleResponse>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.Roles.AsNoTracking().OrderBy(x => x.Name).Select(x => new RoleResponse(x.Id, x.Name)).ToArrayAsync(cancellationToken);
}
