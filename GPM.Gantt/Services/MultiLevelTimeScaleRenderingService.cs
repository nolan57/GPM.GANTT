using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GPM.Gantt.Models;
using GPM.Gantt.Controls;
using GPM.Gantt.Utilities;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Multi-level time scale rendering service interface
    /// </summary>
    public interface IMultiLevelTimeScaleRenderingService
    {
        /// <summary>
        /// Create multi-level time scale control
        /// </summary>
        FrameworkElement CreateMultiLevelTimeScale(MultiLevelTimeScaleConfiguration config, TimeScaleContext context);

        /// <summary>
        /// Update time scale display
        /// </summary>
        void UpdateTimeScale(FrameworkElement timeScale, MultiLevelTimeScaleConfiguration config, TimeScaleContext context);

        /// <summary>
        /// Expand specified time segment
        /// </summary>
        void ExpandTimeSegment(FrameworkElement timeScale, DateTime segmentStart, ExtendedTimeUnit toLevel);

        /// <summary>
        /// Collapse specified time segment
        /// </summary>
        void CollapseTimeSegment(FrameworkElement timeScale, DateTime segmentStart);

        /// <summary>
        /// Get time segment expansion state
        /// </summary>
        bool IsSegmentExpanded(DateTime segmentStart);
    }

    /// <summary>
    /// Multi-level time scale rendering service implementation
    /// </summary>
    public class MultiLevelTimeScaleRenderingService : IMultiLevelTimeScaleRenderingService
    {
        private readonly Dictionary<DateTime, ExtendedTimeUnit> _expandedSegments;
        private readonly Queue<MultiLevelTimeScaleTick> _tickPool;
        private const int MaxPoolSize = 100;

        public MultiLevelTimeScaleRenderingService()
        {
            _expandedSegments = new Dictionary<DateTime, ExtendedTimeUnit>();
            _tickPool = new Queue<MultiLevelTimeScaleTick>();
        }

        public FrameworkElement CreateMultiLevelTimeScale(MultiLevelTimeScaleConfiguration config, TimeScaleContext context)
        {
            var container = new Grid();
            container.Background = Brushes.White;

            // Optimize configuration
            config.OptimizeForContext(context.TotalSpan, context.AvailableWidth);

            var rowIndex = 0;
            var visibleLevels = config.Levels.Where(l => l.IsVisible).OrderBy(l => l.DisplayOrder).ToList();

            foreach (var level in visibleLevels)
            {
                container.RowDefinitions.Add(new RowDefinition { Height = new GridLength(level.Height, GridUnitType.Pixel) });

                var levelCanvas = CreateLevelCanvas(level, context);
                Grid.SetRow(levelCanvas, rowIndex);
                container.Children.Add(levelCanvas);

                rowIndex++;
            }

            return container;
        }

        public void UpdateTimeScale(FrameworkElement timeScale, MultiLevelTimeScaleConfiguration config, TimeScaleContext context)
        {
            if (!(timeScale is Grid container))
                return;

            // Clear existing content
            container.Children.Clear();
            container.RowDefinitions.Clear();

            // Re-create
            var newTimeScale = CreateMultiLevelTimeScale(config, context);
            if (newTimeScale is Grid newContainer)
            {
                foreach (UIElement child in newContainer.Children)
                {
                    container.Children.Add(child);
                }
                foreach (var rowDef in newContainer.RowDefinitions)
                {
                    container.RowDefinitions.Add(rowDef);
                }
            }
        }

        public void ExpandTimeSegment(FrameworkElement timeScale, DateTime segmentStart, ExtendedTimeUnit toLevel)
        {
            _expandedSegments[segmentStart] = toLevel;
            
            // Trigger re-rendering for affected areas
            if (timeScale is Grid container)
            {
                // Find affected time scales and update
                UpdateAffectedSegments(container, segmentStart, toLevel);
            }
        }

        public void CollapseTimeSegment(FrameworkElement timeScale, DateTime segmentStart)
        {
            if (_expandedSegments.ContainsKey(segmentStart))
            {
                _expandedSegments.Remove(segmentStart);
                
                // Trigger re-rendering
                if (timeScale is Grid container)
                {
                    UpdateAffectedSegments(container, segmentStart, ExtendedTimeUnit.Week); // Restore to default level
                }
            }
        }

        public bool IsSegmentExpanded(DateTime segmentStart)
        {
            return _expandedSegments.ContainsKey(segmentStart);
        }

        private Canvas CreateLevelCanvas(TimeLevelConfiguration level, TimeScaleContext context)
        {
            var canvas = new Canvas
            {
                Background = level.Background,
                Height = level.Height
            };

            var ticks = GenerateTimeTicksForLevel(level, context);
            
            foreach (var tick in ticks)
            {
                canvas.Children.Add(tick);
            }

            return canvas;
        }

        private List<MultiLevelTimeScaleTick> GenerateTimeTicksForLevel(TimeLevelConfiguration level, TimeScaleContext context)
        {
            var ticks = new List<MultiLevelTimeScaleTick>();

            var currentDate = GetLevelStartDate(context.StartDate, level.LevelType);
            var endDate = context.EndDate;
            double currentX = 0;

            while (currentDate < endDate)
            {
                var nextDate = GetNextLevelDate(currentDate, level.LevelType);
                var segmentWidth = CalculateSegmentWidth(currentDate, nextDate, context);

                // Check if there's an expanded sub-level
                var expandedLevel = GetExpandedLevel(currentDate);
                if (expandedLevel.HasValue && expandedLevel.Value != level.LevelType)
                {
                    // Create expanded sub-level ticks
                    var subTicks = GenerateExpandedSubTicks(currentDate, nextDate, expandedLevel.Value, level, context, currentX);
                    ticks.AddRange(subTicks);
                }
                else
                {
                    // Create normal tick
                    var tick = GetOrCreateTick();
                    ConfigureTick(tick, currentDate, level, segmentWidth);
                    Canvas.SetLeft(tick, currentX);
                    Canvas.SetTop(tick, 0);
                    ticks.Add(tick);
                }

                currentX += segmentWidth;
                currentDate = nextDate;
            }

            return ticks;
        }

        private List<MultiLevelTimeScaleTick> GenerateExpandedSubTicks(DateTime startDate, DateTime endDate, 
            ExtendedTimeUnit expandedLevel, TimeLevelConfiguration parentLevel, TimeScaleContext context, double startX)
        {
            var subTicks = new List<MultiLevelTimeScaleTick>();
            var subConfig = TimeLevelConfiguration.CreateDefault(expandedLevel);
            subConfig.Height = parentLevel.Height;

            var currentDate = GetLevelStartDate(startDate, expandedLevel);
            double currentX = startX;

            while (currentDate < endDate)
            {
                var nextDate = GetNextLevelDate(currentDate, expandedLevel);
                var segmentWidth = CalculateSegmentWidth(currentDate, nextDate, context);

                var tick = GetOrCreateTick();
                ConfigureTick(tick, currentDate, subConfig, segmentWidth);
                Canvas.SetLeft(tick, currentX);
                Canvas.SetTop(tick, 0);
                subTicks.Add(tick);

                currentX += segmentWidth;
                currentDate = nextDate;
            }

            return subTicks;
        }

        private void ConfigureTick(MultiLevelTimeScaleTick tick, DateTime date, TimeLevelConfiguration level, double width)
        {
            tick.Width = width;
            tick.Height = level.Height;
            tick.Date = date;
            tick.LevelType = level.LevelType;
            tick.DisplayText = FormatDateForLevel(date, level);
            tick.UpdateStyle(level);
            
            // If supports expansion, add expand button
            if (level.IsExpandable)
            {
                tick.ShowExpandButton = true;
                tick.IsExpanded = IsSegmentExpanded(date);
                
                // Subscribe to events
                tick.ExpandRequested += OnTickExpandRequested;
                tick.CollapseRequested += OnTickCollapseRequested;
            }
        }

        private void OnTickExpandRequested(object? sender, TimeSegmentExpandEventArgs e)
        {
            ExpandTimeSegment(sender as FrameworkElement, e.SegmentStart, e.ToLevel);
        }

        private void OnTickCollapseRequested(object? sender, TimeSegmentCollapseEventArgs e)
        {
            CollapseTimeSegment(sender as FrameworkElement, e.SegmentStart);
        }

        private ExtendedTimeUnit? GetExpandedLevel(DateTime date)
        {
            return _expandedSegments.TryGetValue(date, out var level) ? level : null;
        }

        private DateTime GetLevelStartDate(DateTime date, ExtendedTimeUnit level)
        {
            return level switch
            {
                ExtendedTimeUnit.Year => new DateTime(date.Year, 1, 1),
                ExtendedTimeUnit.Quarter => GetQuarterStart(date),
                ExtendedTimeUnit.Month => new DateTime(date.Year, date.Month, 1),
                ExtendedTimeUnit.Week => GetWeekStart(date),
                ExtendedTimeUnit.Day => date.Date,
                ExtendedTimeUnit.Hour => new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0),
                _ => date
            };
        }

        private DateTime GetNextLevelDate(DateTime date, ExtendedTimeUnit level)
        {
            return level switch
            {
                ExtendedTimeUnit.Year => date.AddYears(1),
                ExtendedTimeUnit.Quarter => date.AddMonths(3),
                ExtendedTimeUnit.Month => date.AddMonths(1),
                ExtendedTimeUnit.Week => date.AddDays(7),
                ExtendedTimeUnit.Day => date.AddDays(1),
                ExtendedTimeUnit.Hour => date.AddHours(1),
                _ => date.AddDays(1)
            };
        }

        private DateTime GetQuarterStart(DateTime date)
        {
            int quarter = (date.Month - 1) / 3 + 1;
            int startMonth = (quarter - 1) * 3 + 1;
            return new DateTime(date.Year, startMonth, 1);
        }

        private DateTime GetWeekStart(DateTime date)
        {
            // Get Monday of the week
            var dayOfWeek = (int)date.DayOfWeek;
            if (dayOfWeek == 0) dayOfWeek = 7; // Sunday = 7
            return date.AddDays(1 - dayOfWeek);
        }

        private double CalculateSegmentWidth(DateTime startDate, DateTime endDate, TimeScaleContext context)
        {
            var totalDays = context.TotalSpan.TotalDays;
            var segmentDays = (endDate - startDate).TotalDays;
            return (segmentDays / totalDays) * context.AvailableWidth;
        }

        private string FormatDateForLevel(DateTime date, TimeLevelConfiguration level)
        {
            if (!string.IsNullOrEmpty(level.FormatString))
            {
                return date.ToString(level.FormatString);
            }

            return level.LevelType switch
            {
                ExtendedTimeUnit.Year => date.ToString("yyyy"),
                ExtendedTimeUnit.Quarter => $"Q{(date.Month - 1) / 3 + 1} {date.Year}",
                ExtendedTimeUnit.Month => date.ToString("MMM yyyy"),
                ExtendedTimeUnit.Week => $"Week {GetWeekNumber(date)}",
                ExtendedTimeUnit.Day => date.ToString("dd/MM"),
                ExtendedTimeUnit.Hour => date.ToString("HH:mm"),
                _ => date.ToString()
            };
        }

        private int GetWeekNumber(DateTime date)
        {
            var jan1 = new DateTime(date.Year, 1, 1);
            var dayOfYear = date.DayOfYear;
            var dayOfWeek = (int)jan1.DayOfWeek;
            if (dayOfWeek == 0) dayOfWeek = 7;
            
            return (dayOfYear + dayOfWeek - 2) / 7 + 1;
        }

        private void UpdateAffectedSegments(Grid container, DateTime segmentStart, ExtendedTimeUnit level)
        {
            // This method would handle partial updates for performance
            // For now, we'll trigger a full refresh
            // In a real implementation, you'd only update the affected Canvas and its ticks
        }

        private MultiLevelTimeScaleTick GetOrCreateTick()
        {
            if (_tickPool.Count > 0)
            {
                return _tickPool.Dequeue();
            }
            return new MultiLevelTimeScaleTick();
        }

        private void ReturnTick(MultiLevelTimeScaleTick tick)
        {
            if (tick != null && _tickPool.Count < MaxPoolSize)
            {
                // Reset tick properties
                tick.Date = DateTime.MinValue;
                tick.DisplayText = string.Empty;
                tick.ShowExpandButton = false;
                tick.IsExpanded = false;
                
                _tickPool.Enqueue(tick);
            }
        }
    }
}