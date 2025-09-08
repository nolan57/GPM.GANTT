# GPM.Gantt Framework - Updated Documentation (v2.1.0)

## Overview

This document provides updated documentation for the GPM.Gantt WPF component framework, incorporating all recent fixes, improvements, and feature additions. The framework is designed for high-performance project management applications with full MVVM support and advanced performance optimizations.

## Recent Fixes and Improvements (v2.1.0)

### Critical Bug Fixes

1. **Grid Cell Visibility Issue**: Resolved the issue where grid cells were not displaying due to the `ShowGridCells` property being set to `False` by default
2. **Row Height Calculation**: Fixed row height issues where `GridLength.Auto` was not providing sufficient space for elements to render
3. **Theme Application**: Addressed accessibility issues with theme styling methods and ensured proper visual property application
4. **Rendering Logic**: Enhanced the rendering logic in `GanttShapeBase` to properly draw background and border elements
5. **Layout Construction**: Improved the layout building process with proper invalidation and UI updates

### Performance Enhancements

1. **BorderThickness Resource Conversion**: Fixed type conversion issues for BorderThickness properties in theme resource dictionaries
2. **Memory Optimization**: Enhanced memory management with configurable optimization strategies
3. **Element Pooling**: Improved element pooling for better rendering performance
4. **Timeline Caching**: Enhanced caching mechanism for timeline calculations
5. **GPU Rendering Support**: Added support for GPU-accelerated rendering with multiple technology options (Direct2D, DirectX, OpenGL, Vulkan)

## Core Architecture

### Modular Design

The GPM.Gantt framework follows a modular architecture with clear separation of concerns:

- **Models**: Core data models (`GanttTask`, `TimeUnit`, etc.)
- **ViewModels**: MVVM support with `GanttChartViewModel` and `GanttTaskViewModel`
- **Services**: Business logic and data operations (`IGanttService`, `IValidationService`)
- **Utilities**: Helper classes for timeline calculations and conversions
- **Configuration**: Flexible configuration system for customization

### Design Patterns

- **MVVM (Model-View-ViewModel)**: For UI and logic separation
- **Factory Pattern**: `TaskBarShapeRendererFactory` for creating shape renderers
- **Strategy Pattern**: For memory optimization strategies
- **Command Pattern**: In ViewModel for handling user interactions
- **Observer Pattern**: For property change notifications in ViewModel
- **Dependency Injection**: In services like `IThemeService`, `IValidationService`

## Key Components

### GanttContainer

The main Gantt chart container that uses Grid layout to display tasks and timeline. It generates rows and columns based on time range and task count.

#### Properties

- **StartTime/EndTime**: Timeline range properties
- **TaskCount**: Number of task rows to display
- **ShowGridCells**: Toggle for showing individual grid cells vs. full rows
- **ClampTasksToVisibleRows**: Control whether tasks outside visible range are clamped or skipped
- **TimeUnit**: Time scale unit (Hour, Day, Week, Month, Year)
- **HeaderRowHeight/TaskRowHeight**: Row height configuration
- **Tasks**: Collection of tasks to display
- **Configuration**: Gantt configuration settings
- **DateFormat/TimeFormat**: Date and time formatting strings
- **Culture**: Culture used for formatting
- **Theme**: Current theme for the Gantt chart
- **Dependencies**: Collection of task dependencies
- **Services**: Various service integrations (Calendar, Dependency, Export, Template)
- **IsInteractionEnabled**: Enable/disable interactive features
- **IsDragDropEnabled**: Enable/disable drag and drop operations
- **IsResizeEnabled**: Enable/disable task resizing operations
- **IsMultiSelectionEnabled**: Enable/disable multi-selection
- **ShowDependencyLines**: Toggle for showing dependency lines
- **HighlightCriticalPath**: Highlight critical path tasks

#### Recent Improvements

- Enhanced row height calculation with fallback to default 30px height when auto or zero heights are detected
- Improved layout invalidation handling
- Explicit theme application after layout construction
- Proper Z-index setting for task bars
- Added GPU rendering support with multiple technology options
- Enhanced virtualization with viewport tracking
- Performance optimizations with element pooling and caching

### Task Management

#### GanttTask Model

Primary model representing a task in the Gantt chart with properties including:
- `Id`: Unique identifier
- `Start/End`: Task duration
- `RowIndex`: Display position
- `Progress`: Completion percentage
- `Shape`: Task bar shape
- `Dependencies`: List of predecessor task IDs
- `Priority`: Task priority level
- `Status`: Current task status
- `ShapeParameters`: Custom parameters for task shapes
- `IsCritical`: Whether task is on critical path
- `FreeFloat/TotalFloat`: Float time calculations
- `EarliestStart/LatestStart`: Schedule constraints

#### Task Dependencies

Support for task relationships with visual dependency lines:
- Finish-to-Start (default)
- Start-to-Start
- Finish-to-Finish
- Start-to-Finish
- Lag time support
- Critical path highlighting

### Custom Shapes

Support for various task bar shapes beyond traditional rectangles:
- **Rectangle**: Standard rectangular task bar
- **DiamondEnds**: Rectangular bar with diamond-shaped ends
- **RoundedRectangle**: Rectangle with rounded corners
- **Chevron**: Arrow/chevron-shaped task bar
- **Milestone**: Diamond-shaped milestone marker
- **Custom**: User-defined custom shapes

#### Shape Rendering Parameters

Configuration for shape appearance including:
- Diamond end height and width
- Corner radius for rounded shapes
- Chevron angle
- Custom properties for extensibility

### Theme Management

Comprehensive theming system with built-in and custom themes:
- **Default**: Professional appearance with balanced colors
- **Dark**: Dark theme suitable for low-light environments
- **Light**: High-contrast theme for improved readability
- **Modern**: Modern flat design with vibrant colors

#### Theme Properties

- Background colors and styling
- Grid line colors and thickness
- Task bar colors and styling
- Time scale styling
- Selection effects
- Annotation theming

### Performance Optimization

Advanced performance features for handling large datasets:
- **UI Virtualization**: Efficient handling of large datasets with viewport-based rendering
- **Memory Optimization**: Automatic memory management with configurable strategies
- **Element Pooling**: Reusable UI elements for improved rendering performance
- **Timeline Caching**: Intelligent caching of timeline calculations
- **Performance Monitoring**: Real-time performance diagnostics and recommendations
- **GPU Rendering**: Hardware-accelerated rendering for improved performance

#### Virtualization Configuration

```csharp
var config = new GanttConfiguration
{
    Rendering = new RenderingConfiguration
    {
        EnableVirtualization = true,
        MaxVisibleTasks = 1000,
        PerformanceLevel = PerformanceLevel.Performance
    }
};
```

#### GPU Rendering Configuration

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
```

### Date and Time Formatting

Flexible date/time formatting with full culture and localization support:
- Configurable date format strings
- Configurable time format strings
- Culture-specific formatting
- Performance-optimized format caching

### Plugin-Based Annotation System

Extensible annotation framework with:
- **Text Annotations**: Text-based annotations with rich formatting
- **Shape Annotations**: Geometric shapes for visual emphasis
- **Line Annotations**: Lines and arrows for connections

### Multi-Level Time Scale Display

Simultaneous visualization of multiple time granularities:
- Year, Quarter, Month, Week, Day, Hour levels
- Configurable styling for each level
- Intelligent visibility management

### Expandable Time Axis Segments

Dynamically expand specific time periods while maintaining other granularities:
- Week to day expansion
- Month to week expansion
- Custom expansion configurations

## Integration Examples

### Basic Usage

```xml
<!-- In your XAML -->
<Window xmlns:gantt="clr-namespace:GPM.Gantt;assembly=GPM.Gantt">
    <gantt:GanttContainer x:Name="ganttChart"
                         StartTime="{Binding StartDate}"
                         EndTime="{Binding EndDate}"
                         Tasks="{Binding Tasks}"
                         TimeUnit="Day"
                         Theme="{Binding SelectedTheme}"
                         DateFormat="MMM dd"
                         TimeFormat="HH:mm" />
</Window>
```

```csharp
// In your code-behind or ViewModel
public ObservableCollection<GanttTask> Tasks { get; } = new();

// Add basic tasks
Tasks.Add(new GanttTask
{
    Title = "Project Planning",
    Start = DateTime.Today,
    End = DateTime.Today.AddDays(5),
    RowIndex = 1,
    Progress = 75,
    Shape = TaskBarShape.Rectangle,
    Status = TaskStatus.InProgress
});
```

### Advanced Configuration

```csharp
var config = new GanttConfiguration
{
    Rendering = new RenderingConfiguration
    {
        ShowGridCells = true,
        GridMode = GridRenderingMode.Rectangles,
        UseEnhancedShapeRendering = true,
        EnableVirtualization = true,
        MaxVisibleTasks = 1000,
        EnableAutoMemoryOptimization = true,
        PerformanceLevel = PerformanceLevel.Performance,
        EnableGpuAcceleration = true,
        GpuRenderingTechnology = GpuRenderingTechnology.Direct2D
    }
};
ganttChart.Configuration = config;
```

### Theme Application

```csharp
// Apply built-in themes
ganttChart.Theme = ThemeManager.GetTheme("Dark");

// Create custom theme
var customTheme = ThemeManager.CreateCustomTheme("Corporate", theme =>
{
    theme.Task.DefaultColor = Colors.Blue;
    theme.Background.PrimaryColor = Colors.White;
    theme.Grid.LineColor = Color.FromRgb(200, 200, 200);
});
```

### GPU Rendering Setup

```csharp
// Enable GPU acceleration
var config = new GanttConfiguration
{
    Rendering = new RenderingConfiguration
    {
        EnableGpuAcceleration = true,
        GpuRenderingTechnology = GpuRenderingTechnology.Direct2D,
        EnableBatchRendering = true
    }
};
ganttChart.Configuration = config;
```

## Best Practices

### Performance Optimization

1. **Enable Virtualization**: For datasets with more than 500 tasks
2. **Use Appropriate Time Units**: Match time unit to data range
3. **Configure Memory Optimization**: Enable automatic memory management
4. **Batch Updates**: Use BeginInit/EndInit for bulk operations
5. **Enable GPU Rendering**: For improved rendering performance on supported hardware

### Error Handling

1. **Validate Task Data**: Ensure tasks have valid dates and row indices
2. **Handle Theme Errors**: Implement fallback mechanisms for theme application
3. **Monitor Performance**: Use performance diagnostics for optimization

### Data Binding

1. **Use ObservableCollection**: For dynamic task collections
2. **Implement INotifyPropertyChanged**: For task property updates
3. **Initialize in Correct Order**: Configuration → Theme → Data

## Troubleshooting

### Common Issues and Solutions

1. **Tasks Not Displaying**:
   - Verify `ShowGridCells` is set to `True` if grid visibility is needed
   - Check that row heights are properly configured (not `Auto`)
   - Ensure tasks have valid `Start`/`End` dates and `RowIndex` values

2. **Theme Not Applied**:
   - Ensure theme is set after container is loaded
   - Verify BorderThickness resource conversion (fixed in v2.0.1+)

3. **Performance Issues**:
   - Enable virtualization for large datasets
   - Configure memory optimization strategies
   - Monitor with performance diagnostics
   - Enable GPU rendering for improved performance

4. **Custom Shapes Not Rendering**:
   - Enable enhanced shape rendering in configuration
   - Verify shape parameters are correctly set
   - Check that shape renderers are properly registered

## API Reference Summary

For detailed API documentation, refer to the [API_REFERENCE.md](API_REFERENCE.md) file which includes:
- Core Models (GanttTask, TaskDependency, DependencyLine)
- Services (IDependencyService, IExportService, ICalendarService, ITemplateService)
- Plugin System (IAnnotationPlugin, IPluginService)
- Multi-Level Time Scale components
- Calendar System (WorkingCalendar, WorkingTime, CalendarException)
- Templates (ProjectTemplate, TaskTemplate)
- Complete usage examples

## Testing and Quality Assurance

The framework includes comprehensive unit tests covering:
- Timeline calculations and caching
- Task validation and error handling
- ViewModel functionality
- Service operations
- Theme management
- Performance optimization
- Shape rendering
- Error handling and recovery

## Roadmap and Future Features

### Current Version (2.1.0)
- ✅ Advanced Gantt chart with custom shapes
- ✅ Comprehensive theme management system
- ✅ Performance optimization and memory management
- ✅ UI virtualization for large datasets
- ✅ Custom date/time formatting with culture support
- ✅ Interactive controls with drag-drop and resize
- ✅ Plugin-based annotation system
- ✅ Multi-level time scale display
- ✅ Expandable time axis segments
- ✅ GPU rendering support with multiple technologies

### Upcoming Features (v3.0.0)
- 🚧 Task dependencies with automatic layout
- 🚧 Export capabilities (PDF, PNG, Excel, SVG)
- 🚧 Advanced timeline features (baselines, critical path)
- 🚧 Collaborative editing support
- 🚧 Mobile/touch support optimization

## Technical Specifications

### Performance Characteristics
- **Large Dataset Support**: Tested with 10,000+ tasks
- **Memory Efficiency**: Automatic optimization with configurable strategies
- **Rendering Performance**: 60 FPS target with virtualization
- **Memory Footprint**: Optimized for enterprise applications

### System Requirements
- **.NET 9.0** or later
- **Windows OS** (WPF requirement)
- **Minimum RAM**: 512 MB (2 GB recommended for large datasets)
- **Visual Studio 2022** or compatible IDE for development

## Conclusion

The GPM.Gantt framework provides a robust, high-performance solution for project management visualization in WPF applications. With its modular architecture, comprehensive feature set, and performance optimizations, it is well-suited for enterprise-grade applications requiring advanced Gantt chart capabilities.