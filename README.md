# GPM.Gantt - WPF Gantt Chart Component Library

[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![WPF](https://img.shields.io/badge/WPF-Windows-blue.svg)](https://docs.microsoft.com/en-us/dotnet/framework/wpf/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

A modern, enterprise-grade WPF Gantt chart component library designed for high-performance project management applications. GPM.Gantt provides a flexible, customizable, and scalable solution for visualizing project timelines, tasks, and dependencies with full MVVM support and advanced performance optimizations.

## 🚀 Features

### Core Features
- **Advanced Grid Layout**: Responsive grid-based rendering with rectangle and line modes
- **Multiple Time Units**: Support for Hour, Day, Week, Month, and Year time scales with intelligent scaling
- **MVVM Architecture**: Complete MVVM support with data binding and command patterns
- **Custom Task Shapes**: Support for rectangle, diamond-end, chevron, rounded, milestone, and custom shapes
- **Comprehensive Theming**: Built-in themes (Default, Dark, Light, Modern) with custom theme support
- **Date/Time Formatting**: Configurable formats with full culture and localization support

### Performance & Scalability
- **UI Virtualization**: Efficient handling of large datasets with viewport-based rendering
- **Memory Optimization**: Automatic memory management with configurable optimization strategies
- **Performance Monitoring**: Real-time performance diagnostics and optimization recommendations
- **Element Pooling**: Reusable UI elements for improved rendering performance
- **Async Operations**: Non-blocking async service layer for data operations

### Advanced Features
- **Plugin-Based Annotation System**: Extensible annotation framework with text, shape, and line plugins
- **Expandable Time Axis Segments**: Dynamically expand specific time periods (e.g., weeks to days) while maintaining other granularities
- **Multi-Level Time Scale Display**: Simultaneous visualization of year, quarter, month, week, day, and hour levels
- **Interactive Controls**: Full drag-drop, resize, and selection capabilities
- **Validation System**: Comprehensive task and data validation with detailed error reporting
- **Error Handling**: Robust error handling with fallback mechanisms and debugging support
- **Extensible Architecture**: Plugin-ready architecture with service abstraction

## 📦 Installation

### NuGet Package (Coming Soon)
```bash
Install-Package GPM.Gantt
```

### Build from Source
```bash
git clone https://github.com/yourorg/GPM.git
cd GPM
dotnet build
```

## 🎯 Quick Start

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

// Add milestone
Tasks.Add(new GanttTask
{
    Title = "Project Kickoff",
    Start = DateTime.Today,
    End = DateTime.Today,
    RowIndex = 2,
    Shape = TaskBarShape.Milestone,
    Priority = TaskPriority.High
});
```

### Advanced MVVM Integration

```csharp
public class ProjectViewModel : ViewModelBase
{
    private readonly GanttChartViewModel _ganttViewModel;
    private readonly IThemeService _themeService;
    private readonly IPerformanceService _performanceService;
    
    public ProjectViewModel()
    {
        _ganttViewModel = new GanttChartViewModel();
        _themeService = new ThemeService();
        _performanceService = new PerformanceService();
        
        LoadSampleData();
        ConfigurePerformance();
    }
    
    public GanttChartViewModel GanttChart => _ganttViewModel;
    public IEnumerable<string> AvailableThemes => _themeService.GetAvailableThemes();
    
    private void LoadSampleData()
    {
        _ganttViewModel.Tasks.Add(new GanttTaskViewModel(new GanttTask
        {
            Title = "Design Phase",
            Start = DateTime.Today,
            End = DateTime.Today.AddDays(10),
            RowIndex = 1,
            Status = TaskStatus.InProgress,
            Shape = TaskBarShape.DiamondEnds,
            Priority = TaskPriority.High
        }));
    }
    
    private void ConfigurePerformance()
    {
        // Enable performance optimizations for large datasets
        _performanceService.GetDiagnostics().StartMonitoring();
        _performanceService.GetMemoryOptimization().EnableAutoOptimization(TimeSpan.FromMinutes(5));
    }
}
```

## 📖 Documentation

### Core Documentation
- [**Custom Shapes Guide**](CUSTOM_SHAPES_GUIDE.md) - Complete guide to custom task bar shapes
- [**Theme Management**](THEME_MANAGEMENT_GUIDE.md) - Comprehensive theming system documentation
- [**Date/Time Formatting**](Date_Time_Format_Implementation.md) - Custom date and time format implementation

### API & Development
- [**API Reference**](docs/api-reference.md) - Detailed API documentation
- [**Performance Guide**](docs/performance.md) - Performance optimization and monitoring
- [**Memory Management**](docs/memory-management.md) - Memory optimization strategies
- [**Virtualization**](docs/virtualization.md) - UI virtualization for large datasets
- [**Error Handling**](docs/error-handling.md) - Error handling and debugging
- [**Troubleshooting**](docs/troubleshooting.md) - Common issues and solutions

## 🏗️ Architecture

GPM.Gantt follows a modular architecture with clear separation of concerns:

- **Models**: Core data models (`GanttTask`, `TimeUnit`, etc.)
- **ViewModels**: MVVM support with `GanttChartViewModel` and `GanttTaskViewModel`
- **Services**: Business logic and data operations (`IGanttService`, `IValidationService`)
- **Utilities**: Helper classes for timeline calculations and conversions
- **Configuration**: Flexible configuration system for customization

## 🎨 Customization

### Theme Management
```csharp
// Apply built-in themes
ganttChart.Theme = ThemeManager.GetTheme("Dark");
ganttChart.Theme = ThemeManager.GetTheme("Modern");

// Create custom theme
var customTheme = ThemeManager.CreateCustomTheme("Corporate", theme =>
{
    theme.Task.DefaultColor = Colors.Blue;
    theme.Background.PrimaryColor = Colors.White;
    theme.Grid.LineColor = Color.FromRgb(200, 200, 200);
});
```

### Time and Date Formatting
```csharp
ganttChart.DateFormat = "yyyy-MM-dd";
ganttChart.TimeFormat = "HH:mm";
ganttChart.Culture = new CultureInfo("en-US");
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
        PerformanceLevel = PerformanceLevel.Performance
    }
};
ganttChart.Configuration = config;
```

### Custom Task Shapes
```csharp
// Diamond-ended task
var importantTask = new GanttTask
{
    Title = "Critical Milestone",
    Shape = TaskBarShape.DiamondEnds,
    ShapeParameters = new ShapeRenderingParameters
    {
        DiamondEndHeight = 0.9,
        DiamondEndWidth = 16
    }
};

// Chevron arrow task
var sequentialTask = new GanttTask
{
    Title = "Sequential Step",
    Shape = TaskBarShape.Chevron,
    ShapeParameters = new ShapeRenderingParameters
    {
        ChevronAngle = 20
    }
};
```

## 🧪 Testing

Run the test suite:
```bash
dotnet test GPM.Gantt.Tests
```

The library includes comprehensive unit tests covering:
- **Timeline Calculations**: Date/time calculations and caching
- **Task Validation**: Comprehensive validation rules and error handling
- **ViewModel Functionality**: MVVM pattern implementation
- **Service Operations**: All service layer functionality
- **Theme Management**: Theme creation, application, and switching
- **Performance Testing**: Memory optimization and performance monitoring
- **Shape Rendering**: Custom task bar shape rendering
- **Error Handling**: Exception handling and recovery mechanisms

### Test Results
- **Total Tests**: 65+
- **Coverage**: Core functionality, edge cases, and error scenarios
- **Performance Tests**: Memory usage, rendering performance, large datasets

## 🚀 Demo Application

The `GPM.Gantt.Demo` project provides a full-featured example showing:
- Interactive controls for all properties
- Real-time configuration changes
- Async task operations
- MVVM data binding

To run the demo:
```bash
cd GPM.Gantt.Demo
dotnet run
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup
1. Clone the repository
2. Open in Visual Studio 2022 or VS Code
3. Build the solution: `dotnet build`
4. Run tests: `dotnet test`

## 📋 Requirements

- .NET 9.0 or later
- Windows OS (WPF requirement)
- Visual Studio 2022 or compatible IDE

## 🗺️ Roadmap

### Current Version (2.1.0)
- ✅ Advanced Gantt chart with custom shapes
- ✅ Comprehensive theme management system
- ✅ Performance optimization and memory management
- ✅ UI virtualization for large datasets
- ✅ Custom date/time formatting with culture support
- ✅ Interactive controls with drag-drop and resize
- ✅ Comprehensive validation and error handling
- ✅ Performance monitoring and diagnostics
- ✅ Plugin-based annotation system with text, shape, and line annotations
- ✅ Expandable time axis segments for detailed time period exploration
- ✅ Multi-level time scale display with intelligent visibility management

### Recent Improvements (v2.0.1)
- ✅ Fixed BorderThickness theme resource conversion bug
- ✅ Enhanced memory optimization with multiple strategies
- ✅ Improved error handling with detailed logging
- ✅ Performance diagnostics with real-time recommendations
- ✅ Element pooling for better rendering performance

### Upcoming Features (v3.0.0)
- 🚧 Task dependencies with automatic layout
- 🚧 Export capabilities (PDF, PNG, Excel, SVG)
- 🚧 Advanced timeline features (baselines, critical path)
- 🚧 Collaborative editing support
- 🚧 Mobile/touch support optimization

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Documentation**: Check our [docs](docs/) folder
- **Issues**: Report bugs on [GitHub Issues](https://github.com/yourorg/GPM/issues)
- **Discussions**: Join our [GitHub Discussions](https://github.com/yourorg/GPM/discussions)

## 🔧 Technical Specifications

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

## 🙏 Acknowledgments

- Built with modern WPF and .NET 9.0
- Inspired by enterprise project management requirements
- Extensive performance testing and optimization
- Community feedback and real-world usage scenarios
- Advanced architectural patterns and best practices

---

*GPM.Gantt - Enterprise-grade project visualization for modern applications.*