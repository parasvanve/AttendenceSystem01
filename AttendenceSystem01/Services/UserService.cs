using AttendenceSystem01.Dtos;
using AttendenceSystem01.Interfaces;
using AttendenceSystem01.Iservices;
using AttendenceSystem01.IServices;
using AttendenceSystem01.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AttendenceSystem01.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IEncryptionService _encryption;
        private readonly IJwtService _jwtService;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository repository, IEncryptionService encryption, IJwtService jwtService, ILogger<UserService> logger)
        {
            _repository = repository;
            _encryption = encryption;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<string> RegisterAsync(RegisterUserDto dto)
        {
            try
            {
                _logger.LogInformation("RegisterAsync called for email {Email}", dto.Email);

                var existing = await _repository.GetByEmailAsync(dto.Email);
                if (existing != null)
                {
                    _logger.LogWarning("Email {Email} already exists", dto.Email);
                    return "Email already exists!";
                }

                var user = new User
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PasswordHash = _encryption.Encrypt(dto.Password),
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.Now,
                    CreatedById = dto.CreatedById
                };

                await _repository.AddUserAsync(user);

                if (dto.RoleIds != null && dto.RoleIds.Count > 0)
                {
                    foreach (var roleId in dto.RoleIds)
                    {
                        var userRole = new UserRole
                        {
                            UserId = user.UserId,
                            RoleId = roleId
                        };
                        await _repository.AddUserRoleAsync(userRole);
                    }
                }

                _logger.LogInformation("User registered successfully with email {Email}", dto.Email);
                return "User registered";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email {Email}", dto.Email);
                return $"Error during registration: {ex.Message}";
            }
        }

        public async Task<(string Message, string? Token, int UserId,string FullName, List<object> Roles)> LoginAsync(LoginDto dto)
        {
            try
            {
                _logger.LogInformation("LoginAsync called for email {Email}", dto.Email);

                if (string.IsNullOrWhiteSpace(dto.Email))
                    return ("Invalid credentials", null, 0,null, new List<object>());

                string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                bool isEmailFormatValid = Regex.IsMatch(dto.Email, pattern, RegexOptions.IgnoreCase);

                if (!isEmailFormatValid)
                {
                    _logger.LogWarning("Invalid email format for {Email}", dto.Email);
                    return ("Invalid email format", null, 0,null, new List<object>());
                }

                var user = await _repository.GetByEmailAsync(dto.Email);

                if (user == null)
                    return ("Invalid email", null, 0,null, new List<object>());

                var decrypted = _encryption.Decrypt(user.PasswordHash);
                if (decrypted != dto.Password)
                    return ("Invalid password", null, 0,null, new List<object>());
             

                var roles = user.UserRoles
                    .Select(ur => new
                    {
                        RoleId = ur.Role.RoleId,
                        RoleName = ur.Role.RoleName
                    })
                    .ToList();

                var roleNames = roles.Select(r => r.RoleName).ToList();
                var token = _jwtService.GenerateToken(user.UserId,user.FullName, user.Email, roleNames);

                return ("Login successful", token, user.UserId,user.FullName, roles.Cast<object>().ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email {Email}", dto.Email);
                return ($"Error during login: {ex.Message}", null, 0, null,new List<object>());
            }
        }

        public async Task<object> GetAllUsersAsync()
        {
            try
            {
                _logger.LogInformation("GetAllUsersAsync called");

                var users = await _repository.GetAllAsync();

                if (users == null || !users.Any())
                {
                    _logger.LogWarning("No users found");
                    return new { message = "No users found", data = new List<object>() };
                }

                var result = users.Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    Roles = u.UserRoles.Select(r => r.Role.RoleName).ToList()
                }).ToList();

                _logger.LogInformation("{Count} users fetched successfully", result.Count);
                return new { message = "Users fetched successfully", data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                return new { message = $"Error: {ex.Message}", data = new List<object>() };
            }
        }

        public async Task<object> GetAllRolesAsync()
        {
            try
            {
                _logger.LogInformation("GetAllRolesAsync called");

                var roles = await _repository.GetAllrolesAsync();

                if (roles == null || !roles.Any())
                {
                    _logger.LogWarning("No roles found");
                    return new { message = "No roles found", data = new List<object>() };
                }

                var result = roles.Select(r => new { r.RoleId, r.RoleName }).ToList();
                _logger.LogInformation("{Count} roles fetched successfully", result.Count);
                return new { message = "Roles fetched successfully", data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching roles");
                return new { message = $"Error: {ex.Message}", data = new List<object>() };
            }
        }

        public async Task<string> UpdateUserAsync(UserUpdateDto dto, bool isAdmin)
        {
            try
            {
                _logger.LogInformation("UpdateUserAsync called for userId {UserId}", dto.UserId);

                var user = await _repository.GetByIdAsync(dto.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User with Id {UserId} not found", dto.UserId);
                    return "User not found";
                }

                user.FullName = dto.FullName ?? user.FullName;
                user.Email = dto.Email ?? user.Email;

                if (!string.IsNullOrEmpty(dto.Password))
                    user.PasswordHash = _encryption.Encrypt(dto.Password);

                if (dto.IsActive.HasValue)
                    user.IsActive = dto.IsActive.Value;

                if (isAdmin && dto.RoleIds != null)
                    await _repository.UpdateUserRolesAsync(user.UserId, dto.RoleIds);

                await _repository.UpdateAsync(user);
                _logger.LogInformation("User with Id {UserId} updated successfully", dto.UserId);
                return "User updated successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with Id {UserId}", dto.UserId);
                return $"Error updating user: {ex.Message}";
            }
        }

        public async Task<string> DeleteUserAsync(int userId)
        {
            try
            {
                _logger.LogInformation("DeleteUserAsync called for userId {UserId}", userId);

                var user = await _repository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User with Id {UserId} not found", userId);
                    return "User not found";
                }

                await _repository.UpdateUserRolesAsync(userId, new List<int>());
                await _repository.DeleteAsync(user);

                _logger.LogInformation("User with Id {UserId} deleted successfully", userId);
                return "User deleted successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with Id {UserId}", userId);
                return $"Error deleting user: {ex.Message}";
            }
        }

        public async Task<object?> GetUserByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("GetUserByIdAsync called for userId {UserId}", id);

                var user = await _repository.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User with Id {UserId} not found", id);
                    return null;
                }

                var roles = user.UserRoles
                    .Select(ur => new
                    {
                        RoleId = ur.Role.RoleId,
                        RoleName = ur.Role.RoleName
                    })
                    .ToList();

                // ✅ Har din ke attendance group kar rahe hain
                var dailyAttendance = user.Attendances?
                    .GroupBy(a => a.AttendanceDate)
                    .Select(g =>
                    {
                        var firstCheckIn = g
                            .Where(x => x.CheckInTime.HasValue)
                            .OrderBy(x => x.CheckInTime)
                            .FirstOrDefault()?.CheckInTime;

                        var lastCheckOut = g
                            .Where(x => x.CheckOutTime.HasValue)
                            .OrderByDescending(x => x.CheckOutTime)
                            .FirstOrDefault()?.CheckOutTime;

                        // ✅ Working hours calculate
                        TimeSpan? totalHours = null;
                        if (firstCheckIn.HasValue && lastCheckOut.HasValue)
                            totalHours = lastCheckOut.Value - firstCheckIn.Value;

                        // ✅ Status (pehle record ka ya latest ka)
                        var status = g.FirstOrDefault()?.Status;

                        return new
                        {
                            AttendanceDate = g.Key,
                            Status = status,
                            FirstCheckIn = firstCheckIn?.ToString(@"hh\:mm\:ss"),
                            LastCheckOut = lastCheckOut?.ToString(@"hh\:mm\:ss"),
                            TotalWorkingHours = totalHours?.ToString(@"hh\:mm\:ss")
                        };
                    })
                    .OrderByDescending(a => a.AttendanceDate)
                    .ToList();

                _logger.LogInformation("User with Id {UserId} retrieved successfully with {RoleCount} roles", id, roles.Count);

                return new
                {
                    user.UserId,
                    user.FullName,
                    user.Email,
                    user.IsActive,
                    Roles = roles,
                    Attendances = dailyAttendance
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user with Id {UserId}", id);
                throw;
            }
        }

    }
}
