using AttendenceSystem01.Dtos;
using AttendenceSystem01.Iservices;
using AttendenceSystem01.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly AttendanceDbContext _context;
    private readonly IEncryptionService _encryption;

    public UsersController(AttendanceDbContext context, IEncryptionService encryption)
    {
        _context = context;
        _encryption = encryption;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Email already exists!");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = _encryption.Encrypt(dto.Password),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.Now,
            CreatedById = dto.CreatedById
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        if (dto.RoleIds != null && dto.RoleIds.Count > 0)
        {
            foreach (var roleId in dto.RoleIds)
            {
                var userRole = new UserRole
                {
                    UserId = user.UserId,
                    RoleId = roleId
                };
                _context.UserRoles.Add(userRole);
            }
            await _context.SaveChangesAsync();
        }

        return Ok(new { message = "User registered" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
            return Unauthorized("Invalid credentials");

        var decrypted = _encryption.Decrypt(user.PasswordHash);
        if (decrypted != dto.Password)
            return Unauthorized("Invalid credentials");

        var roles = user.UserRoles.Select(r => new { r.RoleId, r.Role.RoleName }).ToList();

        return Ok(new
        {
            message = "Login successful",
            userId = user.UserId,
            fullName = user.FullName,
            roles = roles
        });
    }
}
