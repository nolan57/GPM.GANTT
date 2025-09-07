# Configuration Guide - GPM.Gantt v2.1.0

This guide covers all configuration options available in GPM.Gantt to customize the appearance and behavior of your Gantt charts, including the advanced features introduced in version 2.1.0.

## Configuration Overview

GPM.Gantt uses a hierarchical configuration system with three main areas:
- **Time Scale Configuration**: Controls time display and formatting
- **Rendering Configuration**: Controls visual appearance and performance
- **Localization Configuration**: Controls culture and internationalization

## GanttConfiguration

The main configuration class that contains all settings:

```csharp
var config = new GanttConfiguration
{
    TimeScale = new TimeScaleConfiguration { /* settings */ },
    Rendering = new RenderingConfiguration { /* settings */ },
    Localization = new LocalizationConfiguration { /* settings */ }
};

ganttContainer.Configuration = config;
```

### Using Default Configuration

```csharp
// Use default settings
ganttContainer.Configuration = GanttConfiguration.Default();
```

## Time Scale Configuration

Controls how time is displayed and formatted in the Gantt chart.

### Basic Time Scale Settings

```csharp
var timeScale = new TimeScaleConfiguration
{
    DefaultTimeUnit = TimeUnit.Day,     // Default time unit
    DateFormat = "yyyy-MM-dd",          // Date format string
    TimeFormat = "HH:mm",               // Time format string
    Culture = new CultureInfo("en-US"), // Formatting culture
    HighlightWeekends = true,           // Highlight weekends
    HighlightToday = true               // Highlight current day
};
```

### Time Units

Available time units and their behaviors:

| Time Unit | Description | Typical Use Case |
|-----------|-------------|------------------|
| `TimeUnit.Hour` | Hourly granularity | Short-term detailed planning |
| `TimeUnit.Day` | Daily granularity | Standard project planning |
| `TimeUnit.Week` | Weekly granularity | High-level project overview |
| `TimeUnit.Month` | Monthly granularity | Long-term planning |
| `TimeUnit.Year` | Yearly granularity | Strategic planning |

### Date and Time Formatting

Customize how dates and times are displayed:

```csharp
// Different date format examples
ganttContainer.DateFormat = "MM/dd/yyyy";    // 12/31/2023
ganttContainer.DateFormat = "dd MMM yyyy";   // 31 Dec 2023
ganttContainer.DateFormat = "yyyy-MM-dd";    // 2023-12-31
ganttContainer.DateFormat = "MMM dd";        // Dec 31

// Different time format examples
ganttContainer.TimeFormat = "HH:mm";         // 14:30
ganttContainer.TimeFormat = "hh:mm tt";      // 02:30 PM
ganttContainer.TimeFormat = "HH:mm:ss";      // 14:30:45
```

### Culture-Specific Formatting

Set culture for localized formatting:

```csharp
// German culture
ganttContainer.Culture = new CultureInfo("de-DE");

// Japanese culture
ganttContainer.Culture = new CultureInfo("ja-JP");

// Custom culture with specific settings
var culture = new CultureInfo("en-US");
culture.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Monday;
ganttContainer.Culture = culture;
```

## Rendering Configuration

Controls visual appearance and performance aspects.

### Basic Rendering Settings

```csharp
var rendering = new RenderingConfiguration
{
    EnableVirtualization = true,              // Enable virtualization for performance
    MaxVisibleTasks = 1000,                  // Max tasks before virtualization
    EnableCaching = true,                    // Cache rendered elements
    GridMode = GridRenderingMode.Rectangles, // Grid rendering mode
    ShowGridCells = false,                   // Show individual grid cells
    TaskBarCornerRadius = 4.0                // Task bar corner radius
};
```

### Grid Rendering Modes

```csharp
// Rectangle-based grid (default, better performance)
config.Rendering.GridMode = GridRenderingMode.Rectangles;

// Line-based grid (alternative rendering)
config.Rendering.GridMode = GridRenderingMode.Lines;
```

### Grid Display Options

```csharp
// Show full rows (default)
ganttContainer.ShowGridCells = false;

// Show individual cells for more granular styling
ganttContainer.ShowGridCells = true;
```

### Performance Settings

```csharp
var rendering = new RenderingConfiguration
{
    // Enable virtualization for large datasets
    EnableVirtualization = true,
    
    // Virtualization threshold
    MaxVisibleTasks = 500,
    
    // Cache rendered elements for better scrolling performance
    EnableCaching = true
};
```

## Localization Configuration

Configure culture and internationalization settings.

### Basic Localization

```csharp
var localization = new LocalizationConfiguration
{
    UICulture = new CultureInfo("fr-FR"),  // French UI culture
    IsRightToLeft = false                  // Left-to-right layout
};
```

### Right-to-Left Languages

```csharp
var localization = new LocalizationConfiguration
{
    UICulture = new CultureInfo("ar-SA"),  // Arabic culture
    IsRightToLeft = true                   // Right-to-left layout
};
```

## Complete Configuration Examples

### Project Management Setup

```csharp
var projectConfig = new GanttConfiguration
{
    TimeScale = new TimeScaleConfiguration
    {
        DefaultTimeUnit = TimeUnit.Day,
        DateFormat = "MMM dd, yyyy",
        HighlightWeekends = true,
        HighlightToday = true,
        Culture = CultureInfo.CurrentCulture
    },
    Rendering = new RenderingConfiguration
    {
        ShowGridCells = true,
        GridMode = GridRenderingMode.Rectangles,
        TaskBarCornerRadius = 6.0,
        EnableVirtualization = true,
        MaxVisibleTasks = 1000
    },
    Localization = new LocalizationConfiguration
    {
        UICulture = CultureInfo.CurrentUICulture,
        IsRightToLeft = false
    }
};
```

### Manufacturing Schedule Setup

```csharp
var manufacturingConfig = new GanttConfiguration
{
    TimeScale = new TimeScaleConfiguration
    {
        DefaultTimeUnit = TimeUnit.Hour,
        DateFormat = "yyyy-MM-dd",
        TimeFormat = "HH:mm",
        HighlightWeekends = false,  // 24/7 operation
        HighlightToday = true
    },
    Rendering = new RenderingConfiguration
    {
        ShowGridCells = false,
        TaskBarCornerRadius = 2.0,
        EnableVirtualization = true
    }
};
```

### Long-term Strategic Planning

```csharp
var strategicConfig = new GanttConfiguration
{
    TimeScale = new TimeScaleConfiguration
    {
        DefaultTimeUnit = TimeUnit.Month,
        DateFormat = "yyyy MMM",
        HighlightWeekends = false,  // Not relevant at month level
        HighlightToday = false
    },
    Rendering = new RenderingConfiguration
    {
        ShowGridCells = true,
        TaskBarCornerRadius = 8.0,
        EnableVirtualization = false  // Fewer tasks at strategic level
    }
};
```

## Dynamic Configuration Changes

Configuration can be changed at runtime:

```csharp
// Switch to weekly view
ganttContainer.TimeUnit = TimeUnit.Week;

// Change date format on the fly
ganttContainer.DateFormat = "dd/MM/yyyy";

// Toggle grid cells
ganttContainer.ShowGridCells = !ganttContainer.ShowGridCells;

// Update entire configuration
ganttContainer.Configuration = newConfiguration;
```

## XAML Configuration

You can also configure many settings directly in XAML:

```xml
<gantt:GanttContainer x:Name="ganttChart"
                     TimeUnit="Week"
                     DateFormat="MMM dd"
                     TimeFormat="HH:mm"
                     ShowGridCells="True"
                     TaskRowHeight="50"
                     HeaderRowHeight="40">
    
    <!-- Configure via Configuration property -->
    <gantt:GanttContainer.Configuration>
        <gantt:GanttConfiguration>
            <gantt:GanttConfiguration.TimeScale>
                <gantt:TimeScaleConfiguration DefaultTimeUnit="Day"
                                            HighlightWeekends="True"
                                            HighlightToday="True" />
            </gantt:GanttConfiguration.TimeScale>
            <gantt:GanttConfiguration.Rendering>
                <gantt:RenderingConfiguration ShowGridCells="True"
                                            TaskBarCornerRadius="4"
                                            EnableVirtualization="True" />
            </gantt:GanttConfiguration.Rendering>
        </gantt:GanttConfiguration>
    </gantt:GanttContainer.Configuration>
</gantt:GanttContainer>
```

## Styling and Themes

### Custom Styles

Define custom styles for Gantt components:

```xml
<Window.Resources>
    <!-- Time cell styling -->
    <Style TargetType="{x:Type gantt:GanttTimeCell}">
        <Setter Property="Background" Value="#F8F9FA"/>
        <Setter Property="FontWeight" Value="SemiBold"/>
        <Setter Property="FontSize" Value="12"/>
        <Style.Triggers>
            <Trigger Property="IsWeekend" Value="True">
                <Setter Property="Background" Value="#FFF5F5"/>
            </Trigger>
            <Trigger Property="IsToday" Value="True">
                <Setter Property="Background" Value="#E8F5E8"/>
                <Setter Property="FontWeight" Value="Bold"/>
            </Trigger>
        </Style.Triggers>
    </Style>
    
    <!-- Grid cell styling -->
    <Style TargetType="{x:Type gantt:GanttGridCell}">
        <Setter Property="Background" Value="White"/>
        <Setter Property="OutlineBrush" Value="#E0E0E0"/>
        <Setter Property="OutlineThickness" Value="0,0,1,1"/>
        <Style.Triggers>
            <Trigger Property="IsWeekend" Value="True">
                <Setter Property="Background" Value="#FCFCFC"/>
            </Trigger>
            <Trigger Property="IsToday" Value="True">
                <Setter Property="Background" Value="#E8F5E8"/>
            </Trigger>
        </Style.Triggers>
    </Style>
    
    <!-- Task bar styling -->
    <Style TargetType="{x:Type gantt:GanttTaskBar}">
        <Setter Property="Background" Value="#2196F3"/>
        <Setter Property="OutlineBrush" Value="#1976D2"/>
        <Setter Property="OutlineThickness" Value="1"/>
        <Style.Triggers>
            <Trigger Property="IsHovered" Value="True">
                <Setter Property="Background" Value="#1976D2"/>
            </Trigger>
        </Style.Triggers>
    </Style>
</Window.Resources>
```

### Resource Dictionary Themes

Create reusable theme resource dictionaries:

```xml
<!-- Themes/DarkTheme.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:gantt="clr-namespace:GPM.Gantt;assembly=GPM.Gantt">
    
    <!-- Dark theme colors -->
    <SolidColorBrush x:Key="GanttBackgroundBrush" Color="#2D2D30"/>
    <SolidColorBrush x:Key="GanttGridBrush" Color="#3F3F46"/>
    <SolidColorBrush x:Key="GanttTextBrush" Color="#FFFFFF"/>
    
    <!-- Apply dark theme styles -->
    <Style TargetType="{x:Type gantt:GanttTimeCell}">
        <Setter Property="Background" Value="{StaticResource GanttBackgroundBrush}"/>
        <Setter Property="Foreground" Value="{StaticResource GanttTextBrush}"/>
        <Setter Property="OutlineBrush" Value="{StaticResource GanttGridBrush}"/>
    </Style>
    
    <!-- ... other styles ... -->
</ResourceDictionary>
```

## Best Practices

### Performance Optimization

1. **Use appropriate time units**: Don't use `TimeUnit.Hour` for long-term projects
2. **Enable virtualization**: For datasets > 100 tasks
3. **Limit visible tasks**: Use `TaskCount` to limit rendered rows
4. **Cache configuration**: Reuse configuration objects when possible

### User Experience

1. **Consistent formatting**: Use consistent date/time formats throughout your application
2. **Cultural awareness**: Set appropriate culture for your users
3. **Visual hierarchy**: Use different styles for different task types/priorities
4. **Responsive design**: Test with different window sizes and DPI settings

### Maintainability

1. **Centralized configuration**: Create configuration in one place and reuse
2. **Theme consistency**: Use resource dictionaries for consistent theming
3. **Configuration validation**: Validate configuration settings at runtime
4. **Documentation**: Document custom configurations and their use cases

## Advanced Configuration (v2.1.0)

GPM.Gantt v2.1.0 introduces powerful new configuration options for enhanced visualization.

### Multi-Level Time Scale Configuration

Configure multiple time granularities simultaneously:

```csharp
var multiLevelConfig = new MultiLevelTimeScaleConfiguration
{
    Levels = new List<TimeLevelConfiguration>
    {
        new TimeLevelConfiguration
        {
            Unit = ExtendedTimeUnit.Year,
            IsVisible = true,
            Height = 35,
            BackgroundColor = "#FF2C3E50",
            TextColor = "#FFFFFFFF",
            FontFamily = "Segoe UI",
            FontSize = 16,
            DateFormat = "yyyy",
            TextAlignment = HorizontalAlignment.Center,
            ShowBorders = true,
            BorderColor = "#FF34495E",
            BorderThickness = 1,
            ZIndex = 4
        },
        new TimeLevelConfiguration
        {
            Unit = ExtendedTimeUnit.Month,
            IsVisible = true,
            Height = 25,
            BackgroundColor = "#FF3498DB",
            TextColor = "#FFFFFFFF",
            FontFamily = "Segoe UI",
            FontSize = 12,
            DateFormat = "MMM",
            TextAlignment = HorizontalAlignment.Center,
            ShowBorders = true,
            BorderColor = "#FF2980B9",
            BorderThickness = 1,
            ZIndex = 2
        }
    },
    TotalHeight = 60,
    EnableSmartVisibility = true,
    VisibilityThreshold = TimeSpan.FromDays(365),
    EnableAutoFit = true
};

ganttContainer.MultiLevelTimeScale = multiLevelConfig;
```

### Time Segment Expansion Configuration

Set up interactive time period expansion:

```csharp
// Configure default expansion behavior
var expansionConfig = new TimeSegmentExpansionConfiguration
{
    DefaultExpandedUnit = ExtendedTimeUnit.Day,
    AnimationDuration = TimeSpan.FromMilliseconds(300),
    EnableSmoothTransitions = true,
    MaxConcurrentExpansions = 3
};

ganttContainer.TimeSegmentExpansionConfig = expansionConfig;

// Add specific expansions
var expansion = new TimeSegmentExpansion
{
    StartTime = DateTime.Today.AddDays(14),
    EndTime = DateTime.Today.AddDays(21),
    OriginalUnit = ExtendedTimeUnit.Week,
    ExpandedUnit = ExtendedTimeUnit.Day,
    IsExpanded = false,
    IsCollapsible = true,
    DisplayName = "Detailed View"
};

ganttContainer.TimeSegmentExpansions.Add(expansion);
```

### Plugin System Configuration

Configure annotation plugins and their behavior:

```csharp
var pluginConfig = new PluginConfiguration
{
    EnableElementPooling = true,
    MaxPooledElements = 100,
    EnableLazyLoading = true,
    PluginLoadingTimeout = TimeSpan.FromSeconds(30)
};

ganttContainer.PluginConfiguration = pluginConfig;

// Register plugins
var pluginService = new PluginService(pluginConfig);
pluginService.RegisterPlugin(new TextAnnotationPlugin());
pluginService.RegisterPlugin(new ShapeAnnotationPlugin());
pluginService.RegisterPlugin(new LineAnnotationPlugin());
```

These advanced configuration options provide powerful capabilities for creating sophisticated Gantt chart visualizations.