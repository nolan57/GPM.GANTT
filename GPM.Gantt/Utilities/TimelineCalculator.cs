using System;
using System.Collections.Generic;
using System.Globalization;
using System.Collections.Concurrent;
using GPM.Gantt.Models;

namespace GPM.Gantt.Utilities
{
    /// <summary>
    /// Provides utility methods for timeline calculations and time-based operations.
    /// Includes performance optimizations with caching for frequently accessed calculations.
    /// </summary>
    public static class TimelineCalculator
    {
        // Thread-safe caches for performance optimization
        private static readonly ConcurrentDictionary<string, List<DateTime>> _ticksCache = new();
        private static readonly ConcurrentDictionary<string, string> _formatCache = new();
        private static readonly object _cacheCleanupLock = new();
        private static DateTime _lastCacheCleanup = DateTime.Now;
        private const int MaxCacheSize = 1000;
        private static readonly TimeSpan CacheCleanupInterval = TimeSpan.FromMinutes(5);
        /// <summary>
        /// Generates a sequence of time points (ticks) between start and end dates based on the specified time unit.
        /// Uses caching for improved performance on repeated calls with the same parameters.
        /// </summary>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="unit">The time unit for generating ticks.</param>
        /// <param name="culture">The culture for culture-specific calculations. If null, uses current culture.</param>
        /// <returns>A list of DateTime values representing the timeline ticks.</returns>
        /// <exception cref="ArgumentException">Thrown when the time unit is not supported.</exception>
        public static List<DateTime> GenerateTicks(DateTime start, DateTime end, TimeUnit unit, CultureInfo? culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            
            // Create cache key
            var cacheKey = CreateCacheKey(start, end, unit, culture);
            
            // Check cache first
            if (_ticksCache.TryGetValue(cacheKey, out var cachedTicks))
            {
                return new List<DateTime>(cachedTicks); // Return copy to prevent modification
            }
            
            // Clean up cache if needed
            CleanupCacheIfNeeded();
            
            var result = new List<DateTime>();
            
            // Ensure start is before end
            if (end < start)
            {
                (start, end) = (end, start);
            }

            switch (unit)
            {
                case TimeUnit.Hour:
                    GenerateHourlyTicks(start, end, result);
                    break;
                case TimeUnit.Day:
                    GenerateDailyTicks(start, end, result);
                    break;
                case TimeUnit.Week:
                    GenerateWeeklyTicks(start, end, result, culture);
                    break;
                case TimeUnit.Month:
                    GenerateMonthlyTicks(start, end, result);
                    break;
                case TimeUnit.Year:
                    GenerateYearlyTicks(start, end, result);
                    break;
                default:
                    throw new ArgumentException($"Unsupported time unit: {unit}", nameof(unit));
            }

            // Ensure at least one tick is returned
            if (result.Count == 0)
                result.Add(start);

            // Cache the result
            if (_ticksCache.Count < MaxCacheSize)
            {
                _ticksCache.TryAdd(cacheKey, new List<DateTime>(result));
            }

            return result;
        }
        
        /// <summary>
        /// Aligns a DateTime to the floor boundary of the specified time unit.
        /// </summary>
        /// <param name="dateTime">The DateTime to align.</param>
        /// <param name="unit">The time unit to align to.</param>
        /// <param name="culture">The culture for culture-specific calculations. If null, uses current culture.</param>
        /// <returns>The aligned DateTime.</returns>
        public static DateTime AlignToUnitFloor(DateTime dateTime, TimeUnit unit, CultureInfo? culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            
            return unit switch
            {
                TimeUnit.Hour => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0),
                TimeUnit.Day => dateTime.Date,
                TimeUnit.Week => AlignToWeekStart(dateTime, culture.DateTimeFormat.FirstDayOfWeek),
                TimeUnit.Month => new DateTime(dateTime.Year, dateTime.Month, 1),
                TimeUnit.Year => new DateTime(dateTime.Year, 1, 1),
                _ => dateTime
            };
        }
        
        /// <summary>
        /// Aligns a DateTime to the ceiling boundary of the specified time unit.
        /// </summary>
        /// <param name="dateTime">The DateTime to align.</param>
        /// <param name="unit">The time unit to align to.</param>
        /// <param name="culture">The culture for culture-specific calculations. If null, uses current culture.</param>
        /// <returns>The aligned DateTime.</returns>
        public static DateTime AlignToUnitCeiling(DateTime dateTime, TimeUnit unit, CultureInfo? culture = null)
        {
            var floor = AlignToUnitFloor(dateTime, unit, culture);
            
            // If already aligned, return as-is
            if (floor == dateTime)
                return dateTime;
                
            // Move to next unit boundary
            return unit switch
            {
                TimeUnit.Hour => floor.AddHours(1),
                TimeUnit.Day => floor.AddDays(1),
                TimeUnit.Week => floor.AddDays(7),
                TimeUnit.Month => floor.AddMonths(1),
                TimeUnit.Year => floor.AddYears(1),
                _ => dateTime
            };
        }
        
        /// <summary>
        /// Formats a DateTime for display based on the time unit.
        /// </summary>
        /// <param name="dateTime">The DateTime to format.</param>
        /// <param name="unit">The time unit context for formatting.</param>
        /// <param name="culture">The culture for formatting. If null, uses current culture.</param>
        /// <returns>A formatted string representation of the DateTime.</returns>
        public static string FormatTick(DateTime dateTime, TimeUnit unit, CultureInfo? culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            
            return unit switch
            {
                TimeUnit.Hour => dateTime.ToString("MMM dd HH:mm", culture),
                TimeUnit.Day => dateTime.ToString("MMM dd", culture),
                TimeUnit.Week => FormatWeekTick(dateTime, culture),
                TimeUnit.Month => dateTime.ToString("yyyy MMM", culture),
                TimeUnit.Year => dateTime.ToString("yyyy", culture),
                _ => dateTime.ToString(culture)
            };
        }
        
        /// <summary>
        /// Formats a DateTime for display using custom format strings based on the time unit.
        /// </summary>
        /// <param name="dateTime">The DateTime to format.</param>
        /// <param name="unit">The time unit context for formatting.</param>
        /// <param name="dateFormat">Custom date format string to use for date-based units.</param>
        /// <param name="timeFormat">Custom time format string to use for time-based units.</param>
        /// <param name="culture">The culture for formatting. If null, uses current culture.</param>
        /// <returns>A formatted string representation of the DateTime.</returns>
        public static string FormatTick(DateTime dateTime, TimeUnit unit, string? dateFormat, string? timeFormat, CultureInfo? culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            
            return unit switch
            {
                TimeUnit.Hour => string.IsNullOrEmpty(timeFormat) ? 
                    dateTime.ToString("MMM dd HH:mm", culture) : 
                    dateTime.ToString($"{dateFormat ?? "MMM dd"} {timeFormat}", culture),
                TimeUnit.Day => string.IsNullOrEmpty(dateFormat) ? 
                    dateTime.ToString("MMM dd", culture) : 
                    dateTime.ToString(dateFormat, culture),
                TimeUnit.Week => FormatWeekTick(dateTime, culture),
                TimeUnit.Month => string.IsNullOrEmpty(dateFormat) ? 
                    dateTime.ToString("yyyy MMM", culture) : 
                    dateTime.ToString(dateFormat, culture),
                TimeUnit.Year => string.IsNullOrEmpty(dateFormat) ? 
                    dateTime.ToString("yyyy", culture) : 
                    dateTime.ToString(dateFormat, culture),
                _ => dateTime.ToString(culture)
            };
        }
        
        /// <summary>
        /// Calculates the time span covered by one unit of the specified time unit type.
        /// </summary>
        /// <param name="unit">The time unit.</param>
        /// <param name="referenceDate">A reference date for month/year calculations. If null, uses current date.</param>
        /// <returns>The TimeSpan representing one unit duration.</returns>
        public static TimeSpan GetUnitDuration(TimeUnit unit, DateTime? referenceDate = null)
        {
            var reference = referenceDate ?? DateTime.Now;
            
            return unit switch
            {
                TimeUnit.Hour => TimeSpan.FromHours(1),
                TimeUnit.Day => TimeSpan.FromDays(1),
                TimeUnit.Week => TimeSpan.FromDays(7),
                TimeUnit.Month => reference.AddMonths(1) - reference,
                TimeUnit.Year => reference.AddYears(1) - reference,
                _ => TimeSpan.Zero
            };
        }
        
        /// <summary>
        /// Determines if a DateTime falls on a weekend (Saturday or Sunday).
        /// </summary>
        /// <param name="dateTime">The DateTime to check.</param>
        /// <returns>True if the date is a weekend, false otherwise.</returns>
        public static bool IsWeekend(DateTime dateTime)
        {
            return dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday;
        }
        
        /// <summary>
        /// Determines if a DateTime is today.
        /// </summary>
        /// <param name="dateTime">The DateTime to check.</param>
        /// <returns>True if the date is today, false otherwise.</returns>
        public static bool IsToday(DateTime dateTime)
        {
            return dateTime.Date == DateTime.Today;
        }
        
        #region Private Helper Methods
        
        private static void GenerateHourlyTicks(DateTime start, DateTime end, List<DateTime> result)
        {
            var current = start;
            while (current <= end)
            {
                result.Add(current);
                current = current.AddHours(1);
            }
        }
        
        private static void GenerateDailyTicks(DateTime start, DateTime end, List<DateTime> result)
        {
            var current = start.Date;
            var endDate = end.Date;
            while (current <= endDate)
            {
                result.Add(current);
                current = current.AddDays(1);
            }
        }
        
        private static void GenerateWeeklyTicks(DateTime start, DateTime end, List<DateTime> result, CultureInfo culture)
        {
            var firstDay = culture.DateTimeFormat.FirstDayOfWeek;
            var current = AlignToWeekStart(start, firstDay);
            var endAligned = AlignToWeekStart(end, firstDay);
            
            while (current <= endAligned)
            {
                result.Add(current);
                current = current.AddDays(7);
            }
        }
        
        private static void GenerateMonthlyTicks(DateTime start, DateTime end, List<DateTime> result)
        {
            var current = new DateTime(start.Year, start.Month, 1);
            var endMonth = new DateTime(end.Year, end.Month, 1);
            
            while (current <= endMonth)
            {
                result.Add(current);
                current = current.AddMonths(1);
            }
        }
        
        private static void GenerateYearlyTicks(DateTime start, DateTime end, List<DateTime> result)
        {
            var current = new DateTime(start.Year, 1, 1);
            var endYear = new DateTime(end.Year, 1, 1);
            
            while (current <= endYear)
            {
                result.Add(current);
                current = current.AddYears(1);
            }
        }
        
        private static DateTime AlignToWeekStart(DateTime dateTime, DayOfWeek firstDay)
        {
            int diff = (7 + (dateTime.DayOfWeek - firstDay)) % 7;
            return dateTime.Date.AddDays(-diff);
        }
        
        private static string FormatWeekTick(DateTime dateTime, CultureInfo culture)
        {
            int week = culture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, culture.DateTimeFormat.FirstDayOfWeek);
            return $"Week {week}, {dateTime:yyyy}";
        }
        
        #endregion
        
        #region Cache Management
        
        /// <summary>
        /// Creates a cache key for timeline calculations.
        /// </summary>
        private static string CreateCacheKey(DateTime start, DateTime end, TimeUnit unit, CultureInfo culture)
        {
            return $"{start:yyyy-MM-dd HH:mm:ss}|{end:yyyy-MM-dd HH:mm:ss}|{unit}|{culture.Name}";
        }
        
        /// <summary>
        /// Cleans up cache if it's getting too large or if enough time has passed.
        /// </summary>
        private static void CleanupCacheIfNeeded()
        {
            var now = DateTime.Now;
            
            if (_ticksCache.Count > MaxCacheSize || (now - _lastCacheCleanup) > CacheCleanupInterval)
            {
                lock (_cacheCleanupLock)
                {
                    // Double-check pattern
                    if (_ticksCache.Count > MaxCacheSize || (now - _lastCacheCleanup) > CacheCleanupInterval)
                    {
                        // Remove oldest entries (approximate LRU by clearing half)
                        if (_ticksCache.Count > MaxCacheSize / 2)
                        {
                            var keysToRemove = _ticksCache.Keys.Take(_ticksCache.Count / 2).ToList();
                            foreach (var key in keysToRemove)
                            {
                                _ticksCache.TryRemove(key, out _);
                            }
                        }
                        
                        // Clean format cache as well
                        if (_formatCache.Count > MaxCacheSize / 2)
                        {
                            var keysToRemove = _formatCache.Keys.Take(_formatCache.Count / 2).ToList();
                            foreach (var key in keysToRemove)
                            {
                                _formatCache.TryRemove(key, out _);
                            }
                        }
                        
                        _lastCacheCleanup = now;
                    }
                }
            }
        }
        
        /// <summary>
        /// Clears all cached data. Useful for testing or memory management.
        /// </summary>
        public static void ClearCache()
        {
            _ticksCache.Clear();
            _formatCache.Clear();
        }
        
        #endregion
    }
}