using GPM.Gantt.Models;
using GPM.Gantt.Utilities;
using System.Globalization;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Service interface for performance optimization and caching in the Gantt chart framework.
    /// Provides caching, debouncing, and performance monitoring capabilities.
    /// </summary>
    public interface IPerformanceService
    {
        /// <summary>
        /// Gets cached timeline metadata for the specified parameters.
        /// </summary>
        /// <param name="start">Start time.</param>
        /// <param name="end">End time.</param>
        /// <param name="unit">Time unit.</param>
        /// <param name="culture">Culture for calculations.</param>
        /// <returns>Cached or calculated timeline metadata.</returns>
        TimelineMetadata GetCachedTimelineMetadata(DateTime start, DateTime end, TimeUnit unit, CultureInfo? culture = null);

        /// <summary>
        /// Gets cached timeline ticks for the specified parameters.
        /// </summary>
        /// <param name="start">Start time.</param>
        /// <param name="end">End time.</param>
        /// <param name="unit">Time unit.</param>
        /// <param name="culture">Culture for calculations.</param>
        /// <returns>Cached or calculated timeline ticks.</returns>
        List<DateTime> GetCachedTimelineTicks(DateTime start, DateTime end, TimeUnit unit, CultureInfo? culture = null);

        /// <summary>
        /// Debounces layout operations to prevent excessive rebuilds.
        /// </summary>
        /// <param name="action">The action to debounce.</param>
        /// <param name="delay">Delay in milliseconds.</param>
        /// <param name="key">Unique key for the debounced operation.</param>
        void DebounceOperation(Action action, int delay = 150, string key = "default");

        /// <summary>
        /// Clears the cache for timeline calculations.
        /// </summary>
        void ClearTimelineCache();

        /// <summary>
        /// Clears all caches.
        /// </summary>
        void ClearAllCaches();

        /// <summary>
        /// Gets performance metrics for monitoring.
        /// </summary>
        /// <returns>Current performance metrics.</returns>
        PerformanceMetrics GetPerformanceMetrics();

        /// <summary>
        /// Records the start of a performance measurement.
        /// </summary>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns>Performance measurement token.</returns>
        IDisposable BeginMeasurement(string operationName);

        /// <summary>
        /// Gets the memory optimization service.
        /// </summary>
        /// <returns>The memory optimization service instance.</returns>
        IMemoryOptimization GetMemoryOptimization();
        
        /// <summary>
        /// Gets the performance diagnostics service.
        /// </summary>
        /// <returns>The diagnostics service instance.</returns>
        IPerformanceDiagnostics GetDiagnostics();
        
        /// <summary>
        /// Optimizes memory usage by cleaning up unused cached items.
        /// </summary>
        void OptimizeMemoryUsage();
    }
}