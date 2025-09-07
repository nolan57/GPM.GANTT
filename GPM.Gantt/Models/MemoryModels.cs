using System;

namespace GPM.Gantt.Models
{
    /// <summary>
    /// Strategies for memory optimization in the Gantt chart framework.
    /// </summary>
    public enum MemoryOptimizationStrategy
    {
        /// <summary>
        /// Conservative optimization - minimal impact on performance.
        /// Only performs lightweight cleanup operations.
        /// </summary>
        Conservative,
        
        /// <summary>
        /// Balanced optimization - moderate cleanup with good performance balance.
        /// Performs standard garbage collection and cleanup.
        /// </summary>
        Balanced,
        
        /// <summary>
        /// Aggressive optimization - maximum memory cleanup.
        /// May cause temporary performance impact but maximizes memory recovery.
        /// </summary>
        Aggressive
    }
    
    /// <summary>
    /// Garbage collection modes for performance optimization.
    /// </summary>
    public enum GCMode
    {
        /// <summary>
        /// Workstation garbage collection - optimized for desktop applications.
        /// </summary>
        Workstation,
        
        /// <summary>
        /// Server garbage collection - optimized for server applications.
        /// </summary>
        Server,
        
        /// <summary>
        /// Low latency mode - minimizes GC pauses but may use more memory.
        /// </summary>
        LowLatency,
        
        /// <summary>
        /// Batch mode - optimizes for throughput over responsiveness.
        /// </summary>
        Batch,
        
        /// <summary>
        /// Sustained low latency - balanced approach for consistent performance.
        /// </summary>
        Sustained
    }
    
    /// <summary>
    /// Memory pressure levels for monitoring system resources.
    /// </summary>
    public enum MemoryPressureLevel
    {
        /// <summary>
        /// Normal memory usage - no action required.
        /// </summary>
        Normal,
        
        /// <summary>
        /// Elevated memory usage - monitoring recommended.
        /// </summary>
        Elevated,
        
        /// <summary>
        /// High memory usage - optimization recommended.
        /// </summary>
        High,
        
        /// <summary>
        /// Critical memory usage - immediate optimization required.
        /// </summary>
        Critical
    }
    
    /// <summary>
    /// Comprehensive memory usage information for the application.
    /// </summary>
    public class MemoryUsageInfo
    {
        /// <summary>
        /// Gets or sets the timestamp when this information was captured.
        /// </summary>
        public DateTime Timestamp { get; set; }
        
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
        /// Gets or sets the virtual memory usage in bytes.
        /// </summary>
        public long VirtualMemoryBytes { get; set; }
        
        /// <summary>
        /// Gets or sets the paged memory usage in bytes.
        /// </summary>
        public long PagedMemoryBytes { get; set; }
        
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
        
        /// <summary>
        /// Gets or sets whether automatic memory optimization is enabled.
        /// </summary>
        public bool IsAutoOptimizationEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp of the last memory optimization.
        /// </summary>
        public DateTime LastOptimizationTime { get; set; }
        
        /// <summary>
        /// Gets or sets the current memory pressure level.
        /// </summary>
        public MemoryPressureLevel MemoryPressureLevel { get; set; }
        
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
        /// Gets the virtual memory usage in megabytes.
        /// </summary>
        public double VirtualMemoryMB => VirtualMemoryBytes / (1024.0 * 1024.0);
        
        /// <summary>
        /// Gets the paged memory usage in megabytes.
        /// </summary>
        public double PagedMemoryMB => PagedMemoryBytes / (1024.0 * 1024.0);
        
        /// <summary>
        /// Gets the total garbage collection count across all generations.
        /// </summary>
        public int TotalCollections => Gen0Collections + Gen1Collections + Gen2Collections;
        
        /// <summary>
        /// Gets the time elapsed since the last optimization.
        /// </summary>
        public TimeSpan TimeSinceLastOptimization => Timestamp - LastOptimizationTime;
    }
    
    /// <summary>
    /// Event arguments for memory pressure detection events.
    /// </summary>
    public class MemoryPressureEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the current memory usage in bytes.
        /// </summary>
        public long CurrentMemoryBytes { get; set; }
        
        /// <summary>
        /// Gets or sets the previous memory usage in bytes.
        /// </summary>
        public long PreviousMemoryBytes { get; set; }
        
        /// <summary>
        /// Gets or sets the memory growth ratio.
        /// </summary>
        public double MemoryGrowthRatio { get; set; }
        
        /// <summary>
        /// Gets or sets the current memory pressure level.
        /// </summary>
        public MemoryPressureLevel PressureLevel { get; set; }
        
        /// <summary>
        /// Gets or sets the recommended optimization strategy.
        /// </summary>
        public MemoryOptimizationStrategy Recommendation { get; set; }
        
        /// <summary>
        /// Gets the current memory usage in megabytes.
        /// </summary>
        public double CurrentMemoryMB => CurrentMemoryBytes / (1024.0 * 1024.0);
        
        /// <summary>
        /// Gets the previous memory usage in megabytes.
        /// </summary>
        public double PreviousMemoryMB => PreviousMemoryBytes / (1024.0 * 1024.0);
        
        /// <summary>
        /// Gets the absolute memory growth in bytes.
        /// </summary>
        public long MemoryGrowthBytes => CurrentMemoryBytes - PreviousMemoryBytes;
        
        /// <summary>
        /// Gets the absolute memory growth in megabytes.
        /// </summary>
        public double MemoryGrowthMB => MemoryGrowthBytes / (1024.0 * 1024.0);
        
        /// <summary>
        /// Gets the memory growth percentage.
        /// </summary>
        public double MemoryGrowthPercentage => PreviousMemoryBytes > 0 
            ? ((double)MemoryGrowthBytes / PreviousMemoryBytes) * 100 
            : 0;
    }
}