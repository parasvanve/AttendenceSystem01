using AttendenceSystem01.Dtos;
using AttendenceSystem01.Interfaces;
using AttendenceSystem01.Iservices;
using AttendenceSystem01.IServices;
using AttendenceSystem01.Models;


namespace AttendenceSystem01.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IEncryptionService _encryption;

        public UserService(IUserRepository repository, IEncryptionService encryption)
        {
            _repository = repository;
            _encryption = encryption;
        }

        public async Task<string> RegisterAsync(RegisterUserDto dto)
        {
            try
            {
                var existing = await _repository.GetByEmailAsync(dto.Email);
                if (existing != null)
                    return "Email already exists!";

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

                return "User registered";
            }
            catch (Exception ex)
            {
                return $"Error during registration: {ex.Message}";
            }
        }


        public async Task<string> LoginAsync(LoginDto dto)
        {
            try
            {
                var user = await _repository.GetByEmailAsync(dto.Email);
                if (user == null)
                    return "Invalid credentials";

                var decrypted = _encryption.Decrypt(user.PasswordHash);
                if (decrypted != dto.Password)
                    return "Invalid credentials";

                return "Login successful";
            }
            catch (Exception ex)
            {
                return $"Error during login: {ex.Message}";
            }
        }

        public async Task<object> GetAllUsersAsync()
        {
            try
            {
                var users = await _repository.GetAllAsync();

                if (users == null || !users.Any())
                    return new { message = "No users found", data = new List<object>() };

                var result = users.Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    Roles = u.UserRoles.Select(r => r.Role.RoleName).ToList()
                }).ToList();

                return new { message = "Users fetched successfully", data = result };
            }
            catch (Exception ex)
            {
                return new { message = $"Error: {ex.Message}", data = new List<object>() };
            }
        }
        public async Task<object> GetAllRolesAsync()
        {
            try
            {
                var roles = await _repository.GetAllrolesAsync();

                if (roles == null || !roles.Any())
                    return new { message = "No roles found", data = new List<object>() };

                var result = roles.Select(r => new { r.RoleId, r.RoleName }).ToList();
                return new { message = "Roles fetched successfully", data = result };
            }
            catch (Exception ex)
            {
                return new { message = $"Error: {ex.Message}", data = new List<object>() };
            }
        }


    }
}
