# GPM.Gantt API Reference

Comprehensive API documentation for the GPM.Gantt WPF component library.

## Table of Contents

1. [Core Models](#core-models)
2. [Services](#services)
3. [Plugin System](#plugin-system)
4. [Multi-Level Time Scale](#multi-level-time-scale)
5. [Expandable Time Axis](#expandable-time-axis)
6. [Rendering](#rendering)
7. [Templates](#templates)
8. [Calendar System](#calendar-system)
9. [Utilities](#utilities)
10. [GPU Rendering](#gpu-rendering)
11. [Examples](#examples)

## Core Models

### GanttTask

The primary model representing a task in the Gantt chart.

```csharp
public class GanttTask
{
    public Guid Id { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int RowIndex { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public double Progress { get; set; } // 0-100
    public TaskPriority Priority { get; set; }
    public Guid? ParentTaskId { get; set; }
    public List<Guid> Dependencies { get; set; }
    public List<string> AssignedResources { get; set; }
    public TaskStatus Status { get; set; }
    public TaskBarShape Shape { get; set; }
    public ShapeRenderingParameters? ShapeParameters { get; set; }
    
    // Critical Path Properties
    public bool IsCritical { get; set; }
    public TimeSpan FreeFloat { get; set; }
    public TimeSpan TotalFloat { get; set; }
    public DateTime? EarliestStart { get; set; }
    public DateTime? LatestStart { get; set; }
    public DateTime? EarliestFinish { get; set; }
    public DateTime? LatestFinish { get; set; }
    
    // Validation
    public bool IsValid()
    public List<string> GetValidationErrors()
    public bool OverlapsWith(GanttTask other)
    public GanttTask Clone()
}
```

**Key Properties:**
- `Id`: Unique identifier for the task
- `Start/End`: Task duration
- `RowIndex`: Display position (1-based)
- `Progress`: Completion percentage (0-100)
- `IsCritical`: Whether task is on critical path
- `Dependencies`: List of predecessor task IDs

### TaskDependency

Represents relationships between tasks.

```csharp
public class TaskDependency
{
    public string Id { get; set; }
    public string PredecessorTaskId { get; set; }
    public string SuccessorTaskId { get; set; }
    public DependencyType Type { get; set; }
    public TimeSpan Lag { get; set; }
    public bool IsActive { get; set; }
    public string Description { get; set; }
    public bool IsCritical { get; set; }
    public int Priority { get; set; }
}

public enum DependencyType
{
    FinishToStart,    // Default - predecessor must finish before successor starts
    StartToStart,     // Both tasks start simultaneously  
    FinishToFinish,   // Both tasks finish simultaneously
    StartToFinish     // Successor finishes when predecessor starts
}
```

### DependencyLine

Visual representation of task dependencies.

```csharp
public class DependencyLine
{
    public TaskDependency Dependency { get; set; }
    public Point StartPoint { get; set; }
    public Point EndPoint { get; set; }
    public Point[] ControlPoints { get; set; }
    public bool IsHighlighted { get; set; }
    public Brush LineColor { get; set; }
    public double LineThickness { get; set; }
    public bool ShowArrow { get; set; }
    public double ArrowSize { get; set; }
    public bool ShowLagLabel { get; set; }
}
```

## Services

### IDependencyService

Manages task dependencies and critical path analysis.

```csharp
public interface IDependencyService
{
    // CRUD Operations
    Task<List<TaskDependency>> GetDependenciesAsync(string projectId);
    Task<TaskDependency> CreateDependencyAsync(TaskDependency dependency);
    Task<bool> UpdateDependencyAsync(TaskDependency dependency);
    Task<bool> DeleteDependencyAsync(string dependencyId);
    
    // Dependency Analysis
    Task<List<string>> GetCriticalPathAsync(List<GanttTask> tasks, List<TaskDependency> dependencies);
    Task<Dictionary<string, TimeSpan>> CalculateFloatTimesAsync(List<GanttTask> tasks, List<TaskDependency> dependencies);
    bool HasCircularDependency(List<GanttTask> tasks, List<TaskDependency> dependencies, TaskDependency newDependency);
    
    // Scheduling
    Task<List<GanttTask>> CalculateScheduleAsync(List<GanttTask> tasks, List<TaskDependency> dependencies);
    Task<List<GanttTask>> AutoScheduleTasksAsync(List<GanttTask> tasks, List<TaskDependency> dependencies, DateTime projectStart);
    
    // Validation
    Task<bool> ValidateDependencyAsync(TaskDependency dependency);
    Task<List<TaskDependency>> GetTaskDependenciesAsync(string taskId);
}
```

**Usage Example:**
```csharp
var dependencyService = new DependencyService();

// Create a dependency
var dependency = new TaskDependency
{
    PredecessorTaskId = "task1",
    SuccessorTaskId = "task2", 
    Type = DependencyType.FinishToStart,
    Lag = TimeSpan.FromDays(1)
};

await dependencyService.CreateDependencyAsync(dependency);

// Calculate critical path
var criticalPath = await dependencyService.GetCriticalPathAsync(tasks, dependencies);

// Auto-schedule tasks
var scheduledTasks = await dependencyService.AutoScheduleTasksAsync(tasks, dependencies, DateTime.Today);
```

### IExportService

Handles exporting Gantt charts to various formats.

```csharp
public interface IExportService
{
    Task<bool> ExportAsync(FrameworkElement element, string filePath, ExportOptions options);
    Task<byte[]> ExportToBytesAsync(FrameworkElement element, ExportOptions options);
    Task<bool> ExportGanttChartAsync(GanttContainer ganttContainer, string filePath, ExportOptions options);
    
    string GetDefaultFileName(ExportFormat format);
    string[] GetSupportedFormats();
    bool ValidateExportOptions(ExportOptions options);
    string GetFileFilter();
}

public class ExportOptions
{
    public ExportFormat Format { get; set; } = ExportFormat.PNG;
    public double DPI { get; set; } = 96;
    public int Width { get; set; } = 0;
    public int Height { get; set; } = 0;
    public bool IncludeBackground { get; set; } = true;
    public bool IncludeDependencies { get; set; } = true;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int Quality { get; set; } = 95; // For JPEG
}

public enum ExportFormat
{
    PNG, PDF, SVG, JPEG, BMP
}
```

**Usage Example:**
```csharp
var exportService = new ExportService();

var options = new ExportOptions
{
    Format = ExportFormat.PDF,
    DPI = 300,
    IncludeDependencies = true,
    Title = "Project Schedule",
    Author = "Project Manager"
};

await exportService.ExportGanttChartAsync(ganttContainer, "schedule.pdf", options);
```

### ICalendarService

Manages working calendars and business day calculations.

```csharp
public interface ICalendarService
{
    // Calendar Management
    Task<List<WorkingCalendar>> GetCalendarsAsync();
    Task<WorkingCalendar?> GetCalendarAsync(string calendarId);
    Task<WorkingCalendar> GetDefaultCalendarAsync();
    Task<WorkingCalendar> CreateCalendarAsync(WorkingCalendar calendar);
    Task<bool> UpdateCalendarAsync(WorkingCalendar calendar);
    Task<bool> DeleteCalendarAsync(string calendarId);
    
    // Business Day Calculations
    Task<bool> IsWorkingDayAsync(DateTime date, string? calendarId = null);
    Task<List<WorkingTime>> GetWorkingHoursAsync(DateTime date, string? calendarId = null);
    Task<TimeSpan> CalculateWorkingHoursAsync(DateTime start, DateTime end, string? calendarId = null);
    Task<DateTime> AddWorkingTimeAsync(DateTime start, TimeSpan workingTime, string? calendarId = null);
    Task<DateTime> GetNextWorkingDayAsync(DateTime fromDate, string? calendarId = null);
    Task<int> CountWorkingDaysAsync(DateTime start, DateTime end, string? calendarId = null);
    
    // Exception Management  
    Task<bool> AddCalendarExceptionAsync(string calendarId, CalendarException exception);
    Task<bool> RemoveCalendarExceptionAsync(string calendarId, string exceptionId);
    Task<List<CalendarException>> GetCalendarExceptionsAsync(string calendarId);
}
```

**Usage Example:**
```csharp
var calendarService = new CalendarService();

// Check if date is working day
bool isWorkingDay = await calendarService.IsWorkingDayAsync(DateTime.Today);

// Calculate working hours between dates
var workingHours = await calendarService.CalculateWorkingHoursAsync(
    DateTime.Today, 
    DateTime.Today.AddDays(5)
);

// Add working time to a date
var endDate = await calendarService.AddWorkingTimeAsync(
    DateTime.Today, 
    TimeSpan.FromHours(40) // 40 working hours
);
```

### ITemplateService

Manages project templates for rapid project setup.

```csharp
public interface ITemplateService
{
    // Template Management
    Task<List<ProjectTemplate>> GetTemplatesAsync();
    Task<List<ProjectTemplate>> GetTemplatesByCategoryAsync(TemplateCategory category);
    Task<ProjectTemplate?> GetTemplateAsync(string templateId);
    Task<ProjectTemplate> CreateTemplateAsync(ProjectTemplate template);
    Task<bool> UpdateTemplateAsync(ProjectTemplate template);
    Task<bool> DeleteTemplateAsync(string templateId);
    
    // Template Application
    Task<List<GanttTask>> ApplyTemplateAsync(string templateId, TemplateApplicationOptions options);
    Task<ProjectTemplate> CreateTemplateFromTasksAsync(List<GanttTask> tasks, List<TaskDependency> dependencies, 
        string templateName, TemplateCategory category);
    
    // Search and Discovery
    Task<List<ProjectTemplate>> SearchTemplatesAsync(string searchTerm);
    Task<List<ProjectTemplate>> GetPopularTemplatesAsync(int count = 10);
    Task<List<ProjectTemplate>> GetRecentTemplatesAsync(int count = 10);
    
    // Import/Export
    Task<byte[]> ExportTemplateAsync(string templateId, TemplateExportFormat format);
    Task<ProjectTemplate> ImportTemplateAsync(byte[] data, TemplateExportFormat format);
}
```

**Usage Example:**
```csharp
var templateService = new TemplateService();

// Apply a software development template
var options = new TemplateApplicationOptions
{
    ProjectStartDate = DateTime.Today,
    AutoSchedule = true,
    IncludeDependencies = true,
    ResourceMappings = new Dictionary<string, string>
    {
        ["Frontend Developer"] = "John Doe",
        ["Backend Developer"] = "Jane Smith"
    }
};

var tasks = await templateService.ApplyTemplateAsync("software-dev-template", options);
```

## Plugin System

### IAnnotationPlugin Interface

Base interface for all annotation plugins.

```csharp
public interface IAnnotationPlugin
{
    string Name { get; }
    AnnotationType Type { get; }
    string Version { get; }
    string Description { get; }
    
    UIElement CreateElement(IAnnotationConfig config);
    void UpdateElement(UIElement element, IAnnotationConfig config);
    bool ValidateConfig(IAnnotationConfig config);
    IAnnotationConfig CreateDefaultConfig();
    void Initialize(IServiceProvider serviceProvider);
    void Cleanup();
}
```

### Built-in Annotation Plugins

**TextAnnotationPlugin**
Provides text-based annotations with rich formatting options.

```csharp
var textConfig = new TextAnnotationConfig
{
    Text = "Important Milestone",
    FontFamily = "Arial",
    FontSize = 14,
    Color = "#FF0000",
    BackgroundColor = "#FFFF00",
    X = 100, Y = 50,
    Width = 200, Height = 30
};

var textElement = textPlugin.CreateElement(textConfig);
```

**ShapeAnnotationPlugin**
Creates geometric shapes for visual emphasis.

```csharp
var shapeConfig = new ShapeAnnotationConfig
{
    ShapeType = "Rectangle", // Rectangle, Ellipse, Triangle, Diamond
    FillColor = "#FF4CAF50",
    StrokeColor = "#FF2196F3",
    StrokeThickness = 2,
    X = 150, Y = 75,
    Width = 100, Height = 50
};

var shapeElement = shapePlugin.CreateElement(shapeConfig);
```

**LineAnnotationPlugin**
Draws lines and arrows for connections and emphasis.

```csharp
var lineConfig = new LineAnnotationConfig
{
    X = 50, Y = 25,
    X2 = 200, Y2 = 25,
    StrokeColor = "#FFFF5722",
    StrokeThickness = 3,
    EndCapType = "Arrow"
};

var lineElement = linePlugin.CreateElement(lineConfig);
```

### IPluginService

Manages plugin registration and lifecycle.

```csharp
public interface IPluginService
{
    void RegisterPlugin(IAnnotationPlugin plugin);
    void UnregisterPlugin(string pluginName);
    IAnnotationPlugin? GetPlugin(string name);
    IAnnotationPlugin? GetPlugin(AnnotationType type);
    List<IAnnotationPlugin> GetAllPlugins();
    List<IAnnotationPlugin> GetPluginsByType(AnnotationType type);
    void LoadPluginsFromDirectory(string directoryPath);
    bool IsPluginRegistered(string name);
}
```

**Usage Example:**
```csharp
var pluginService = new PluginService();

// Register built-in plugins
plugginService.RegisterPlugin(new TextAnnotationPlugin());
plugginService.RegisterPlugin(new ShapeAnnotationPlugin());
plugginService.RegisterPlugin(new LineAnnotationPlugin());

// Use plugin to create annotation
var textPlugin = pluginService.GetPlugin(AnnotationType.Text);
var config = textPlugin.CreateDefaultConfig();
var element = textPlugin.CreateElement(config);
```

## Multi-Level Time Scale

Simultaneous visualization of multiple time granularities.

### ExtendedTimeUnit Enumeration

```csharp
public enum ExtendedTimeUnit
{
    Minute = 0,
    Hour = 1,
    Day = 2,
    Week = 3,
    Month = 4,
    Quarter = 5,
    Year = 6
}
```

### TimeLevelConfiguration

```csharp
public class TimeLevelConfiguration
{
    public ExtendedTimeUnit Unit { get; set; }
    public bool IsVisible { get; set; }
    public double Height { get; set; }
    public string BackgroundColor { get; set; }
    public string TextColor { get; set; }
    public string FontFamily { get; set; }
    public double FontSize { get; set; }
    public string DateFormat { get; set; }
    public HorizontalAlignment TextAlignment { get; set; }
    public bool ShowBorders { get; set; }
    public string BorderColor { get; set; }
    public double BorderThickness { get; set; }
    public int ZIndex { get; set; }
}
```

### MultiLevelTimeScaleConfiguration

```csharp
public class MultiLevelTimeScaleConfiguration
{
    public List<TimeLevelConfiguration> Levels { get; set; }
    public double TotalHeight { get; set; }
    public bool EnableSmartVisibility { get; set; }
    public TimeSpan VisibilityThreshold { get; set; }
    public bool EnableAutoFit { get; set; }
    public string Theme { get; set; }
}
```

### TimeScaleContext

```csharp
public class TimeScaleContext
{
    public DateTime ViewportStart { get; set; }
    public DateTime ViewportEnd { get; set; }
    public double ViewportWidth { get; set; }
    public double PixelsPerDay { get; set; }
    public ExtendedTimeUnit PrimaryUnit { get; set; }
    public CultureInfo Culture { get; set; }
    public List<TimeSegmentExpansion> ActiveExpansions { get; set; }
}
```

**Configuration Example:**
```csharp
var multiLevelConfig = new MultiLevelTimeScaleConfiguration
{
    Levels = new List<TimeLevelConfiguration>
    {
        new TimeLevelConfiguration
        {
            Unit = ExtendedTimeUnit.Year,
            IsVisible = true,
            Height = 30,
            DateFormat = "yyyy",
            FontSize = 14
        },
        new TimeLevelConfiguration
        {
            Unit = ExtendedTimeUnit.Quarter,
            IsVisible = true,
            Height = 25,
            DateFormat = "Q{0}",
            FontSize = 12
        },
        new TimeLevelConfiguration
        {
            Unit = ExtendedTimeUnit.Month,
            IsVisible = true,
            Height = 25,
            DateFormat = "MMM",
            FontSize = 11
        },
        new TimeLevelConfiguration
        {
            Unit = ExtendedTimeUnit.Week,
            IsVisible = true,
            Height = 20,
            DateFormat = "ww",
            FontSize = 10
        }
    },
    EnableSmartVisibility = true,
    VisibilityThreshold = TimeSpan.FromDays(365)
};

ganttContainer.MultiLevelTimeScale = multiLevelConfig;
```

## Expandable Time Axis

Allows dynamic expansion of specific time periods while maintaining other granularities.

### TimeSegmentExpansion Model

```csharp
public class TimeSegmentExpansion
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ExtendedTimeUnit OriginalUnit { get; set; }
    public ExtendedTimeUnit ExpandedUnit { get; set; }
    public bool IsExpanded { get; set; }
    public string SegmentId { get; set; }
    public bool IsCollapsible { get; set; }
    public string DisplayName { get; set; }
}
```

### MultiLevelTimeScaleTick

Interactive time scale component with expand/collapse functionality.

```csharp
public class MultiLevelTimeScaleTick : UserControl
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ExtendedTimeUnit Unit { get; set; }
    public string DisplayText { get; set; }
    public bool IsExpandable { get; set; }
    public bool IsExpanded { get; set; }
    
    public event EventHandler<TimeSegmentExpansionRequestedEventArgs> ExpansionRequested;
    public event EventHandler<TimeSegmentCollapseRequestedEventArgs> CollapseRequested;
}
```

**Usage Example:**
```csharp
// Expand a specific week to show daily details
var expansion = new TimeSegmentExpansion
{
    StartTime = new DateTime(2024, 3, 18), // Monday
    EndTime = new DateTime(2024, 3, 24),   // Sunday
    OriginalUnit = ExtendedTimeUnit.Week,
    ExpandedUnit = ExtendedTimeUnit.Day,
    IsExpanded = true,
    DisplayName = "Week 12 - Detailed View"
};

ganttContainer.TimeSegmentExpansions.Add(expansion);
```

## Rendering

### DependencyLineRenderer

Renders visual dependency lines between tasks.

```csharp
public class DependencyLineRenderer
{
    public List<UIElement> RenderDependencyLines(List<DependencyLine> dependencyLines, 
        Dictionary<string, Rect> taskPositions);
}
```

**Usage:**
```csharp
var renderer = new DependencyLineRenderer();
var taskPositions = new Dictionary<string, Rect>
{
    ["task1"] = new Rect(10, 20, 100, 30),
    ["task2"] = new Rect(150, 20, 100, 30)
};

var dependencyLines = new List<DependencyLine>
{
    new DependencyLine
    {
        Dependency = dependency,
        LineColor = Brushes.Blue,
        LineThickness = 2.0,
        ShowArrow = true
    }
};

var elements = renderer.RenderDependencyLines(dependencyLines, taskPositions);
```

## Calendar System

### WorkingCalendar

Defines working days, hours, and exceptions.

```csharp
public class WorkingCalendar
{
    public string Id { get; set; }
    public string Name { get; set; }
    public CalendarType Type { get; set; }
    public List<WorkingDay> WorkingDays { get; set; }
    public List<CalendarException> Exceptions { get; set; }
    public CultureInfo Culture { get; set; }
    public TimeZoneInfo TimeZone { get; set; }
    public bool IsDefault { get; set; }
    
    // Methods
    public bool IsWorkingDay(DateTime date);
    public List<WorkingTime> GetWorkingHours(DateTime date);
    public TimeSpan CalculateWorkingHours(DateTime start, DateTime end);
    public DateTime AddWorkingTime(DateTime start, TimeSpan workingTime);
}
```

### WorkingTime

Defines working hours within a day.

```csharp
public class WorkingTime
{
    public TimeSpan StartTime { get; set; }  // e.g., 09:00
    public TimeSpan EndTime { get; set; }    // e.g., 17:00
    public bool IsWorking { get; set; } = true;
    public string Description { get; set; }
    
    public TimeSpan Duration => EndTime - StartTime;
    public bool IsValid() => StartTime < EndTime;
}
```

### CalendarException

Represents holidays or special working arrangements.

```csharp
public class CalendarException
{
    public string Id { get; set; }
    public DateTime Date { get; set; }
    public DateTime? EndDate { get; set; }  // For multi-day exceptions
    public List<WorkingTime> WorkingTimes { get; set; }
    public bool IsWorkingDay { get; set; }
    public string Name { get; set; }  // e.g., "Christmas Day"
    public bool IsRecurring { get; set; }
    public RecurrencePattern RecurrencePattern { get; set; }
    
    public bool AppliesToDate(DateTime date);
}
```

## Templates

### ProjectTemplate

Complete project template with tasks and dependencies.

```csharp
public class ProjectTemplate
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public TemplateCategory Category { get; set; }
    public List<TaskTemplate> TaskTemplates { get; set; }
    public List<DependencyTemplate> DependencyTemplates { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
    public List<string> Tags { get; set; }
    public bool IsBuiltIn { get; set; }
    public int UsageCount { get; set; }
    public double Rating { get; set; }
    
    public List<string> Validate();
    public double TotalEstimatedEffort { get; }
    public int MilestoneCount { get; }
}

public enum TemplateCategory
{
    SoftwareDevelopment,
    Construction,
    Marketing,
    Research,
    Manufacturing,
    Event,
    General,
    Custom
}
```

### TaskTemplate

Template for individual tasks.

```csharp
public class TaskTemplate
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
    public TemplatePriority Priority { get; set; }
    public TaskBarShape Shape { get; set; }
    public int Order { get; set; }
    public string? ParentTaskId { get; set; }
    public List<string> PredecessorTaskIds { get; set; }
    public List<string> RequiredSkills { get; set; }
    public double EstimatedEffort { get; set; }
    public bool IsMilestone { get; set; }
    public Dictionary<string, object> CustomAttributes { get; set; }
}
```

## GPU Rendering

### GpuRenderingTechnology

Enumeration of supported GPU rendering technologies.

```csharp
public enum GpuRenderingTechnology
{
    Default,    // Default WPF rendering (software rendering)
    Direct2D,   // Direct2D rendering
    DirectX,    // DirectX rendering
    OpenGL,     // OpenGL rendering
    Vulkan      // Vulkan rendering
}
```

### IGpuRenderingService

Interface for GPU rendering services.

```csharp
public interface IGpuRenderingService
{
    /// <summary>
    /// Gets the rendering technology type
    /// </summary>
    GpuRenderingTechnology Technology { get; }
    
    /// <summary>
    /// Initialize the rendering service
    /// </summary>
    /// <returns>Whether initialization was successful</returns>
    bool Initialize();
    
    /// <summary>
    /// Render a rectangular shape
    /// </summary>
    /// <param name="drawingContext">Drawing context</param>
    /// <param name="bounds">Boundary rectangle</param>
    /// <param name="fillBrush">Fill brush</param>
    /// <param name="borderBrush">Border brush</param>
    /// <param name="borderThickness">Border thickness</param>
    /// <param name="cornerRadius">Corner radius</param>
    void RenderRectangle(
        DrawingContext drawingContext, 
        Rect bounds, 
        Brush fillBrush, 
        Brush borderBrush, 
        double borderThickness,
        double cornerRadius = 0);
    
    /// <summary>
    /// Render a line
    /// </summary>
    /// <param name="drawingContext">Drawing context</param>
    /// <param name="startPoint">Start point</param>
    /// <param name="endPoint">End point</param>
    /// <param name="strokeBrush">Line brush</param>
    /// <param name="strokeThickness">Line thickness</param>
    void RenderLine(
        DrawingContext drawingContext,
        Point startPoint,
        Point endPoint,
        Brush strokeBrush,
        double strokeThickness);
    
    /// <summary>
    /// Render text
    /// </summary>
    /// <param name="drawingContext">Drawing context</param>
    /// <param name="text">Text content</param>
    /// <param name="fontFamily">Font family</param>
    /// <param name="fontSize">Font size</param>
    /// <param name="foreground">Foreground color</param>
    /// <param name="origin">Text starting position</param>
    void RenderText(
        DrawingContext drawingContext,
        string text,
        FontFamily fontFamily,
        double fontSize,
        Brush foreground,
        Point origin);
    
    /// <summary>
    /// Render complex path
    /// </summary>
    /// <param name="drawingContext">Drawing context</param>
    /// <param name="geometry">Geometric path</param>
    /// <param name="fillBrush">Fill brush</param>
    /// <param name="strokeBrush">Border brush</param>
    /// <param name="strokeThickness">Border thickness</param>
    void RenderGeometry(
        DrawingContext drawingContext,
        Geometry geometry,
        Brush fillBrush,
        Brush strokeBrush,
        double strokeThickness);
    
    /// <summary>
    /// Begin batch rendering operation
    /// </summary>
    void BeginBatchRender();
    
    /// <summary>
    /// End batch rendering operation
    /// </summary>
    void EndBatchRender();
    
    /// <summary>
    /// Release resources
    /// </summary>
    void Dispose();
    
    /// <summary>
    /// Get rendering performance metrics
    /// </summary>
    /// <returns>Performance metrics</returns>
    GpuRenderingMetrics GetPerformanceMetrics();
}

/// <summary>
/// GPU rendering performance metrics
/// </summary>
public class GpuRenderingMetrics
{
    /// <summary>
    /// Number of rendered elements
    /// </summary>
    public int RenderedElements { get; set; }
    
    /// <summary>
    /// Rendering time (milliseconds)
    /// </summary>
    public double RenderTimeMs { get; set; }
    
    /// <summary>
    /// GPU memory usage (bytes)
    /// </summary>
    public long GpuMemoryUsage { get; set; }
    
    /// <summary>
    /// Frame rate
    /// </summary>
    public double Fps { get; set; }
}
```

### GpuRenderingServiceFactory

Factory for creating GPU rendering services.

```csharp
public static class GpuRenderingServiceFactory
{
    /// <summary>
    /// Create a rendering service for the specified technology
    /// </summary>
    /// <param name="technology">Rendering technology</param>
    /// <returns>Rendering service instance</returns>
    public static IGpuRenderingService CreateService(GpuRenderingTechnology technology);
    
    /// <summary>
    /// Register a new rendering service implementation
    /// </summary>
    /// <param name="technology">Rendering technology</param>
    /// <param name="factory">Service factory function</param>
    public static void RegisterService(GpuRenderingTechnology technology, Func<IGpuRenderingService> factory);
    
    /// <summary>
    /// Get all supported rendering technologies
    /// </summary>
    /// <returns>Array of supported rendering technologies</returns>
    public static GpuRenderingTechnology[] GetSupportedTechnologies();
    
    /// <summary>
    /// Clear all cached service instances
    /// </summary>
    public static void ClearCache();
}
```

### Configuration

GPU rendering can be configured through the RenderingConfiguration:

```csharp
var config = new GanttConfiguration
{
    Rendering = new RenderingConfiguration
    {
        EnableGpuAcceleration = true,
        GpuRenderingTechnology = GpuRenderingTechnology.Direct2D,
        EnableBatchRendering = true
    }
};
ganttContainer.Configuration = config;
```

## Examples

### Creating a Project with Dependencies

```csharp
// Create tasks
var task1 = new GanttTask
{
    Id = Guid.NewGuid(),
    Title = "Requirements Analysis",
    Start = DateTime.Today,
    End = DateTime.Today.AddDays(5),
    RowIndex = 1,
    Priority = TaskPriority.High
};

var task2 = new GanttTask
{
    Id = Guid.NewGuid(), 
    Title = "System Design",
    Start = DateTime.Today.AddDays(5),
    End = DateTime.Today.AddDays(12),
    RowIndex = 2,
    Priority = TaskPriority.High
};

// Create dependency
var dependency = new TaskDependency
{
    PredecessorTaskId = task1.Id.ToString(),
    SuccessorTaskId = task2.Id.ToString(),
    Type = DependencyType.FinishToStart,
    Lag = TimeSpan.FromDays(1)
};

// Add to dependency service
var dependencyService = new DependencyService();
await dependencyService.CreateDependencyAsync(dependency);

// Calculate critical path
var tasks = new List<GanttTask> { task1, task2 };
var dependencies = new List<TaskDependency> { dependency };
var criticalPath = await dependencyService.GetCriticalPathAsync(tasks, dependencies);
```

### Exporting to PDF

```csharp
var exportService = new ExportService();
var options = new ExportOptions
{
    Format = ExportFormat.PDF,
    DPI = 300,
    IncludeDependencies = true,
    Title = "Project Schedule Q1 2024",
    Author = "Project Manager"
};

bool success = await exportService.ExportGanttChartAsync(
    ganttContainer, 
    @"C:\exports\project-schedule.pdf", 
    options
);
```

### Using Working Calendars

```csharp
var calendarService = new CalendarService();

// Create custom calendar
var calendar = new WorkingCalendar
{
    Name = "Project Calendar",
    Type = CalendarType.Custom
};

// Add holiday exception
var holiday = new CalendarException
{
    Name = "Christmas Day",
    Date = new DateTime(2024, 12, 25),
    IsWorkingDay = false,
    IsRecurring = true,
    RecurrencePattern = new RecurrencePattern
    {
        Type = RecurrenceType.Yearly,
        Interval = 1,
        Month = 12,
        DayOfMonth = 25
    }
};

await calendarService.AddCalendarExceptionAsync(calendar.Id, holiday);

// Calculate project duration excluding holidays
var workingDays = await calendarService.CountWorkingDaysAsync(
    DateTime.Today, 
    DateTime.Today.AddDays(30)
);
```

### Applying Project Templates

```csharp
var templateService = new TemplateService();

// Get software development template
var templates = await templateService.GetTemplatesByCategoryAsync(
    TemplateCategory.SoftwareDevelopment
);
var template = templates.FirstOrDefault();

if (template != null)
{
    var options = new TemplateApplicationOptions
    {
        ProjectStartDate = DateTime.Today,
        AutoSchedule = true,
        DurationScale = 1.2, // 20% buffer
        ResourceMappings = new Dictionary<string, string>
        {
            ["Frontend Developer"] = "Alice Johnson",
            ["Backend Developer"] = "Bob Smith", 
            ["QA Engineer"] = "Carol Davis"
        }
    };
    
    var generatedTasks = await templateService.ApplyTemplateAsync(template.Id, options);
}
```

### Complete Integration Example

```csharp
public async Task<List<GanttTask>> CreateProjectFromTemplate(string templateId)
{
    var templateService = new TemplateService();
    var dependencyService = new DependencyService();
    var calendarService = new CalendarService();
    
    // Apply template
    var options = new TemplateApplicationOptions
    {
        ProjectStartDate = DateTime.Today,
        AutoSchedule = true,
        IncludeDependencies = true
    };
    
    var tasks = await templateService.ApplyTemplateAsync(templateId, options);
    
    // Get template dependencies 
    var template = await templateService.GetTemplateAsync(templateId);
    var dependencies = template?.DependencyTemplates.Select(dt => new TaskDependency
    {
        PredecessorTaskId = dt.PredecessorTaskId,
        SuccessorTaskId = dt.SuccessorTaskId,
        Type = dt.Type,
        Lag = dt.Lag
    }).ToList() ?? new List<TaskDependency>();
    
    // Calculate critical path
    var criticalPath = await dependencyService.GetCriticalPathAsync(tasks, dependencies);
    
    // Update critical tasks
    foreach (var task in tasks)
    {
        task.IsCritical = criticalPath.Contains(task.Id.ToString());
    }
    
    // Adjust for working calendar
    var calendar = await calendarService.GetDefaultCalendarAsync();
    foreach (var task in tasks)
    {
        // Adjust dates to working days only
        task.Start = await calendarService.GetNextWorkingDayAsync(task.Start);
        var workingTime = TimeSpan.FromHours(8); // 8 hours per day
        task.End = await calendarService.AddWorkingTimeAsync(task.Start, workingTime);
    }
    
    return tasks;
}
```

## Error Handling

All services implement proper error handling and return meaningful error messages:

```csharp
try
{
    var result = await dependencyService.CreateDependencyAsync(dependency);
}
catch (ArgumentException ex)
{
    // Handle validation errors
    Console.WriteLine($"Validation error: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    // Handle business logic errors (e.g., circular dependencies)
    Console.WriteLine($"Business logic error: {ex.Message}");
}
```

## Performance Considerations

- **Large Datasets**: Use virtualization for 1000+ tasks
- **Dependency Calculation**: Critical path calculation is O(V + E) complexity
- **Export Operations**: Run export operations on background threads
- **Calendar Calculations**: Working time calculations are cached for performance
- **GPU Rendering**: Enable for improved rendering performance on supported hardware

## Thread Safety

- All services are thread-safe for read operations
- Write operations should be synchronized externally
- UI operations must be performed on the UI thread

This concludes the comprehensive API reference for GPM.Gantt v2.1.0.