using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Threading;
using System.Windows;
using GPM.Gantt.Models;
using GPM.Gantt.Utilities;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Implementation of the performance service for optimizing framework performance.
    /// Provides caching, debouncing, and performance monitoring capabilities.
    /// </summary>
    public class PerformanceService : IPerformanceService
    {
        private readonly ConcurrentDictionary<string, TimelineMetadata> _timelineMetadataCache = new();
        private readonly ConcurrentDictionary<string, List<DateTime>> _timelineTicksCache = new();
        private readonly ConcurrentDictionary<string, System.Threading.Timer> _debouncedOperations = new();
        private readonly PerformanceMetrics _metrics = new();
        private readonly IPerformanceDiagnostics _diagnostics;
        private readonly IMemoryOptimization _memoryOptimization;
        private readonly object _metricsLock = new();
        
        public PerformanceService()
        {
            _diagnostics = new PerformanceDiagnostics();
            _memoryOptimization = new MemoryOptimizationService();
            
            // Configure memory optimization
            _memoryOptimization.ConfigureGarbageCollection(GCMode.Sustained);
            _memoryOptimization.EnableAutoOptimization(TimeSpan.FromMinutes(5));
            _memoryOptimization.StartMemoryPressureMonitoring();
            
            // Subscribe to memory pressure events
            _memoryOptimization.MemoryPressureDetected += OnMemoryPressureDetected;
            
            // Start monitoring by default if diagnostics are enabled
            if (System.Diagnostics.Debugger.IsAttached)
            {
                _diagnostics.StartMonitoring();
            }
        }

        /// <summary>
        /// Gets cached timeline metadata for the specified parameters.
        /// </summary>
        /// <param name="start">Start time.</param>
        /// <param name="end">End time.</param>
        /// <param name="unit">Time unit.</param>
        /// <param name="culture">Culture for calculations.</param>
        /// <returns>Cached or calculated timeline metadata.</returns>
        public TimelineMetadata GetCachedTimelineMetadata(DateTime start, DateTime end, TimeUnit unit, CultureInfo? culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            var cacheKey = CreateTimelineCacheKey(start, end, unit, culture);

            if (_timelineMetadataCache.TryGetValue(cacheKey, out var cached))
            {
                RecordCacheHit();
                return cached;
            }

            RecordCacheMiss();
            var metadata = TimelineHelper.GetTimelineMetadata(start, end, unit, culture);
            _timelineMetadataCache.TryAdd(cacheKey, metadata);
            
            return metadata;
        }

        /// <summary>
        /// Gets cached timeline ticks for the specified parameters.
        /// </summary>
        /// <param name="start">Start time.</param>
        /// <param name="end">End time.</param>
        /// <param name="unit">Time unit.</param>
        /// <param name="culture">Culture for calculations.</param>
        /// <returns>Cached or calculated timeline ticks.</returns>
        public List<DateTime> GetCachedTimelineTicks(DateTime start, DateTime end, TimeUnit unit, CultureInfo? culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            var cacheKey = CreateTimelineCacheKey(start, end, unit, culture);

            if (_timelineTicksCache.TryGetValue(cacheKey, out var cached))
            {
                RecordCacheHit();
                return new List<DateTime>(cached); // Return a copy to prevent modification
            }

            RecordCacheMiss();
            var ticks = TimelineCalculator.GenerateTicks(start, end, unit, culture);
            _timelineTicksCache.TryAdd(cacheKey, new List<DateTime>(ticks));
            
            return ticks;
        }

        /// <summary>
        /// Debounces layout operations to prevent excessive rebuilds.
        /// </summary>
        /// <param name="action">The action to debounce.</param>
        /// <param name="delay">Delay in milliseconds.</param>
        /// <param name="key">Unique key for the debounced operation.</param>
        public void DebounceOperation(Action action, int delay = 150, string key = "default")
        {
            // Cancel existing timer for this key
            if (_debouncedOperations.TryGetValue(key, out var existingTimer))
            {
                existingTimer.Dispose();
            }

            // Create new timer
            var timer = new System.Threading.Timer(_ =>
            {
                // Execute on UI thread
                Application.Current?.Dispatcher.BeginInvoke(action, DispatcherPriority.Normal);
                
                // Clean up timer
                if (_debouncedOperations.TryRemove(key, out var timerToDispose))
                {
                    timerToDispose.Dispose();
                }
            }, null, delay, Timeout.Infinite);

            _debouncedOperations.TryAdd(key, timer);
        }

        /// <summary>
        /// Clears the cache for timeline calculations.
        /// </summary>
        public void ClearTimelineCache()
        {
            _timelineMetadataCache.Clear();
            _timelineTicksCache.Clear();
        }

        /// <summary>
        /// Clears all caches.
        /// </summary>
        public void ClearAllCaches()
        {
            ClearTimelineCache();
            
            // Dispose debounced operations
            foreach (var timer in _debouncedOperations.Values)
            {
                timer.Dispose();
            }
            _debouncedOperations.Clear();
        }

        /// <summary>
        /// Gets performance metrics for monitoring.
        /// </summary>
        /// <returns>Current performance metrics.</returns>
        public PerformanceMetrics GetPerformanceMetrics()
        {
            lock (_metricsLock)
            {
                var metrics = new PerformanceMetrics
                {
                    CacheHits = _metrics.CacheHits,
                    CacheMisses = _metrics.CacheMisses,
                    LayoutRebuilds = _metrics.LayoutRebuilds,
                    AverageLayoutBuildTime = _metrics.AverageLayoutBuildTime,
                    MemoryUsage = GC.GetTotalMemory(false),
                    ActiveUIElements = _metrics.ActiveUIElements,
                    OperationMetrics = new Dictionary<string, OperationMetrics>(_metrics.OperationMetrics)
                };
                
                return metrics;
            }
        }

        /// <summary>
        /// Gets the memory optimization service.
        /// </summary>
        /// <returns>The memory optimization service instance.</returns>
        public IMemoryOptimization GetMemoryOptimization()
        {
            return _memoryOptimization;
        }
        
        /// <summary>
        /// Gets the performance diagnostics service.
        /// </summary>
        /// <returns>The diagnostics service instance.</returns>
        public IPerformanceDiagnostics GetDiagnostics()
        {
            return _diagnostics;
        }
        
        /// <summary>
        /// Records the start of a performance measurement.
        /// </summary>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns>Performance measurement token.</returns>
        public IDisposable BeginMeasurement(string operationName)
        {
            return new PerformanceMeasurement(this, _diagnostics, operationName);
        }

        /// <summary>
        /// Optimizes memory usage by cleaning up unused cached items.
        /// </summary>
        public void OptimizeMemoryUsage()
        {
            // Use the dedicated memory optimization service
            _memoryOptimization.OptimizeMemory(MemoryOptimizationStrategy.Balanced);
            
            // Clear old cache entries if cache is large
            if (_timelineMetadataCache.Count > 100)
            {
                var keysToRemove = _timelineMetadataCache.Keys.Take(50).ToList();
                foreach (var key in keysToRemove)
                {
                    _timelineMetadataCache.TryRemove(key, out _);
                }
            }

            if (_timelineTicksCache.Count > 100)
            {
                var keysToRemove = _timelineTicksCache.Keys.Take(50).ToList();
                foreach (var key in keysToRemove)
                {
                    _timelineTicksCache.TryRemove(key, out _);
                }
            }
        }

        internal void RecordLayoutRebuild(double executionTime)
        {
            lock (_metricsLock)
            {
                _metrics.LayoutRebuilds++;
                var totalTime = _metrics.AverageLayoutBuildTime * (_metrics.LayoutRebuilds - 1) + executionTime;
                _metrics.AverageLayoutBuildTime = totalTime / _metrics.LayoutRebuilds;
            }
        }

        internal void RecordOperation(string operationName, double executionTime)
        {
            lock (_metricsLock)
            {
                if (!_metrics.OperationMetrics.TryGetValue(operationName, out var opMetrics))
                {
                    opMetrics = new OperationMetrics();
                    _metrics.OperationMetrics[operationName] = opMetrics;
                }

                opMetrics.ExecutionCount++;
                opMetrics.TotalExecutionTime += executionTime;
                opMetrics.LastExecutionTime = executionTime;
            }
        }

        private void RecordCacheHit()
        {
            lock (_metricsLock)
            {
                _metrics.CacheHits++;
            }
        }

        private void RecordCacheMiss()
        {
            lock (_metricsLock)
            {
                _metrics.CacheMisses++;
            }
        }

        private static string CreateTimelineCacheKey(DateTime start, DateTime end, TimeUnit unit, CultureInfo culture)
        {
            return $"{start:yyyy-MM-dd HH:mm:ss}|{end:yyyy-MM-dd HH:mm:ss}|{unit}|{culture.Name}";
        }

        /// <summary>
        /// Performance measurement token for tracking operation execution time.
        /// </summary>
        private class PerformanceMeasurement : IDisposable
        {
            private readonly PerformanceService _service;
            private readonly IPerformanceDiagnostics _diagnostics;
            private readonly string _operationName;
            private readonly Stopwatch _stopwatch;
            private bool _disposed;

            public PerformanceMeasurement(PerformanceService service, IPerformanceDiagnostics diagnostics, string operationName)
            {
                _service = service;
                _diagnostics = diagnostics;
                _operationName = operationName;
                _stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _stopwatch.Stop();
                    var duration = _stopwatch.Elapsed;
                    
                    _service.RecordOperation(_operationName, duration.TotalMilliseconds);
                    _diagnostics.RecordEvent(_operationName, duration);
                    
                    _disposed = true;
                }
            }
        }
        
        private void OnMemoryPressureDetected(object? sender, MemoryPressureEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Memory pressure detected: {e.PressureLevel} (Growth: {e.MemoryGrowthPercentage:F1}%)");
            
            // Clear caches on high memory pressure
            if (e.PressureLevel >= MemoryPressureLevel.High)
            {
                ClearAllCaches();
            }
        }
    }
}