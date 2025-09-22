namespace AttendenceSystem01
{
   
    using global::AttendenceSystem01.Interfaces;
    using global::AttendenceSystem01.Models;
    using Microsoft.EntityFrameworkCore;

    namespace AttendenceSystem01.Repositories
    {
        public class UserRepository : IUserRepository
        {
            private readonly AttendanceDbContext _context;

            public UserRepository(AttendanceDbContext context)
            {
                _context = context;
            }

            public async Task<User?> GetByEmailAsync(string email)
            {
                try
                {
                    return await _context.Users
                        .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                        .FirstOrDefaultAsync(u => u.Email == email);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error fetching user by email: {ex.Message}", ex);
                }
            }

            public async Task AddUserAsync(User user)
            {
                try
                {
                    await _context.Users.AddAsync(user);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error adding user: {ex.Message}", ex);
                }
            }

            public async Task AddUserRoleAsync(UserRole userRole)
            {
                try
                {
                    await _context.UserRoles.AddAsync(userRole);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error adding user role: {ex.Message}", ex);
                }
            }


            public async Task<List<User>> GetAllAsync()
            {
                try
                {
                    return await _context.Users
                        .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                        .ToListAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error fetching users: {ex.Message}", ex);
                }
            }


            public async Task<List<Role>> GetAllrolesAsync()
            {
                try
                {
                    return await _context.Roles.ToListAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error fetching roles: {ex.Message}", ex);
                }
            }

            public async Task UpdateAsync(User user)
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }


            public async Task UpdateUserRolesAsync(int userId, List<int> roleIds)
            {
                var existingRoles = _context.UserRoles.Where(ur => ur.UserId == userId);
                _context.UserRoles.RemoveRange(existingRoles);

                foreach (var roleId in roleIds)
                {
                    _context.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
                }

                await _context.SaveChangesAsync();
            }

            public async Task DeleteAsync(User user)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            public async Task<User?> GetByIdAsync(int userId)
            {
                return await _context.Users
                    .Include(u => u.UserRoles)   
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userId);
            }


        }
    }

}
