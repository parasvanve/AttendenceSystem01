using AttendenceSystem01.Dtos;
using AttendenceSystem01.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AttendenceSystem01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _service;

        public AttendanceController(IAttendanceService service)
        {
            _service = service;
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromBody] AttendanceDto dto)
        {
            var result = await _service.CheckInAsync(dto);

            if (result.StartsWith("Error"))
                return StatusCode(500, new { message = result });

            return Ok(new { message = result });
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> CheckOut([FromBody] AttendanceDto dto)
        {
            var result = await _service.CheckOutAsync(dto);

            if (result.Contains("not checked in") || result.Contains("already checked out"))
                return BadRequest(new { message = result });

            if (result.StartsWith("Error"))
                return StatusCode(500, new { message = result });

            return Ok(new { message = result });
        }

        [HttpGet("TodayAttendenceByUserId/{userId}")]
        public async Task<IActionResult> GetTodayAttendance(int userId)
        {
            var records = await _service.GetTodayAttendanceAsync(userId);
            return Ok(records);
        }

        [HttpGet("GetAllAttendenceByUserId/{userId}")]
        public async Task<IActionResult> GetAllAttendance(int userId)
        {
            var records = await _service.GetAllAttendanceAsync(userId);
            return Ok(records);
        }

        [HttpGet("GetAllUsersAttendance")]
        public async Task<IActionResult> GetAllUsersAttendance()
        {
            var result = await _service.GetAllUsersAttendanceAsync();
            return Ok(result);
        }


    }
}
