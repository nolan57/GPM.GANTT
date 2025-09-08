using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using GPM.Gantt.Models;
using GPM.Gantt.Services;
using GPM.Gantt.Utilities;

namespace GPM.Gantt.Layout
{
    /// <summary>
    /// Manages time scale operations for the Gantt chart.
    /// </summary>
    public class TimeScaleManager
    {
        /// <summary>
        /// Creates time headers with performance optimizations.
        /// </summary>
        public static void BuildTimeHeaders(Grid container, List<DateTime> ticks, int columns, PerformanceLevel performanceLevel, ElementPool elementPool, List<UIElement> visibleElements)
        {
            using var measurement = new PerformanceService().BeginMeasurement("TimeHeadersBuild");
            
            for (int c = 0; c < columns; c++)
            {
                var dt = ticks[c];
                var timeCell = elementPool.GetOrCreateTimeCell();
                
                timeCell.TimeIndex = c;
                timeCell.RowIndex = 0;
                timeCell.TimeText = TimelineCalculator.FormatTick(dt, TimeUnit.Day); // Default to Day, but should be configurable
                timeCell.IsWeekend = TimelineCalculator.IsWeekend(dt);
                timeCell.IsToday = TimelineCalculator.IsToday(dt);
                
                Grid.SetRow(timeCell, 0);
                Grid.SetColumn(timeCell, c);
                container.Children.Add(timeCell);
                visibleElements.Add(timeCell);
            }
        }
        
        /// <summary>
        /// Gets cached timeline ticks.
        /// </summary>
        public static List<DateTime> GetCachedTimelineTicks(DateTime startTime, DateTime endTime, TimeUnit timeUnit, CultureInfo culture, IPerformanceService performanceService)
        {
            var cacheKey = $"{startTime:yyyy-MM-dd HH:mm:ss}|{endTime:yyyy-MM-dd HH:mm:ss}|{timeUnit}|{culture?.Name ?? "current"}";
            
            // In a real implementation, this would use the performance service's cache
            return TimelineCalculator.GenerateTicks(startTime, endTime, timeUnit, culture);
        }
    }
}