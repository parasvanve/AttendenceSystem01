using AttendenceSystem01.Interfaces;
using AttendenceSystem01.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AttendenceSystem01.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly AttendanceDbContext _context;
        private readonly ILogger<AttendanceRepository> _logger;

        public AttendanceRepository(AttendanceDbContext context, ILogger<AttendanceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(Attendance attendance)
        {
            try
            {
                _logger.LogInformation("Adding attendance for UserId: {UserId}", attendance.UserId);
                await _context.Attendances.AddAsync(attendance);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Attendance added successfully for UserId: {UserId}", attendance.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding attendance for UserId: {UserId}", attendance.UserId);
                throw new Exception($"Error adding attendance: {ex.Message}", ex);
            }
        }

        public async Task<List<Attendance>> GetByUserAndDateAsync(int userId, DateOnly date)
        {
            try
            {
                _logger.LogInformation("Fetching attendance for UserId: {UserId} on Date: {Date}", userId, date);
                return await _context.Attendances
                    .Where(a => a.UserId == userId && a.AttendanceDate == date)
                    .OrderBy(a => a.CheckInTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching attendance for UserId: {UserId}", userId);
                throw new Exception($"Error fetching attendance: {ex.Message}", ex);
            }
        }

        public async Task UpdateAsync(Attendance attendance)
        {
            try
            {
                _logger.LogInformation("Updating attendance for AttendanceId: {AttendanceId}", attendance.AttendanceId);
                _context.Attendances.Update(attendance);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Attendance updated successfully for AttendanceId: {AttendanceId}", attendance.AttendanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating attendance for AttendanceId: {AttendanceId}", attendance.AttendanceId);
                throw new Exception($"Error updating attendance: {ex.Message}", ex);
            }
        }

        public async Task<List<Attendance>> GetByUserAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Fetching all attendance for UserId: {UserId}", userId);
                return await _context.Attendances
                    .Where(a => a.UserId == userId)
                    .OrderBy(a => a.AttendanceDate)
                    .ThenBy(a => a.CheckInTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all attendance for UserId: {UserId}", userId);
                throw new Exception($"Error fetching all user attendance: {ex.Message}", ex);
            }
        }

        public async Task<List<Attendance>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all attendances");
                return await _context.Attendances
                    .OrderBy(a => a.UserId)
                    .ThenBy(a => a.AttendanceDate)
                    .ThenBy(a => a.CheckInTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all attendances");
                throw new Exception($"Error fetching all attendances: {ex.Message}", ex);
            }
        }
    }
}