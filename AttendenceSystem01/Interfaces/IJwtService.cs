using System.Collections.Generic;

namespace AttendenceSystem01.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string email, List<string> roles);

        bool ValidateToken(string token, out int userId);
    }
}
