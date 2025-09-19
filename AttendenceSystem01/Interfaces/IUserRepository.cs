using AttendenceSystem01.Models;
using System.Threading.Tasks;

namespace AttendenceSystem01.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task AddUserAsync(User user);
        Task AddUserRoleAsync(UserRole userRole);
        Task<List<User>> GetAllAsync();
        Task<List<Role>> GetAllrolesAsync();

    }
}
