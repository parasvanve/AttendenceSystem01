using AttendenceSystem01.Dtos;
using AttendenceSystem01.IServices;
using AttendenceSystem01.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

                if (result == "Invalid credentials")
                    return Unauthorized(new { message = result });

                if (result.StartsWith("Error"))
                    return StatusCode(500, new { message = result });

                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
            }
        }
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _service.GetAllUsersAsync();
            return Ok(result);
        }

        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var result = await _service.GetAllRolesAsync();
            return Ok(result);
        }
    }
}
