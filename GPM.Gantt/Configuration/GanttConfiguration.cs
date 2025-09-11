using System;
using System.Globalization;
using GPM.Gantt.Models;
using GPM.Gantt.Services;

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
        /// Validates the configuration and throws exceptions for invalid settings.
        /// </summary>
        public void Validate()
        {
            if (Rendering.MaxVisibleTasks <= 0)
                throw new InvalidOperationException("MaxVisibleTasks must be greater than zero.");
                
            if (Rendering.LayoutDebounceDelay < 0)
                throw new InvalidOperationException("LayoutDebounceDelay cannot be negative.");
                
            if (Rendering.EnableGpuAcceleration && Rendering.GpuRenderingTechnology == GpuRenderingTechnology.Default)
                throw new InvalidOperationException("GpuRenderingTechnology must be specified when GPU acceleration is enabled. Use Direct2D, DirectX, OpenGL, or Vulkan.");
        }
        
        /// <summary>
        /// Validates the configuration and returns validation results.
        /// </summary>
        public ValidationResult ValidateWithResult()
        {
            var result = new ValidationResult();
            
            if (Rendering.MaxVisibleTasks <= 0)
            {
                result.AddError("MaxVisibleTasks must be greater than zero.");
            }
            
            if (Rendering.LayoutDebounceDelay < 0)
            {
                result.AddError("LayoutDebounceDelay cannot be negative.");
            }
            
            if (Rendering.EnableGpuAcceleration && Rendering.GpuRenderingTechnology == GpuRenderingTechnology.Default)
            {
                result.AddError("GpuRenderingTechnology must be specified when GPU acceleration is enabled. Use Direct2D, DirectX, OpenGL, or Vulkan.");
            }
            
            return result;
        }
        
        /// <summary>
        /// Creates a configuration optimized for large datasets.
        /// </summary>
        /// <param name="maxVisibleTasks">Maximum number of visible tasks.</param>
        /// <returns>A new GanttConfiguration optimized for large datasets.</returns>
        public static GanttConfiguration ForLargeDatasets(int maxVisibleTasks = 1000) => new()
        {
            Rendering = RenderingConfiguration.HighPerformance(),
            TimeScale = new TimeScaleConfiguration
            {
                DefaultTimeUnit = TimeUnit.Week,
                HighlightWeekends = true,
                HighlightToday = true
            }
        };

        /// <summary>
        /// Creates a configuration optimized for detailed project planning.
        /// </summary>
        /// <returns>A new GanttConfiguration optimized for detailed planning.</returns>
        public static GanttConfiguration ForDetailedPlanning() => new()
        {
            Rendering = RenderingConfiguration.HighQuality(),
            TimeScale = new TimeScaleConfiguration
            {
                DefaultTimeUnit = TimeUnit.Day,
                HighlightWeekends = true,
                HighlightToday = true
            }
        };

        /// <summary>
        /// Creates a configuration with balanced settings for general use.
        /// </summary>
        /// <returns>A new GanttConfiguration with balanced settings.</returns>
        public static GanttConfiguration Balanced() => new()
        {
            Rendering = RenderingConfiguration.Balanced(),
            TimeScale = new TimeScaleConfiguration()
        };
        
        /// <summary>
        /// Creates a new configuration builder.
        /// </summary>
        public static GanttConfigurationBuilder CreateBuilder() => new();
        
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
    
    /// <summary>
    /// Fluent builder for GanttConfiguration.
    /// </summary>
    public class GanttConfigurationBuilder
    {
        private readonly GanttConfiguration _configuration;

        public GanttConfigurationBuilder()
        {
            _configuration = new GanttConfiguration();
        }

        /// <summary>
        /// Sets the rendering configuration.
        /// </summary>
        public GanttConfigurationBuilder WithRendering(Action<RenderingConfiguration> configure)
        {
            configure(_configuration.Rendering);
            return this;
        }

        /// <summary>
        /// Sets the time scale configuration.
        /// </summary>
        public GanttConfigurationBuilder WithTimeScale(Action<TimeScaleConfiguration> configure)
        {
            configure(_configuration.TimeScale);
            return this;
        }

        /// <summary>
        /// Configures the gantt for large datasets.
        /// </summary>
        public GanttConfigurationBuilder ForLargeDatasets(int maxVisibleTasks = 1000)
        {
            _configuration.Rendering = RenderingConfiguration.HighPerformance();
            _configuration.Rendering.MaxVisibleTasks = maxVisibleTasks;
            return this;
        }

        /// <summary>
        /// Configures the gantt for detailed planning.
        /// </summary>
        public GanttConfigurationBuilder ForDetailedPlanning()
        {
            _configuration.Rendering = RenderingConfiguration.HighQuality();
            return this;
        }

        /// <summary>
        /// Builds the configuration.
        /// </summary>
        public GanttConfiguration Build()
        {
            return _configuration;
        }
    }
}