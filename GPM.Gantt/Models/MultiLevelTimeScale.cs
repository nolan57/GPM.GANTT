using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace GPM.Gantt.Models
{
    /// <summary>
    /// Extended time unit enumeration
    /// </summary>
    public enum ExtendedTimeUnit
    {
        Minute = 0,
        Hour = 1,
        Day = 2,
        Week = 3,
        Month = 4,
        Quarter = 5,
        Year = 6,
        Custom = 7
    }

    /// <summary>
    /// Time level configuration
    /// </summary>
    public class TimeLevelConfiguration : INotifyPropertyChanged
    {
        private ExtendedTimeUnit _levelType;
        private bool _isVisible = true;
        private bool _isExpandable;
        private bool _isExpanded;
        private double _height = 30;
        private string _formatString = string.Empty;
        private Brush _background = Brushes.White;
        private Brush _foreground = Brushes.Black;
        private string _fontFamily = "Microsoft YaHei";
        private double _fontSize = 12;
        private int _displayOrder;

        public ExtendedTimeUnit LevelType
        {
            get => _levelType;
            set => SetProperty(ref _levelType, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public bool IsExpandable
        {
            get => _isExpandable;
            set => SetProperty(ref _isExpandable, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public string FormatString
        {
            get => _formatString;
            set => SetProperty(ref _formatString, value);
        }

        public Brush Background
        {
            get => _background;
            set => SetProperty(ref _background, value);
        }

        public Brush Foreground
        {
            get => _foreground;
            set => SetProperty(ref _foreground, value);
        }

        public string FontFamily
        {
            get => _fontFamily;
            set => SetProperty(ref _fontFamily, value);
        }

        public double FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        public int DisplayOrder
        {
            get => _displayOrder;
            set => SetProperty(ref _displayOrder, value);
        }

        /// <summary>
        /// Expandable sub-level list
        /// </summary>
        public List<TimeLevelConfiguration> ExpandableLevels { get; set; } = new List<TimeLevelConfiguration>();

        /// <summary>
        /// Smart visibility condition function
        /// </summary>
        public Func<TimeSpan, double, bool> VisibilityCondition { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Create default configuration
        /// </summary>
        public static TimeLevelConfiguration CreateDefault(ExtendedTimeUnit levelType)
        {
            var config = new TimeLevelConfiguration
            {
                LevelType = levelType,
                DisplayOrder = (int)levelType
            };

            switch (levelType)
            {
                case ExtendedTimeUnit.Year:
                    config.FormatString = "yyyy";
                    config.Height = 35;
                    config.FontSize = 14;
                    config.IsExpandable = true;
                    config.ExpandableLevels.Add(CreateDefault(ExtendedTimeUnit.Quarter));
                    break;

                case ExtendedTimeUnit.Quarter:
                    config.FormatString = "Q\\Q yyyy";
                    config.Height = 30;
                    config.FontSize = 12;
                    config.IsExpandable = true;
                    config.ExpandableLevels.Add(CreateDefault(ExtendedTimeUnit.Month));
                    break;

                case ExtendedTimeUnit.Month:
                    config.FormatString = "MMM yyyy";
                    config.Height = 28;
                    config.FontSize = 11;
                    config.IsExpandable = true;
                    config.ExpandableLevels.Add(CreateDefault(ExtendedTimeUnit.Week));
                    break;

                case ExtendedTimeUnit.Week:
                    config.FormatString = "Week dd";
                    config.Height = 25;
                    config.FontSize = 10;
                    config.IsExpandable = true;
                    config.ExpandableLevels.Add(CreateDefault(ExtendedTimeUnit.Day));
                    break;

                case ExtendedTimeUnit.Day:
                    config.FormatString = "dd/MM";
                    config.Height = 22;
                    config.FontSize = 9;
                    config.IsExpandable = true;
                    config.ExpandableLevels.Add(CreateDefault(ExtendedTimeUnit.Hour));
                    break;

                case ExtendedTimeUnit.Hour:
                    config.FormatString = "HH:mm";
                    config.Height = 20;
                    config.FontSize = 8;
                    break;
            }

            return config;
        }
    }

    /// <summary>
    /// Multi-level time scale configuration
    /// </summary>
    public class MultiLevelTimeScaleConfiguration : INotifyPropertyChanged
    {
        private List<TimeLevelConfiguration> _levels = new List<TimeLevelConfiguration>();
        private double _totalHeight;
        private int _maxVisibleLevels = 3;
        private bool _enableSmartVisibility = true;
        private bool _enableExpandableSegments = true;

        public List<TimeLevelConfiguration> Levels
        {
            get => _levels;
            set => SetProperty(ref _levels, value);
        }

        public double TotalHeight
        {
            get => _totalHeight;
            set => SetProperty(ref _totalHeight, value);
        }

        public int MaxVisibleLevels
        {
            get => _maxVisibleLevels;
            set => SetProperty(ref _maxVisibleLevels, value);
        }

        public bool EnableSmartVisibility
        {
            get => _enableSmartVisibility;
            set => SetProperty(ref _enableSmartVisibility, value);
        }

        public bool EnableExpandableSegments
        {
            get => _enableExpandableSegments;
            set => SetProperty(ref _enableExpandableSegments, value);
        }

        /// <summary>
        /// Expanded time segment information
        /// </summary>
        public Dictionary<DateTime, ExtendedTimeUnit> ExpandedSegments { get; set; } = new Dictionary<DateTime, ExtendedTimeUnit>();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Create default configuration
        /// </summary>
        public static MultiLevelTimeScaleConfiguration CreateDefault()
        {
            var config = new MultiLevelTimeScaleConfiguration();
            
            config.Levels.Add(TimeLevelConfiguration.CreateDefault(ExtendedTimeUnit.Year));
            config.Levels.Add(TimeLevelConfiguration.CreateDefault(ExtendedTimeUnit.Month));
            config.Levels.Add(TimeLevelConfiguration.CreateDefault(ExtendedTimeUnit.Week));

            config.UpdateTotalHeight();
            return config;
        }

        /// <summary>
        /// Update total height
        /// </summary>
        public void UpdateTotalHeight()
        {
            TotalHeight = 0;
            foreach (var level in Levels.Where(l => l.IsVisible))
            {
                TotalHeight += level.Height;
            }
        }

        /// <summary>
        /// Smart optimization for display levels
        /// </summary>
        public void OptimizeForContext(TimeSpan timeSpan, double availableWidth)
        {
            if (!EnableSmartVisibility) return;

            var totalDays = timeSpan.TotalDays;

            // Automatically adjust display levels based on time span
            foreach (var level in Levels)
            {
                level.IsVisible = ShouldShowLevel(level.LevelType, totalDays, availableWidth);
            }

            UpdateTotalHeight();
        }

        private bool ShouldShowLevel(ExtendedTimeUnit levelType, double totalDays, double availableWidth)
        {
            return levelType switch
            {
                ExtendedTimeUnit.Year => totalDays > 365,
                ExtendedTimeUnit.Quarter => totalDays > 90,
                ExtendedTimeUnit.Month => totalDays > 30,
                ExtendedTimeUnit.Week => totalDays > 7 && totalDays < 365,
                ExtendedTimeUnit.Day => totalDays <= 31,
                ExtendedTimeUnit.Hour => totalDays <= 3,
                _ => true
            };
        }

        /// <summary>
        /// Expand specified time segment
        /// </summary>
        public void ExpandSegment(DateTime segmentStart, ExtendedTimeUnit fromLevel, ExtendedTimeUnit toLevel)
        {
            if (!EnableExpandableSegments) return;

            ExpandedSegments[segmentStart] = toLevel;
            
            // Trigger re-rendering
            OnPropertyChanged(nameof(ExpandedSegments));
        }

        /// <summary>
        /// Collapse specified time segment
        /// </summary>
        public void CollapseSegment(DateTime segmentStart)
        {
            if (ExpandedSegments.ContainsKey(segmentStart))
            {
                ExpandedSegments.Remove(segmentStart);
                OnPropertyChanged(nameof(ExpandedSegments));
            }
        }
    }

    /// <summary>
    /// Time scale context information
    /// </summary>
    public class TimeScaleContext
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double AvailableWidth { get; set; }
        public double AvailableHeight { get; set; }
        public int MaxVisibleLevels { get; set; }
        public double ZoomLevel { get; set; } = 1.0;
        public TimeSpan TotalSpan => EndDate - StartDate;
        public double DaysSpan => TotalSpan.TotalDays;
    }
}