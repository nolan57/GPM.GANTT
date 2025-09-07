using System;
using System.Collections.Generic;
using System.Globalization;

namespace GPM.Gantt.Models.Calendar
{
    /// <summary>
    /// Types of working calendars available
    /// </summary>
    public enum CalendarType
    {
        /// <summary>
        /// Standard business calendar (8 hours/day, Mon-Fri)
        /// </summary>
        Standard,
        
        /// <summary>
        /// Night shift calendar
        /// </summary>
        NightShift,
        
        /// <summary>
        /// 24-hour operations calendar
        /// </summary>
        TwentyFourHour,
        
        /// <summary>
        /// Custom calendar defined by user
        /// </summary>
        Custom
    }

    /// <summary>
    /// Represents a working time period within a day
    /// </summary>
    public class WorkingTime
    {
        /// <summary>
        /// Start time of the working period
        /// </summary>
        public TimeSpan StartTime { get; set; }
        
        /// <summary>
        /// End time of the working period
        /// </summary>
        public TimeSpan EndTime { get; set; }
        
        /// <summary>
        /// Whether this is a working period (true) or break period (false)
        /// </summary>
        public bool IsWorking { get; set; } = true;
        
        /// <summary>
        /// Description of the working time period
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets the duration of this working time period
        /// </summary>
        public TimeSpan Duration => EndTime - StartTime;
        
        /// <summary>
        /// Validates the working time period
        /// </summary>
        public bool IsValid() => StartTime < EndTime;
    }

    /// <summary>
    /// Represents working times for a specific day of the week
    /// </summary>
    public class WorkingDay
    {
        /// <summary>
        /// Day of the week this configuration applies to
        /// </summary>
        public DayOfWeek DayOfWeek { get; set; }
        
        /// <summary>
        /// List of working time periods for this day
        /// </summary>
        public List<WorkingTime> WorkingTimes { get; set; } = new();
        
        /// <summary>
        /// Whether this is a working day
        /// </summary>
        public bool IsWorkingDay { get; set; } = true;
        
        /// <summary>
        /// Description of the working day
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets the total working hours for this day
        /// </summary>
        public TimeSpan TotalWorkingHours => 
            TimeSpan.FromTicks(WorkingTimes.Where(wt => wt.IsWorking).Sum(wt => wt.Duration.Ticks));
    }

    /// <summary>
    /// Recurrence pattern for calendar exceptions
    /// </summary>
    public enum RecurrenceType
    {
        None,
        Daily,
        Weekly,
        Monthly,
        Yearly
    }

    /// <summary>
    /// Defines how a recurring exception repeats
    /// </summary>
    public class RecurrencePattern
    {
        /// <summary>
        /// Type of recurrence
        /// </summary>
        public RecurrenceType Type { get; set; } = RecurrenceType.None;
        
        /// <summary>
        /// Interval between recurrences (e.g., every 2 weeks)
        /// </summary>
        public int Interval { get; set; } = 1;
        
        /// <summary>
        /// Day of month for monthly/yearly recurrence
        /// </summary>
        public int DayOfMonth { get; set; }
        
        /// <summary>
        /// Day of week for weekly recurrence
        /// </summary>
        public DayOfWeek DayOfWeek { get; set; }
        
        /// <summary>
        /// Week of month for monthly recurrence
        /// </summary>
        public int WeekOfMonth { get; set; }
        
        /// <summary>
        /// Month for yearly recurrence
        /// </summary>
        public int Month { get; set; }
        
        /// <summary>
        /// End date for the recurrence (null = no end)
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Maximum number of occurrences (null = no limit)
        /// </summary>
        public int? MaxOccurrences { get; set; }
    }

    /// <summary>
    /// Represents an exception to the standard working calendar
    /// </summary>
    public class CalendarException
    {
        /// <summary>
        /// Unique identifier for the exception
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Start date of the exception
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// End date of the exception (for multi-day exceptions)
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Working times for this exception (overrides default working times)
        /// </summary>
        public List<WorkingTime> WorkingTimes { get; set; } = new();
        
        /// <summary>
        /// Whether this is a working day during the exception
        /// </summary>
        public bool IsWorkingDay { get; set; }
        
        /// <summary>
        /// Name of the exception (e.g., "Christmas Day")
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the exception
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether this exception recurs
        /// </summary>
        public bool IsRecurring { get; set; }
        
        /// <summary>
        /// Recurrence pattern for this exception
        /// </summary>
        public RecurrencePattern RecurrencePattern { get; set; } = new();
        
        /// <summary>
        /// Priority of this exception (higher priority overrides lower)
        /// </summary>
        public int Priority { get; set; } = 0;
        
        /// <summary>
        /// Whether this exception is active
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Gets the effective end date (EndDate or Date if single day)
        /// </summary>
        public DateTime EffectiveEndDate => EndDate ?? Date;
        
        /// <summary>
        /// Checks if a specific date falls within this exception
        /// </summary>
        public bool AppliesToDate(DateTime date)
        {
            if (!IsActive) return false;
            
            if (!IsRecurring)
            {
                return date.Date >= Date.Date && date.Date <= EffectiveEndDate.Date;
            }
            
            return RecurrencePattern.Type != RecurrenceType.None && 
                   CheckRecurrenceMatch(date);
        }
        
        private bool CheckRecurrenceMatch(DateTime date)
        {
            switch (RecurrencePattern.Type)
            {
                case RecurrenceType.Daily:
                    return (date.Date - Date.Date).Days % RecurrencePattern.Interval == 0;
                    
                case RecurrenceType.Weekly:
                    return date.DayOfWeek == RecurrencePattern.DayOfWeek &&
                           (date.Date - Date.Date).Days % (7 * RecurrencePattern.Interval) == 0;
                           
                case RecurrenceType.Monthly:
                    return date.Day == RecurrencePattern.DayOfMonth &&
                           (date.Year - Date.Year) * 12 + date.Month - Date.Month % RecurrencePattern.Interval == 0;
                           
                case RecurrenceType.Yearly:
                    return date.Month == RecurrencePattern.Month &&
                           date.Day == RecurrencePattern.DayOfMonth &&
                           (date.Year - Date.Year) % RecurrencePattern.Interval == 0;
                           
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Represents a complete working calendar with days, times, and exceptions
    /// </summary>
    public class WorkingCalendar
    {
        /// <summary>
        /// Unique identifier for the calendar
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Name of the calendar
        /// </summary>
        public string Name { get; set; } = "Standard Calendar";
        
        /// <summary>
        /// Type of calendar
        /// </summary>
        public CalendarType Type { get; set; } = CalendarType.Standard;
        
        /// <summary>
        /// Working days configuration
        /// </summary>
        public List<WorkingDay> WorkingDays { get; set; } = new();
        
        /// <summary>
        /// Calendar exceptions (holidays, special working days, etc.)
        /// </summary>
        public List<CalendarException> Exceptions { get; set; } = new();
        
        /// <summary>
        /// Culture for date formatting and calculations
        /// </summary>
        public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;
        
        /// <summary>
        /// Time zone for this calendar
        /// </summary>
        public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Local;
        
        /// <summary>
        /// Description of the calendar
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether this is the default calendar
        /// </summary>
        public bool IsDefault { get; set; }
        
        /// <summary>
        /// Whether this calendar is active
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Default working hours per day
        /// </summary>
        public double DefaultWorkingHoursPerDay { get; set; } = 8.0;
        
        /// <summary>
        /// Default working days per week
        /// </summary>
        public double DefaultWorkingDaysPerWeek { get; set; } = 5.0;

        public WorkingCalendar()
        {
            InitializeStandardWorkingDays();
        }

        /// <summary>
        /// Initializes standard working days (Monday-Friday, 9-5)
        /// </summary>
        private void InitializeStandardWorkingDays()
        {
            var standardWorkingTime = new WorkingTime
            {
                StartTime = new TimeSpan(9, 0, 0),  // 9:00 AM
                EndTime = new TimeSpan(17, 0, 0),   // 5:00 PM
                IsWorking = true,
                Description = "Standard working hours"
            };

            for (int i = 1; i <= 5; i++) // Monday to Friday
            {
                WorkingDays.Add(new WorkingDay
                {
                    DayOfWeek = (DayOfWeek)i,
                    IsWorkingDay = true,
                    WorkingTimes = new List<WorkingTime> { standardWorkingTime },
                    Description = $"Standard working day"
                });
            }

            // Weekend days
            WorkingDays.Add(new WorkingDay
            {
                DayOfWeek = DayOfWeek.Saturday,
                IsWorkingDay = false,
                WorkingTimes = new List<WorkingTime>(),
                Description = "Weekend"
            });

            WorkingDays.Add(new WorkingDay
            {
                DayOfWeek = DayOfWeek.Sunday,
                IsWorkingDay = false,
                WorkingTimes = new List<WorkingTime>(),
                Description = "Weekend"
            });
        }

        /// <summary>
        /// Checks if a specific date is a working day
        /// </summary>
        public bool IsWorkingDay(DateTime date)
        {
            // Check for exceptions first (they override default settings)
            var applicableExceptions = Exceptions
                .Where(ex => ex.AppliesToDate(date))
                .OrderByDescending(ex => ex.Priority)
                .ToList();

            if (applicableExceptions.Any())
            {
                return applicableExceptions.First().IsWorkingDay;
            }

            // Check standard working day configuration
            var workingDay = WorkingDays.FirstOrDefault(wd => wd.DayOfWeek == date.DayOfWeek);
            return workingDay?.IsWorkingDay ?? false;
        }

        /// <summary>
        /// Gets working hours for a specific date
        /// </summary>
        public List<WorkingTime> GetWorkingHours(DateTime date)
        {
            // Check for exceptions first
            var applicableExceptions = Exceptions
                .Where(ex => ex.AppliesToDate(date))
                .OrderByDescending(ex => ex.Priority)
                .ToList();

            if (applicableExceptions.Any())
            {
                var exception = applicableExceptions.First();
                return exception.IsWorkingDay ? exception.WorkingTimes : new List<WorkingTime>();
            }

            // Use standard working day configuration
            var workingDay = WorkingDays.FirstOrDefault(wd => wd.DayOfWeek == date.DayOfWeek);
            return workingDay?.IsWorkingDay == true ? workingDay.WorkingTimes : new List<WorkingTime>();
        }

        /// <summary>
        /// Calculates working hours between two dates
        /// </summary>
        public TimeSpan CalculateWorkingHours(DateTime start, DateTime end)
        {
            if (start >= end) return TimeSpan.Zero;

            var totalWorkingTime = TimeSpan.Zero;
            var current = start.Date;

            while (current <= end.Date)
            {
                if (IsWorkingDay(current))
                {
                    var workingHours = GetWorkingHours(current);
                    var dayStart = current == start.Date ? start.TimeOfDay : TimeSpan.Zero;
                    var dayEnd = current == end.Date ? end.TimeOfDay : new TimeSpan(23, 59, 59);

                    foreach (var workingHour in workingHours.Where(wh => wh.IsWorking))
                    {
                        var workStart = workingHour.StartTime > dayStart ? workingHour.StartTime : dayStart;
                        var workEnd = workingHour.EndTime < dayEnd ? workingHour.EndTime : dayEnd;

                        if (workStart < workEnd)
                        {
                            totalWorkingTime = totalWorkingTime.Add(workEnd - workStart);
                        }
                    }
                }

                current = current.AddDays(1);
            }

            return totalWorkingTime;
        }

        /// <summary>
        /// Adds working time to a date
        /// </summary>
        public DateTime AddWorkingTime(DateTime start, TimeSpan workingTime)
        {
            var current = start;
            var remainingTime = workingTime;

            while (remainingTime > TimeSpan.Zero)
            {
                if (IsWorkingDay(current.Date))
                {
                    var workingHours = GetWorkingHours(current.Date);
                    var currentTime = current.TimeOfDay;

                    foreach (var workingHour in workingHours.Where(wh => wh.IsWorking).OrderBy(wh => wh.StartTime))
                    {
                        if (remainingTime <= TimeSpan.Zero) break;

                        var workStart = workingHour.StartTime > currentTime ? workingHour.StartTime : currentTime;
                        var workEnd = workingHour.EndTime;

                        if (workStart < workEnd)
                        {
                            var availableTime = workEnd - workStart;
                            if (remainingTime <= availableTime)
                            {
                                return current.Date.Add(workStart + remainingTime);
                            }
                            else
                            {
                                remainingTime = remainingTime.Subtract(availableTime);
                                currentTime = TimeSpan.Zero; // Start from beginning of next day
                            }
                        }
                    }
                }

                current = current.Date.AddDays(1);
            }

            return current;
        }
    }
}