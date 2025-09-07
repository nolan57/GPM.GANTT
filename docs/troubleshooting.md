# Troubleshooting Guide - GPM.Gantt

This guide helps you diagnose and resolve common issues when working with GPM.Gantt.

## Common Issues and Solutions

### 1. Tasks Not Displaying

#### Problem
Tasks are not visible in the Gantt chart despite being added to the collection.

#### Possible Causes and Solutions

**Check TaskCount Property**
```csharp
// Ensure TaskCount is sufficient for your tasks' RowIndex values
ganttContainer.TaskCount = 20; // Increase if you have tasks with RowIndex > current value
```

**Verify Time Range**
```csharp
// Ensure tasks fall within the visible time range
var minTaskStart = tasks.Min(t => t.Start);
var maxTaskEnd = tasks.Max(t => t.End);

ganttContainer.StartTime = minTaskStart.Date;
ganttContainer.EndTime = maxTaskEnd.Date.AddDays(1);
```

**Check RowIndex Values**
```csharp
// Ensure RowIndex is within valid range (1-based)
foreach (var task in tasks)
{
    if (task.RowIndex < 1 || task.RowIndex > ganttContainer.TaskCount)
    {
        Console.WriteLine($"Task '{task.Title}' has invalid RowIndex: {task.RowIndex}");
        task.RowIndex = Math.Max(1, Math.Min(ganttContainer.TaskCount, task.RowIndex));
    }
}
```

**Verify Collection Binding**
```csharp
// Check if the Tasks property is properly bound
if (ganttContainer.Tasks == null)
{
    ganttContainer.Tasks = new ObservableCollection<GanttTask>();
}

// Ensure collection implements INotifyCollectionChanged
var collection = ganttContainer.Tasks as ObservableCollection<GanttTask>;
if (collection == null)
{
    ganttContainer.Tasks = new ObservableCollection<GanttTask>(existingTasks);
}
```

### 2. Layout Issues

#### Problem
Grid layout appears incorrect, overlapping elements, or misaligned components.

#### Solutions

**Force Layout Rebuild**
```csharp
// Trigger layout recalculation
ganttContainer.InvalidateVisual();
ganttContainer.UpdateLayout();

// Or recreate the container
var parent = ganttContainer.Parent as Panel;
if (parent != null)
{
    parent.Children.Remove(ganttContainer);
    parent.Children.Add(new GanttContainer { /* copy properties */ });
}
```

**Check Container Size**
```csharp
// Ensure container has valid size
if (ganttContainer.ActualWidth == 0 || ganttContainer.ActualHeight == 0)
{
    ganttContainer.Width = 800;
    ganttContainer.Height = 600;
    // Or ensure parent container provides size
}
```

**Verify Row/Column Definitions**
```csharp
// Check if grid definitions are being created
Console.WriteLine($"Rows: {ganttContainer.RowDefinitions.Count}");
Console.WriteLine($"Columns: {ganttContainer.ColumnDefinitions.Count}");

// If zero, check time range and task count
if (ganttContainer.ColumnDefinitions.Count == 0)
{
    // Time range might be invalid
    ganttContainer.EndTime = ganttContainer.StartTime.AddDays(7);
}
```

### 3. Performance Issues

#### Problem
Slow rendering, UI freezing, or high memory usage with large datasets.

#### Solutions

**Enable Virtualization**
```csharp
ganttContainer.Configuration = new GanttConfiguration
{
    Rendering = new RenderingConfiguration
    {
        EnableVirtualization = true,
        MaxVisibleTasks = 500, // Adjust based on performance
        EnableCaching = true
    }
};
```

**Limit Visible Tasks**
```csharp
// Show only relevant tasks
var visibleTasks = allTasks
    .Where(t => t.End >= ganttContainer.StartTime && 
                t.Start <= ganttContainer.EndTime)
    .Take(100) // Limit number
    .ToList();

ganttContainer.Tasks = new ObservableCollection<GanttTask>(visibleTasks);
```

**Optimize Time Unit**
```csharp
// Use appropriate time unit for your data
var timeRange = ganttContainer.EndTime - ganttContainer.StartTime;
ganttContainer.TimeUnit = timeRange.TotalDays switch
{
    <= 30 => TimeUnit.Day,
    <= 365 => TimeUnit.Week,
    _ => TimeUnit.Month
};
```

**Reduce UI Updates**
```csharp
// Batch updates to avoid frequent redraws
ganttContainer.Tasks.Clear();
foreach (var task in newTasks)
{
    ganttContainer.Tasks.Add(task);
}
// Consider using BeginUpdate/EndUpdate pattern if implemented
```

### 4. Data Binding Issues

#### Problem
Property changes not reflecting in UI or binding errors.

#### Solutions

**Verify INotifyPropertyChanged**
```csharp
// Ensure ViewModels implement INotifyPropertyChanged
public class MyGanttViewModel : INotifyPropertyChanged
{
    private DateTime _startTime;
    public DateTime StartTime
    {
        get => _startTime;
        set
        {
            _startTime = value;
            OnPropertyChanged(); // Make sure this is called
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

**Check Binding Expressions**
```xml
<!-- Ensure binding paths are correct -->
<gantt:GanttContainer StartTime="{Binding GanttChart.StartTime, Mode=TwoWay}"
                     EndTime="{Binding GanttChart.EndTime, Mode=TwoWay}"
                     Tasks="{Binding GanttChart.TaskModels}" />

<!-- Enable binding error debugging -->
<gantt:GanttContainer StartTime="{Binding StartTime, 
                     ValidatesOnDataErrors=True,
                     ValidatesOnExceptions=True}" />
```

**Debug Binding Errors**
```csharp
// Add binding error handler to see issues
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Listen for binding errors
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
        PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
    }
}
```

### 5. Validation Issues

#### Problem
Tasks appear to be valid but validation fails, or invalid tasks are accepted.

#### Solutions

**Check Validation Rules**
```csharp
// Verify built-in validation
var task = new GanttTask
{
    Title = "Test Task",
    Start = DateTime.Today,
    End = DateTime.Today.AddDays(-1), // Invalid: end before start
    RowIndex = 1
};

var errors = task.GetValidationErrors();
foreach (var error in errors)
{
    Console.WriteLine($"Validation Error: {error}");
}
```

**Custom Validation Service**
```csharp
// Implement custom validation if needed
public class DebugValidationService : IValidationService
{
    public ValidationResult ValidateTask(GanttTask task)
    {
        Console.WriteLine($"Validating task: {task.Title}");
        
        var result = new ValidationService().ValidateTask(task);
        
        Console.WriteLine($"Validation result: {result.IsValid}");
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  Error: {error}");
            }
        }
        
        return result;
    }
    
    public ValidationResult ValidateTaskCollection(IEnumerable<GanttTask> tasks)
    {
        Console.WriteLine($"Validating {tasks.Count()} tasks");
        return new ValidationService().ValidateTaskCollection(tasks);
    }
}
```

### 6. Culture and Formatting Issues

#### Problem
Dates/times display incorrectly or in wrong format for user's locale.

#### Solutions

**Set Explicit Culture**
```csharp
// Set specific culture
ganttContainer.Culture = new CultureInfo("en-US");

// Or use system culture
ganttContainer.Culture = CultureInfo.CurrentCulture;

// For UI culture
ganttContainer.Culture = CultureInfo.CurrentUICulture;
```

**Verify Format Strings**
```csharp
// Test format strings with sample data
var testDate = new DateTime(2024, 1, 15, 14, 30, 0);
var culture = new CultureInfo("de-DE");

try
{
    var formatted = testDate.ToString("MMM dd", culture);
    Console.WriteLine($"Formatted date: {formatted}");
}
catch (FormatException ex)
{
    Console.WriteLine($"Invalid format string: {ex.Message}");
}
```

**Handle Culture-Specific Issues**
```csharp
// Account for different first day of week
var culture = new CultureInfo("en-US");
culture.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Monday;
ganttContainer.Culture = culture;

// Handle right-to-left languages
ganttContainer.FlowDirection = culture.TextInfo.IsRightToLeft ? 
    FlowDirection.RightToLeft : FlowDirection.LeftToRight;
```

### 7. Memory Leaks

#### Problem
Application memory usage increases over time, especially with frequent updates.

#### Solutions

**Proper Event Cleanup**
```csharp
public class MyGanttViewModel : ViewModelBase, IDisposable
{
    private readonly GanttChartViewModel _ganttChart;
    
    public MyGanttViewModel()
    {
        _ganttChart = new GanttChartViewModel();
        
        // Subscribe to events
        _ganttChart.Tasks.CollectionChanged += OnTasksChanged;
    }
    
    public void Dispose()
    {
        // Unsubscribe from events
        _ganttChart.Tasks.CollectionChanged -= OnTasksChanged;
        
        // Dispose ViewModels if they implement IDisposable
        if (_ganttChart is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
    
    private void OnTasksChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Handle collection changes
    }
}
```

**Avoid Circular References**
```csharp
// Use weak references for event handlers
public class WeakEventManager
{
    public static void AddHandler(INotifyPropertyChanged source, 
        PropertyChangedEventHandler handler)
    {
        // Use WeakEventManager.SetCurrentManager or similar pattern
        PropertyChangedEventManager.AddHandler(source, handler, string.Empty);
    }
}
```

## Diagnostic Tools

### Enable Debug Output

```csharp
// Add to App.xaml.cs or main window constructor
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        #if DEBUG
        // Enable WPF binding debugging
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
        PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
        
        // Enable resource debugging
        PresentationTraceSources.ResourceDictionarySource.Switch.Level = SourceLevels.Critical;
        PresentationTraceSources.ResourceDictionarySource.Listeners.Add(new ConsoleTraceListener());
        #endif
    }
}
```

### Performance Monitoring

```csharp
public class PerformanceMonitor
{
    private readonly Stopwatch _stopwatch = new();
    
    public void StartMeasure(string operation)
    {
        Console.WriteLine($"Starting {operation}...");
        _stopwatch.Restart();
    }
    
    public void EndMeasure(string operation)
    {
        _stopwatch.Stop();
        Console.WriteLine($"{operation} completed in {_stopwatch.ElapsedMilliseconds}ms");
    }
    
    public static void MonitorMemory()
    {
        var before = GC.GetTotalMemory(false);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var after = GC.GetTotalMemory(false);
        
        Console.WriteLine($"Memory: {before / 1024 / 1024:F2}MB -> {after / 1024 / 1024:F2}MB");
    }
}

// Usage
var monitor = new PerformanceMonitor();
monitor.StartMeasure("Loading Tasks");
await LoadTasksAsync();
monitor.EndMeasure("Loading Tasks");
PerformanceMonitor.MonitorMemory();
```

### Layout Debugging

```csharp
public static class LayoutDebugger
{
    public static void DumpVisualTree(DependencyObject obj, int depth = 0)
    {
        var indent = new string(' ', depth * 2);
        Console.WriteLine($"{indent}{obj.GetType().Name}");
        
        if (obj is FrameworkElement fe)
        {
            Console.WriteLine($"{indent}  Size: {fe.ActualWidth} x {fe.ActualHeight}");
            Console.WriteLine($"{indent}  Margin: {fe.Margin}");
            Console.WriteLine($"{indent}  Visibility: {fe.Visibility}");
        }
        
        var childCount = VisualTreeHelper.GetChildrenCount(obj);
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(obj, i);
            DumpVisualTree(child, depth + 1);
        }
    }
    
    public static void HighlightElement(FrameworkElement element)
    {
        var originalBrush = element.Background;
        element.Background = Brushes.Red;
        
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        timer.Tick += (s, e) =>
        {
            element.Background = originalBrush;
            timer.Stop();
        };
        timer.Start();
    }
}
```

## Error Reporting

### Structured Error Information

```csharp
public class GanttDiagnostics
{
    public static string GenerateDiagnosticReport(GanttContainer container)
    {
        var report = new StringBuilder();
        
        report.AppendLine("=== GPM.Gantt Diagnostic Report ===");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();
        
        // Container properties
        report.AppendLine("Container Properties:");
        report.AppendLine($"  StartTime: {container.StartTime:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine($"  EndTime: {container.EndTime:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine($"  TimeUnit: {container.TimeUnit}");
        report.AppendLine($"  TaskCount: {container.TaskCount}");
        report.AppendLine($"  ShowGridCells: {container.ShowGridCells}");
        report.AppendLine($"  ActualSize: {container.ActualWidth} x {container.ActualHeight}");
        report.AppendLine();
        
        // Layout information
        report.AppendLine("Layout Information:");
        report.AppendLine($"  Rows: {container.RowDefinitions.Count}");
        report.AppendLine($"  Columns: {container.ColumnDefinitions.Count}");
        report.AppendLine($"  Children: {container.Children.Count}");
        report.AppendLine();
        
        // Tasks information
        report.AppendLine("Tasks Information:");
        if (container.Tasks != null)
        {
            report.AppendLine($"  Total Tasks: {container.Tasks.Count}");
            
            var visibleTasks = container.Tasks.Where(t => 
                t.End >= container.StartTime && t.Start <= container.EndTime).ToList();
            report.AppendLine($"  Visible Tasks: {visibleTasks.Count}");
            
            var invalidTasks = container.Tasks.Where(t => !t.IsValid()).ToList();
            report.AppendLine($"  Invalid Tasks: {invalidTasks.Count}");
            
            if (invalidTasks.Any())
            {
                report.AppendLine("  Invalid Task Details:");
                foreach (var task in invalidTasks)
                {
                    report.AppendLine($"    - {task.Title}: {string.Join(", ", task.GetValidationErrors())}");
                }
            }
        }
        else
        {
            report.AppendLine("  Tasks collection is null");
        }
        
        return report.ToString();
    }
}
```

## Getting Help

### Before Reporting Issues

1. **Check this troubleshooting guide** for known solutions
2. **Update to the latest version** of GPM.Gantt
3. **Create a minimal reproduction case** that demonstrates the issue
4. **Gather diagnostic information** using the tools above

### When Reporting Issues

Include the following information:

1. **Version Information**
   - GPM.Gantt version
   - .NET version
   - Operating system

2. **Code Sample**
   - Minimal reproducible example
   - XAML markup if applicable
   - Relevant configuration

3. **Error Details**
   - Exception messages and stack traces
   - Binding errors from output window
   - Diagnostic report output

4. **Expected vs Actual Behavior**
   - What you expected to happen
   - What actually happened
   - Screenshots if visual issues

### Community Resources

- **Documentation**: Check all documentation files for detailed guidance
- **Sample Code**: Review examples in the documentation
- **Source Code**: Examine the GPM.Gantt source for implementation details

### Temporary Workarounds

If you encounter a blocking issue, consider these temporary workarounds:

1. **Simplify the scenario** to isolate the problem
2. **Use alternative approaches** (e.g., different time units, smaller datasets)
3. **Implement custom validation** if built-in validation is insufficient
4. **Create wrapper components** to encapsulate problematic functionality
5. **Use timer-based updates** instead of real-time binding for performance issues