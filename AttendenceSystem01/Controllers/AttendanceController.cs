using AttendenceSystem01.Dtos;
using AttendenceSystem01.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace AttendenceSystem01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _service;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(IAttendanceService service, ILogger<AttendanceController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn()
        {
            try
            {
                var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                _logger.LogInformation("CheckIn called by UserId: {UserId}", userIdFromToken);

                if (userIdFromToken == 0)
                {
                    _logger.LogWarning("Invalid token or user not found for CheckIn");
                    return Unauthorized(new { message = "Invalid token or user not found." });
                }

                var result = await _service.CheckInAsync(userIdFromToken);

                if (result.StartsWith("Error"))
                {
                    _logger.LogError("Error during check-in for UserId {UserId}: {Result}", userIdFromToken, result);
                    return StatusCode(500, new { message = result });
                }

                _logger.LogInformation("Check-in successful for UserId {UserId}", userIdFromToken);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during check-in");
                return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> CheckOut()
        {
            try
            {
                var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                _logger.LogInformation("CheckOut called by UserId: {UserId}", userIdFromToken);

                if (userIdFromToken == 0)
                {
                    _logger.LogWarning("Invalid token or user not found for CheckOut");
                    return Unauthorized(new { message = "Invalid token or user not found." });
                }

                var result = await _service.CheckOutAsync(userIdFromToken);

                if (result.Contains("not checked in") || result.Contains("already checked out"))
                {
                    _logger.LogWarning("CheckOut validation failed for UserId {UserId}: {Result}", userIdFromToken, result);
                    return BadRequest(new { message = result });
                }

                if (result.StartsWith("Error"))
                {
                    _logger.LogError("Error during check-out for UserId {UserId}: {Result}", userIdFromToken, result);
                    return StatusCode(500, new { message = result });
                }

                _logger.LogInformation("Check-out successful for UserId {UserId}", userIdFromToken);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during check-out");
                return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpGet("TodayAttendenceByUser")]
        public async Task<IActionResult> GetTodayAttendance()
        {
            var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation("GetTodayAttendance called by UserId: {UserId}", userIdFromToken);

            var records = await _service.GetTodayAttendanceAsync(userIdFromToken);
            return Ok(records);
        }

        [HttpGet("GetAllAttendenceByUser")]
        [Authorize(Roles = "Admin,Super Admin")]
        public async Task<IActionResult> GetAllAttendance()
        {
            var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation("GetAllAttendance called by UserId: {UserId}", userIdFromToken);

            var records = await _service.GetAllAttendanceAsync(userIdFromToken);
            return Ok(records);
        }

        [HttpGet("GetAllUsersAttendance")]
        [Authorize(Roles = "Admin,Super Admin")]
        public async Task<IActionResult> GetAllUsersAttendance()
        {
            _logger.LogInformation("GetAllUsersAttendance called");
            var result = await _service.GetAllUsersAttendanceAsync();
            return Ok(result);
        }
    }
}