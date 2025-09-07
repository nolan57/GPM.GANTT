using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GPM.Gantt.Models.Calendar;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Service interface for managing working calendars and business day calculations
    /// </summary>
    public interface ICalendarService
    {
        /// <summary>
        /// Gets all available calendars
        /// </summary>
        /// <returns>List of working calendars</returns>
        Task<List<WorkingCalendar>> GetCalendarsAsync();
        
        /// <summary>
        /// Gets a specific calendar by ID
        /// </summary>
        /// <param name="calendarId">Calendar identifier</param>
        /// <returns>Working calendar or null if not found</returns>
        Task<WorkingCalendar?> GetCalendarAsync(string calendarId);
        
        /// <summary>
        /// Gets the default calendar
        /// </summary>
        /// <returns>Default working calendar</returns>
        Task<WorkingCalendar> GetDefaultCalendarAsync();
        
        /// <summary>
        /// Creates a new calendar
        /// </summary>
        /// <param name="calendar">Calendar to create</param>
        /// <returns>Created calendar with assigned ID</returns>
        Task<WorkingCalendar> CreateCalendarAsync(WorkingCalendar calendar);
        
        /// <summary>
        /// Updates an existing calendar
        /// </summary>
        /// <param name="calendar">Calendar to update</param>
        /// <returns>True if update was successful</returns>
        Task<bool> UpdateCalendarAsync(WorkingCalendar calendar);
        
        /// <summary>
        /// Deletes a calendar
        /// </summary>
        /// <param name="calendarId">ID of calendar to delete</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteCalendarAsync(string calendarId);
        
        /// <summary>
        /// Sets a calendar as the default
        /// </summary>
        /// <param name="calendarId">ID of calendar to set as default</param>
        /// <returns>True if successful</returns>
        Task<bool> SetDefaultCalendarAsync(string calendarId);
        
        /// <summary>
        /// Checks if a date is a working day according to specified calendar
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <param name="calendarId">Calendar ID (null = use default)</param>
        /// <returns>True if it's a working day</returns>
        Task<bool> IsWorkingDayAsync(DateTime date, string? calendarId = null);
        
        /// <summary>
        /// Gets working hours for a specific date
        /// </summary>
        /// <param name="date">Date to get working hours for</param>
        /// <param name="calendarId">Calendar ID (null = use default)</param>
        /// <returns>List of working time periods</returns>
        Task<List<WorkingTime>> GetWorkingHoursAsync(DateTime date, string? calendarId = null);
        
        /// <summary>
        /// Calculates working hours between two dates
        /// </summary>
        /// <param name="start">Start date and time</param>
        /// <param name="end">End date and time</param>
        /// <param name="calendarId">Calendar ID (null = use default)</param>
        /// <returns>Total working time</returns>
        Task<TimeSpan> CalculateWorkingHoursAsync(DateTime start, DateTime end, string? calendarId = null);
        
        /// <summary>
        /// Adds working time to a date
        /// </summary>
        /// <param name="start">Starting date and time</param>
        /// <param name="workingTime">Working time to add</param>
        /// <param name="calendarId">Calendar ID (null = use default)</param>
        /// <returns>Resulting date and time</returns>
        Task<DateTime> AddWorkingTimeAsync(DateTime start, TimeSpan workingTime, string? calendarId = null);
        
        /// <summary>
        /// Subtracts working time from a date
        /// </summary>
        /// <param name="end">Ending date and time</param>
        /// <param name="workingTime">Working time to subtract</param>
        /// <param name="calendarId">Calendar ID (null = use default)</param>
        /// <returns>Resulting date and time</returns>
        Task<DateTime> SubtractWorkingTimeAsync(DateTime end, TimeSpan workingTime, string? calendarId = null);
        
        /// <summary>
        /// Gets the next working day from the specified date
        /// </summary>
        /// <param name="fromDate">Starting date</param>
        /// <param name="calendarId">Calendar ID (null = use default)</param>
        /// <returns>Next working day</returns>
        Task<DateTime> GetNextWorkingDayAsync(DateTime fromDate, string? calendarId = null);
        
        /// <summary>
        /// Gets the previous working day from the specified date
        /// </summary>
        /// <param name="fromDate">Starting date</param>
        /// <param name="calendarId">Calendar ID (null = use default)</param>
        /// <returns>Previous working day</returns>
        Task<DateTime> GetPreviousWorkingDayAsync(DateTime fromDate, string? calendarId = null);
        
        /// <summary>
        /// Gets all working days in a date range
        /// </summary>
        /// <param name="start">Start date</param>
        /// <param name="end">End date</param>
        /// <param name="calendarId">Calendar ID (null = use default)</param>
        /// <returns>List of working days</returns>
        Task<List<DateTime>> GetWorkingDaysInRangeAsync(DateTime start, DateTime end, string? calendarId = null);
        
        /// <summary>
        /// Counts working days between two dates
        /// </summary>
        /// <param name="start">Start date</param>
        /// <param name="end">End date</param>
        /// <param name="calendarId">Calendar ID (null = use default)</param>
        /// <returns>Number of working days</returns>
        Task<int> CountWorkingDaysAsync(DateTime start, DateTime end, string? calendarId = null);
        
        /// <summary>
        /// Creates standard calendar templates
        /// </summary>
        /// <returns>List of standard calendar templates</returns>
        Task<List<WorkingCalendar>> CreateStandardCalendarTemplatesAsync();
        
        /// <summary>
        /// Adds a calendar exception
        /// </summary>
        /// <param name="calendarId">Calendar ID</param>
        /// <param name="exception">Exception to add</param>
        /// <returns>True if successful</returns>
        Task<bool> AddCalendarExceptionAsync(string calendarId, CalendarException exception);
        
        /// <summary>
        /// Removes a calendar exception
        /// </summary>
        /// <param name="calendarId">Calendar ID</param>
        /// <param name="exceptionId">Exception ID to remove</param>
        /// <returns>True if successful</returns>
        Task<bool> RemoveCalendarExceptionAsync(string calendarId, string exceptionId);
        
        /// <summary>
        /// Gets all exceptions for a calendar
        /// </summary>
        /// <param name="calendarId">Calendar ID</param>
        /// <returns>List of calendar exceptions</returns>
        Task<List<CalendarException>> GetCalendarExceptionsAsync(string calendarId);
    }
}