using Microsoft.AspNetCore.Mvc;
using AttendenceSystem01.Models;
using Microsoft.EntityFrameworkCore;
using AttendenceSystem01.Dtos;

namespace AttendenceSystem01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly AttendanceDbContext _context;

        public AttendanceController(AttendanceDbContext context)
        {
            _context = context;
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromBody] AttendanceDto dto)
        {
            var now = DateTime.UtcNow;

            var attendance = new Attendance
            {
                UserId = dto.UserId,
                CheckInTime = DateTime.Now.TimeOfDay,           
                AttendanceDate = DateOnly.FromDateTime(now),     
                Status = "Pending"
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Check-in successful"});
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> CheckOut([FromBody] AttendanceDto dto)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var records = await _context.Attendances
                .Where(a => a.UserId == dto.UserId && a.AttendanceDate == today)
                .OrderBy(a => a.CheckInTime)
                .ToListAsync();

            if (!records.Any())
                return BadRequest("User has not checked in today.");

            var lastRecord = records.LastOrDefault(a => a.CheckOutTime == null);

            if (lastRecord == null)
                return BadRequest("All check-ins already checked out.");

            lastRecord.CheckOutTime = DateTime.Now.TimeOfDay;
            _context.Attendances.Update(lastRecord);
            await _context.SaveChangesAsync();

            var firstCheckIn = records.Min(a => a.CheckInTime);
            var lastCheckOutTime = records.Max(a => a.CheckOutTime);
            var duration = lastCheckOutTime.Value - firstCheckIn.Value;

            string workingHours = duration.ToString(@"hh\:mm\:ss");

            foreach (var rec in records)
            {
                rec.WorkingHours = workingHours;   
                rec.Status = "Present";
                _context.Attendances.Update(rec);
            }
            await _context.SaveChangesAsync();

            return Ok(new { message = "Check-out successful"});
        }


        [HttpGet("today/{userId}")]
        public async Task<IActionResult> GetTodayAttendance(int userId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var records = await _context.Attendances
                .Where(a => a.UserId == userId && a.AttendanceDate == today)
                .OrderBy(a => a.CheckInTime)
                .ToListAsync();

            return Ok(records);
        }
    }
}
