# MVVM Integration Guide - GPM.Gantt v2.1.0

This guide demonstrates how to effectively integrate GPM.Gantt with the Model-View-ViewModel (MVVM) pattern in WPF applications, including the advanced features introduced in version 2.1.0.

## MVVM Overview

GPM.Gantt is designed with MVVM principles in mind, providing:
- **ViewModels** with property change notifications
- **Commands** for user interactions
- **Data binding** support for all properties
- **Async operations** with cancellation support
- **Validation** integration

## Core MVVM Components

### GanttChartViewModel

The main ViewModel for Gantt chart functionality:

```csharp
using GPM.Gantt.ViewModels;
using GPM.Gantt.Services;

public class ProjectViewModel : ViewModelBase
{
    private readonly GanttChartViewModel _ganttViewModel;
    
    public ProjectViewModel()
    {
        // Initialize with dependency injection or manual instantiation
        var validationService = new ValidationService();
        var ganttService = new GanttService(validationService);
        
        _ganttViewModel = new GanttChartViewModel(validationService, ganttService);
        _ganttViewModel.ProjectId = Guid.NewGuid(); // Set project context
        
        InitializeCommands();
        LoadInitialData();
    }
    
    public GanttChartViewModel GanttChart => _ganttViewModel;
}
```

### GanttTaskViewModel

ViewModel for individual tasks with full property binding:

```csharp
// Access task ViewModels
foreach (var taskVM in ganttChart.Tasks)
{
    Console.WriteLine($"Task: {taskVM.Title}, Progress: {taskVM.Progress}%");
    
    // Modify task properties with automatic change notification
    taskVM.Progress = 75;
    taskVM.Status = TaskStatus.InProgress;
}
```

## Data Binding Patterns

### Basic Property Binding

```xml
<gantt:GanttContainer StartTime="{Binding GanttChart.StartTime}"
                     EndTime="{Binding GanttChart.EndTime}"
                     TimeUnit="{Binding GanttChart.TimeUnit}"
                     Tasks="{Binding GanttChart.TaskModels}"
                     Configuration="{Binding GanttChart.Configuration}" />
```

### Two-Way Binding for User Controls

```xml
<StackPanel>
    <!-- Time range controls -->
    <DatePicker SelectedDate="{Binding GanttChart.StartTime, Mode=TwoWay}" />
    <DatePicker SelectedDate="{Binding GanttChart.EndTime, Mode=TwoWay}" />
    
    <!-- Time unit selector -->
    <ComboBox SelectedValue="{Binding GanttChart.TimeUnit, Mode=TwoWay}">
        <ComboBoxItem Content="Day" Tag="{x:Static models:TimeUnit.Day}" />
        <ComboBoxItem Content="Week" Tag="{x:Static models:TimeUnit.Week}" />
        <ComboBoxItem Content="Month" Tag="{x:Static models:TimeUnit.Month}" />
    </ComboBox>
    
    <!-- Selected task details -->
    <TextBox Text="{Binding GanttChart.SelectedTask.Title, Mode=TwoWay}" />
    <Slider Value="{Binding GanttChart.SelectedTask.Progress, Mode=TwoWay}"
            Minimum="0" Maximum="100" />
</StackPanel>
```

### Observable Collections

The ViewModel automatically maintains synchronized collections:

```csharp
public class ProjectViewModel : ViewModelBase
{
    public ProjectViewModel()
    {
        // Tasks collection automatically notifies UI of changes
        _ganttViewModel.Tasks.CollectionChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(TaskCount));
            UpdateProjectStatistics();
        };
    }
    
    public int TaskCount => _ganttViewModel.Tasks.Count;
    
    private void UpdateProjectStatistics()
    {
        // Calculate project metrics when tasks change
        var completedTasks = _ganttViewModel.Tasks.Count(t => t.Status == TaskStatus.Completed);
        var progressPercentage = TaskCount > 0 ? (completedTasks * 100.0 / TaskCount) : 0;
        
        OnPropertyChanged(nameof(ProjectProgress));
    }
    
    public double ProjectProgress => /* calculation */;
}
```

## Command Patterns

### Built-in Commands

GPM.Gantt provides several built-in commands:

```xml
<StackPanel>
    <!-- Task management commands -->
    <Button Content="Add Task" 
            Command="{Binding GanttChart.AddTaskCommand}" />
    
    <Button Content="Delete Task" 
            Command="{Binding GanttChart.DeleteTaskCommand}"
            CommandParameter="{Binding GanttChart.SelectedTask}" />
    
    <Button Content="Validate All" 
            Command="{Binding GanttChart.ValidateAllCommand}" />
    
    <!-- Async operations -->
    <Button Content="Load Tasks" 
            Command="{Binding GanttChart.LoadTasksAsyncCommand}" />
    
    <Button Content="Add Task (Async)" 
            Command="{Binding GanttChart.AddTaskAsyncCommand}" />
    
    <Button Content="Cancel" 
            Command="{Binding GanttChart.CancelAsyncCommand}"
            IsEnabled="{Binding GanttChart.IsBusy}" />
</StackPanel>
```

### Custom Commands

Extend functionality with custom commands:

```csharp
public class ProjectViewModel : ViewModelBase
{
    public ICommand ExportProjectCommand { get; }
    public ICommand ImportProjectCommand { get; }
    public ICommand SaveProjectCommand { get; }
    
    private void InitializeCommands()
    {
        ExportProjectCommand = new RelayCommand(ExportProject, CanExportProject);
        ImportProjectCommand = new RelayCommand(ImportProject);
        SaveProjectCommand = new AsyncRelayCommand(SaveProjectAsync);
    }
    
    private void ExportProject()
    {
        var tasks = _ganttViewModel.Tasks.Select(vm => vm.Model).ToList();
        // Export logic here
    }
    
    private bool CanExportProject() => _ganttViewModel.Tasks.Any();
    
    private async Task SaveProjectAsync(CancellationToken cancellationToken)
    {
        try
        {
            foreach (var taskVM in _ganttViewModel.Tasks)
            {
                await _ganttService.UpdateTaskAsync(_ganttViewModel.ProjectId, 
                                                  taskVM.Model, 
                                                  cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation
        }
    }
}
```

## Validation Integration

### Task-Level Validation

```csharp
public class TaskEditViewModel : ViewModelBase, IDataErrorInfo
{
    private readonly GanttTaskViewModel _taskViewModel;
    
    public TaskEditViewModel(GanttTaskViewModel taskViewModel)
    {
        _taskViewModel = taskViewModel;
    }
    
    public string Title
    {
        get => _taskViewModel.Title;
        set => _taskViewModel.Title = value;
    }
    
    // IDataErrorInfo implementation
    public string Error => _taskViewModel.ValidationMessage;
    
    public string this[string columnName]
    {
        get
        {
            var errors = _taskViewModel.ValidationErrors;
            return errors.FirstOrDefault(e => e.Contains(columnName)) ?? string.Empty;
        }
    }
}
```

### Form Validation in XAML

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
    
    <!-- Task title with validation -->
    <TextBox Grid.Row="0" 
             Text="{Binding SelectedTask.Title, 
                           Mode=TwoWay, 
                           ValidatesOnDataErrors=True,
                           UpdateSourceTrigger=PropertyChanged}" />
    
    <!-- Start date with validation -->
    <DatePicker Grid.Row="1"
                SelectedDate="{Binding SelectedTask.Start, 
                              Mode=TwoWay, 
                              ValidatesOnDataErrors=True}" />
    
    <!-- End date with validation -->
    <DatePicker Grid.Row="2"
                SelectedDate="{Binding SelectedTask.End, 
                              Mode=TwoWay, 
                              ValidatesOnDataErrors=True}" />
    
    <!-- Validation summary -->
    <TextBlock Grid.Row="3" 
               Text="{Binding SelectedTask.ValidationMessage}"
               Foreground="Red"
               Visibility="{Binding SelectedTask.IsValid, 
                           Converter={StaticResource InverseBoolToVisibilityConverter}}" />
</Grid>
```

## Async Operations

### Loading Data Asynchronously

```csharp
public class ProjectViewModel : ViewModelBase
{
    public async Task LoadProjectAsync(Guid projectId)
    {
        try
        {
            IsBusy = true;
            _ganttViewModel.ProjectId = projectId;
            
            // Load tasks asynchronously
            await _ganttViewModel.LoadTasksAsync();
            
            // Update project metadata
            await LoadProjectMetadataAsync(projectId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load project: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy || _ganttViewModel.IsBusy;
        set => SetProperty(ref _isBusy, value);
    }
}
```

### Progress Reporting

```csharp
public class ProjectViewModel : ViewModelBase
{
    private double _loadProgress;
    public double LoadProgress
    {
        get => _loadProgress;
        set => SetProperty(ref _loadProgress, value);
    }
    
    public async Task LoadLargeDatasetAsync(IProgress<double> progress = null)
    {
        var tasks = await GetTasksFromDatabaseAsync();
        
        for (int i = 0; i < tasks.Count; i++)
        {
            var taskVM = new GanttTaskViewModel(tasks[i]);
            _ganttViewModel.Tasks.Add(taskVM);
            
            // Report progress
            var progressPercentage = (i + 1) * 100.0 / tasks.Count;
            progress?.Report(progressPercentage);
            LoadProgress = progressPercentage;
            
            // Allow UI updates
            await Task.Delay(1);
        }
    }
}
```

## Advanced MVVM Patterns

### Service Layer Integration

```csharp
public class ProjectViewModel : ViewModelBase
{
    private readonly IProjectService _projectService;
    private readonly IGanttService _ganttService;
    private readonly IDialogService _dialogService;
    
    public ProjectViewModel(IProjectService projectService, 
                          IGanttService ganttService,
                          IDialogService dialogService)
    {
        _projectService = projectService;
        _ganttService = ganttService;
        _dialogService = dialogService;
        
        _ganttViewModel = new GanttChartViewModel(ganttService);
    }
    
    private async Task CreateNewTaskAsync()
    {
        var taskData = await _dialogService.ShowTaskCreationDialogAsync();
        if (taskData != null)
        {
            await _ganttViewModel.AddTaskAsync(taskData);
        }
    }
}
```

### Messenger Pattern for Loose Coupling

```csharp
// Using a messenger service for communication between ViewModels
public class ProjectViewModel : ViewModelBase
{
    public ProjectViewModel(IMessenger messenger)
    {
        // Listen for task selection changes
        messenger.Register<TaskSelectedMessage>(this, OnTaskSelected);
        
        // Listen for project changes
        messenger.Register<ProjectChangedMessage>(this, OnProjectChanged);
    }
    
    private void OnTaskSelected(TaskSelectedMessage message)
    {
        _ganttViewModel.SelectedTask = message.SelectedTask;
    }
    
    private void OnProjectChanged(ProjectChangedMessage message)
    {
        // Reload Gantt data for new project
        _ = LoadProjectAsync(message.ProjectId);
    }
}
```

### Dependency Injection

```csharp
// Configure DI container (example with Microsoft.Extensions.DependencyInjection)
public void ConfigureServices(IServiceCollection services)
{
    // Register services
    services.AddSingleton<IValidationService, ValidationService>();
    services.AddScoped<IGanttService, GanttService>();
    services.AddScoped<IProjectService, ProjectService>();
    
    // Register ViewModels
    services.AddTransient<GanttChartViewModel>();
    services.AddTransient<ProjectViewModel>();
    services.AddTransient<MainViewModel>();
}

// Use in ViewModel constructor
public class ProjectViewModel : ViewModelBase
{
    public ProjectViewModel(IGanttService ganttService, 
                          IValidationService validationService)
    {
        _ganttViewModel = new GanttChartViewModel(validationService, ganttService);
    }
}
```

## Complete MVVM Example

Here's a complete example showing proper MVVM implementation:

```csharp
public class MainViewModel : ViewModelBase
{
    private readonly IGanttService _ganttService;
    private readonly GanttChartViewModel _ganttViewModel;
    private string _projectName = "New Project";
    private bool _isProjectDirty;
    
    public MainViewModel(IGanttService ganttService, IValidationService validationService)
    {
        _ganttService = ganttService;
        _ganttViewModel = new GanttChartViewModel(validationService, ganttService);
        
        InitializeCommands();
        SetupEventHandlers();
        LoadDefaultProject();
    }
    
    #region Properties
    
    public GanttChartViewModel GanttChart => _ganttViewModel;
    
    public string ProjectName
    {
        get => _projectName;
        set
        {
            if (SetProperty(ref _projectName, value))
            {
                IsProjectDirty = true;
            }
        }
    }
    
    public bool IsProjectDirty
    {
        get => _isProjectDirty;
        set => SetProperty(ref _isProjectDirty, value);
    }
    
    public string StatusMessage => _ganttViewModel.ErrorMessage;
    
    #endregion
    
    #region Commands
    
    public ICommand NewProjectCommand { get; private set; }
    public ICommand SaveProjectCommand { get; private set; }
    public ICommand LoadProjectCommand { get; private set; }
    
    private void InitializeCommands()
    {
        NewProjectCommand = new RelayCommand(CreateNewProject);
        SaveProjectCommand = new AsyncRelayCommand(SaveProjectAsync, () => IsProjectDirty);
        LoadProjectCommand = new AsyncRelayCommand(LoadProjectAsync);
    }
    
    #endregion
    
    #region Private Methods
    
    private void SetupEventHandlers()
    {
        // Monitor for changes to mark project as dirty
        _ganttViewModel.Tasks.CollectionChanged += (s, e) => IsProjectDirty = true;
        
        foreach (var task in _ganttViewModel.Tasks)
        {
            task.PropertyChanged += (s, e) => IsProjectDirty = true;
        }
    }
    
    private void CreateNewProject()
    {
        ProjectName = "New Project";
        _ganttViewModel.Tasks.Clear();
        _ganttViewModel.ProjectId = Guid.NewGuid();
        IsProjectDirty = false;
    }
    
    private async Task SaveProjectAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Save all tasks
            foreach (var taskVM in _ganttViewModel.Tasks)
            {
                await _ganttService.CreateTaskAsync(_ganttViewModel.ProjectId, 
                                                  taskVM.Model, 
                                                  cancellationToken);
            }
            
            IsProjectDirty = false;
        }
        catch (Exception ex)
        {
            // Handle error
            _ganttViewModel.ErrorMessage = $"Save failed: {ex.Message}";
        }
    }
    
    private async Task LoadProjectAsync(CancellationToken cancellationToken)
    {
        await _ganttViewModel.LoadTasksAsync(cancellationToken);
        IsProjectDirty = false;
    }
    
    private void LoadDefaultProject()
    {
        // Create sample data
        var sampleTasks = new[]
        {
            new GanttTask
            {
                Title = "Project Planning",
                Start = DateTime.Today,
                End = DateTime.Today.AddDays(5),
                RowIndex = 1,
                Status = TaskStatus.Completed,
                Progress = 100
            },
            new GanttTask
            {
                Title = "Development Phase",
                Start = DateTime.Today.AddDays(3),
                End = DateTime.Today.AddDays(15),
                RowIndex = 2,
                Status = TaskStatus.InProgress,
                Progress = 60
            }
        };
        
        foreach (var task in sampleTasks)
        {
            _ganttViewModel.Tasks.Add(new GanttTaskViewModel(task));
        }
        
        IsProjectDirty = false;
    }
    
    #endregion
}
```

## Best Practices

### Performance
1. **Use observable collections appropriately** - Don't create new collections unnecessarily
2. **Implement proper disposal** - Unsubscribe from events in ViewModels
3. **Use async patterns** - Keep UI responsive with async operations
4. **Validate efficiently** - Don't validate on every property change

### Maintainability
1. **Separate concerns** - Keep ViewModels focused on UI logic
2. **Use dependency injection** - Make services testable and mockable
3. **Implement proper error handling** - Show meaningful error messages
4. **Follow naming conventions** - Use consistent property and command names

### Testing
1. **Mock services** - Use interfaces for all service dependencies
2. **Test ViewModels independently** - Don't depend on UI for ViewModel tests
3. **Test commands** - Verify command execution and CanExecute logic
4. **Test data binding** - Verify property change notifications

## Advanced MVVM Features (v2.1.0)

### Plugin System Integration

ViewModels can integrate with the annotation plugin system:

```csharp
public class AdvancedProjectViewModel : ViewModelBase
{
    private readonly IPluginService _pluginService;
    private readonly ObservableCollection<AnnotationViewModel> _annotations;
    
    public AdvancedProjectViewModel(IPluginService pluginService)
    {
        _pluginService = pluginService;
        _annotations = new ObservableCollection<AnnotationViewModel>();
        
        InitializePluginSystem();
    }
    
    public ReadOnlyObservableCollection<AnnotationViewModel> Annotations { get; }
    
    private void InitializePluginSystem()
    {
        // Register built-in plugins
        _pluginService.RegisterPlugin(new TextAnnotationPlugin());
        _pluginService.RegisterPlugin(new ShapeAnnotationPlugin());
        _pluginService.RegisterPlugin(new LineAnnotationPlugin());
        
        // Create annotation ViewModels
        CreateAnnotationViewModels();
    }
    
    private void CreateAnnotationViewModels()
    {
        var textPlugin = _pluginService.GetPlugin(AnnotationType.Text);
        var textConfig = textPlugin.CreateDefaultConfig() as TextAnnotationConfig;
        textConfig.Text = "Critical Milestone";
        
        var annotationVM = new AnnotationViewModel(textConfig, textPlugin);
        _annotations.Add(annotationVM);
    }
    
    public ICommand AddTextAnnotationCommand => new RelayCommand(AddTextAnnotation);
    
    private void AddTextAnnotation()
    {
        var textPlugin = _pluginService.GetPlugin(AnnotationType.Text);
        var config = textPlugin.CreateDefaultConfig();
        var annotationVM = new AnnotationViewModel(config, textPlugin);
        _annotations.Add(annotationVM);
    }
}
```

### Multi-Level Time Scale ViewModel

Manage complex time scale configurations through ViewModels:

```csharp
public class TimeScaleViewModel : ViewModelBase
{
    private MultiLevelTimeScaleConfiguration _configuration;
    
    public TimeScaleViewModel()
    {
        InitializeTimeScaleConfiguration();
    }
    
    public MultiLevelTimeScaleConfiguration Configuration
    {
        get => _configuration;
        set => SetProperty(ref _configuration, value);
    }
    
    private void InitializeTimeScaleConfiguration()
    {
        Configuration = new MultiLevelTimeScaleConfiguration
        {
            Levels = new List<TimeLevelConfiguration>
            {
                new TimeLevelConfiguration
                {
                    Unit = ExtendedTimeUnit.Year,
                    IsVisible = true,
                    Height = 35,
                    DateFormat = "yyyy"
                },
                new TimeLevelConfiguration
                {
                    Unit = ExtendedTimeUnit.Month,
                    IsVisible = true,
                    Height = 25,
                    DateFormat = "MMM"
                }
            },
            EnableSmartVisibility = true
        };
    }
    
    public ICommand AddTimeLevelCommand => new RelayCommand(AddTimeLevel);
    public ICommand RemoveTimeLevelCommand => new RelayCommand<TimeLevelConfiguration>(RemoveTimeLevel);
    
    private void AddTimeLevel()
    {
        Configuration.Levels.Add(new TimeLevelConfiguration
        {
            Unit = ExtendedTimeUnit.Week,
            IsVisible = true,
            Height = 20,
            DateFormat = "ww"
        });
        
        OnPropertyChanged(nameof(Configuration));
    }
    
    private void RemoveTimeLevel(TimeLevelConfiguration level)
    {
        Configuration.Levels.Remove(level);
        OnPropertyChanged(nameof(Configuration));
    }
}
```

### Expandable Time Segments ViewModel

Manage interactive time segment expansions:

```csharp
public class TimeSegmentViewModel : ViewModelBase
{
    private readonly ObservableCollection<TimeSegmentExpansion> _expansions;
    
    public TimeSegmentViewModel()
    {
        _expansions = new ObservableCollection<TimeSegmentExpansion>();
    }
    
    public ReadOnlyObservableCollection<TimeSegmentExpansion> Expansions { get; }
    
    public ICommand AddExpansionCommand => new RelayCommand<DateTime>(AddExpansion);
    public ICommand ToggleExpansionCommand => new RelayCommand<TimeSegmentExpansion>(ToggleExpansion);
    
    private void AddExpansion(DateTime date)
    {
        var expansion = new TimeSegmentExpansion
        {
            StartTime = date,
            EndTime = date.AddDays(7),
            OriginalUnit = ExtendedTimeUnit.Week,
            ExpandedUnit = ExtendedTimeUnit.Day,
            IsExpanded = false,
            DisplayName = $"Week of {date:MMM dd}"
        };
        
        _expansions.Add(expansion);
    }
    
    private void ToggleExpansion(TimeSegmentExpansion expansion)
    {
        expansion.IsExpanded = !expansion.IsExpanded;
        OnPropertyChanged(nameof(Expansions));
        
        // Notify parent ViewModel to refresh time scale
        OnExpansionChanged?.Invoke(this, EventArgs.Empty);
    }
    
    public event EventHandler OnExpansionChanged;
}
```

These advanced MVVM patterns enable sophisticated integration scenarios with the new GPM.Gantt v2.1.0 features.