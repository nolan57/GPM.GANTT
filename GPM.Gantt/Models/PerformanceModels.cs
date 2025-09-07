using System;
using System.Collections.Generic;

namespace GPM.Gantt.Models
{
    /// <summary>
    /// Comprehensive performance metrics for the Gantt chart framework.
    /// </summary>
    public class PerformanceMetrics
    {
        /// <summary>
        /// Gets or sets the timestamp when these metrics were captured.
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Gets or sets whether performance monitoring is currently active.
        /// </summary>
        public bool IsMonitoring { get; set; }
        
        /// <summary>
        /// Gets or sets the total duration of performance monitoring.
        /// </summary>
        public TimeSpan MonitoringDuration { get; set; }
        
        /// <summary>
        /// Gets or sets the total number of performance measurements taken.
        /// </summary>
        public long TotalMeasurements { get; set; }
        
        /// <summary>
        /// Gets or sets the number of cache hits.
        /// </summary>
        public int CacheHits { get; set; }

        /// <summary>
        /// Gets or sets the number of cache misses.
        /// </summary>
        public int CacheMisses { get; set; }

        /// <summary>
        /// Gets the cache hit ratio.
        /// </summary>
        public double CacheHitRatio => CacheHits + CacheMisses > 0 ? (double)CacheHits / (CacheHits + CacheMisses) : 0;

        /// <summary>
        /// Gets or sets the number of layout rebuilds.
        /// </summary>
        public int LayoutRebuilds { get; set; }

        /// <summary>
        /// Gets or sets the average layout build time in milliseconds.
        /// </summary>
        public double AverageLayoutBuildTime { get; set; }

        /// <summary>
        /// Gets or sets the number of active UI elements.
        /// </summary>
        public int ActiveUIElements { get; set; }

        /// <summary>
        /// Gets or sets performance measurements by operation.
        /// </summary>
        public Dictionary<string, OperationMetrics> OperationMetrics { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the total memory usage in bytes (compatibility property).
        /// </summary>
        public long MemoryUsage 
        {
            get => ManagedMemoryBytes;
            set => ManagedMemoryBytes = value;
        }

        #region Memory Metrics
        
        /// <summary>
        /// Gets or sets the managed memory usage in bytes.
        /// </summary>
        public long ManagedMemoryBytes { get; set; }
        
        /// <summary>
        /// Gets or sets the working set memory in bytes.
        /// </summary>
        public long WorkingSetBytes { get; set; }
        
        /// <summary>
        /// Gets or sets the private memory usage in bytes.
        /// </summary>
        public long PrivateMemoryBytes { get; set; }
        
        /// <summary>
        /// Gets or sets the number of Generation 0 garbage collections.
        /// </summary>
        public int Gen0Collections { get; set; }
        
        /// <summary>
        /// Gets or sets the number of Generation 1 garbage collections.
        /// </summary>
        public int Gen1Collections { get; set; }
        
        /// <summary>
        /// Gets or sets the number of Generation 2 garbage collections.
        /// </summary>
        public int Gen2Collections { get; set; }
        
        #endregion
        
        #region System Metrics
        
        /// <summary>
        /// Gets or sets the CPU usage percentage.
        /// </summary>
        public double CpuUsagePercent { get; set; }
        
        /// <summary>
        /// Gets or sets the available system memory in megabytes.
        /// </summary>
        public double AvailableMemoryMB { get; set; }
        
        #endregion
        
        #region Custom Metrics
        
        /// <summary>
        /// Gets or sets custom performance metrics.
        /// </summary>
        public Dictionary<string, object> CustomMetrics { get; set; } = new();
        
        /// <summary>
        /// Gets or sets event-specific performance statistics.
        /// </summary>
        public Dictionary<string, EventStatistics> EventStatistics { get; set; } = new();
        
        #endregion
        
        /// <summary>
        /// Gets the managed memory usage in megabytes.
        /// </summary>
        public double ManagedMemoryMB => ManagedMemoryBytes / (1024.0 * 1024.0);
        
        /// <summary>
        /// Gets the working set memory in megabytes.
        /// </summary>
        public double WorkingSetMB => WorkingSetBytes / (1024.0 * 1024.0);
        
        /// <summary>
        /// Gets the private memory usage in megabytes.
        /// </summary>
        public double PrivateMemoryMB => PrivateMemoryBytes / (1024.0 * 1024.0);
        
        /// <summary>
        /// Gets the total garbage collection count across all generations.
        /// </summary>
        public int TotalCollections => Gen0Collections + Gen1Collections + Gen2Collections;
    }
    
    /// <summary>
    /// Represents metrics for a specific operation.
    /// </summary>
    public class OperationMetrics
    {
        /// <summary>
        /// Gets or sets the number of executions.
        /// </summary>
        public int ExecutionCount { get; set; }

        /// <summary>
        /// Gets or sets the total execution time in milliseconds.
        /// </summary>
        public double TotalExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the average execution time in milliseconds.
        /// </summary>
        public double AverageExecutionTime => ExecutionCount > 0 ? TotalExecutionTime / ExecutionCount : 0;

        /// <summary>
        /// Gets or sets the last execution time in milliseconds.
        /// </summary>
        public double LastExecutionTime { get; set; }
    }
    
    /// <summary>
    /// Statistics for a specific performance event type.
    /// </summary>
    public class EventStatistics
    {
        /// <summary>
        /// Gets or sets the number of times this event was recorded.
        /// </summary>
        public int Count { get; set; }
        
        /// <summary>
        /// Gets or sets the average duration in milliseconds.
        /// </summary>
        public double AverageMs { get; set; }
        
        /// <summary>
        /// Gets or sets the minimum duration in milliseconds.
        /// </summary>
        public double MinMs { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum duration in milliseconds.
        /// </summary>
        public double MaxMs { get; set; }
        
        /// <summary>
        /// Gets or sets the total time spent in milliseconds.
        /// </summary>
        public double TotalMs { get; set; }
        
        /// <summary>
        /// Gets the average duration as a TimeSpan.
        /// </summary>
        public TimeSpan AverageTimeSpan => TimeSpan.FromMilliseconds(AverageMs);
        
        /// <summary>
        /// Gets the total duration as a TimeSpan.
        /// </summary>
        public TimeSpan TotalTimeSpan => TimeSpan.FromMilliseconds(TotalMs);
    }
    
    /// <summary>
    /// A snapshot of performance metrics at a specific point in time.
    /// </summary>
    public class PerformanceSnapshot
    {
        /// <summary>
        /// Gets or sets the timestamp of this snapshot.
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Gets or sets the performance metrics at this timestamp.
        /// </summary>
        public PerformanceMetrics Metrics { get; set; } = new();
    }
    
    /// <summary>
    /// Performance optimization recommendation.
    /// </summary>
    public class PerformanceRecommendation
    {
        /// <summary>
        /// Gets or sets the type of recommendation.
        /// </summary>
        public RecommendationType Type { get; set; }
        
        /// <summary>
        /// Gets or sets the severity of the performance issue.
        /// </summary>
        public RecommendationSeverity Severity { get; set; }
        
        /// <summary>
        /// Gets or sets the recommendation title.
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the detailed description of the issue.
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the list of recommended action items.
        /// </summary>
        public string[] ActionItems { get; set; } = Array.Empty<string>();
        
        /// <summary>
        /// Gets or sets the expected impact of implementing this recommendation.
        /// </summary>
        public ImpactLevel? ExpectedImpact { get; set; }
        
        /// <summary>
        /// Gets or sets the implementation difficulty level.
        /// </summary>
        public DifficultyLevel? ImplementationDifficulty { get; set; }
    }
    
    /// <summary>
    /// Types of performance recommendations.
    /// </summary>
    public enum RecommendationType
    {
        /// <summary>
        /// Memory usage optimization.
        /// </summary>
        Memory,
        
        /// <summary>
        /// Rendering performance optimization.
        /// </summary>
        Rendering,
        
        /// <summary>
        /// Garbage collection optimization.
        /// </summary>
        GarbageCollection,
        
        /// <summary>
        /// CPU usage optimization.
        /// </summary>
        CPU,
        
        /// <summary>
        /// General configuration optimization.
        /// </summary>
        Configuration,
        
        /// <summary>
        /// Data management optimization.
        /// </summary>
        Data
    }
    
    /// <summary>
    /// Severity levels for performance recommendations.
    /// </summary>
    public enum RecommendationSeverity
    {
        /// <summary>
        /// Low priority optimization.
        /// </summary>
        Low,
        
        /// <summary>
        /// Medium priority optimization.
        /// </summary>
        Medium,
        
        /// <summary>
        /// High priority optimization.
        /// </summary>
        High,
        
        /// <summary>
        /// Critical performance issue.
        /// </summary>
        Critical
    }
    
    /// <summary>
    /// Expected impact levels for performance improvements.
    /// </summary>
    public enum ImpactLevel
    {
        /// <summary>
        /// Low performance impact.
        /// </summary>
        Low,
        
        /// <summary>
        /// Medium performance impact.
        /// </summary>
        Medium,
        
        /// <summary>
        /// High performance impact.
        /// </summary>
        High,
        
        /// <summary>
        /// Dramatic performance impact.
        /// </summary>
        Dramatic
    }
    
    /// <summary>
    /// Implementation difficulty levels.
    /// </summary>
    public enum DifficultyLevel
    {
        /// <summary>
        /// Easy to implement (configuration change).
        /// </summary>
        Easy,
        
        /// <summary>
        /// Medium difficulty (code changes required).
        /// </summary>
        Medium,
        
        /// <summary>
        /// Hard to implement (architectural changes).
        /// </summary>
        Hard,
        
        /// <summary>
        /// Very difficult (major refactoring required).
        /// </summary>
        VeryHard
    }
    
    /// <summary>
    /// Event arguments for performance threshold exceeded events.
    /// </summary>
    public class PerformanceThresholdEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the name of the event that exceeded the threshold.
        /// </summary>
        public string EventName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the actual duration that was recorded.
        /// </summary>
        public TimeSpan Duration { get; set; }
        
        /// <summary>
        /// Gets or sets the threshold that was exceeded.
        /// </summary>
        public TimeSpan Threshold { get; set; }
        
        /// <summary>
        /// Gets or sets a descriptive message about the threshold violation.
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets the amount by which the threshold was exceeded.
        /// </summary>
        public TimeSpan ExcessDuration => Duration - Threshold;
        
        /// <summary>
        /// Gets the percentage by which the threshold was exceeded.
        /// </summary>
        public double ExcessPercentage => Threshold.TotalMilliseconds > 0 
            ? (ExcessDuration.TotalMilliseconds / Threshold.TotalMilliseconds) * 100 
            : 0;
    }
}