# GPM.Gantt Troubleshooting Guide

This comprehensive guide helps you diagnose and resolve common issues when working with the GPM.Gantt component library.

## Common Issues

### 1. Theme and Resource Issues

#### Issue: BorderThickness Resource Conversion Error
**Symptoms:**
- Exception: "Empty string is not a valid value for BorderThickness"
- Visual elements not rendering correctly
- Theme application failures

**Solution:**
Ensure you're using the latest version (v2.0.1+) which includes the BorderThickness fix:

```csharp
// Verify theme resources are properly configured
var theme = ThemeManager.GetTheme("Default");
ganttContainer.Theme = theme;

// Check if resources are correctly applied
if (ganttContainer.Resources.Contains("GanttGridLineThickness"))
{
    var thickness = ganttContainer.Resources["GanttGridLineThickness"];
    System.Diagnostics.Debug.WriteLine($"Thickness type: {thickness?.GetType()}");
}
```

#### Issue: Theme Not Applied
**Symptoms:**
- Default styling instead of custom theme
- Theme changes not reflected in UI

**Solution:**
```csharp
// Ensure theme is set after container is loaded
ganttContainer.Loaded += (s, e) =>
{
    ganttContainer.Theme = ThemeManager.GetTheme("Dark");
};

// Or force theme refresh
if (ganttContainer.IsLoaded)
{
    var currentTheme = ganttContainer.Theme;
    ganttContainer.Theme = null;
    ganttContainer.Theme = currentTheme;
}
```

### 2. Performance Issues

#### Issue: Slow Rendering with Large Datasets
**Symptoms:**
- UI freezing during task loading
- High memory usage
- Slow scrolling performance

**Solution:**
```csharp
// Enable virtualization
var config = new GanttConfiguration
{
    Rendering = new RenderingConfiguration
    {
        EnableVirtualization = true,
        MaxVisibleTasks = 1000,
        PerformanceLevel = PerformanceLevel.Performance,
        EnableAutoMemoryOptimization = true
    }
};
ganttContainer.Configuration = config;

// Monitor performance
var performanceService = new PerformanceService();
performanceService.GetDiagnostics().StartMonitoring();

// Get recommendations
var recommendations = performanceService.GetDiagnostics().GetRecommendations();
foreach (var rec in recommendations)
{
    Console.WriteLine($"{rec.Title}: {rec.Description}");
}
```

#### Issue: Memory Leaks
**Symptoms:**
- Gradually increasing memory usage
- Out of memory exceptions with large datasets

**Solution:**
```csharp
// Enable automatic memory optimization
var memoryService = performanceService.GetMemoryOptimization();
memoryService.EnableAutoOptimization(TimeSpan.FromMinutes(5));
memoryService.StartMemoryPressureMonitoring();

// Manual memory optimization
memoryService.OptimizeMemory(MemoryOptimizationStrategy.Aggressive);

// Monitor memory usage
var usage = memoryService.GetMemoryUsage();
Console.WriteLine($"Memory: {usage.ManagedMemoryBytes / (1024 * 1024):F1} MB");
```

### 3. Data Binding Issues

#### Issue: Tasks Not Displaying
**Symptoms:**
- Empty Gantt chart despite having task data
- Tasks collection bound but not visible

**Solution:**
```csharp
// Ensure proper data binding
public ObservableCollection<GanttTask> Tasks { get; } = new();

// Verify task properties
foreach (var task in Tasks)
{
    System.Diagnostics.Debug.WriteLine($"Task: {task.Title}, Start: {task.Start}, End: {task.End}, Row: {task.RowIndex}");
    
    // Validate task data
    if (task.Start >= task.End && task.Shape != TaskBarShape.Milestone)
    {
        Console.WriteLine($"Warning: Task '{task.Title}' has invalid date range");
    }
    
    if (task.RowIndex < 1)
    {
        Console.WriteLine($"Warning: Task '{task.Title}' has invalid row index: {task.RowIndex}");
    }
}

// Force layout refresh
ganttContainer.InvalidateLayout();
```

#### Issue: Task Updates Not Reflected
**Symptoms:**
- Changes to task properties not visible in UI
- Bindings appear broken

**Solution:**
```csharp
// Ensure tasks implement INotifyPropertyChanged
public class GanttTask : INotifyPropertyChanged
{
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged();
            }
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Use GanttTaskViewModel for better MVVM support
var taskViewModel = new GanttTaskViewModel(task);
Tasks.Add(taskViewModel);
```

### 4. Time Range and Calculation Issues

#### Issue: Incorrect Time Scale
**Symptoms:**
- Time labels don't match expectations
- Grid alignment issues

**Solution:**
```csharp
// Verify time range configuration
ganttContainer.StartTime = new DateTime(2025, 1, 1);
ganttContainer.EndTime = new DateTime(2025, 12, 31);
ganttContainer.TimeUnit = TimeUnit.Day;

// Check culture settings
ganttContainer.Culture = new CultureInfo("en-US");
ganttContainer.DateFormat = "MMM dd";
ganttContainer.TimeFormat = "HH:mm";

// Validate time span
var timeSpan = ganttContainer.EndTime - ganttContainer.StartTime;
if (timeSpan.TotalDays < 1)
{
    Console.WriteLine("Warning: Time range too small, consider using Hour unit");
}
```

#### Issue: Tasks Outside Visible Range
**Symptoms:**
- Tasks not showing despite being in collection
- Partial task visibility

**Solution:**
```csharp
// Ensure tasks are within visible time range
foreach (var task in Tasks)
{
    if (task.End < ganttContainer.StartTime || task.Start > ganttContainer.EndTime)
    {
        Console.WriteLine($"Task '{task.Title}' is outside visible range");
        
        // Option 1: Adjust task dates
        task.Start = Math.Max(task.Start.Ticks, ganttContainer.StartTime.Ticks);
        task.End = Math.Min(task.End.Ticks, ganttContainer.EndTime.Ticks);
        
        // Option 2: Expand time range
        ganttContainer.StartTime = DateTime.Min(ganttContainer.StartTime, task.Start);
        ganttContainer.EndTime = DateTime.Max(ganttContainer.EndTime, task.End);
    }
}
```

### 5. Custom Shape Issues

#### Issue: Custom Shapes Not Rendering
**Symptoms:**
- Tasks showing as rectangles despite custom shape settings
- Shape parameters ignored

**Solution:**
```csharp
// Enable enhanced shape rendering
var config = new GanttConfiguration
{
    Rendering = new RenderingConfiguration
    {
        UseEnhancedShapeRendering = true,
        DefaultTaskBarShape = TaskBarShape.Rectangle
    }
};
ganttContainer.Configuration = config;

// Ensure proper shape configuration
var task = new GanttTask
{
    Title = "Custom Shape Task",
    Shape = TaskBarShape.DiamondEnds,
    ShapeParameters = new ShapeRenderingParameters
    {
        DiamondEndHeight = 0.8,
        DiamondEndWidth = 15
    }
};

// Verify shape renderer is available
var renderer = TaskBarShapeRendererFactory.GetRenderer(TaskBarShape.DiamondEnds);
if (renderer == null)
{
    Console.WriteLine("Shape renderer not found, falling back to rectangle");
}
```

### 6. Validation Errors

#### Issue: Task Validation Failures
**Symptoms:**
- Tasks rejected during addition
- Validation error messages

**Solution:**
```csharp
// Use validation service
var validationService = new ValidationService();
var result = validationService.ValidateTask(task);

if (!result.IsValid)
{
    Console.WriteLine("Task validation failed:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"- {error}");
    }
    
    // Fix common validation issues
    if (task.Start >= task.End && task.Shape != TaskBarShape.Milestone)
    {
        task.End = task.Start.AddDays(1); // Set minimum duration
    }
    
    if (string.IsNullOrWhiteSpace(task.Title))
    {
        task.Title = "Untitled Task";
    }
    
    if (task.RowIndex < 1)
    {
        task.RowIndex = 1;
    }
}
```

## Debugging Tips

### 1. Enable Debug Output
```csharp
// Enable debug logging
System.Diagnostics.Debug.WriteLine("Gantt debug mode enabled");

// Monitor layout building
ganttContainer.LayoutInvalidated += (s, e) =>
{
    System.Diagnostics.Debug.WriteLine("Layout invalidated");
};

// Track performance
using var measurement = performanceService.BeginMeasurement("TaskOperation");
// Your code here
```

### 2. Visual Tree Inspection
```csharp
// Inspect visual tree
private void InspectVisualTree(DependencyObject obj, int level = 0)
{
    var indent = new string(' ', level * 2);
    System.Diagnostics.Debug.WriteLine($"{indent}{obj.GetType().Name}");
    
    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
    {
        var child = VisualTreeHelper.GetChild(obj, i);
        InspectVisualTree(child, level + 1);
    }
}

// Usage
InspectVisualTree(ganttContainer);
```

### 3. Performance Monitoring
```csharp
// Set up performance monitoring
var diagnostics = performanceService.GetDiagnostics();
diagnostics.ThresholdExceeded += (s, e) =>
{
    Console.WriteLine($"Performance threshold exceeded: {e.EventName} took {e.Duration.TotalMilliseconds:F1}ms");
};

// Get current metrics
var metrics = diagnostics.GetCurrentMetrics();
Console.WriteLine($"CPU: {metrics.CpuUsagePercent:F1}%");
Console.WriteLine($"Memory: {metrics.ManagedMemoryBytes / (1024 * 1024):F1} MB");
```

## Best Practices

### 1. Initialization Order
```csharp
// Correct initialization order
ganttContainer.Configuration = config;  // 1. Set configuration first
ganttContainer.Theme = theme;           // 2. Apply theme
ganttContainer.Tasks = tasks;           // 3. Set data last
```

### 2. Error Handling
```csharp
try
{
    ganttContainer.Tasks = tasks;
}
catch (Exception ex)
{
    System.Diagnostics.Debug.WriteLine($"Error setting tasks: {ex.Message}");
    
    // Fallback to empty collection
    ganttContainer.Tasks = new ObservableCollection<GanttTask>();
    
    // Notify user
    MessageBox.Show($"Error loading tasks: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
}
```

### 3. Performance Optimization
```csharp
// Batch updates
ganttContainer.BeginInit();
try
{
    ganttContainer.Tasks.Clear();
    foreach (var task in newTasks)
    {
        ganttContainer.Tasks.Add(task);
    }
}
finally
{
    ganttContainer.EndInit();
}
```

## Getting Help

### 1. Enable Diagnostics
```csharp
// Enable comprehensive diagnostics
var diagnostics = performanceService.GetDiagnostics();
diagnostics.StartMonitoring();

// Generate diagnostic report
var report = diagnostics.GenerateReport();
Console.WriteLine(report);
```

### 2. Collect System Information
```csharp
// System information for support
Console.WriteLine($".NET Version: {Environment.Version}");
Console.WriteLine($"OS: {Environment.OSVersion}");
Console.WriteLine($"Memory: {GC.GetTotalMemory(false) / (1024 * 1024):F1} MB");
Console.WriteLine($"GPM.Gantt Version: {typeof(GanttContainer).Assembly.GetName().Version}");
```

### 3. Create Minimal Reproduction
When reporting issues, create a minimal example:

```csharp
public partial class MinimalExample : Window
{
    public MinimalExample()
    {
        InitializeComponent();
        
        var gantt = new GanttContainer
        {
            StartTime = DateTime.Today,
            EndTime = DateTime.Today.AddDays(7),
            Tasks = new ObservableCollection<GanttTask>
            {
                new GanttTask
                {
                    Title = "Test Task",
                    Start = DateTime.Today,
                    End = DateTime.Today.AddDays(2),
                    RowIndex = 1
                }
            }
        };
        
        Content = gantt;
    }
}
```

This troubleshooting guide should help you resolve most common issues with the GPM.Gantt component. For additional support, refer to the project documentation or create an issue on GitHub.