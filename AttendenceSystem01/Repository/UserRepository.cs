using AttendenceSystem01.Interfaces;
using AttendenceSystem01.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttendenceSystem01.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AttendanceDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AttendanceDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation("GetByEmailAsync called for email {Email}", email);

                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                    _logger.LogWarning("User not found with email {Email}", email);
                else
                    _logger.LogInformation("User retrieved successfully with email {Email}", email);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user by email {Email}", email);
                throw new Exception($"Error fetching user by email: {ex.Message}", ex);
            }
        }

        public async Task AddUserAsync(User user)
        {
            try
            {
                _logger.LogInformation("AddUserAsync called for email {Email}", user.Email);

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User added successfully with email {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user with email {Email}", user.Email);
                throw new Exception($"Error adding user: {ex.Message}", ex);
            }
        }

        public async Task AddUserRoleAsync(UserRole userRole)
        {
            try
            {
                _logger.LogInformation("AddUserRoleAsync called for UserId {UserId} RoleId {RoleId}", userRole.UserId, userRole.RoleId);

                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();

                _logger.LogInformation("UserRole added successfully for UserId {UserId} RoleId {RoleId}", userRole.UserId, userRole.RoleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user role for UserId {UserId} RoleId {RoleId}", userRole.UserId, userRole.RoleId);
                throw new Exception($"Error adding user role: {ex.Message}", ex);
            }
        }

        public async Task<List<User>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("GetAllAsync called");

                var users = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .ToListAsync();

                _logger.LogInformation("{Count} users retrieved successfully", users.Count);
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all users");
                throw new Exception($"Error fetching users: {ex.Message}", ex);
            }
        }

        public async Task<List<Role>> GetAllrolesAsync()
        {
            try
            {
                _logger.LogInformation("GetAllrolesAsync called");

                var roles = await _context.Roles.ToListAsync();

                _logger.LogInformation("{Count} roles retrieved successfully", roles.Count);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching roles");
                throw new Exception($"Error fetching roles: {ex.Message}", ex);
            }
        }

        public async Task UpdateAsync(User user)
        {
            try
            {
                _logger.LogInformation("UpdateAsync called for UserId {UserId}", user.UserId);

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User with UserId {UserId} updated successfully", user.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with UserId {UserId}", user.UserId);
                throw;
            }
        }

        public async Task UpdateUserRolesAsync(int userId, List<int> roleIds)
        {
            try
            {
                _logger.LogInformation("UpdateUserRolesAsync called for UserId {UserId}", userId);

                var existingRoles = _context.UserRoles.Where(ur => ur.UserId == userId);
                _context.UserRoles.RemoveRange(existingRoles);

                foreach (var roleId in roleIds)
                {
                    _context.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("User roles updated successfully for UserId {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user roles for UserId {UserId}", userId);
                throw;
            }
        }

        public async Task DeleteAsync(User user)
        {
            try
            {
                _logger.LogInformation("DeleteAsync called for UserId {UserId}", user.UserId);

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User with UserId {UserId} deleted successfully", user.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with UserId {UserId}", user.UserId);
                throw;
            }
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("GetByIdAsync called for UserId {UserId}", userId);

                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    _logger.LogWarning("User with UserId {UserId} not found", userId);
                else
                    _logger.LogInformation("User with UserId {UserId} retrieved successfully", userId);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user with UserId {UserId}", userId);
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("GetUserByIdAsync called for UserId {UserId}", id);

                var user = await _context.Users
                    .Include(u => u.Attendances)
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.UserId == id);

                if (user == null)
                    _logger.LogWarning("User with UserId {UserId} not found", id);
                else
                    _logger.LogInformation("User with UserId {UserId} retrieved successfully", id);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user with UserId {UserId}", id);
                throw;
            }
        }
    }
}
