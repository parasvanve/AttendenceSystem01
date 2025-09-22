using AttendenceSystem01.Models;

namespace AttendenceSystem01.Interfaces
{
    public interface IAttendanceService
    {
        Task<string> CheckInAsync(int userId);
        Task<string> CheckOutAsync(int userId);
        Task<object> GetTodayAttendanceAsync(int userId);
        Task<object> GetAllAttendanceAsync(int userId);
        Task<object> GetAllUsersAttendanceAsync();
    }
}
