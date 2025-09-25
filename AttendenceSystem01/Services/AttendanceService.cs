using AttendenceSystem01.Interfaces;
using AttendenceSystem01.Models;

namespace AttendenceSystem01.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repository;

        public AttendanceService(IAttendanceRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> CheckInAsync(int userId)
        {
            try
            {
                var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

                var today = DateOnly.FromDateTime(istNow.Date);

                var todayRecords = await _repository.GetByUserAndDateAsync(userId, today);
                if (todayRecords.Any(a => a.CheckOutTime == null))
                    return "You are already checked in. Please checkout first.";

                var attendance = new Attendance
                {
                    UserId = userId,
                    CheckInTime = istNow.TimeOfDay,
                    AttendanceDate = today,
                    Status = "Pending"
                };

                await _repository.AddAsync(attendance);
                return "Check-in successful";
            }
            catch (Exception ex)
            {
                return $"Error during check-in: {ex.Message}";
            }
        }

        public async Task<string> CheckOutAsync(int userId)
        {
            try
            {
                var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

                var today = DateOnly.FromDateTime(istNow.Date);
                var records = await _repository.GetByUserAndDateAsync(userId, today);

                if (!records.Any())
                    return "You have not checked in today.";

                var lastRecord = records.LastOrDefault(a => a.CheckOutTime == null);
                if (lastRecord == null)
                    return "You have already checked out today.";

                lastRecord.CheckOutTime = istNow.TimeOfDay;
                lastRecord.Status = "Present";
                await _repository.UpdateAsync(lastRecord);

                var firstCheckIn = records.Min(a => a.CheckInTime);
                var lastCheckOutTime = records.Max(a => a.CheckOutTime);

                if (firstCheckIn != null && lastCheckOutTime != null)
                {
                    var duration = lastCheckOutTime.Value - firstCheckIn.Value;
                    string workingHours = duration.ToString(@"hh\:mm\:ss");

                    foreach (var rec in records)
                    {
                        rec.WorkingHours = workingHours;
                        rec.Status = "Present";
                        await _repository.UpdateAsync(rec);
                    }
                }

                return "Check-out successful";
            }
            catch (Exception ex)
            {
                return $"Error during check-out: {ex.Message}";
            }
        }

        public async Task<object> GetTodayAttendanceAsync(int userId)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var records = await _repository.GetByUserAndDateAsync(userId, today);

                if (!records.Any())
                    return new { workingHours = "00:00:00", attendances = new List<object>() };

                var firstCheckIn = records.Min(a => a.CheckInTime);
                var lastCheckOut = records.Max(a => a.CheckOutTime);

                string workingHours = "00:00:00";
                if (firstCheckIn != null && lastCheckOut != null)
                {
                    var duration = lastCheckOut.Value - firstCheckIn.Value;
                    workingHours = duration.ToString(@"hh\:mm\:ss");
                }

                return new
                {
                    workingHours,
                    attendances = records.Select(a => new
                    {
                        a.AttendanceId,
                        a.UserId,
                        a.AttendanceDate,
                        CheckInTime = a.CheckInTime?.ToString(@"hh\:mm\:ss"),
                        CheckOutTime = a.CheckOutTime?.ToString(@"hh\:mm\:ss"),
                        a.Status
                    }).ToList()
                };
            }
            catch
            {
                return new { workingHours = "00:00:00", attendances = new List<object>() };
            }
        }

        // ✅ All Attendance (single user)
        public async Task<object> GetAllAttendanceAsync(int userId)
        {
            try
            {
                var records = await _repository.GetByUserAsync(userId);
                if (!records.Any())
                    return new { totalWorkingHours = "00:00:00", attendances = new List<object>() };

                TimeSpan totalDuration = TimeSpan.Zero;
                foreach (var day in records.GroupBy(r => r.AttendanceDate))
                {
                    var firstCheckIn = day.Min(a => a.CheckInTime);
                    var lastCheckOut = day.Max(a => a.CheckOutTime);

                    if (firstCheckIn != null && lastCheckOut != null)
                        totalDuration += lastCheckOut.Value - firstCheckIn.Value;
                }

                return new
                {
                    totalWorkingHours = totalDuration.ToString(@"hh\:mm\:ss"),
                    attendances = records.Select(a => new
                    {
                        a.AttendanceId,
                        a.UserId,
                        a.AttendanceDate,
                        CheckInTime = a.CheckInTime?.ToString(@"hh\:mm\:ss"),
                        CheckOutTime = a.CheckOutTime?.ToString(@"hh\:mm\:ss"),
                        a.Status
                    }).ToList()
                };
            }
            catch
            {
                return new { totalWorkingHours = "00:00:00", attendances = new List<object>() };
            }
        }

        // ✅ All Users Attendance (Admin Only)
        public async Task<object> GetAllUsersAttendanceAsync()
        {
            try
            {
                var records = await _repository.GetAllAsync();
                if (!records.Any())
                    return new List<object>();

                return records
                    .GroupBy(r => r.UserId)
                    .Select(g =>
                    {
                        TimeSpan totalDuration = TimeSpan.Zero;
                        foreach (var day in g.GroupBy(r => r.AttendanceDate))
                        {
                            var firstCheckIn = day.Min(a => a.CheckInTime);
                            var lastCheckOut = day.Max(a => a.CheckOutTime);

                            if (firstCheckIn != null && lastCheckOut != null)
                                totalDuration += lastCheckOut.Value - firstCheckIn.Value;
                        }

                        return new
                        {
                            UserId = g.Key,
                            TotalWorkingHours = totalDuration.ToString(@"hh\:mm\:ss"),
                            Attendances = g.Select(a => new
                            {
                                a.AttendanceId,
                                a.AttendanceDate,
                                CheckInTime = a.CheckInTime?.ToString(@"hh\:mm\:ss"),
                                CheckOutTime = a.CheckOutTime?.ToString(@"hh\:mm\:ss"),
                                a.Status
                            }).ToList()
                        };
                    }).ToList();
            }
            catch
            {
                return new List<object>();
            }
        }
    }
}
