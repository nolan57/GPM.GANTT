using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;
using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Provides comprehensive performance monitoring and diagnostics for the Gantt chart framework.
    /// Tracks rendering performance, memory usage, and provides optimization recommendations.
    /// </summary>
    public interface IPerformanceDiagnostics
    {
        /// <summary>
        /// Starts performance monitoring session.
        /// </summary>
        void StartMonitoring();
        
        /// <summary>
        /// Stops performance monitoring session.
        /// </summary>
        void StopMonitoring();
        
        /// <summary>
        /// Gets current performance metrics.
        /// </summary>
        PerformanceMetrics GetCurrentMetrics();
        
        /// <summary>
        /// Gets performance history over time.
        /// </summary>
        IEnumerable<PerformanceSnapshot> GetPerformanceHistory();
        
        /// <summary>
        /// Records a custom performance event.
        /// </summary>
        void RecordEvent(string eventName, TimeSpan duration, IDictionary<string, object>? metadata = null);
        
        /// <summary>
        /// Gets performance optimization recommendations.
        /// </summary>
        IEnumerable<PerformanceRecommendation> GetRecommendations();
        
        /// <summary>
        /// Clears all performance data.
        /// </summary>
        void Clear();
        
        /// <summary>
        /// Event raised when performance threshold is exceeded.
        /// </summary>
        event EventHandler<PerformanceThresholdEventArgs>? ThresholdExceeded;
    }
    
    /// <summary>
    /// Implementation of performance diagnostics with comprehensive monitoring capabilities.
    /// </summary>
    public class PerformanceDiagnostics : IPerformanceDiagnostics
    {
        private readonly List<PerformanceSnapshot> _performanceHistory = new();
        private readonly Dictionary<string, List<double>> _eventDurations = new();
        private readonly Dictionary<string, object> _customMetrics = new();
        private readonly Stopwatch _monitoringStopwatch = new();
        private readonly DispatcherTimer _samplingTimer;
        private readonly PerformanceCounter? _cpuCounter;
        private readonly PerformanceCounter? _memoryCounter;
        
        private bool _isMonitoring;
        private DateTime _monitoringStartTime;
        private long _totalMeasurements;
        private readonly object _lock = new();
        
        // Performance thresholds
        private static readonly TimeSpan LayoutBuildThreshold = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan RenderingThreshold = TimeSpan.FromMilliseconds(16.67); // 60 FPS
        private static readonly long MemoryThreshold = 100 * 1024 * 1024; // 100 MB
        
        public event EventHandler<PerformanceThresholdEventArgs>? ThresholdExceeded;
        
        public PerformanceDiagnostics()
        {
            // Initialize performance counters if available
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Performance counters not available: {ex.Message}");
            }
            
            // Setup sampling timer for continuous monitoring
            _samplingTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _samplingTimer.Tick += OnSamplingTimerTick;
        }
        
        public void StartMonitoring()
        {
            lock (_lock)
            {
                if (_isMonitoring)
                    return;
                
                _isMonitoring = true;
                _monitoringStartTime = DateTime.UtcNow;
                _monitoringStopwatch.Start();
                _samplingTimer.Start();
                
                Debug.WriteLine("Performance monitoring started");
            }
        }
        
        public void StopMonitoring()
        {
            lock (_lock)
            {
                if (!_isMonitoring)
                    return;
                
                _isMonitoring = false;
                _monitoringStopwatch.Stop();
                _samplingTimer.Stop();
                
                Debug.WriteLine($"Performance monitoring stopped after {_monitoringStopwatch.Elapsed}");
            }
        }
        
        public PerformanceMetrics GetCurrentMetrics()
        {
            lock (_lock)
            {
                var gcMemory = GC.GetTotalMemory(false);
                var process = Process.GetCurrentProcess();
                
                return new PerformanceMetrics
                {
                    Timestamp = DateTime.UtcNow,
                    IsMonitoring = _isMonitoring,
                    MonitoringDuration = _isMonitoring ? _monitoringStopwatch.Elapsed : TimeSpan.Zero,
                    TotalMeasurements = _totalMeasurements,
                    
                    // Memory metrics
                    ManagedMemoryBytes = gcMemory,
                    WorkingSetBytes = process.WorkingSet64,
                    PrivateMemoryBytes = process.PrivateMemorySize64,
                    Gen0Collections = GC.CollectionCount(0),
                    Gen1Collections = GC.CollectionCount(1),
                    Gen2Collections = GC.CollectionCount(2),
                    
                    // Performance counters (if available)
                    CpuUsagePercent = GetCpuUsage(),
                    AvailableMemoryMB = GetAvailableMemory(),
                    
                    // Custom metrics
                    CustomMetrics = new Dictionary<string, object>(_customMetrics),
                    
                    // Event statistics
                    EventStatistics = _eventDurations.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new EventStatistics
                        {
                            Count = kvp.Value.Count,
                            AverageMs = kvp.Value.Average(),
                            MinMs = kvp.Value.Min(),
                            MaxMs = kvp.Value.Max(),
                            TotalMs = kvp.Value.Sum()
                        })
                };
            }
        }
        
        public IEnumerable<PerformanceSnapshot> GetPerformanceHistory()
        {
            lock (_lock)
            {
                return _performanceHistory.ToList();
            }
        }
        
        public void RecordEvent(string eventName, TimeSpan duration, IDictionary<string, object>? metadata = null)
        {
            lock (_lock)
            {
                _totalMeasurements++;
                
                if (!_eventDurations.ContainsKey(eventName))
                    _eventDurations[eventName] = new List<double>();
                
                var durationMs = duration.TotalMilliseconds;
                _eventDurations[eventName].Add(durationMs);
                
                // Limit history size to prevent memory leaks
                if (_eventDurations[eventName].Count > 1000)
                {
                    _eventDurations[eventName].RemoveRange(0, 100);
                }
                
                // Store metadata if provided
                if (metadata != null)
                {
                    foreach (var kvp in metadata)
                    {
                        _customMetrics[$"{eventName}.{kvp.Key}"] = kvp.Value;
                    }
                }
                
                // Check thresholds
                CheckPerformanceThresholds(eventName, duration);
            }
        }
        
        public IEnumerable<PerformanceRecommendation> GetRecommendations()
        {
            var recommendations = new List<PerformanceRecommendation>();
            var metrics = GetCurrentMetrics();
            
            // Memory recommendations
            if (metrics.ManagedMemoryBytes > MemoryThreshold)
            {
                recommendations.Add(new PerformanceRecommendation
                {
                    Type = RecommendationType.Memory,
                    Severity = RecommendationSeverity.High,
                    Title = "High Memory Usage Detected",
                    Description = $"Managed memory usage is {metrics.ManagedMemoryBytes / (1024 * 1024):F1} MB. Consider enabling virtualization or reducing dataset size.",
                    ActionItems = new[]
                    {
                        "Enable virtualization in RenderingConfiguration",
                        "Reduce the number of visible tasks",
                        "Enable automatic memory optimization",
                        "Consider data pagination"
                    }
                });
            }
            
            // Performance recommendations
            if (metrics.EventStatistics.TryGetValue("LayoutBuild", out var layoutStats) && 
                layoutStats.AverageMs > LayoutBuildThreshold.TotalMilliseconds)
            {
                recommendations.Add(new PerformanceRecommendation
                {
                    Type = RecommendationType.Rendering,
                    Severity = RecommendationSeverity.Medium,
                    Title = "Slow Layout Building",
                    Description = $"Layout building takes {layoutStats.AverageMs:F1}ms on average. Target is under {LayoutBuildThreshold.TotalMilliseconds}ms.",
                    ActionItems = new[]
                    {
                        "Enable virtualization for large datasets",
                        "Increase layout debounce delay",
                        "Set performance level to 'Performance'",
                        "Reduce time granularity"
                    }
                });
            }
            
            // GC pressure recommendations
            if (metrics.Gen0Collections > 100)
            {
                recommendations.Add(new PerformanceRecommendation
                {
                    Type = RecommendationType.GarbageCollection,
                    Severity = RecommendationSeverity.Medium,
                    Title = "High Garbage Collection Activity",
                    Description = $"Generation 0 collections: {metrics.Gen0Collections}. This indicates frequent memory allocations.",
                    ActionItems = new[]
                    {
                        "Enable element pooling",
                        "Reduce layout rebuilds",
                        "Use cached timeline calculations",
                        "Optimize data binding"
                    }
                });
            }
            
            return recommendations;
        }
        
        public void Clear()
        {
            lock (_lock)
            {
                _performanceHistory.Clear();
                _eventDurations.Clear();
                _customMetrics.Clear();
                _totalMeasurements = 0;
            }
        }
        
        private void OnSamplingTimerTick(object? sender, EventArgs e)
        {
            if (!_isMonitoring)
                return;
            
            var snapshot = new PerformanceSnapshot
            {
                Timestamp = DateTime.UtcNow,
                Metrics = GetCurrentMetrics()
            };
            
            lock (_lock)
            {
                _performanceHistory.Add(snapshot);
                
                // Limit history size
                if (_performanceHistory.Count > 300) // 5 minutes at 1 second intervals
                {
                    _performanceHistory.RemoveRange(0, 50);
                }
            }
        }
        
        private void CheckPerformanceThresholds(string eventName, TimeSpan duration)
        {
            bool thresholdExceeded = false;
            string message = string.Empty;
            
            switch (eventName)
            {
                case "LayoutBuild" when duration > LayoutBuildThreshold:
                    thresholdExceeded = true;
                    message = $"Layout building took {duration.TotalMilliseconds:F1}ms (threshold: {LayoutBuildThreshold.TotalMilliseconds}ms)";
                    break;
                    
                case "Rendering" when duration > RenderingThreshold:
                    thresholdExceeded = true;
                    message = $"Rendering took {duration.TotalMilliseconds:F1}ms (threshold: {RenderingThreshold.TotalMilliseconds}ms)";
                    break;
            }
            
            if (thresholdExceeded)
            {
                ThresholdExceeded?.Invoke(this, new PerformanceThresholdEventArgs
                {
                    EventName = eventName,
                    Duration = duration,
                    Threshold = eventName == "LayoutBuild" ? LayoutBuildThreshold : RenderingThreshold,
                    Message = message
                });
            }
        }
        
        private double GetCpuUsage()
        {
            try
            {
                return _cpuCounter?.NextValue() ?? 0.0;
            }
            catch
            {
                return 0.0;
            }
        }
        
        private double GetAvailableMemory()
        {
            try
            {
                return _memoryCounter?.NextValue() ?? 0.0;
            }
            catch
            {
                return 0.0;
            }
        }
        
        public void Dispose()
        {
            StopMonitoring();
            _samplingTimer?.Stop();
            _cpuCounter?.Dispose();
            _memoryCounter?.Dispose();
        }
    }
}