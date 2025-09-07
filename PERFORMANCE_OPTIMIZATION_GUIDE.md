# GPM.Gantt Performance Optimization Guide

This guide provides comprehensive information on optimizing performance when working with the GPM.Gantt component library, especially for large datasets and enterprise applications.

## Performance Overview

The GPM.Gantt library is designed for high performance with the following key optimizations:

- **UI Virtualization**: Efficient rendering of large datasets
- **Memory Management**: Automatic optimization with configurable strategies
- **Element Pooling**: Reusable UI elements for improved rendering
- **Timeline Caching**: Intelligent caching of timeline calculations
- **Performance Monitoring**: Real-time diagnostics and recommendations

## Quick Performance Setup

### Basic Performance Configuration
```csharp
var config = new GanttConfiguration
{
    Rendering = new RenderingConfiguration
    {
        EnableVirtualization = true,
        MaxVisibleTasks = 1000,
        PerformanceLevel = PerformanceLevel.Performance,
        EnableAutoMemoryOptimization = true,
        LayoutDebounceDelay = 150
    }
};
ganttContainer.Configuration = config;
```

### Initialize Performance Services
```csharp
var performanceService = new PerformanceService();

// Enable diagnostics
performanceService.GetDiagnostics().StartMonitoring();

// Configure memory optimization
var memoryService = performanceService.GetMemoryOptimization();
memoryService.EnableAutoOptimization(TimeSpan.FromMinutes(5));
memoryService.StartMemoryPressureMonitoring();
```

## Virtualization

### When to Use Virtualization
Enable virtualization for datasets with:
- **More than 500 tasks**
- **Complex task hierarchies**
- **Frequent data updates**
- **Resource-constrained environments**

### Virtualization Configuration
```csharp
var virtualizationService = new VirtualizationService();

// Check if virtualization should be enabled
bool shouldVirtualize = virtualizationService.ShouldVirtualize(
    itemCount: tasks.Count,
    maxVisibleItems: 1000
);

if (shouldVirtualize)
{
    config.Rendering.EnableVirtualization = true;
    config.Rendering.MaxVisibleTasks = 1000;
}
```

### Viewport Management
```csharp
// Update viewport for optimal rendering
var viewport = new VirtualizationViewport
{
    ViewportWidth = ganttContainer.ActualWidth,
    ViewportHeight = ganttContainer.ActualHeight,
    HorizontalOffset = scrollViewer.HorizontalOffset,
    VerticalOffset = scrollViewer.VerticalOffset
};

virtualizationService.UpdateViewport(viewport);

// Calculate visible range
var visibleRange = virtualizationService.CalculateVisibleRange(
    totalItems: tasks.Count,
    viewportSize: viewport.ViewportHeight,
    scrollOffset: viewport.VerticalOffset,
    itemSize: 30 // Average task height
);
```

## Memory Optimization

### Memory Optimization Strategies

#### Conservative Strategy
- Minimal impact on performance
- Gentle memory cleanup
- Suitable for continuous operation

```csharp
memoryService.OptimizeMemory(MemoryOptimizationStrategy.Conservative);
```

#### Balanced Strategy (Recommended)
- Good balance of performance and memory efficiency
- Moderate garbage collection
- Default strategy for most applications

```csharp
memoryService.OptimizeMemory(MemoryOptimizationStrategy.Balanced);
```

#### Aggressive Strategy
- Maximum memory reclamation
- May cause temporary performance impact
- Use during low-activity periods

```csharp
memoryService.OptimizeMemory(MemoryOptimizationStrategy.Aggressive);
```

### Automatic Memory Management
```csharp
// Configure automatic optimization
memoryService.EnableAutoOptimization(TimeSpan.FromMinutes(5));

// Monitor memory pressure
memoryService.MemoryPressureDetected += (sender, args) =>
{
    Console.WriteLine($"Memory pressure: {args.PressureLevel}");
    Console.WriteLine($"Current memory: {args.CurrentMemoryBytes / (1024 * 1024):F1} MB");
    Console.WriteLine($"Recommendation: {args.Recommendation}");
    
    // Auto-optimize on high pressure
    if (args.PressureLevel >= MemoryPressureLevel.High)
    {
        Task.Run(() => memoryService.OptimizeMemory(args.Recommendation));
    }
};
```

### Memory Usage Monitoring
```csharp
var usage = memoryService.GetMemoryUsage();
Console.WriteLine($"Managed Memory: {usage.ManagedMemoryBytes / (1024 * 1024):F1} MB");
Console.WriteLine($"Working Set: {usage.WorkingSetBytes / (1024 * 1024):F1} MB");
Console.WriteLine($"Private Memory: {usage.PrivateMemoryBytes / (1024 * 1024):F1} MB");
Console.WriteLine($"GC Collections (Gen 0): {usage.Gen0Collections}");
Console.WriteLine($"GC Collections (Gen 1): {usage.Gen1Collections}");
Console.WriteLine($"GC Collections (Gen 2): {usage.Gen2Collections}");
```

## Performance Monitoring

### Real-time Diagnostics
```csharp
var diagnostics = performanceService.GetDiagnostics();

// Start monitoring
diagnostics.StartMonitoring();

// Monitor specific operations
using var measurement = performanceService.BeginMeasurement("TaskDataLoad");
await LoadTaskDataAsync();
measurement.Dispose();

// Get current metrics
var metrics = diagnostics.GetCurrentMetrics();
Console.WriteLine($"CPU Usage: {metrics.CpuUsagePercent:F1}%");
Console.WriteLine($"Memory Usage: {metrics.ManagedMemoryBytes / (1024 * 1024):F1} MB");
Console.WriteLine($"Frame Rate: {metrics.FrameRate:F1} FPS");
```

### Performance Thresholds
```csharp
// Configure performance thresholds
diagnostics.ThresholdExceeded += (sender, args) =>
{
    Console.WriteLine($"Performance threshold exceeded:");
    Console.WriteLine($"Operation: {args.EventName}");
    Console.WriteLine($"Duration: {args.Duration.TotalMilliseconds:F1}ms");
    Console.WriteLine($"Threshold: {args.Threshold.TotalMilliseconds:F1}ms");
    
    // Take corrective action
    if (args.EventName == "LayoutBuild" && args.Duration.TotalMilliseconds > 100)
    {
        // Increase debounce delay to reduce layout frequency
        config.Rendering.LayoutDebounceDelay = Math.Min(500, config.Rendering.LayoutDebounceDelay + 50);
    }
};
```

### Performance Recommendations
```csharp
var recommendations = diagnostics.GetRecommendations();
foreach (var recommendation in recommendations)
{
    Console.WriteLine($"{recommendation.Severity}: {recommendation.Title}");
    Console.WriteLine($"Description: {recommendation.Description}");
    Console.WriteLine("Actions:");
    foreach (var action in recommendation.ActionItems)
    {
        Console.WriteLine($"- {action}");
    }
    Console.WriteLine();
}
```

## Timeline Optimization

### Timeline Caching
```csharp
// Cache timeline calculations for reuse
var timelineMetadata = performanceService.GetCachedTimelineMetadata(
    start: DateTime.Today,
    end: DateTime.Today.AddMonths(1),
    unit: TimeUnit.Day,
    culture: CultureInfo.CurrentCulture
);

// Cache timeline ticks
var ticks = performanceService.GetCachedTimelineTicks(
    start: DateTime.Today,
    end: DateTime.Today.AddMonths(1),
    unit: TimeUnit.Day,
    culture: CultureInfo.CurrentCulture
);
```

### Timeline Performance Tips
```csharp
// Use appropriate time units for the data range
var timeSpan = endTime - startTime;
TimeUnit optimalUnit;

if (timeSpan.TotalHours <= 48)
    optimalUnit = TimeUnit.Hour;
else if (timeSpan.TotalDays <= 90)
    optimalUnit = TimeUnit.Day;
else if (timeSpan.TotalDays <= 730)
    optimalUnit = TimeUnit.Week;
else if (timeSpan.TotalDays <= 1095)
    optimalUnit = TimeUnit.Month;
else
    optimalUnit = TimeUnit.Year;

ganttContainer.TimeUnit = optimalUnit;
```

## Rendering Optimization

### Performance Levels
```csharp
public enum PerformanceLevel
{
    Quality,      // Best visual quality, slower rendering
    Balanced,     // Balance of quality and performance
    Performance   // Best performance, reduced visual effects
}

// Configure based on requirements
config.Rendering.PerformanceLevel = PerformanceLevel.Performance;
```

### Grid Rendering Modes
```csharp
// Line mode - better performance for simple grids
config.Rendering.GridMode = GridRenderingMode.Lines;

// Rectangle mode - better visual quality, higher memory usage
config.Rendering.GridMode = GridRenderingMode.Rectangles;
```

### Shape Rendering Optimization
```csharp
// Disable enhanced shapes for better performance
config.Rendering.UseEnhancedShapeRendering = false;

// Or optimize shape parameters
var optimizedParams = new ShapeRenderingParameters
{
    DiamondEndWidth = 12,  // Smaller size for better performance
    CornerRadius = 2,      // Simpler corners
    ChevronAngle = 15      // Less complex angles
};
```

## Large Dataset Strategies

### Data Pagination
```csharp
public class PaginatedTaskLoader
{
    private const int PageSize = 1000;
    
    public async Task<IEnumerable<GanttTask>> LoadTaskPageAsync(int pageIndex)
    {
        var skip = pageIndex * PageSize;
        return await dataService.GetTasksAsync(skip, PageSize);
    }
    
    public async Task LoadVisibleTasksAsync(DateTime startTime, DateTime endTime)
    {
        var visibleTasks = await dataService.GetTasksByDateRangeAsync(startTime, endTime);
        
        // Update UI on main thread
        await Dispatcher.InvokeAsync(() =>
        {
            ganttContainer.Tasks.Clear();
            foreach (var task in visibleTasks)
            {
                ganttContainer.Tasks.Add(task);
            }
        });
    }
}
```

### Lazy Loading
```csharp
public class LazyTaskCollection : ObservableCollection<GanttTask>
{
    private readonly ITaskService _taskService;
    private bool _isLoaded = false;
    
    public async Task EnsureLoadedAsync()
    {
        if (!_isLoaded)
        {
            var tasks = await _taskService.GetTasksAsync();
            foreach (var task in tasks)
            {
                Add(task);
            }
            _isLoaded = true;
        }
    }
}
```

### Background Processing
```csharp
// Process data updates in background
private async Task UpdateTasksAsync(IEnumerable<GanttTask> newTasks)
{
    // Process data on background thread
    await Task.Run(() =>
    {
        var processedTasks = ProcessTaskData(newTasks);
        
        // Update UI on main thread
        Dispatcher.BeginInvoke(() =>
        {
            UpdateUI(processedTasks);
        });
    });
}
```

## Debugging Performance Issues

### Performance Profiling
```csharp
// Profile specific operations
private async Task ProfiledOperation(string operationName, Func<Task> operation)
{
    var stopwatch = Stopwatch.StartNew();
    var startMemory = GC.GetTotalMemory(false);
    
    try
    {
        await operation();
    }
    finally
    {
        stopwatch.Stop();
        var endMemory = GC.GetTotalMemory(false);
        var memoryDelta = endMemory - startMemory;
        
        Console.WriteLine($"Operation: {operationName}");
        Console.WriteLine($"Duration: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Memory Delta: {memoryDelta / 1024:F1} KB");
    }
}
```

### Visual Tree Analysis
```csharp
private void AnalyzeVisualTree()
{
    var elementCount = CountVisualChildren(ganttContainer);
    Console.WriteLine($"Total visual elements: {elementCount}");
    
    // Check for performance bottlenecks
    if (elementCount > 10000)
    {
        Console.WriteLine("Warning: High element count detected. Consider enabling virtualization.");
    }
}

private int CountVisualChildren(DependencyObject obj)
{
    int count = 1;
    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
    {
        count += CountVisualChildren(VisualTreeHelper.GetChild(obj, i));
    }
    return count;
}
```

## Performance Best Practices

### 1. Initialization
```csharp
// Initialize performance services early
var performanceService = new PerformanceService();
ganttContainer.PerformanceService = performanceService;

// Configure before adding data
ganttContainer.Configuration = config;
ganttContainer.Theme = theme;
// Add data last
ganttContainer.Tasks = tasks;
```

### 2. Data Binding
```csharp
// Use efficient collections
public ObservableCollection<GanttTask> Tasks { get; } = new();

// Batch updates
ganttContainer.BeginInit();
try
{
    Tasks.Clear();
    foreach (var task in newTasks)
    {
        Tasks.Add(task);
    }
}
finally
{
    ganttContainer.EndInit();
}
```

### 3. Memory Management
```csharp
// Dispose resources properly
public void Cleanup()
{
    performanceService?.GetDiagnostics()?.StopMonitoring();
    performanceService?.GetMemoryOptimization()?.DisableAutoOptimization();
    performanceService?.ClearAllCaches();
}
```

### 4. Error Handling
```csharp
// Handle performance degradation gracefully
try
{
    await LoadLargeDatasetAsync();
}
catch (OutOfMemoryException)
{
    // Fallback to smaller dataset
    await LoadReducedDatasetAsync();
    
    // Enable aggressive memory optimization
    memoryService.OptimizeMemory(MemoryOptimizationStrategy.Aggressive);
}
```

## Monitoring Dashboard Example

```csharp
public class PerformanceDashboard : UserControl
{
    private readonly IPerformanceService _performanceService;
    private readonly DispatcherTimer _updateTimer;
    
    public PerformanceDashboard(IPerformanceService performanceService)
    {
        _performanceService = performanceService;
        _updateTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _updateTimer.Tick += UpdateMetrics;
        _updateTimer.Start();
    }
    
    private void UpdateMetrics(object sender, EventArgs e)
    {
        var metrics = _performanceService.GetDiagnostics().GetCurrentMetrics();
        var memoryUsage = _performanceService.GetMemoryOptimization().GetMemoryUsage();
        
        // Update UI
        CpuUsageText.Text = $"CPU: {metrics.CpuUsagePercent:F1}%";
        MemoryUsageText.Text = $"Memory: {memoryUsage.ManagedMemoryBytes / (1024 * 1024):F1} MB";
        FrameRateText.Text = $"FPS: {metrics.FrameRate:F1}";
        
        // Update recommendations
        var recommendations = _performanceService.GetDiagnostics().GetRecommendations();
        RecommendationsPanel.Children.Clear();
        foreach (var rec in recommendations)
        {
            var textBlock = new TextBlock
            {
                Text = $"{rec.Severity}: {rec.Title}",
                Foreground = GetSeverityBrush(rec.Severity)
            };
            RecommendationsPanel.Children.Add(textBlock);
        }
    }
}
```

This comprehensive performance guide should help you optimize your GPM.Gantt applications for maximum efficiency and scalability.