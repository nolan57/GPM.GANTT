using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GPM.Gantt.Models;

namespace GPM.Gantt.Utilities
{
    /// <summary>
    /// Provides advanced timeline helper functions and utilities for Gantt chart operations.
    /// </summary>
    public static class TimelineHelper
    {
        /// <summary>
        /// Calculates the optimal time unit for a given time range to ensure good visual representation.
        /// </summary>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="maxColumns">Maximum number of columns desired in the timeline.</param>
        /// <returns>The recommended time unit.</returns>
        public static TimeUnit CalculateOptimalTimeUnit(DateTime start, DateTime end, int maxColumns = 50)
        {
            var duration = end - start;
            
            // Calculate approximate columns for each time unit
            var hourColumns = (int)duration.TotalHours;
            var dayColumns = (int)duration.TotalDays;
            var weekColumns = (int)(duration.TotalDays / 7);
            var monthColumns = CalculateMonthsBetween(start, end);
            var yearColumns = end.Year - start.Year + 1;
            
            // Choose the best fit based on maxColumns
            if (hourColumns <= maxColumns && hourColumns > 0)
                return TimeUnit.Hour;
            if (dayColumns <= maxColumns && dayColumns > 0)
                return TimeUnit.Day;
            if (weekColumns <= maxColumns && weekColumns > 0)
                return TimeUnit.Week;
            if (monthColumns <= maxColumns && monthColumns > 0)
                return TimeUnit.Month;
            
            return TimeUnit.Year;
        }
        
        /// <summary>
        /// Finds the index of a DateTime within a list of timeline ticks.
        /// </summary>
        /// <param name="ticks">The list of timeline ticks.</param>
        /// <param name="target">The target DateTime to find.</param>
        /// <param name="alignToUnit">Whether to align the target to the time unit before searching.</param>
        /// <param name="unit">The time unit for alignment (if alignToUnit is true).</param>
        /// <returns>The index of the closest tick, or -1 if not found.</returns>
        public static int FindTickIndex(List<DateTime> ticks, DateTime target, bool alignToUnit = true, TimeUnit unit = TimeUnit.Day)
        {
            if (ticks == null || ticks.Count == 0)
                return -1;
                
            var searchTarget = alignToUnit ? TimelineCalculator.AlignToUnitFloor(target, unit) : target;
            
            // Find exact match first
            var exactIndex = ticks.FindIndex(tick => tick == searchTarget);
            if (exactIndex >= 0)
                return exactIndex;
            
            // Find the closest tick that is less than or equal to the target
            for (int i = ticks.Count - 1; i >= 0; i--)
            {
                if (ticks[i] <= searchTarget)
                    return i;
            }
            
            // If target is before all ticks, return 0
            return 0;
        }
        
        /// <summary>
        /// Calculates the span (number of columns) that a task should occupy in the timeline.
        /// Optimized version with caching for repeated calculations.
        /// </summary>
        /// <param name="ticks">The list of timeline ticks.</param>
        /// <param name="taskStart">The task start time.</param>
        /// <param name="taskEnd">The task end time.</param>
        /// <param name="unit">The time unit for alignment.</param>
        /// <returns>A tuple containing (startIndex, columnSpan).</returns>
        public static (int startIndex, int columnSpan) CalculateTaskSpan(List<DateTime> ticks, DateTime taskStart, DateTime taskEnd, TimeUnit unit)
        {
            if (ticks == null || ticks.Count == 0)
                return (0, 1);
            
            // Pre-align times for consistent calculations
            var alignedStart = TimelineCalculator.AlignToUnitFloor(taskStart, unit);
            var alignedEnd = TimelineCalculator.AlignToUnitFloor(taskEnd, unit);
            
            // Use binary search for large tick lists for better performance
            int startIndex;
            int endIndex;
            
            if (ticks.Count > 100)
            {
                startIndex = BinarySearchTickIndex(ticks, alignedStart);
                endIndex = BinarySearchTickIndex(ticks, alignedEnd);
            }
            else
            {
                startIndex = FindTickIndex(ticks, alignedStart, false);
                endIndex = FindTickIndex(ticks, alignedEnd, false);
            }
            
            // Ensure valid indices
            startIndex = Math.Max(0, Math.Min(startIndex, ticks.Count - 1));
            endIndex = Math.Max(startIndex, Math.Min(endIndex, ticks.Count - 1));
            
            // If task ends after the aligned start of the end tick, extend by one column
            if (taskEnd > alignedEnd)
            {
                endIndex = Math.Min(endIndex + 1, ticks.Count - 1);
            }
            
            var columnSpan = Math.Max(1, endIndex - startIndex + 1);
            
            return (startIndex, columnSpan);
        }
        
        /// <summary>
        /// Validates if a time range is within reasonable bounds for the specified time unit.
        /// </summary>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="unit">The time unit.</param>
        /// <returns>A validation result with any warnings or errors.</returns>
        public static (bool isValid, string message) ValidateTimeRange(DateTime start, DateTime end, TimeUnit unit)
        {
            if (start > end)
                return (false, "Start time must be before or equal to end time");
            
            var duration = end - start;
            
            // Check for reasonable limits based on time unit
            return unit switch
            {
                TimeUnit.Hour when duration.TotalDays > 30 => 
                    (false, "Hour-based timeline should not exceed 30 days"),
                TimeUnit.Day when duration.TotalDays > 365 => 
                    (false, "Day-based timeline should not exceed 1 year"),
                TimeUnit.Week when duration.TotalDays > 365 * 2 => 
                    (false, "Week-based timeline should not exceed 2 years"),
                TimeUnit.Month when duration.TotalDays > 365 * 10 => 
                    (false, "Month-based timeline should not exceed 10 years"),
                TimeUnit.Year when duration.TotalDays > 365 * 50 => 
                    (false, "Year-based timeline should not exceed 50 years"),
                _ => (true, "Valid time range")
            };
        }
        
        /// <summary>
        /// Expands a time range to align with time unit boundaries.
        /// </summary>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="unit">The time unit for alignment.</param>
        /// <param name="culture">The culture for culture-specific calculations.</param>
        /// <returns>A tuple containing the aligned (start, end) times.</returns>
        public static (DateTime alignedStart, DateTime alignedEnd) ExpandToUnitBoundaries(
            DateTime start, DateTime end, TimeUnit unit, CultureInfo? culture = null)
        {
            var alignedStart = TimelineCalculator.AlignToUnitFloor(start, unit, culture);
            var alignedEnd = TimelineCalculator.AlignToUnitCeiling(end, unit, culture);
            
            return (alignedStart, alignedEnd);
        }
        
        /// <summary>
        /// Calculates working days between two dates, excluding weekends.
        /// </summary>
        /// <param name="start">The start date.</param>
        /// <param name="end">The end date.</param>
        /// <param name="includeHolidays">Optional list of holiday dates to exclude.</param>
        /// <returns>The number of working days.</returns>
        public static int CalculateWorkingDays(DateTime start, DateTime end, IEnumerable<DateTime>? includeHolidays = null)
        {
            var holidays = includeHolidays?.ToHashSet() ?? new HashSet<DateTime>();
            var workingDays = 0;
            
            for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                if (!TimelineCalculator.IsWeekend(date) && !holidays.Contains(date))
                {
                    workingDays++;
                }
            }
            
            return workingDays;
        }
        
        /// <summary>
        /// Gets timeline metadata for the specified time range and unit.
        /// </summary>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="unit">The time unit.</param>
        /// <param name="culture">The culture for calculations.</param>
        /// <returns>Timeline metadata including tick count, duration, and formatting info.</returns>
        public static TimelineMetadata GetTimelineMetadata(DateTime start, DateTime end, TimeUnit unit, CultureInfo? culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            
            var ticks = TimelineCalculator.GenerateTicks(start, end, unit, culture);
            var (alignedStart, alignedEnd) = ExpandToUnitBoundaries(start, end, unit, culture);
            var unitDuration = TimelineCalculator.GetUnitDuration(unit, start);
            
            return new TimelineMetadata
            {
                OriginalStart = start,
                OriginalEnd = end,
                AlignedStart = alignedStart,
                AlignedEnd = alignedEnd,
                TimeUnit = unit,
                TickCount = ticks.Count,
                TotalDuration = end - start,
                AlignedDuration = alignedEnd - alignedStart,
                UnitDuration = unitDuration,
                Culture = culture
            };
        }
        
        private static int CalculateMonthsBetween(DateTime start, DateTime end)
        {
            return (end.Year - start.Year) * 12 + end.Month - start.Month + 1;
        }
        
        /// <summary>
        /// Binary search for tick index in large collections for improved performance.
        /// </summary>
        /// <param name="ticks">The sorted list of timeline ticks.</param>
        /// <param name="target">The target DateTime to find.</param>
        /// <returns>The index of the closest tick that is less than or equal to the target.</returns>
        private static int BinarySearchTickIndex(List<DateTime> ticks, DateTime target)
        {
            if (ticks.Count == 0) return 0;
            
            int left = 0;
            int right = ticks.Count - 1;
            int result = 0;
            
            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                
                if (ticks[mid] <= target)
                {
                    result = mid;
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// Contains metadata about a timeline configuration.
    /// </summary>
    public class TimelineMetadata
    {
        /// <summary>
        /// Gets or sets the original start time.
        /// </summary>
        public DateTime OriginalStart { get; set; }
        
        /// <summary>
        /// Gets or sets the original end time.
        /// </summary>
        public DateTime OriginalEnd { get; set; }
        
        /// <summary>
        /// Gets or sets the aligned start time.
        /// </summary>
        public DateTime AlignedStart { get; set; }
        
        /// <summary>
        /// Gets or sets the aligned end time.
        /// </summary>
        public DateTime AlignedEnd { get; set; }
        
        /// <summary>
        /// Gets or sets the time unit.
        /// </summary>
        public TimeUnit TimeUnit { get; set; }
        
        /// <summary>
        /// Gets or sets the number of ticks in the timeline.
        /// </summary>
        public int TickCount { get; set; }
        
        /// <summary>
        /// Gets or sets the total duration of the original time range.
        /// </summary>
        public TimeSpan TotalDuration { get; set; }
        
        /// <summary>
        /// Gets or sets the duration of the aligned time range.
        /// </summary>
        public TimeSpan AlignedDuration { get; set; }
        
        /// <summary>
        /// Gets or sets the duration of one time unit.
        /// </summary>
        public TimeSpan UnitDuration { get; set; }
        
        /// <summary>
        /// Gets or sets the culture used for calculations.
        /// </summary>
        public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;
    }
}