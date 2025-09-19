using AttendenceSystem01.Dtos;
using AttendenceSystem01.Interfaces;
using AttendenceSystem01.Models;
using AttendenceSystem01.Interfaces;
using AttendenceSystem01.Interfaces;

namespace AttendenceSystem01.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repository;

        public AttendanceService(IAttendanceRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> CheckInAsync(AttendanceDto dto)
        {
            try
            {
                var now = DateTime.UtcNow;

                var attendance = new Attendance
                {
                    UserId = dto.UserId,
                    CheckInTime = DateTime.Now.TimeOfDay,
                    AttendanceDate = DateOnly.FromDateTime(now),
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

        public async Task<string> CheckOutAsync(AttendanceDto dto)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var records = await _repository.GetByUserAndDateAsync(dto.UserId, today);

                if (!records.Any())
                    return "User has not checked in today.";

                var lastRecord = records.LastOrDefault(a => a.CheckOutTime == null);

                if (lastRecord == null)
                    return "All check-ins already checked out.";

                lastRecord.CheckOutTime = DateTime.Now.TimeOfDay;
                await _repository.UpdateAsync(lastRecord);

                // Calculate working hours
                var firstCheckIn = records.Min(a => a.CheckInTime);
                var lastCheckOutTime = records.Max(a => a.CheckOutTime);
                var duration = lastCheckOutTime.Value - firstCheckIn.Value;

                string workingHours = duration.ToString(@"hh\:mm\:ss");

                foreach (var rec in records)
                {
                    rec.WorkingHours = workingHours;
                    rec.Status = "Present";
                    await _repository.UpdateAsync(rec);
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

                if (records == null || !records.Any())
                {
                    return new { workingHours = "00:00:00", attendances = new List<object>() };
                }

                // calculate working hours (first check-in → last checkout)
                var firstCheckIn = records.Min(a => a.CheckInTime);
                var lastCheckOut = records.Max(a => a.CheckOutTime);

                string workingHours = "00:00:00";
                if (firstCheckIn != null && lastCheckOut != null)
                {
                    var duration = lastCheckOut.Value - firstCheckIn.Value;
                    workingHours = duration.ToString(@"hh\:mm\:ss");
                }

                // return shaped response
                var response = new
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
                    })
                };
                return response;
            }
            catch (Exception)
            {
                return new { workingHours = "00:00:00", attendances = new List<object>() };
            }
        }

        // AttendanceService.cs
        public async Task<object> GetAllAttendanceAsync(int userId)
        {
            try
            {
                var records = await _repository.GetByUserAsync(userId);

                if (records == null || !records.Any())
                {
                    return new { totalWorkingHours = "00:00:00", attendances = new List<object>() };
                }

                // total working hours across all days
                TimeSpan totalDuration = TimeSpan.Zero;
                foreach (var day in records.GroupBy(r => r.AttendanceDate))
                {
                    var firstCheckIn = day.Min(a => a.CheckInTime);
                    var lastCheckOut = day.Max(a => a.CheckOutTime);

                    if (firstCheckIn != null && lastCheckOut != null)
                        totalDuration += lastCheckOut.Value - firstCheckIn.Value;
                }

                string totalWorkingHours = totalDuration.ToString(@"hh\:mm\:ss");

                var response = new
                {
                    totalWorkingHours,
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

                return response;
            }
            catch (Exception)
            {
                return new { totalWorkingHours = "00:00:00", attendances = new List<object>() };
            }
        }

        // AttendanceService.cs
        public async Task<object> GetAllUsersAttendanceAsync()
        {
            try
            {
                var records = await _repository.GetAllAsync();

                if (records == null || !records.Any())
                {
                    return new List<object>();
                }

                // Group by UserId
                var grouped = records
                    .GroupBy(r => r.UserId)
                    .Select(g =>
                    {
                        // Total working hours calculation
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

                return grouped;
            }
            catch (Exception)
            {
                return new List<object>();
            }
        }


    }
}
