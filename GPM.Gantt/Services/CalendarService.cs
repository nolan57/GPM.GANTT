using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GPM.Gantt.Models.Calendar;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Service for managing working calendars and performing business day calculations
    /// </summary>
    public class CalendarService : ICalendarService
    {
        private readonly List<WorkingCalendar> _calendars = new();
        private string _defaultCalendarId = string.Empty;

        public CalendarService()
        {
            InitializeDefaultCalendars();
        }

        public async Task<List<WorkingCalendar>> GetCalendarsAsync()
        {
            await Task.Delay(1); // Simulate async operation
            return _calendars.Where(c => c.IsActive).ToList();
        }

        public async Task<WorkingCalendar?> GetCalendarAsync(string calendarId)
        {
            await Task.Delay(1);
            return _calendars.FirstOrDefault(c => c.Id == calendarId && c.IsActive);
        }

        public async Task<WorkingCalendar> GetDefaultCalendarAsync()
        {
            await Task.Delay(1);
            var defaultCalendar = _calendars.FirstOrDefault(c => c.Id == _defaultCalendarId && c.IsActive);
            return defaultCalendar ?? _calendars.First(c => c.IsActive);
        }

        public async Task<WorkingCalendar> CreateCalendarAsync(WorkingCalendar calendar)
        {
            await Task.Delay(1);
            if (string.IsNullOrEmpty(calendar.Id))
                calendar.Id = Guid.NewGuid().ToString();
            
            _calendars.Add(calendar);
            
            // Set as default if it's the first calendar or explicitly marked as default
            if (!_calendars.Any(c => c.IsDefault) || calendar.IsDefault)
            {
                await SetDefaultCalendarAsync(calendar.Id);
            }
            
            return calendar;
        }

        public async Task<bool> UpdateCalendarAsync(WorkingCalendar calendar)
        {
            await Task.Delay(1);
            var existing = _calendars.FirstOrDefault(c => c.Id == calendar.Id);
            if (existing != null)
            {
                var index = _calendars.IndexOf(existing);
                _calendars[index] = calendar;
                
                if (calendar.IsDefault)
                {
                    await SetDefaultCalendarAsync(calendar.Id);
                }
                
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteCalendarAsync(string calendarId)
        {
            await Task.Delay(1);
            var calendar = _calendars.FirstOrDefault(c => c.Id == calendarId);
            if (calendar != null)
            {
                // Don't allow deletion of the last calendar or default calendar
                if (_calendars.Count(c => c.IsActive) <= 1)
                    return false;
                
                calendar.IsActive = false;
                
                // If this was the default calendar, set another as default
                if (_defaultCalendarId == calendarId)
                {
                    var newDefault = _calendars.First(c => c.IsActive && c.Id != calendarId);
                    await SetDefaultCalendarAsync(newDefault.Id);
                }
                
                return true;
            }
            return false;
        }

        public async Task<bool> SetDefaultCalendarAsync(string calendarId)
        {
            await Task.Delay(1);
            var calendar = await GetCalendarAsync(calendarId);
            if (calendar != null)
            {
                // Remove default flag from all calendars
                foreach (var cal in _calendars)
                {
                    cal.IsDefault = false;
                }
                
                // Set new default
                calendar.IsDefault = true;
                _defaultCalendarId = calendarId;
                return true;
            }
            return false;
        }

        public async Task<bool> IsWorkingDayAsync(DateTime date, string? calendarId = null)
        {
            var calendar = calendarId != null 
                ? await GetCalendarAsync(calendarId) 
                : await GetDefaultCalendarAsync();
            
            return calendar?.IsWorkingDay(date) ?? false;
        }

        public async Task<List<WorkingTime>> GetWorkingHoursAsync(DateTime date, string? calendarId = null)
        {
            var calendar = calendarId != null 
                ? await GetCalendarAsync(calendarId) 
                : await GetDefaultCalendarAsync();
            
            return calendar?.GetWorkingHours(date) ?? new List<WorkingTime>();
        }

        public async Task<TimeSpan> CalculateWorkingHoursAsync(DateTime start, DateTime end, string? calendarId = null)
        {
            var calendar = calendarId != null 
                ? await GetCalendarAsync(calendarId) 
                : await GetDefaultCalendarAsync();
            
            return calendar?.CalculateWorkingHours(start, end) ?? TimeSpan.Zero;
        }

        public async Task<DateTime> AddWorkingTimeAsync(DateTime start, TimeSpan workingTime, string? calendarId = null)
        {
            var calendar = calendarId != null 
                ? await GetCalendarAsync(calendarId) 
                : await GetDefaultCalendarAsync();
            
            return calendar?.AddWorkingTime(start, workingTime) ?? start.Add(workingTime);
        }

        public async Task<DateTime> SubtractWorkingTimeAsync(DateTime end, TimeSpan workingTime, string? calendarId = null)
        {
            var calendar = calendarId != null 
                ? await GetCalendarAsync(calendarId) 
                : await GetDefaultCalendarAsync();
            
            if (calendar == null)
                return end.Subtract(workingTime);
            
            // Subtract working time by going backwards
            var current = end;
            var remainingTime = workingTime;
            
            while (remainingTime > TimeSpan.Zero)
            {
                if (calendar.IsWorkingDay(current.Date))
                {
                    var workingHours = calendar.GetWorkingHours(current.Date);
                    var currentTime = current.TimeOfDay;
                    
                    // Process working hours in reverse order
                    foreach (var workingHour in workingHours.Where(wh => wh.IsWorking).OrderByDescending(wh => wh.StartTime))
                    {
                        if (remainingTime <= TimeSpan.Zero) break;
                        
                        var workEnd = workingHour.EndTime < currentTime ? workingHour.EndTime : currentTime;
                        var workStart = workingHour.StartTime;
                        
                        if (workStart < workEnd)
                        {
                            var availableTime = workEnd - workStart;
                            if (remainingTime <= availableTime)
                            {
                                return current.Date.Add(workEnd - remainingTime);
                            }
                            else
                            {
                                remainingTime = remainingTime.Subtract(availableTime);
                                currentTime = new TimeSpan(23, 59, 59); // Start from end of previous day
                            }
                        }
                    }
                }
                
                current = current.Date.AddDays(-1).Add(new TimeSpan(23, 59, 59));
            }
            
            return current;
        }

        public async Task<DateTime> GetNextWorkingDayAsync(DateTime fromDate, string? calendarId = null)
        {
            var calendar = calendarId != null 
                ? await GetCalendarAsync(calendarId) 
                : await GetDefaultCalendarAsync();
            
            if (calendar == null)
                return fromDate.AddDays(1);
            
            var current = fromDate.Date.AddDays(1);
            while (!calendar.IsWorkingDay(current))
            {
                current = current.AddDays(1);
                
                // Prevent infinite loop
                if (current > fromDate.AddYears(1))
                    break;
            }
            
            return current;
        }

        public async Task<DateTime> GetPreviousWorkingDayAsync(DateTime fromDate, string? calendarId = null)
        {
            var calendar = calendarId != null 
                ? await GetCalendarAsync(calendarId) 
                : await GetDefaultCalendarAsync();
            
            if (calendar == null)
                return fromDate.AddDays(-1);
            
            var current = fromDate.Date.AddDays(-1);
            while (!calendar.IsWorkingDay(current))
            {
                current = current.AddDays(-1);
                
                // Prevent infinite loop
                if (current < fromDate.AddYears(-1))
                    break;
            }
            
            return current;
        }

        public async Task<List<DateTime>> GetWorkingDaysInRangeAsync(DateTime start, DateTime end, string? calendarId = null)
        {
            var calendar = calendarId != null 
                ? await GetCalendarAsync(calendarId) 
                : await GetDefaultCalendarAsync();
            
            var workingDays = new List<DateTime>();
            if (calendar == null)
                return workingDays;
            
            var current = start.Date;
            while (current <= end.Date)
            {
                if (calendar.IsWorkingDay(current))
                {
                    workingDays.Add(current);
                }
                current = current.AddDays(1);
            }
            
            return workingDays;
        }

        public async Task<int> CountWorkingDaysAsync(DateTime start, DateTime end, string? calendarId = null)
        {
            var workingDays = await GetWorkingDaysInRangeAsync(start, end, calendarId);
            return workingDays.Count;
        }

        public async Task<List<WorkingCalendar>> CreateStandardCalendarTemplatesAsync()
        {
            await Task.Delay(1);
            
            var templates = new List<WorkingCalendar>();
            
            // Standard Business Calendar
            var standardCalendar = new WorkingCalendar
            {
                Name = "Standard Business Calendar",
                Type = CalendarType.Standard,
                Description = "Standard Monday-Friday, 9 AM to 5 PM business calendar"
            };
            templates.Add(standardCalendar);
            
            // 24-Hour Operations Calendar
            var twentyFourHourCalendar = CreateTwentyFourHourCalendar();
            templates.Add(twentyFourHourCalendar);
            
            // Night Shift Calendar
            var nightShiftCalendar = CreateNightShiftCalendar();
            templates.Add(nightShiftCalendar);
            
            return templates;
        }

        public async Task<bool> AddCalendarExceptionAsync(string calendarId, CalendarException exception)
        {
            await Task.Delay(1);
            var calendar = await GetCalendarAsync(calendarId);
            if (calendar != null)
            {
                if (string.IsNullOrEmpty(exception.Id))
                    exception.Id = Guid.NewGuid().ToString();
                
                calendar.Exceptions.Add(exception);
                return true;
            }
            return false;
        }

        public async Task<bool> RemoveCalendarExceptionAsync(string calendarId, string exceptionId)
        {
            await Task.Delay(1);
            var calendar = await GetCalendarAsync(calendarId);
            if (calendar != null)
            {
                var exception = calendar.Exceptions.FirstOrDefault(e => e.Id == exceptionId);
                if (exception != null)
                {
                    calendar.Exceptions.Remove(exception);
                    return true;
                }
            }
            return false;
        }

        public async Task<List<CalendarException>> GetCalendarExceptionsAsync(string calendarId)
        {
            await Task.Delay(1);
            var calendar = await GetCalendarAsync(calendarId);
            return calendar?.Exceptions.Where(e => e.IsActive).ToList() ?? new List<CalendarException>();
        }

        private void InitializeDefaultCalendars()
        {
            var standardCalendar = new WorkingCalendar
            {
                Name = "Standard Business Calendar",
                Type = CalendarType.Standard,
                Description = "Standard Monday-Friday, 9 AM to 5 PM business calendar",
                IsDefault = true
            };
            
            // Add common holidays as exceptions
            AddCommonHolidays(standardCalendar);
            
            _calendars.Add(standardCalendar);
            _defaultCalendarId = standardCalendar.Id;
        }

        private WorkingCalendar CreateTwentyFourHourCalendar()
        {
            var calendar = new WorkingCalendar
            {
                Name = "24-Hour Operations",
                Type = CalendarType.TwentyFourHour,
                Description = "24-hour operations calendar, 7 days a week",
                DefaultWorkingHoursPerDay = 24.0,
                DefaultWorkingDaysPerWeek = 7.0
            };
            
            calendar.WorkingDays.Clear();
            
            var twentyFourHourTime = new WorkingTime
            {
                StartTime = new TimeSpan(0, 0, 0),   // 12:00 AM
                EndTime = new TimeSpan(23, 59, 59),  // 11:59:59 PM
                IsWorking = true,
                Description = "24-hour operations"
            };
            
            for (int i = 0; i <= 6; i++) // All days of the week
            {
                calendar.WorkingDays.Add(new WorkingDay
                {
                    DayOfWeek = (DayOfWeek)i,
                    IsWorkingDay = true,
                    WorkingTimes = new List<WorkingTime> { twentyFourHourTime },
                    Description = "24-hour working day"
                });
            }
            
            return calendar;
        }

        private WorkingCalendar CreateNightShiftCalendar()
        {
            var calendar = new WorkingCalendar
            {
                Name = "Night Shift",
                Type = CalendarType.NightShift,
                Description = "Night shift calendar, 11 PM to 7 AM",
                DefaultWorkingHoursPerDay = 8.0,
                DefaultWorkingDaysPerWeek = 5.0
            };
            
            calendar.WorkingDays.Clear();
            
            var nightShiftTime = new WorkingTime
            {
                StartTime = new TimeSpan(23, 0, 0),  // 11:00 PM
                EndTime = new TimeSpan(7, 0, 0),     // 7:00 AM (next day)
                IsWorking = true,
                Description = "Night shift hours"
            };
            
            for (int i = 1; i <= 5; i++) // Monday to Friday
            {
                calendar.WorkingDays.Add(new WorkingDay
                {
                    DayOfWeek = (DayOfWeek)i,
                    IsWorkingDay = true,
                    WorkingTimes = new List<WorkingTime> { nightShiftTime },
                    Description = "Night shift working day"
                });
            }
            
            // Weekend days
            calendar.WorkingDays.Add(new WorkingDay
            {
                DayOfWeek = DayOfWeek.Saturday,
                IsWorkingDay = false,
                WorkingTimes = new List<WorkingTime>(),
                Description = "Weekend"
            });
            
            calendar.WorkingDays.Add(new WorkingDay
            {
                DayOfWeek = DayOfWeek.Sunday,
                IsWorkingDay = false,
                WorkingTimes = new List<WorkingTime>(),
                Description = "Weekend"
            });
            
            return calendar;
        }

        private void AddCommonHolidays(WorkingCalendar calendar)
        {
            var currentYear = DateTime.Now.Year;
            
            // New Year's Day
            calendar.Exceptions.Add(new CalendarException
            {
                Name = "New Year's Day",
                Date = new DateTime(currentYear, 1, 1),
                IsWorkingDay = false,
                IsRecurring = true,
                RecurrencePattern = new RecurrencePattern
                {
                    Type = RecurrenceType.Yearly,
                    Interval = 1,
                    Month = 1,
                    DayOfMonth = 1
                },
                Description = "New Year's Day holiday"
            });
            
            // Independence Day (US)
            calendar.Exceptions.Add(new CalendarException
            {
                Name = "Independence Day",
                Date = new DateTime(currentYear, 7, 4),
                IsWorkingDay = false,
                IsRecurring = true,
                RecurrencePattern = new RecurrencePattern
                {
                    Type = RecurrenceType.Yearly,
                    Interval = 1,
                    Month = 7,
                    DayOfMonth = 4
                },
                Description = "Independence Day holiday"
            });
            
            // Christmas Day
            calendar.Exceptions.Add(new CalendarException
            {
                Name = "Christmas Day",
                Date = new DateTime(currentYear, 12, 25),
                IsWorkingDay = false,
                IsRecurring = true,
                RecurrencePattern = new RecurrencePattern
                {
                    Type = RecurrenceType.Yearly,
                    Interval = 1,
                    Month = 12,
                    DayOfMonth = 25
                },
                Description = "Christmas Day holiday"
            });
        }
    }
}