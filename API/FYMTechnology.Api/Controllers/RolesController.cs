using FYMTechnology.Api.Contracts;
using FYMTechnology.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FYMTechnology.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/roles")]
public class RolesController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<RoleResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await roleService.GetAllAsync(cancellationToken));
}
