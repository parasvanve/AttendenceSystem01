using System.ComponentModel.DataAnnotations;

namespace AttendenceSystem01.Dtos
{
        public class RegisterUserDto
        {
            [Required]
            [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters.")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Email is required.")]

            [RegularExpression(@"^[A-Za-z][A-Za-z0-9._%+-]*@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$",
            ErrorMessage = "Invalid email address format (cannot start with number).")]
            [EmailAddress(ErrorMessage = "Invalid email address format.")]
            public string Email { get; set; }
            [Required]
            public string Password { get; set; }
            public bool IsActive { get; set; } = true;
            [Required]
            public int CreatedById { get; set; }
             [Required]
            public List<int> RoleIds { get; set; } = new List<int>(); 
        }

}
