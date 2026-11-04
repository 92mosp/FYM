using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase {
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context) {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetUsers() {
        var users = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Select(u => new {
                u.Id,
                u.Username,
                Roles = u.UserRoles.Select(ur => ur.Role.Name)
            })
            .ToListAsync();
        return Ok(users);
    }

    [HttpPost("register")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto) {
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest(new { message = "Username already exists" });

        var role = await _context.Roles.FindAsync(dto.RoleId);
        if (role == null) return BadRequest(new { message = "Role not found" });

        var user = new User {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.UserRoles.Add(new UserRole {
            UserId = user.Id,
            RoleId = role.Id
        });
        await _context.SaveChangesAsync();

        return Ok(new { message = "User created successfully" });
    }
}
