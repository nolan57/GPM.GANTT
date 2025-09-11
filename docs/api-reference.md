# API Reference - GPM.Gantt v2.1.0

This document provides comprehensive API documentation for the GPM.Gantt library, including the new advanced features introduced in version 2.1.0.

## Core Components

### GanttContainer

The main control for displaying Gantt charts.

```csharp
public class GanttContainer : Grid
```

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `StartTime` | `DateTime` | `DateTime.Today` | Start of the visible timeline |
| `EndTime` | `DateTime` | `DateTime.Today.AddDays(6)` | End of the visible timeline |
| `TaskCount` | `int` | `3` | Number of task rows to display |
| `Tasks` | `ObservableCollection<GanttTask>` | `null` | Collection of tasks to render |
| `TimeUnit` | `TimeUnit` | `TimeUnit.Day` | Time scale unit |
| `ShowGridCells` | `bool` | `false` | Show individual grid cells |
| `ClampTasksToVisibleRows` | `bool` | `false` | Clamp tasks to visible row range |
| `HeaderRowHeight` | `GridLength` | `GridLength.Auto` | Height of header row |
| `TaskRowHeight` | `GridLength` | `GridLength.Auto` | Height of task rows |
| `Configuration` | `GanttConfiguration` | `GanttConfiguration.Default()` | Configuration settings |
| `DateFormat` | `string` | `"MMM dd"` | Date format string |
| `TimeFormat` | `string` | `"HH:mm"` | Time format string |
| `Culture` | `CultureInfo` | `CultureInfo.CurrentCulture` | Culture for formatting |

#### Methods

The `GanttContainer` automatically manages its layout and doesn't expose public methods for direct manipulation.

---

## Models

### GanttTask

Represents a task in the Gantt chart.

```csharp
public class GanttTask
```

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Id` | `Guid` | `Guid.NewGuid()` | Unique task identifier |
| `Start` | `DateTime` | - | Task start date/time |
| `End` | `DateTime` | - | Task end date/time |
| `RowIndex` | `int` | `1` | Display row (1-based) |
| `Title` | `string` | `string.Empty` | Task title/name |
| `Description` | `string?` | `null` | Task description |
| `Progress` | `double` | `0` | Completion percentage (0-100) |
| `Priority` | `TaskPriority` | `TaskPriority.Normal` | Task priority |
| `ParentTaskId` | `Guid?` | `null` | Parent task ID for hierarchy |
| `Dependencies` | `List<Guid>` | `new()` | List of dependent task IDs |
| `AssignedResources` | `List<string>` | `new()` | Assigned resources |
| `Status` | `TaskStatus` | `TaskStatus.NotStarted` | Task status |
| `Duration` | `TimeSpan` | (calculated) | Task duration (read-only) |

#### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `IsValid()` | `bool` | Checks if task data is valid |
| `GetValidationErrors()` | `List<string>` | Returns validation error messages |
| `OverlapsWith(GanttTask)` | `bool` | Checks if task overlaps with another |
| `Clone()` | `GanttTask` | Creates a copy of the task |

#### Validation Rules

- `Start` must be before or equal to `End`
- `Title` is required and cannot exceed 200 characters
- `Description` cannot exceed 1000 characters
- `RowIndex` must be greater than 0
- `Progress` must be between 0 and 100

### TimeUnit

Enumeration for time scale units.

```csharp
public enum TimeUnit
{
    Hour = 0,
    Day = 1,
    Week = 2,
    Month = 3,
    Year = 4
}
```

### TaskPriority

Enumeration for task priority levels.

```csharp
public enum TaskPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}
```

### TaskStatus

Enumeration for task status.

```csharp
public enum TaskStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3,
    OnHold = 4
}
```

---

## ViewModels

### GanttChartViewModel

Main ViewModel for Gantt chart functionality with MVVM support.

```csharp
public class GanttChartViewModel : ViewModelBase
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `StartTime` | `DateTime` | Timeline start time |
| `EndTime` | `DateTime` | Timeline end time |
| `TimeUnit` | `TimeUnit` | Time scale unit |
| `Tasks` | `ObservableCollection<GanttTaskViewModel>` | Task ViewModels |
| `TaskModels` | `ObservableCollection<GanttTask>` | Task models for binding |
| `Configuration` | `GanttConfiguration` | Configuration settings |
| `SelectedTask` | `GanttTaskViewModel?` | Currently selected task |
| `ErrorMessage` | `string` | Current error message |
| `ProjectId` | `Guid` | Active project ID |
| `IsBusy` | `bool` | Indicates async operation in progress |

#### Commands

| Command | Type | Description |
|---------|------|-------------|
| `AddTaskCommand` | `ICommand` | Adds a new task |
| `DeleteTaskCommand` | `ICommand` | Deletes selected task |
| `UpdateTaskCommand` | `ICommand` | Updates a task |
| `ValidateAllCommand` | `ICommand` | Validates all tasks |
| `LoadTasksAsyncCommand` | `ICommand` | Loads tasks asynchronously |
| `AddTaskAsyncCommand` | `ICommand` | Adds task asynchronously |
| `CancelAsyncCommand` | `ICommand` | Cancels async operations |

#### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `LoadTasksAsync(CancellationToken)` | `Task` | Loads tasks from service |
| `AddTaskAsync(GanttTask?, CancellationToken)` | `Task<GanttTaskViewModel>` | Creates new task via service |

### GanttTaskViewModel

ViewModel for individual tasks.

```csharp
public class GanttTaskViewModel : ViewModelBase
```

#### Properties

All properties from `GanttTask` are exposed with property change notification:

| Property | Type | Description |
|----------|------|-------------|
| `Model` | `GanttTask` | Underlying task model |
| `Id` | `Guid` | Task identifier |
| `Title` | `string` | Task title |
| `Description` | `string?` | Task description |
| `Start` | `DateTime` | Start date/time |
| `End` | `DateTime` | End date/time |
| `RowIndex` | `int` | Display row |
| `Progress` | `double` | Progress percentage |
| `Priority` | `TaskPriority` | Task priority |
| `Status` | `TaskStatus` | Task status |
| `Duration` | `TimeSpan` | Task duration (read-only) |
| `IsValid` | `bool` | Validation status (read-only) |
| `ValidationErrors` | `List<string>` | Validation errors (read-only) |
| `ValidationMessage` | `string` | User-friendly validation message |

#### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Validate()` | `ValidationResult` | Validates using validation service |
| `Clone()` | `GanttTaskViewModel` | Creates a copy |

---

## Services

### IGanttService

Interface for Gantt chart data operations.

```csharp
public interface IGanttService
```

#### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `GetTasksAsync(Guid)` | `Task<IEnumerable<GanttTask>>` | Gets tasks for project |
| `GetTasksAsync(Guid, CancellationToken)` | `Task<IEnumerable<GanttTask>>` | Gets tasks with cancellation |
| `CreateTaskAsync(GanttTask)` | `Task<GanttTask>` | Creates a new task |
| `CreateTaskAsync(Guid, GanttTask, CancellationToken)` | `Task<GanttTask>` | Creates task for project |
| `UpdateTaskAsync(GanttTask)` | `Task<GanttTask>` | Updates existing task |
| `UpdateTaskAsync(Guid, GanttTask, CancellationToken)` | `Task<GanttTask>` | Updates task for project |
| `DeleteTaskAsync(Guid)` | `Task<bool>` | Deletes task by ID |
| `DeleteTaskAsync(Guid, Guid, CancellationToken)` | `Task<bool>` | Deletes task from project |
| `ValidateTasksAsync(IEnumerable<GanttTask>)` | `Task<ValidationResult>` | Validates task collection |
| `ValidateTasksAsync(IEnumerable<GanttTask>, CancellationToken)` | `Task<ValidationResult>` | Validates with cancellation |
| `OptimizeScheduleAsync(IEnumerable<GanttTask>)` | `Task<IEnumerable<GanttTask>>` | Optimizes task schedule |
| `OptimizeScheduleAsync(IEnumerable<GanttTask>, CancellationToken)` | `Task<IEnumerable<GanttTask>>` | Optimizes with cancellation |

### IValidationService

Interface for task validation operations.

```csharp
public interface IValidationService
```

#### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `ValidateTask(GanttTask)` | `ValidationResult` | Validates single task |
| `ValidateTaskCollection(IEnumerable<GanttTask>)` | `ValidationResult` | Validates task collection |

### ValidationResult

Result of validation operations.

```csharp
public class ValidationResult
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsValid` | `bool` | Whether validation passed |
| `Errors` | `List<string>` | List of error messages |

---

## Configuration

### GanttConfiguration

Main configuration class.

```csharp
public class GanttConfiguration
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `TimeScale` | `TimeScaleConfiguration` | Time scale settings |
| `Rendering` | `RenderingConfiguration` | Rendering settings |
| `Localization` | `LocalizationConfiguration` | Localization settings |

#### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Default()` | `GanttConfiguration` | Creates default configuration |

### TimeScaleConfiguration

Configuration for time scale behavior.

```csharp
public class TimeScaleConfiguration
```

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DefaultTimeUnit` | `TimeUnit` | `TimeUnit.Day` | Default time unit |
| `DateFormat` | `string` | `"yyyy-MM-dd"` | Date format string |
| `TimeFormat` | `string` | `"HH:mm"` | Time format string |
| `Culture` | `CultureInfo` | `CultureInfo.CurrentCulture` | Formatting culture |
| `HighlightWeekends` | `bool` | `true` | Highlight weekend days |
| `HighlightToday` | `bool` | `true` | Highlight current day |

### RenderingConfiguration

Configuration for visual rendering.

```csharp
public class RenderingConfiguration
```

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `EnableVirtualization` | `bool` | `true` | Enable UI virtualization |
| `MaxVisibleTasks` | `int` | `1000` | Max tasks before virtualization |
| `EnableCaching` | `bool` | `true` | Enable element caching |
| `GridMode` | `GridRenderingMode` | `GridRenderingMode.Rectangles` | Grid rendering mode |
| `ShowGridCells` | `bool` | `false` | Show individual grid cells |
| `TaskBarCornerRadius` | `double` | `4.0` | Task bar corner radius |

---

## Utilities

### TimelineCalculator

Static utility class for timeline calculations.

```csharp
public static class TimelineCalculator
```

#### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `GenerateTicks(DateTime, DateTime, TimeUnit, CultureInfo?)` | `List<DateTime>` | Generates timeline ticks |
| `AlignToUnitFloor(DateTime, TimeUnit, CultureInfo?)` | `DateTime` | Aligns to unit floor boundary |
| `AlignToUnitCeiling(DateTime, TimeUnit, CultureInfo?)` | `DateTime` | Aligns to unit ceiling boundary |
| `FormatTick(DateTime, TimeUnit, CultureInfo?)` | `string` | Formats tick for display |
| `FormatTick(DateTime, TimeUnit, string?, string?, CultureInfo?)` | `string` | Formats with custom formats |
| `GetUnitDuration(TimeUnit, DateTime?)` | `TimeSpan` | Gets duration of one unit |
| `IsWeekend(DateTime)` | `bool` | Checks if date is weekend |
| `IsToday(DateTime)` | `bool` | Checks if date is today |

### TimelineHelper

Static utility class for timeline position calculations.

```csharp
public static class TimelineHelper
```

#### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `CalculateTaskSpan(List<DateTime>, DateTime, DateTime, TimeUnit)` | `(int startIndex, int columnSpan)` | Calculates task position and span |

### DoubleToGridLengthConverter

Value converter for XAML binding.

```csharp
public class DoubleToGridLengthConverter : IValueConverter
```

---

## Visual Components

### GanttTimeCell

Displays time labels in the header row.

```csharp
public class GanttTimeCell : GanttRectangle
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `TimeIndex` | `int` | Column index |
| `RowIndex` | `int` | Row index |
| `TimeText` | `string` | Display text |
| `IsWeekend` | `bool` | Whether this represents a weekend |
| `IsToday` | `bool` | Whether this represents today |

### GanttGridCell

Represents an individual grid cell.

```csharp
public class GanttGridCell : GanttRectangle
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `RowIndex` | `int` | Row index |
| `TimeIndex` | `int` | Column index |
| `IsWeekend` | `bool` | Whether this is a weekend cell |
| `IsToday` | `bool` | Whether this is today's cell |

### GanttTaskBar

Displays task bars on the timeline.

```csharp
public class GanttTaskBar : GanttRectangle
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `RowIndex` | `int` | Row index |
| `TimeIndex` | `int` | Start column index |
| `CustomText` | `string` | Task display text |
| `IsInteractive` | `bool` | Whether task is interactive |

### GanttGridRow

Represents a full grid row.

```csharp
public class GanttGridRow : GanttRectangle
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `RowIndex` | `int` | Row index |

---

## Events and Notifications

All ViewModels implement `INotifyPropertyChanged` for data binding support. The following properties raise property change notifications:

### GanttChartViewModel Events
- Property changes for all public properties
- Collection change events for `Tasks`

### GanttTaskViewModel Events
- Property changes for all task properties
- Validation state changes

## Error Handling

The library uses standard .NET exception handling patterns:

- `ArgumentNullException` for null parameters
- `ArgumentException` for invalid arguments
- `InvalidOperationException` for invalid state operations
- `ValidationException` for validation failures (future enhancement)

## Advanced Features (v2.1.0)

### Plugin-Based Annotation System

Extensible annotation framework allowing custom markup and identification features.

#### Core Interfaces

```csharp
public interface IAnnotationPlugin
{
    string Name { get; }
    AnnotationType Type { get; }
    UIElement CreateElement(IAnnotationConfig config);
    bool ValidateConfig(IAnnotationConfig config);
}

public interface IAnnotationConfig
{
    string Id { get; set; }
    string Name { get; set; }
    AnnotationType Type { get; set; }
    double X { get; set; }
    double Y { get; set; }
    double Width { get; set; }
    double Height { get; set; }
}
```

#### Built-in Plugins

**Text Annotations**
```csharp
var textConfig = new TextAnnotationConfig
{
    Text = "Critical Phase",
    FontSize = 14,
    Color = "#FF0000",
    X = 100, Y = 50
};
```

**Shape Annotations**
```csharp
var shapeConfig = new ShapeAnnotationConfig
{
    ShapeType = "Rectangle",
    FillColor = "#FF4CAF50",
    StrokeColor = "#FF2196F3"
};
```

**Line Annotations**
```csharp
var lineConfig = new LineAnnotationConfig
{
    X2 = 200, Y2 = 25,
    StrokeColor = "#FFFF5722",
    EndCapType = "Arrow"
};
```

### Multi-Level Time Scale

Simultaneous display of multiple time granularities for enhanced timeline navigation.

#### Configuration Models

```csharp
public class MultiLevelTimeScaleConfiguration
{
    public List<TimeLevelConfiguration> Levels { get; set; }
    public bool EnableSmartVisibility { get; set; }
    public TimeSpan VisibilityThreshold { get; set; }
}

public class TimeLevelConfiguration
{
    public ExtendedTimeUnit Unit { get; set; }
    public bool IsVisible { get; set; }
    public double Height { get; set; }
    public string DateFormat { get; set; }
}

public enum ExtendedTimeUnit
{
    Minute, Hour, Day, Week, Month, Quarter, Year
}
```

#### Usage Example

```csharp
var config = new MultiLevelTimeScaleConfiguration
{
    Levels = new List<TimeLevelConfiguration>
    {
        new() { Unit = ExtendedTimeUnit.Year, Height = 30, DateFormat = "yyyy" },
        new() { Unit = ExtendedTimeUnit.Month, Height = 25, DateFormat = "MMM" },
        new() { Unit = ExtendedTimeUnit.Week, Height = 20, DateFormat = "ww" }
    },
    EnableSmartVisibility = true
};

ganttContainer.MultiLevelTimeScale = config;
```

### Expandable Time Axis Segments

Dynamic expansion of specific time periods for detailed exploration.

#### Expansion Model

```csharp
public class TimeSegmentExpansion
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ExtendedTimeUnit OriginalUnit { get; set; }
    public ExtendedTimeUnit ExpandedUnit { get; set; }
    public bool IsExpanded { get; set; }
    public string DisplayName { get; set; }
}
```

#### Interactive Controls

```csharp
public class MultiLevelTimeScaleTick : UserControl
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ExtendedTimeUnit Unit { get; set; }
    public bool IsExpandable { get; set; }
    
    public event EventHandler<TimeSegmentExpansionRequestedEventArgs> ExpansionRequested;
}
```

#### Expansion Example

```csharp
// Expand specific week to daily view
var expansion = new TimeSegmentExpansion
{
    StartTime = new DateTime(2024, 3, 18),
    EndTime = new DateTime(2024, 3, 24),
    OriginalUnit = ExtendedTimeUnit.Week,
    ExpandedUnit = ExtendedTimeUnit.Day,
    IsExpanded = true
};

ganttContainer.TimeSegmentExpansions.Add(expansion);
```

### Performance Optimizations

- **Element Pooling**: Reusable UI elements for annotations
- **Smart Visibility**: Automatic level management based on viewport
- **Efficient Rendering**: Optimized multi-level time scale rendering
- **Memory Management**: Automatic cleanup of expanded segments

## Threading Considerations

- All service operations are async and support cancellation tokens
- UI updates are automatically marshaled to the UI thread in ViewModels
- The library is designed to be used from the UI thread for WPF controls

## Design Token Service API
The Design Token Service exposes theme-derived, strongly-typed parameters for multiple Gantt subsystems.

Interfaces and classes
- IDesignTokenService: contract for retrieving token groups
- DesignTokenService: default implementation that reads from the active theme and normalizes values

Common methods (examples)
- GetTaskBarTokens(): returns token set for task bars (fills, strokes, corner radii, shadows, etc.)
- GetGridTokens(): returns token set for grid rows/cells (colors, alternation, hover/selection)
- GetTimeScaleTokens(): returns token set for multi-level time scale (font, tick colors)
- GetDependencyLineTokens(): returns token set for dependency lines (stroke, arrow style)

Usage pattern
- Acquire an instance of IDesignTokenService (via your composition root or service locator used in the project).
- Call the relevant getter and apply tokens to your rendering logic.
- Re-fetch tokens when ThemeManager changes the current theme.