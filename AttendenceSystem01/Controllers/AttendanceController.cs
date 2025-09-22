using AttendenceSystem01.Dtos;
using AttendenceSystem01.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AttendenceSystem01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _service;

        public AttendanceController(IAttendanceService service)
        {
            _service = service;
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn()
        {
            try
            {
                var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userIdFromToken == 0)
                    return Unauthorized(new { message = "Invalid token or user not found." });

                var result = await _service.CheckInAsync(userIdFromToken);

                if (result.StartsWith("Error"))
                    return StatusCode(500, new { message = result });

                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Unexpected error: {ex.Message}" });
            }
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> CheckOut()
        {
            try
            {
                var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userIdFromToken == 0)
                    return Unauthorized(new { message = "Invalid token or user not found." });

                var result = await _service.CheckOutAsync(userIdFromToken);

                if (result.Contains("not checked in") || result.Contains("already checked out"))
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

        [HttpGet("TodayAttendenceByUser")]
        public async Task<IActionResult> GetTodayAttendance()
        {
            var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var records = await _service.GetTodayAttendanceAsync(userIdFromToken);
            return Ok(records);
        }

        // ✅ All Attendance (current user only)
        [HttpGet("GetAllAttendenceByUser")]
        public async Task<IActionResult> GetAllAttendance()
        {
            var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var records = await _service.GetAllAttendanceAsync(userIdFromToken);
            return Ok(records);
        }

        // ✅ All Users Attendance (Admin only)
        [HttpGet("GetAllUsersAttendance")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsersAttendance()
        {
            var result = await _service.GetAllUsersAttendanceAsync();
            return Ok(result);
        }
    }
}
