using AttendenceSystem01.Dtos;
using System.Threading.Tasks;
namespace AttendenceSystem01.IServices
{
        public interface IUserService
        {
            Task<string> RegisterAsync(RegisterUserDto dto);
            Task<(string Message, string? Token)> LoginAsync(LoginDto dto);
            Task<object> GetAllUsersAsync();
            Task<object> GetAllRolesAsync();
            Task<string> UpdateUserAsync(UserUpdateDto dto, bool isAdmin);
            Task<string> DeleteUserAsync(int userId);
    } 
}
