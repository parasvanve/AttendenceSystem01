namespace AttendenceSystem01.Dtos
{
    public class AttendanceDto
    {
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string? Status { get; set; }

    }
}
