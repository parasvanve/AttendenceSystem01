namespace AttendenceSystem01.Dtos
{
        public class RegisterUserDto
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public bool IsActive { get; set; } = true;
            public int CreatedById { get; set; }
            public List<int> RoleIds { get; set; } = new List<int>(); 
        }

}
