using AttendenceSystem01.Interfaces;
using AttendenceSystem01.Models;
using AttendenceSystem01.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AttendenceSystem01.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly AttendanceDbContext _context;

        public AttendanceRepository(AttendanceDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Attendance attendance)
        {
            try
            {
                await _context.Attendances.AddAsync(attendance);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding attendance: {ex.Message}", ex);
            }
        }

        public async Task<List<Attendance>> GetByUserAndDateAsync(int userId, DateOnly date)
        {
            try
            {
                return await _context.Attendances
                    .Where(a => a.UserId == userId && a.AttendanceDate == date)
                    .OrderBy(a => a.CheckInTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching attendance: {ex.Message}", ex);
            }
        }

        public async Task UpdateAsync(Attendance attendance)
        {
            try
            {
                _context.Attendances.Update(attendance);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating attendance: {ex.Message}", ex);
            }
        }

        public async Task<List<Attendance>> GetByUserAsync(int userId)
        {
            try
            {
                return await _context.Attendances
                    .Where(a => a.UserId == userId)
                    .OrderBy(a => a.AttendanceDate)
                    .ThenBy(a => a.CheckInTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching all user attendance: {ex.Message}", ex);
            }
        }

        // AttendanceRepository.cs
        public async Task<List<Attendance>> GetAllAsync()
        {
            try
            {
                return await _context.Attendances
                    .OrderBy(a => a.UserId)
                    .ThenBy(a => a.AttendanceDate)
                    .ThenBy(a => a.CheckInTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching all attendances: {ex.Message}", ex);
            }
        }


    }
}
