# Getting Started with GPM.Gantt v2.1.0

This guide will help you get up and running with GPM.Gantt in your WPF application, including the new advanced features introduced in version 2.1.0.

## Installation

### Prerequisites
- .NET 9.0 or later
- Windows OS
- Visual Studio 2022 or compatible IDE with WPF support

### Adding GPM.Gantt to Your Project

#### Option 1: Project Reference (Current)
1. Clone or download the GPM.Gantt source code
2. Add a project reference to `GPM.Gantt.csproj` in your WPF application
3. Add the XML namespace to your XAML files

#### Option 2: NuGet Package (Coming Soon)
```bash
Install-Package GPM.Gantt
```

## Basic Setup

### 1. Add XML Namespace

Add the GPM.Gantt namespace to your XAML files:

```xml
<Window xmlns:gantt="clr-namespace:GPM.Gantt;assembly=GPM.Gantt"
        xmlns:models="clr-namespace:GPM.Gantt.Models;assembly=GPM.Gantt">
    <!-- Your content here -->
</Window>
```

### 2. Add the GanttContainer

The `GanttContainer` is the main control for displaying Gantt charts:

```xml
<gantt:GanttContainer x:Name="MyGanttChart"
                     StartTime="{Binding StartDate}"
                     EndTime="{Binding EndDate}"
                     Tasks="{Binding Tasks}"
                     TimeUnit="Day"
                     TaskCount="10" />
```

### 3. Create Your Data Model

Create a ViewModel that provides data to the Gantt chart:

```csharp
using System;
using System.Collections.ObjectModel;
using GPM.Gantt.Models;
using GPM.Gantt.ViewModels;

public class MainViewModel : ViewModelBase
{
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(14);
    public ObservableCollection<GanttTask> Tasks { get; } = new();

    public MainViewModel()
    {
        LoadSampleTasks();
    }

    private void LoadSampleTasks()
    {
        Tasks.Add(new GanttTask
        {
            Title = "Project Planning",
            Start = DateTime.Today,
            End = DateTime.Today.AddDays(3),
            RowIndex = 1,
            Progress = 50,
            Status = TaskStatus.InProgress
        });

        Tasks.Add(new GanttTask
        {
            Title = "Design Phase",
            Start = DateTime.Today.AddDays(2),
            End = DateTime.Today.AddDays(8),
            RowIndex = 2,
            Progress = 25,
            Status = TaskStatus.InProgress
        });

        Tasks.Add(new GanttTask
        {
            Title = "Development",
            Start = DateTime.Today.AddDays(7),
            End = DateTime.Today.AddDays(14),
            RowIndex = 3,
            Progress = 0,
            Status = TaskStatus.NotStarted
        });
    }
}
```

### 4. Set the DataContext

In your Window's code-behind or through dependency injection:

```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
```

## Understanding Key Properties

### GanttContainer Properties

| Property | Type | Description |
|----------|------|-------------|
| `StartTime` | `DateTime` | The start of the visible timeline |
| `EndTime` | `DateTime` | The end of the visible timeline |
| `Tasks` | `ObservableCollection<GanttTask>` | Collection of tasks to display |
| `TimeUnit` | `TimeUnit` | Time scale (Hour, Day, Week, Month, Year) |
| `TaskCount` | `int` | Number of task rows to display |
| `ShowGridCells` | `bool` | Whether to show individual grid cells |
| `DateFormat` | `string` | Custom date format string |
| `TimeFormat` | `string` | Custom time format string |
| `Culture` | `CultureInfo` | Culture for formatting |

### GanttTask Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `Guid` | Unique task identifier |
| `Title` | `string` | Task name/title |
| `Start` | `DateTime` | Task start date/time |
| `End` | `DateTime` | Task end date/time |
| `RowIndex` | `int` | Display row (1-based) |
| `Progress` | `double` | Completion percentage (0-100) |
| `Priority` | `TaskPriority` | Task priority level |
| `Status` | `TaskStatus` | Current task status |
| `Dependencies` | `List<Guid>` | List of dependent task IDs |

## Basic Styling

You can style the Gantt chart components using standard WPF styling:

```xml
<Window.Resources>
    <!-- Time cell styling -->
    <Style TargetType="{x:Type gantt:GanttTimeCell}">
        <Setter Property="Background" Value="#F0F0F0"/>
        <Setter Property="FontWeight" Value="Bold"/>
    </Style>
    
    <!-- Grid cell styling -->
    <Style TargetType="{x:Type gantt:GanttGridCell}">
        <Setter Property="Background" Value="White"/>
        <Setter Property="OutlineBrush" Value="LightGray"/>
    </Style>
    
    <!-- Task bar styling -->
    <Style TargetType="{x:Type gantt:GanttTaskBar}">
        <Setter Property="Background" Value="#2196F3"/>
    </Style>
</Window.Resources>
```

## Using with MVVM

For full MVVM support, use the provided ViewModels:

```csharp
using GPM.Gantt.ViewModels;
using GPM.Gantt.Services;

public class ProjectViewModel : ViewModelBase
{
    private readonly GanttChartViewModel _ganttViewModel;

    public ProjectViewModel()
    {
        var validationService = new ValidationService();
        var ganttService = new GanttService(validationService);
        
        _ganttViewModel = new GanttChartViewModel(validationService, ganttService);
        _ganttViewModel.ProjectId = Guid.NewGuid(); // Set your project ID
    }

    public GanttChartViewModel GanttChart => _ganttViewModel;
}
```

## Time Scale Management

Change the time scale dynamically:

```csharp
// Switch between different time units
ganttChart.TimeUnit = TimeUnit.Week;  // Week view
ganttChart.TimeUnit = TimeUnit.Month; // Month view
ganttChart.TimeUnit = TimeUnit.Day;   // Day view (default)
```

## Validation

GPM.Gantt includes built-in validation:

```csharp
var task = new GanttTask
{
    Title = "Invalid Task",
    Start = DateTime.Today,
    End = DateTime.Today.AddDays(-1) // End before start - invalid!
};

var errors = task.GetValidationErrors();
if (errors.Any())
{
    Console.WriteLine($"Validation errors: {string.Join(", ", errors)}");
}
```

## Next Steps

- [API Reference](api-reference.md) - Detailed API documentation
- [Configuration Guide](configuration.md) - Advanced configuration options
- [MVVM Integration](mvvm-integration.md) - Deep dive into MVVM patterns
- [Examples](examples.md) - More complex examples and use cases

## Common Issues

### Tasks Not Showing
- Ensure `TaskCount` is sufficient for your `RowIndex` values
- Verify that task dates fall within `StartTime` and `EndTime`
- Check that the `Tasks` collection is properly bound

### Styling Not Applied
- Make sure style `TargetType` matches the exact control type
- Verify that styles are defined in the correct resource scope
- Check for conflicting styles or property setters

### Performance Issues
- For large datasets, consider implementing virtualization
- Use appropriate `TimeUnit` for your data granularity
- Limit the number of visible tasks with `TaskCount`

## Advanced Features (v2.1.0)

GPM.Gantt v2.1.0 introduces powerful advanced features for enhanced project visualization:

### Plugin-Based Annotations

Add rich annotations to your Gantt charts:

```csharp
// Register annotation plugins
var pluginService = new PluginService();
plugginService.RegisterPlugin(new TextAnnotationPlugin());
plugginService.RegisterPlugin(new ShapeAnnotationPlugin());
plugginService.RegisterPlugin(new LineAnnotationPlugin());

// Create text annotation
var textConfig = new TextAnnotationConfig
{
    Text = "Critical Milestone",
    FontSize = 14,
    Color = "#FF0000",
    X = 100, Y = 50
};
```

### Multi-Level Time Scale

Display multiple time granularities simultaneously:

```csharp
var multiLevelConfig = new MultiLevelTimeScaleConfiguration
{
    Levels = new List<TimeLevelConfiguration>
    {
        new() { Unit = ExtendedTimeUnit.Year, Height = 35 },
        new() { Unit = ExtendedTimeUnit.Month, Height = 25 },
        new() { Unit = ExtendedTimeUnit.Week, Height = 20 }
    },
    EnableSmartVisibility = true
};

ganttContainer.MultiLevelTimeScale = multiLevelConfig;
```

### Expandable Time Segments

Interactively expand specific time periods for detailed exploration:

```csharp
var expansion = new TimeSegmentExpansion
{
    StartTime = DateTime.Today.AddDays(14),
    EndTime = DateTime.Today.AddDays(21),
    OriginalUnit = ExtendedTimeUnit.Week,
    ExpandedUnit = ExtendedTimeUnit.Day,
    DisplayName = "Critical Week"
};

ganttContainer.TimeSegmentExpansions.Add(expansion);
```

## Next Steps

1. **Explore Examples**: Check out the comprehensive examples in `docs/examples.md`
2. **API Reference**: Review the detailed API documentation in `docs/api-reference.md`
3. **Advanced Configuration**: Learn about performance optimization and advanced theming
4. **Plugin Development**: Create custom annotation plugins for specialized needs

For more detailed information and advanced scenarios, see the complete documentation in the `docs/` folder.