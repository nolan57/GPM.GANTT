using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPM.Gantt;
using GPM.Gantt.Configuration;
using GPM.Gantt.Models;
using GPM.Gantt.Services;

namespace GPM.Gantt.Tests
{
    /// <summary>
    /// Comprehensive performance tests for the Gantt chart framework.
    /// Tests various performance optimization features and measures their effectiveness.
    /// </summary>
    [TestClass]
    public class PerformanceTests
    {
        private GanttContainer? _ganttContainer;
        private IPerformanceService? _performanceService;
        private IPerformanceDiagnostics? _diagnostics;
        private IMemoryOptimization? _memoryOptimization;
        
        [TestInitialize]
        public void Setup()
        {
            _ganttContainer = new GanttContainer();
            _performanceService = new PerformanceService();
            _diagnostics = _performanceService.GetDiagnostics();
            _memoryOptimization = _performanceService.GetMemoryOptimization();
            
            // Start monitoring for tests
            _diagnostics.StartMonitoring();
        }
        
        [TestCleanup]
        public void Cleanup()
        {
            _diagnostics?.StopMonitoring();
            (_memoryOptimization as IDisposable)?.Dispose();
        }
        
        [TestMethod]
        public void TestLargeDatasetPerformance()
        {
            // Arrange
            const int taskCount = 10000;
            var tasks = GenerateLargeTaskDataset(taskCount);
            var stopwatch = Stopwatch.StartNew();
            
            // Act
            _ganttContainer!.Tasks = tasks;
            _ganttContainer.TaskCount = 100; // Visible rows
            _ganttContainer.StartTime = DateTime.Today;
            _ganttContainer.EndTime = DateTime.Today.AddDays(365);
            
            // Force layout build
            _ganttContainer.Measure(new Size(1000, 600));
            
            stopwatch.Stop();
            
            // Assert
            var metrics = _diagnostics!.GetCurrentMetrics();
            
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 2000, $"Large dataset layout took too long: {stopwatch.ElapsedMilliseconds}ms");
            Assert.IsTrue(metrics.ManagedMemoryMB < 200, $"Memory usage too high: {metrics.ManagedMemoryMB:F1} MB");
            
            Console.WriteLine($"Large dataset performance: {stopwatch.ElapsedMilliseconds}ms, Memory: {metrics.ManagedMemoryMB:F1} MB");
        }
        
        [TestMethod]
        public void TestVirtualizationEffectiveness()
        {
            // Arrange
            const int taskCount = 5000;
            var tasks = GenerateLargeTaskDataset(taskCount);
            
            var configWithVirtualization = new GanttConfiguration
            {
                Rendering = new RenderingConfiguration
                {
                    EnableVirtualization = true,
                    MaxVisibleTasks = 100,
                    PerformanceLevel = PerformanceLevel.Performance
                }
            };
            
            var configWithoutVirtualization = new GanttConfiguration
            {
                Rendering = new RenderingConfiguration
                {
                    EnableVirtualization = false,
                    PerformanceLevel = PerformanceLevel.Quality
                }
            };
            
            // Test with virtualization
            var (timeWithVirt, memWithVirt) = MeasureLayoutPerformance(tasks, configWithVirtualization);
            
            // Test without virtualization (smaller dataset to avoid timeout)
            var smallTasks = new ObservableCollection<GanttTask>(tasks.Take(1000));
            var (timeWithoutVirt, memWithoutVirt) = MeasureLayoutPerformance(smallTasks, configWithoutVirtualization);
            
            // Assert
            Console.WriteLine($"With virtualization: {timeWithVirt}ms, {memWithVirt:F1} MB");
            Console.WriteLine($"Without virtualization: {timeWithoutVirt}ms, {memWithoutVirt:F1} MB");
            
            // Virtualization should be faster for large datasets
            Assert.IsTrue(timeWithVirt < timeWithoutVirt * 2, "Virtualization should improve performance");
        }
        
        [TestMethod]
        public void TestCachingEffectiveness()
        {
            // Arrange
            var start = DateTime.Today;
            var end = DateTime.Today.AddDays(100);
            var unit = TimeUnit.Day;
            
            // First call (cache miss)
            var stopwatch1 = Stopwatch.StartNew();
            var ticks1 = _performanceService!.GetCachedTimelineTicks(start, end, unit);
            stopwatch1.Stop();
            
            // Second call (cache hit)
            var stopwatch2 = Stopwatch.StartNew();
            var ticks2 = _performanceService.GetCachedTimelineTicks(start, end, unit);
            stopwatch2.Stop();
            
            // Assert
            Assert.AreEqual(ticks1.Count, ticks2.Count, "Cached ticks should be identical");
            Assert.IsTrue(stopwatch2.ElapsedMilliseconds < stopwatch1.ElapsedMilliseconds, 
                "Cached call should be faster than initial call");
            
            var metrics = _performanceService.GetPerformanceMetrics();
            Assert.IsTrue(metrics.CacheHitRatio > 0, "Cache hit ratio should be greater than 0");
            
            Console.WriteLine($"First call: {stopwatch1.ElapsedMilliseconds}ms, Second call: {stopwatch2.ElapsedMilliseconds}ms");
            Console.WriteLine($"Cache hit ratio: {metrics.CacheHitRatio:P}");
        }
        
        [TestMethod]
        public void TestMemoryOptimization()
        {
            // Arrange
            var memoryBefore = GC.GetTotalMemory(false);
            
            // Create memory pressure
            var largeTasks = GenerateLargeTaskDataset(5000);
            _ganttContainer!.Tasks = largeTasks;
            _ganttContainer.Measure(new Size(1000, 600));
            
            var memoryAfterLoad = GC.GetTotalMemory(false);
            
            // Act - optimize memory
            _memoryOptimization!.OptimizeMemory(MemoryOptimizationStrategy.Aggressive);
            
            var memoryAfterOptimization = GC.GetTotalMemory(false);
            
            // Assert
            var memoryFreed = memoryAfterLoad - memoryAfterOptimization;
            var memoryFreedMB = memoryFreed / (1024.0 * 1024.0);
            
            Assert.IsTrue(memoryFreed > 0, "Memory optimization should free some memory");
            
            Console.WriteLine($"Memory before: {memoryBefore / (1024 * 1024):F1} MB");
            Console.WriteLine($"Memory after load: {memoryAfterLoad / (1024 * 1024):F1} MB");
            Console.WriteLine($"Memory after optimization: {memoryAfterOptimization / (1024 * 1024):F1} MB");
            Console.WriteLine($"Memory freed: {memoryFreedMB:F1} MB");
        }
        
        [TestMethod]
        public void TestPerformanceDiagnostics()
        {
            // Arrange & Act
            using var measurement = _performanceService!.BeginMeasurement("TestOperation");
            
            // Simulate some work
            System.Threading.Thread.Sleep(100);
            
            measurement.Dispose();
            
            // Allow time for metrics to update
            System.Threading.Thread.Sleep(50);
            
            // Assert
            var metrics = _diagnostics!.GetCurrentMetrics();
            Assert.IsTrue(metrics.TotalMeasurements > 0, "Should have recorded measurements");
            Assert.IsTrue(metrics.EventStatistics.ContainsKey("TestOperation"), "Should contain test operation statistics");
            
            var testOpStats = metrics.EventStatistics["TestOperation"];
            Assert.IsTrue(testOpStats.AverageMs >= 100, $"Average duration should be at least 100ms, was {testOpStats.AverageMs}ms");
            
            Console.WriteLine($"Test operation stats: Count={testOpStats.Count}, Average={testOpStats.AverageMs:F1}ms");
        }
        
        [TestMethod]
        public void TestElementPoolingEfficiency()
        {
            // This test would require access to internal ElementPool statistics
            // For now, we'll test indirectly by measuring memory usage during repeated layouts
            
            var tasks = GenerateLargeTaskDataset(1000);
            var memoryUsages = new List<long>();
            
            // Perform multiple layout operations
            for (int i = 0; i < 10; i++)
            {
                _ganttContainer!.Tasks = tasks;
                _ganttContainer.TaskCount = 50;
                _ganttContainer.StartTime = DateTime.Today.AddDays(i * 10);
                _ganttContainer.EndTime = DateTime.Today.AddDays(i * 10 + 100);
                
                _ganttContainer.Measure(new Size(800, 400));
                
                memoryUsages.Add(GC.GetTotalMemory(false));
            }
            
            // Assert that memory usage doesn't grow linearly with iterations
            var memoryGrowth = memoryUsages.Last() - memoryUsages.First();
            var expectedLinearGrowth = memoryUsages[1] - memoryUsages[0];
            expectedLinearGrowth *= 9; // 9 additional iterations
            
            Assert.IsTrue(memoryGrowth < expectedLinearGrowth * 0.8, 
                "Element pooling should reduce memory growth compared to linear allocation");
            
            Console.WriteLine($"Memory growth: {memoryGrowth / (1024 * 1024):F1} MB vs expected linear: {expectedLinearGrowth / (1024 * 1024):F1} MB");
        }
        
        [TestMethod]
        public void TestDebouncingEffectiveness()
        {
            // Arrange
            var executionCount = 0;
            Action testAction = () => executionCount++;
            
            var stopwatch = Stopwatch.StartNew();
            
            // Act - trigger multiple debounced operations rapidly
            for (int i = 0; i < 10; i++)
            {
                _performanceService!.DebounceOperation(testAction, 100, "test-debounce");
                System.Threading.Thread.Sleep(10);
            }
            
            // Wait for debounced operation to execute
            System.Threading.Thread.Sleep(200);
            stopwatch.Stop();
            
            // Assert
            Assert.AreEqual(1, executionCount, "Debounced operation should execute only once");
            Assert.IsTrue(stopwatch.ElapsedMilliseconds >= 100, "Debounced operation should respect delay");
            
            Console.WriteLine($"Debouncing test: {executionCount} executions in {stopwatch.ElapsedMilliseconds}ms");
        }
        
        [TestMethod]
        public void TestPerformanceRecommendations()
        {
            // Arrange - create memory pressure
            var largeTasks = GenerateLargeTaskDataset(10000);
            _ganttContainer!.Tasks = largeTasks;
            
            // Force memory usage
            for (int i = 0; i < 5; i++)
            {
                _ganttContainer.Measure(new Size(1000, 600));
            }
            
            // Act
            var recommendations = _diagnostics!.GetRecommendations();
            
            // Assert
            Assert.IsNotNull(recommendations, "Should provide recommendations");
            
            var recommendationsList = recommendations.ToList();
            if (recommendationsList.Any())
            {
                Console.WriteLine($"Received {recommendationsList.Count} performance recommendations:");
                foreach (var rec in recommendationsList)
                {
                    Console.WriteLine($"- {rec.Title} ({rec.Severity}): {rec.Description}");
                    Assert.IsFalse(string.IsNullOrEmpty(rec.Title), "Recommendation should have a title");
                    Assert.IsFalse(string.IsNullOrEmpty(rec.Description), "Recommendation should have a description");
                }
            }
        }
        
        [TestMethod]
        public void TestOverallPerformanceImprovement()
        {
            // This is an integration test that measures overall performance
            const int iterations = 5;
            const int taskCount = 2000;
            
            var times = new List<long>();
            var memoryUsages = new List<long>();
            
            for (int i = 0; i < iterations; i++)
            {
                var tasks = GenerateLargeTaskDataset(taskCount);
                var (time, memory) = MeasureLayoutPerformance(tasks, new GanttConfiguration
                {
                    Rendering = new RenderingConfiguration
                    {
                        EnableVirtualization = true,
                        EnableCaching = true,
                        PerformanceLevel = PerformanceLevel.Balanced,
                        EnableAutoMemoryOptimization = true
                    }
                });
                
                times.Add(time);
                memoryUsages.Add((long)memory);
            }
            
            var averageTime = times.Average();
            var averageMemory = memoryUsages.Average() / (1024.0 * 1024.0);
            
            // Performance benchmarks (adjust based on your requirements)
            Assert.IsTrue(averageTime < 1000, $"Average layout time should be under 1000ms, was {averageTime:F1}ms");
            Assert.IsTrue(averageMemory < 150, $"Average memory usage should be under 150MB, was {averageMemory:F1}MB");
            
            Console.WriteLine($"Overall performance - Average time: {averageTime:F1}ms, Average memory: {averageMemory:F1}MB");
            
            // Print performance summary
            var finalMetrics = _diagnostics!.GetCurrentMetrics();
            Console.WriteLine($"\nFinal Performance Summary:");
            Console.WriteLine($"- Total measurements: {finalMetrics.TotalMeasurements}");
            Console.WriteLine($"- Managed memory: {finalMetrics.ManagedMemoryMB:F1} MB");
            Console.WriteLine($"- GC collections: Gen0={finalMetrics.Gen0Collections}, Gen1={finalMetrics.Gen1Collections}, Gen2={finalMetrics.Gen2Collections}");
            
            foreach (var stat in finalMetrics.EventStatistics)
            {
                Console.WriteLine($"- {stat.Key}: {stat.Value.Count} calls, avg {stat.Value.AverageMs:F1}ms");
            }
        }
        
        #region Helper Methods
        
        private static ObservableCollection<GanttTask> GenerateLargeTaskDataset(int count)
        {
            var tasks = new ObservableCollection<GanttTask>();
            var random = new Random(42); // Fixed seed for reproducible tests
            
            for (int i = 0; i < count; i++)
            {
                var startOffset = random.Next(0, 365);
                var duration = random.Next(1, 30);
                
                tasks.Add(new GanttTask
                {
                    Title = $"Task {i + 1}",
                    Start = DateTime.Today.AddDays(startOffset),
                    End = DateTime.Today.AddDays(startOffset + duration),
                    RowIndex = (i % 1000) + 1, // Distribute across 1000 rows
                    Progress = random.NextDouble(),
                    Priority = (TaskPriority)(i % 3),
                    Status = (GPM.Gantt.Models.TaskStatus)(i % 4)
                });
            }
            
            return tasks;
        }
        
        private (long timeMs, double memoryMB) MeasureLayoutPerformance(
            ObservableCollection<GanttTask> tasks, 
            GanttConfiguration config)
        {
            var memoryBefore = GC.GetTotalMemory(true);
            var stopwatch = Stopwatch.StartNew();
            
            _ganttContainer!.Tasks = tasks;
            _ganttContainer.Configuration = config;
            _ganttContainer.TaskCount = Math.Min(100, tasks.Count);
            _ganttContainer.StartTime = DateTime.Today;
            _ganttContainer.EndTime = DateTime.Today.AddDays(365);
            
            _ganttContainer.Measure(new Size(1000, 600));
            
            stopwatch.Stop();
            var memoryAfter = GC.GetTotalMemory(false);
            
            var memoryUsed = (memoryAfter - memoryBefore) / (1024.0 * 1024.0);
            
            return (stopwatch.ElapsedMilliseconds, memoryUsed);
        }
        
        #endregion
    }
}