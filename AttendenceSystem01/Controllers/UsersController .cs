using AttendenceSystem01.Dtos;
using AttendenceSystem01.Interfaces;
using AttendenceSystem01.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace AttendenceSystem01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService service, ILogger<UsersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            try
            {
                _logger.LogInformation("Register called for Email: {Email}", dto.Email);

                var result = await _service.RegisterAsync(dto);

                if (result == "Email already exists!")
                {
                    _logger.LogWarning("Registration failed, email already exists: {Email}", dto.Email);
                    return BadRequest(new { message = result });
                }

                if (result.StartsWith("Error"))
                {
                    _logger.LogError("Error during registration for Email: {Email}", dto.Email);
                    return StatusCode(500, new { message = result });
                }

                _logger.LogInformation("User registered successfully: {Email}", dto.Email);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for Email: {Email}", dto.Email);
                return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                _logger.LogInformation("Login called for Email: {Email}", dto.Email);

                var result = await _service.LoginAsync(dto);

                if (result.Token == null)
                {
                    _logger.LogWarning("Login failed for Email: {Email}", dto.Email);
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                _logger.LogInformation("Login successful for Email: {Email}", dto.Email);
                return Ok(new
                {
                    message = result.Message,
                    token = result.Token,
                    userId = result.UserId,
                    roles = result.Roles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for Email: {Email}", dto.Email);
                return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpGet("GetAllUsers")]
        [Authorize(Roles = "Admin,Super Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("GetAllUsers called");
            var result = await _service.GetAllUsersAsync();
            return Ok(result);
        }

        [HttpGet("GetAllRoles")]
        [Authorize(Roles = "Admin,Super Admin")]
        public async Task<IActionResult> GetAllRoles()
        {
            _logger.LogInformation("GetAllRoles called");
            var result = await _service.GetAllRolesAsync();
            return Ok(result);
        }

        [HttpPut("update")]
        [Authorize(Roles = "Admin,Super Admin")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto dto)
        {
            var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && userIdFromToken != dto.UserId)
            {
                _logger.LogWarning("Unauthorized update attempt by UserId {UserId}", userIdFromToken);
                return Unauthorized(new { message = "You are not authorized to update this user." });
            }

            _logger.LogInformation("UpdateUser called for UserId {UserId}", dto.UserId);
            var result = await _service.UpdateUserAsync(dto, isAdmin);
            return Ok(new { message = result });
        }

        [HttpDelete("delete/{userId}")]
        [Authorize(Roles = "Admin,Super Admin")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            _logger.LogInformation("DeleteUser called for UserId {UserId}", userId);
            var result = await _service.DeleteUserAsync(userId);

            if (result.StartsWith("Error"))
            {
                _logger.LogError("Error deleting user with UserId {UserId}", userId);
                return StatusCode(500, new { message = result });
            }

            _logger.LogInformation("User deleted successfully with UserId {UserId}", userId);
            return Ok(new { message = result });
        }

        [HttpGet("GetUserById/{id}")]
        [Authorize(Roles = "Admin,Super Admin")]
        public async Task<IActionResult> GetUserById(int id)
        {
            _logger.LogInformation("GetUserById called for UserId {UserId}", id);
            var result = await _service.GetUserByIdAsync(id);

            if (result == null)
            {
                _logger.LogWarning("User not found with UserId {UserId}", id);
                return NotFound(new { message = "User not found" });
            }

            _logger.LogInformation("User retrieved successfully with UserId {UserId}", id);
            return Ok(result);
        }
    }
}
