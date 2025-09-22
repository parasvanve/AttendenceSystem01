namespace AttendenceSystem01.Dtos
{
    public class UserUpdateDto
    {
        public int UserId { get; set; }              
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }      
        public bool? IsActive { get; set; }         
        public List<int>? RoleIds { get; set; }      
    }
}
