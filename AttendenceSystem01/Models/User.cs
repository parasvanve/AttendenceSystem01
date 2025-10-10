using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AttendenceSystem01.Models;

[Index("Email", Name = "UQ__Users__A9D10534019A77A7", IsUnique = true)]
public partial class User
{
    [Key]
    public int UserId { get; set; }

    [StringLength(100)]
    [Required]
    public string FullName { get; set; } = null!;

    [StringLength(100)]
    [Required(ErrorMessage = "Email is required.")]
    [RegularExpression(@"^[A-Za-z][A-Za-z0-9._%+-]*@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$",
        ErrorMessage = "Invalid email address format (cannot start with number).")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    [Required]
    public string PasswordHash { get; set; } = null!;

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }
    [Required]
    public int? CreatedById { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    [InverseProperty("User")]
    [Required]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
