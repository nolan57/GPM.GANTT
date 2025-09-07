using System;
using System.Globalization;
using GPM.Gantt.Models;

namespace GPM.Gantt.Configuration
{
    /// <summary>
    /// Comprehensive configuration settings for the Gantt chart component.
    /// </summary>
    public class GanttConfiguration
    {
        /// <summary>
        /// Gets or sets the time scale configuration settings.
        /// </summary>
        public TimeScaleConfiguration TimeScale { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the rendering configuration settings.
        /// </summary>
        public RenderingConfiguration Rendering { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the localization configuration settings.
        /// </summary>
        public LocalizationConfiguration Localization { get; set; } = new();
        
        /// <summary>
        /// Creates a default configuration instance.
        /// </summary>
        /// <returns>A new GanttConfiguration with default settings.</returns>
        public static GanttConfiguration Default() => new();
    }
    
    /// <summary>
    /// Configuration settings for time scale display and behavior.
    /// </summary>
    public class TimeScaleConfiguration
    {
        /// <summary>
        /// Gets or sets the default time unit for the timeline.
        /// </summary>
        public TimeUnit DefaultTimeUnit { get; set; } = TimeUnit.Day;
        
        /// <summary>
        /// Gets or sets the date format string for display.
        /// </summary>
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        
        /// <summary>
        /// Gets or sets the time format string for display.
        /// </summary>
        public string TimeFormat { get; set; } = "HH:mm";
        
        /// <summary>
        /// Gets or sets the culture for time formatting.
        /// </summary>
        public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;
        
        /// <summary>
        /// Gets or sets whether to highlight weekends.
        /// </summary>
        public bool HighlightWeekends { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to highlight today.
        /// </summary>
        public bool HighlightToday { get; set; } = true;
    }
}