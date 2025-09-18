using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AttendenceSystem01.Models;

[Table("Attendance")]
public partial class Attendance
{
    [Key]
    public int AttendanceId { get; set; }

    public int UserId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    [Column(TypeName = "time")]
    public TimeSpan? CheckInTime { get; set; }

    [Column(TypeName = "time")]
    public TimeSpan? CheckOutTime { get; set; }


    [StringLength(20)]
    public string? Status { get; set; }

    public string? WorkingHours { get; set; }


    [ForeignKey("UserId")]
    [InverseProperty("Attendances")]
    public virtual User User { get; set; } = null!;
}
