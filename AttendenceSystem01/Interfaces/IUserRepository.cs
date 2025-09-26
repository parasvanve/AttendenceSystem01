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
        Task UpdateAsync(User user);
        Task UpdateUserRolesAsync(int userId, List<int> roleIds);
        Task DeleteAsync(User user);
        Task<User?> GetByIdAsync(int userId);
        Task<User?> GetUserByIdAsync(int id);

    }
}
