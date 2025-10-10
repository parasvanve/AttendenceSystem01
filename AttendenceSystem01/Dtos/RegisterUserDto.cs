using System.ComponentModel.DataAnnotations;

namespace AttendenceSystem01.Dtos
{
        public class RegisterUserDto
        {
            [Required]
            public string FullName { get; set; }
            [Required]
            public string Email { get; set; }
            [Required]
            public string Password { get; set; }
            [Required]
            public bool IsActive { get; set; } = true;
            [Required]
            public int CreatedById { get; set; }
             [Required]
            public List<int> RoleIds { get; set; } = new List<int>(); 
        }

}
