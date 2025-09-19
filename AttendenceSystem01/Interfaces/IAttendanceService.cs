using AttendenceSystem01.Dtos;
using AttendenceSystem01.Models;

namespace AttendenceSystem01.Interfaces
{
    public interface IAttendanceService
    {
        Task<string> CheckInAsync(AttendanceDto dto);
        Task<string> CheckOutAsync(AttendanceDto dto);
        Task<object> GetTodayAttendanceAsync(int userId);
        Task<object> GetAllAttendanceAsync(int userId);
        Task<object> GetAllUsersAttendanceAsync();
    }
}
