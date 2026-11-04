using FYMTechnology.Api.Contracts;
using FYMTechnology.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FYMTechnology.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await userService.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:int}/roles")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<UserResponse>> AssignRoles(int id, AssignRolesRequest request, CancellationToken cancellationToken)
    {
        var user = await userService.AssignRolesAsync(id, request, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }
}
