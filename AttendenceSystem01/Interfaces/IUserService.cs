using AttendenceSystem01.Dtos;
using System.Threading.Tasks;
namespace AttendenceSystem01.IServices
{
        public interface IUserService
        {
            Task<string> RegisterAsync(RegisterUserDto dto);
            Task<string> LoginAsync(LoginDto dto);
            Task<object> GetAllUsersAsync();
           Task<object> GetAllRolesAsync();
        }
}
