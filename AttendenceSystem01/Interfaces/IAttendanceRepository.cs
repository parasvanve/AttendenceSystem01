using AttendenceSystem01.Models;

namespace AttendenceSystem01.Interfaces
{
    public interface IAttendanceRepository
    {
        Task AddAsync(Attendance attendance);
        Task<List<Attendance>> GetByUserAndDateAsync(int userId, DateOnly date);
        Task UpdateAsync(Attendance attendance);
        Task<List<Attendance>> GetByUserAsync(int userId);
        Task<List<Attendance>> GetAllAsync();
      

    }
}
