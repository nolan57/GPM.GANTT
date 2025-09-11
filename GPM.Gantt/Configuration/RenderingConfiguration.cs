using System.Globalization;
using GPM.Gantt.Models;
using GPM.Gantt.Rendering;
using GPM.Gantt.Services;

namespace GPM.Gantt.Configuration
{
    /// <summary>
    /// Configuration settings for rendering behavior and appearance.
    /// </summary>
    public class RenderingConfiguration
    {
        /// <summary>
        /// Gets or sets whether to enable virtualization for large datasets.
        /// </summary>
        public bool EnableVirtualization { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the maximum number of visible tasks before virtualization kicks in.
        /// </summary>
        public int MaxVisibleTasks { get; set; } = 1000;
        
        /// <summary>
        /// Gets or sets whether to enable caching of rendered elements.
        /// </summary>
        public bool EnableCaching { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the grid rendering mode.
        /// </summary>
        public GridRenderingMode GridMode { get; set; } = GridRenderingMode.Rectangles;
        
        /// <summary>
        /// Gets or sets whether to show grid cells.
        /// </summary>
        public bool ShowGridCells { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the default task bar corner radius.
        /// </summary>
        public double TaskBarCornerRadius { get; set; } = 4.0;
        
        /// <summary>
        /// Gets or sets the default task bar shape.
        /// </summary>
        public TaskBarShape DefaultTaskBarShape { get; set; } = TaskBarShape.Rectangle;
        
        /// <summary>
        /// Gets or sets whether to use enhanced shape rendering.
        /// </summary>
        public bool UseEnhancedShapeRendering { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the default shape rendering parameters.
        /// </summary>
        public ShapeRenderingParameters? DefaultShapeParameters { get; set; }
        
        /// <summary>
        /// Gets or sets the performance optimization level.
        /// </summary>
        public PerformanceLevel PerformanceLevel { get; set; } = PerformanceLevel.Balanced;
        
        /// <summary>
        /// Gets or sets whether to enable performance monitoring.
        /// </summary>
        public bool EnablePerformanceMonitoring { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the debounce delay for layout operations in milliseconds.
        /// </summary>
        public int LayoutDebounceDelay { get; set; } = 150;
        
        /// <summary>
        /// Gets or sets whether to enable automatic memory optimization.
        /// </summary>
        public bool EnableAutoMemoryOptimization { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the memory optimization strategy.
        /// </summary>
        public MemoryOptimizationStrategy MemoryOptimizationStrategy { get; set; } = MemoryOptimizationStrategy.Balanced;
        
        /// <summary>
        /// Gets or sets the auto-optimization interval for memory management.
        /// </summary>
        public TimeSpan AutoOptimizationInterval { get; set; } = TimeSpan.FromMinutes(5);
        
        /// <summary>
        /// Gets or sets whether to enable memory pressure monitoring.
        /// </summary>
        public bool EnableMemoryPressureMonitoring { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the garbage collection mode.
        /// </summary>
        public GCMode GarbageCollectionMode { get; set; } = GCMode.Sustained;
        
        /// <summary>
        /// Gets or sets the GPU rendering technology to use.
        /// </summary>
        public GpuRenderingTechnology GpuRenderingTechnology { get; set; } = GpuRenderingTechnology.Default;
        
        /// <summary>
        /// Gets or sets whether to enable GPU acceleration for rendering.
        /// </summary>
        public bool EnableGpuAcceleration { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to enable batch rendering for improved performance.
        /// </summary>
        public bool EnableBatchRendering { get; set; } = true;
        
        /// <summary>
        /// Creates a high-performance configuration optimized for large datasets.
        /// </summary>
        public static RenderingConfiguration HighPerformance() => new()
        {
            EnableVirtualization = true,
            MaxVisibleTasks = 500,
            EnableCaching = true,
            GridMode = GridRenderingMode.Lines,
            UseEnhancedShapeRendering = false,
            PerformanceLevel = PerformanceLevel.Performance,
            EnableGpuAcceleration = false,
            EnableBatchRendering = true,
            LayoutDebounceDelay = 200
        };

        /// <summary>
        /// Creates a quality-focused configuration with enhanced visual features.
        /// </summary>
        public static RenderingConfiguration HighQuality() => new()
        {
            EnableVirtualization = true,
            MaxVisibleTasks = 1000,
            EnableCaching = true,
            GridMode = GridRenderingMode.Rectangles,
            UseEnhancedShapeRendering = true,
            PerformanceLevel = PerformanceLevel.Quality,
            EnableGpuAcceleration = true,
            EnableBatchRendering = true,
            LayoutDebounceDelay = 100
        };

        /// <summary>
        /// Creates a balanced configuration for general use.
        /// </summary>
        public static RenderingConfiguration Balanced() => new()
        {
            EnableVirtualization = true,
            MaxVisibleTasks = 1000,
            EnableCaching = true,
            GridMode = GridRenderingMode.Rectangles,
            UseEnhancedShapeRendering = true,
            PerformanceLevel = PerformanceLevel.Balanced,
            EnableGpuAcceleration = false,
            EnableBatchRendering = true,
            LayoutDebounceDelay = 150
        };
    }
    
    /// <summary>
    /// Configuration settings for localization and internationalization.
    /// </summary>
    public class LocalizationConfiguration
    {
        /// <summary>
        /// Gets or sets the culture for the user interface.
        /// </summary>
        public CultureInfo UICulture { get; set; } = CultureInfo.CurrentUICulture;
        
        /// <summary>
        /// Gets or sets whether to use right-to-left layout.
        /// </summary>
        public bool IsRightToLeft { get; set; } = false;
    }
}