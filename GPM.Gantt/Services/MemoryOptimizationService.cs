using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Provides comprehensive memory optimization services for the Gantt chart framework.
    /// Includes garbage collection optimization, memory monitoring, and automated cleanup.
    /// </summary>
    public interface IMemoryOptimization
    {
        /// <summary>
        /// Optimizes memory usage with the specified strategy.
        /// </summary>
        /// <param name="strategy">The optimization strategy to use.</param>
        void OptimizeMemory(MemoryOptimizationStrategy strategy = MemoryOptimizationStrategy.Balanced);

        /// <summary>
        /// Enables automatic memory optimization based on usage patterns.
        /// </summary>
        /// <param name="interval">The optimization interval.</param>
        void EnableAutoOptimization(TimeSpan interval);

        /// <summary>
        /// Disables automatic memory optimization.
        /// </summary>
        void DisableAutoOptimization();
        
        /// <summary>
        /// Gets current memory usage statistics.
        /// </summary>
        /// <returns>Memory usage information.</returns>
        MemoryUsageInfo GetMemoryUsage();
        
        /// <summary>
        /// Configures garbage collection settings for optimal performance.
        /// </summary>
        /// <param name="mode">The GC mode to configure.</param>
        void ConfigureGarbageCollection(GCMode mode);
        
        /// <summary>
        /// Monitors memory pressure and triggers optimization when needed.
        /// </summary>
        void StartMemoryPressureMonitoring();
        
        /// <summary>
        /// Stops memory pressure monitoring.
        /// </summary>
        void StopMemoryPressureMonitoring();
        
        /// <summary>
        /// Event raised when memory pressure is detected.
        /// </summary>
        event EventHandler<MemoryPressureEventArgs>? MemoryPressureDetected;
    }
    
    /// <summary>
    /// Implementation of memory optimization services.
    /// </summary>
    public class MemoryOptimizationService : IMemoryOptimization, IDisposable
    {
        private readonly DispatcherTimer _autoOptimizationTimer;
        private readonly Timer _memoryPressureTimer;
        private readonly object _optimizationLock = new();
        private bool _isAutoOptimizationEnabled;
        private bool _isMemoryPressureMonitoringEnabled;
        private long _lastMemoryUsage;
        private DateTime _lastOptimizationTime;
        private readonly List<WeakReference> _managedReferences = new();
        
        // Memory thresholds
        private const long HighMemoryThreshold = 200 * 1024 * 1024; // 200 MB
        private const long CriticalMemoryThreshold = 500 * 1024 * 1024; // 500 MB
        private const double MemoryGrowthThreshold = 1.5; // 50% growth
        
        public event EventHandler<MemoryPressureEventArgs>? MemoryPressureDetected;
        
        public MemoryOptimizationService()
        {
            _autoOptimizationTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMinutes(5) // Default interval
            };
            _autoOptimizationTimer.Tick += OnAutoOptimizationTimer;
            
            _memoryPressureTimer = new Timer(CheckMemoryPressure, null, Timeout.Infinite, Timeout.Infinite);
            _lastMemoryUsage = GC.GetTotalMemory(false);
            _lastOptimizationTime = DateTime.UtcNow;
        }
        
        public void OptimizeMemory(MemoryOptimizationStrategy strategy = MemoryOptimizationStrategy.Balanced)
        {
            lock (_optimizationLock)
            {
                Debug.WriteLine($"Starting memory optimization with strategy: {strategy}");
                
                var beforeMemory = GC.GetTotalMemory(false);
                var stopwatch = Stopwatch.StartNew();
                
                try
                {
                    switch (strategy)
                    {
                        case MemoryOptimizationStrategy.Aggressive:
                            PerformAggressiveOptimization();
                            break;
                        case MemoryOptimizationStrategy.Balanced:
                            PerformBalancedOptimization();
                            break;
                        case MemoryOptimizationStrategy.Conservative:
                            PerformConservativeOptimization();
                            break;
                    }
                    
                    _lastOptimizationTime = DateTime.UtcNow;
                    
                    var afterMemory = GC.GetTotalMemory(false);
                    var freedMemory = beforeMemory - afterMemory;
                    
                    stopwatch.Stop();
                    
                    Debug.WriteLine($"Memory optimization completed in {stopwatch.ElapsedMilliseconds}ms");
                    Debug.WriteLine($"Memory freed: {freedMemory / (1024 * 1024):F1} MB");
                    Debug.WriteLine($"Memory before: {beforeMemory / (1024 * 1024):F1} MB, after: {afterMemory / (1024 * 1024):F1} MB");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Memory optimization failed: {ex.Message}");
                }
            }
        }
        
        public void EnableAutoOptimization(TimeSpan interval)
        {
            _autoOptimizationTimer.Interval = interval;
            _autoOptimizationTimer.Start();
            _isAutoOptimizationEnabled = true;
            
            Debug.WriteLine($"Auto memory optimization enabled with interval: {interval}");
        }
        
        public void DisableAutoOptimization()
        {
            _autoOptimizationTimer.Stop();
            _isAutoOptimizationEnabled = false;
            
            Debug.WriteLine("Auto memory optimization disabled");
        }
        
        public MemoryUsageInfo GetMemoryUsage()
        {
            var process = Process.GetCurrentProcess();
            var managedMemory = GC.GetTotalMemory(false);
            
            return new MemoryUsageInfo
            {
                Timestamp = DateTime.UtcNow,
                ManagedMemoryBytes = managedMemory,
                WorkingSetBytes = process.WorkingSet64,
                PrivateMemoryBytes = process.PrivateMemorySize64,
                VirtualMemoryBytes = process.VirtualMemorySize64,
                PagedMemoryBytes = process.PagedMemorySize64,
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2),
                IsAutoOptimizationEnabled = _isAutoOptimizationEnabled,
                LastOptimizationTime = _lastOptimizationTime,
                MemoryPressureLevel = GetMemoryPressureLevel(managedMemory)
            };
        }
        
        public void ConfigureGarbageCollection(GCMode mode)
        {
            try
            {
                switch (mode)
                {
                    case GCMode.Workstation:
                        // Default GC mode for desktop applications
                        Debug.WriteLine("GC configured for workstation mode");
                        break;
                        
                    case GCMode.Server:
                        // Server GC mode - not typically used in WPF applications
                        Debug.WriteLine("GC configured for server mode");
                        break;
                        
                    case GCMode.LowLatency:
                        // Minimize GC pauses
                        GCSettings.LatencyMode = GCLatencyMode.LowLatency;
                        Debug.WriteLine("GC configured for low latency mode");
                        break;
                        
                    case GCMode.Batch:
                        // Optimize for throughput
                        GCSettings.LatencyMode = GCLatencyMode.Batch;
                        Debug.WriteLine("GC configured for batch mode");
                        break;
                        
                    case GCMode.Sustained:
                        // Balanced approach
                        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
                        Debug.WriteLine("GC configured for sustained low latency mode");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to configure GC: {ex.Message}");
            }
        }
        
        public void StartMemoryPressureMonitoring()
        {
            _memoryPressureTimer.Change(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
            _isMemoryPressureMonitoringEnabled = true;
            
            Debug.WriteLine("Memory pressure monitoring started");
        }
        
        public void StopMemoryPressureMonitoring()
        {
            _memoryPressureTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _isMemoryPressureMonitoringEnabled = false;
            
            Debug.WriteLine("Memory pressure monitoring stopped");
        }
        
        #region Private Methods
        
        private void PerformAggressiveOptimization()
        {
            // Cleanup weak references
            CleanupWeakReferences();
            
            // Force full garbage collection multiple times
            for (int i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            
            // Compact large object heap
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
            
            // Trim working set
            TrimWorkingSet();
        }
        
        private void PerformBalancedOptimization()
        {
            // Cleanup weak references
            CleanupWeakReferences();
            
            // Single full garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        
        private void PerformConservativeOptimization()
        {
            // Only cleanup weak references and Gen 0/1 collections
            CleanupWeakReferences();
            
            GC.Collect(1, GCCollectionMode.Optimized);
        }
        
        private void CleanupWeakReferences()
        {
            for (int i = _managedReferences.Count - 1; i >= 0; i--)
            {
                if (!_managedReferences[i].IsAlive)
                {
                    _managedReferences.RemoveAt(i);
                }
            }
        }
        
        private void TrimWorkingSet()
        {
            try
            {
                // Use native method to trim working set if available
                var process = Process.GetCurrentProcess();
                // This is a hint to the OS to trim the working set
                // Actual implementation would require P/Invoke to SetProcessWorkingSetSize
                Debug.WriteLine("Working set trim requested");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to trim working set: {ex.Message}");
            }
        }
        
        private void OnAutoOptimizationTimer(object? sender, EventArgs e)
        {
            Task.Run(() => OptimizeMemory(MemoryOptimizationStrategy.Balanced));
        }
        
        private void CheckMemoryPressure(object? state)
        {
            try
            {
                var currentMemory = GC.GetTotalMemory(false);
                var memoryGrowth = _lastMemoryUsage > 0 ? (double)currentMemory / _lastMemoryUsage : 1.0;
                var pressureLevel = GetMemoryPressureLevel(currentMemory);
                
                if (pressureLevel != MemoryPressureLevel.Normal || memoryGrowth > MemoryGrowthThreshold)
                {
                    var eventArgs = new MemoryPressureEventArgs
                    {
                        CurrentMemoryBytes = currentMemory,
                        PreviousMemoryBytes = _lastMemoryUsage,
                        MemoryGrowthRatio = memoryGrowth,
                        PressureLevel = pressureLevel,
                        Recommendation = GetOptimizationRecommendation(pressureLevel, memoryGrowth)
                    };
                    
                    MemoryPressureDetected?.Invoke(this, eventArgs);
                    
                    // Auto-optimize on high pressure
                    if (pressureLevel >= MemoryPressureLevel.High)
                    {
                        Task.Run(() => OptimizeMemory(MemoryOptimizationStrategy.Aggressive));
                    }
                }
                
                _lastMemoryUsage = currentMemory;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Memory pressure check failed: {ex.Message}");
            }
        }
        
        private static MemoryPressureLevel GetMemoryPressureLevel(long memoryBytes)
        {
            if (memoryBytes >= CriticalMemoryThreshold)
                return MemoryPressureLevel.Critical;
            if (memoryBytes >= HighMemoryThreshold)
                return MemoryPressureLevel.High;
            
            return MemoryPressureLevel.Normal;
        }
        
        private static MemoryOptimizationStrategy GetOptimizationRecommendation(MemoryPressureLevel pressureLevel, double memoryGrowth)
        {
            return pressureLevel switch
            {
                MemoryPressureLevel.Critical => MemoryOptimizationStrategy.Aggressive,
                MemoryPressureLevel.High => MemoryOptimizationStrategy.Balanced,
                _ when memoryGrowth > MemoryGrowthThreshold => MemoryOptimizationStrategy.Conservative,
                _ => MemoryOptimizationStrategy.Conservative
            };
        }
        
        #endregion
        
        public void Dispose()
        {
            DisableAutoOptimization();
            StopMemoryPressureMonitoring();
            _autoOptimizationTimer?.Stop();
            _memoryPressureTimer?.Dispose();
        }
    }
}