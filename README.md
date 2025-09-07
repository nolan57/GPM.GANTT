# GPM.Gantt - WPF Gantt Chart Component Library

[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![WPF](https://img.shields.io/badge/WPF-Windows-blue.svg)](https://docs.microsoft.com/en-us/dotnet/framework/wpf/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

A modern, feature-rich WPF Gantt chart component library designed for project management applications. GPM.Gantt provides a flexible and customizable way to visualize project timelines, tasks, and dependencies with full MVVM support.

## 🚀 Features

- **Grid-based Layout**: Clean, responsive grid layout for optimal task visualization
- **Multiple Time Units**: Support for Hour, Day, Week, Month, and Year time scales
- **MVVM Architecture**: Full support for data binding and MVVM patterns
- **Customizable Formatting**: Configurable date/time formats with culture support
- **Task Management**: Comprehensive task model with validation and dependencies
- **Interactive UI**: Hover effects and visual feedback
- **Extensible Configuration**: Flexible configuration system for rendering and behavior
- **Async Operations**: Async service layer for data operations

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
                         TimeUnit="Day" />
</Window>
```

```csharp
// In your code-behind or ViewModel
public ObservableCollection<GanttTask> Tasks { get; } = new();

// Add tasks
Tasks.Add(new GanttTask
{
    Title = "Project Planning",
    Start = DateTime.Today,
    End = DateTime.Today.AddDays(5),
    RowIndex = 1,
    Progress = 75
});
```

### With ViewModel (MVVM Pattern)

```csharp
public class ProjectViewModel : ViewModelBase
{
    private readonly GanttChartViewModel _ganttViewModel;
    
    public ProjectViewModel()
    {
        _ganttViewModel = new GanttChartViewModel();
        LoadSampleData();
    }
    
    public GanttChartViewModel GanttChart => _ganttViewModel;
    
    private void LoadSampleData()
    {
        _ganttViewModel.Tasks.Add(new GanttTaskViewModel(new GanttTask
        {
            Title = "Design Phase",
            Start = DateTime.Today,
            End = DateTime.Today.AddDays(10),
            RowIndex = 1,
            Status = TaskStatus.InProgress
        }));
    }
}
```

## 📖 Documentation

- [**Getting Started Guide**](docs/getting-started.md) - Complete setup and first steps
- [**API Reference**](docs/api-reference.md) - Detailed API documentation
- [**Configuration Guide**](docs/configuration.md) - Customization and configuration options
- [**MVVM Integration**](docs/mvvm-integration.md) - Working with ViewModels and data binding
- [**Time Management**](docs/time-management.md) - Time scales and formatting
- [**Task Management**](docs/task-management.md) - Working with tasks and validation
- [**Examples**](docs/examples.md) - Code examples and use cases

## 🏗️ Architecture

GPM.Gantt follows a modular architecture with clear separation of concerns:

- **Models**: Core data models (`GanttTask`, `TimeUnit`, etc.)
- **ViewModels**: MVVM support with `GanttChartViewModel` and `GanttTaskViewModel`
- **Services**: Business logic and data operations (`IGanttService`, `IValidationService`)
- **Utilities**: Helper classes for timeline calculations and conversions
- **Configuration**: Flexible configuration system for customization

## 🎨 Customization

### Time Formatting
```csharp
ganttChart.DateFormat = "yyyy-MM-dd";
ganttChart.TimeFormat = "HH:mm";
ganttChart.Culture = new CultureInfo("en-US");
```

### Visual Configuration
```csharp
var config = new GanttConfiguration
{
    Rendering = new RenderingConfiguration
    {
        ShowGridCells = true,
        TaskBarCornerRadius = 6.0,
        GridMode = GridRenderingMode.Rectangles
    },
    TimeScale = new TimeScaleConfiguration
    {
        DefaultTimeUnit = TimeUnit.Day,
        HighlightWeekends = true,
        HighlightToday = true
    }
};
ganttChart.Configuration = config;
```

## 🧪 Testing

Run the test suite:
```bash
dotnet test GPM.Gantt.Tests
```

The library includes comprehensive unit tests covering:
- Timeline calculations
- Task validation
- ViewModel functionality
- Service operations

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

### Current Version (1.0.0)
- ✅ Basic Gantt chart functionality
- ✅ Grid-based layout
- ✅ Time scale support
- ✅ MVVM integration

### Upcoming Features
- 🚧 Interactive drag-and-drop
- 🚧 Task dependencies visualization
- 🚧 Export capabilities (PDF, PNG, Excel)
- 🚧 Virtualization for large datasets
- 🚧 Advanced theming system

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Documentation**: Check our [docs](docs/) folder
- **Issues**: Report bugs on [GitHub Issues](https://github.com/yourorg/GPM/issues)
- **Discussions**: Join our [GitHub Discussions](https://github.com/yourorg/GPM/discussions)

## 🙏 Acknowledgments

- Built with modern WPF and .NET 9.0
- Inspired by project management best practices
- Community feedback and contributions

---

*GPM.Gantt - Making project visualization simple and powerful.*