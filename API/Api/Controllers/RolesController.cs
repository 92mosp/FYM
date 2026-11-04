using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase {
    private readonly AppDbContext _context;

    public RolesController(AppDbContext context) {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetRoles() {
        var roles = await _context.Roles.Select(r => new { r.Id, r.Name }).ToListAsync();
        return Ok(roles);
    }
}
