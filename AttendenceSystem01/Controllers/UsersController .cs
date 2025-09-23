using AttendenceSystem01.Dtos;
using AttendenceSystem01.Interfaces;
using AttendenceSystem01.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace AttendenceSystem01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            try
            {
                var result = await _service.RegisterAsync(dto);

                if (result == "Email already exists!")
                    return BadRequest(new { message = result });

                if (result.StartsWith("Error"))
                    return StatusCode(500, new { message = result });

                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _service.LoginAsync(dto);

                if (result.Token == null)
                    return Unauthorized(new { message = "Invalid credentials" });

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
                return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
            }
        }



        [HttpGet("GetAllUsers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _service.GetAllUsersAsync();
            return Ok(result);
        }

        [HttpGet("GetAllRoles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllRoles()
        {
            var result = await _service.GetAllRolesAsync();
            return Ok(result);
        }

        [HttpPut("update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto dto)
        {
            var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && userIdFromToken != dto.UserId)
                return Unauthorized(new { message = "You are not authorized to update this user." });

            var result = await _service.UpdateUserAsync(dto, isAdmin);
            return Ok(new { message = result });
        }

        [HttpDelete("delete/{userId}")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var result = await _service.DeleteUserAsync(userId);
            if (result.StartsWith("Error"))
                return StatusCode(500, new { message = result });

            return Ok(new { message = result });
        }


    }
}
