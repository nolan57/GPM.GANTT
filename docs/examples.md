# Examples and Use Cases - GPM.Gantt

This document provides practical examples and common use cases for implementing GPM.Gantt in various scenarios.

## Table of Contents

1. [Basic Project Management](#basic-project-management)
2. [Software Development Sprint Planning](#software-development-sprint-planning)
3. [Manufacturing Production Schedule](#manufacturing-production-schedule)
4. [Resource Management](#resource-management)
5. [Multi-Project Portfolio View](#multi-project-portfolio-view)
6. [Task Dependencies](#task-dependencies)
7. [Custom Task Types](#custom-task-types)
8. [Real-time Updates](#real-time-updates)
9. [Integration Examples](#integration-examples)

## Basic Project Management

A simple project management application with standard Gantt chart functionality.

### ViewModel Implementation

```csharp
public class ProjectManagementViewModel : ViewModelBase
{
    private readonly GanttChartViewModel _ganttChart;
    private string _projectName = "Software Project v1.0";
    
    public ProjectManagementViewModel()
    {
        _ganttChart = new GanttChartViewModel();
        _ganttChart.StartTime = DateTime.Today;
        _ganttChart.EndTime = DateTime.Today.AddMonths(3);
        _ganttChart.TimeUnit = TimeUnit.Week;
        
        CreateSampleProject();
    }
    
    public GanttChartViewModel GanttChart => _ganttChart;
    public string ProjectName
    {
        get => _projectName;
        set => SetProperty(ref _projectName, value);
    }
    
    private void CreateSampleProject()
    {
        var tasks = new[]
        {
            new GanttTask
            {
                Title = "Requirements Analysis",
                Start = DateTime.Today,
                End = DateTime.Today.AddDays(14),
                RowIndex = 1,
                Status = TaskStatus.Completed,
                Progress = 100,
                Priority = TaskPriority.High
            },
            new GanttTask
            {
                Title = "System Design",
                Start = DateTime.Today.AddDays(10),
                End = DateTime.Today.AddDays(28),
                RowIndex = 2,
                Status = TaskStatus.InProgress,
                Progress = 75,
                Priority = TaskPriority.High
            },
            new GanttTask
            {
                Title = "UI/UX Design",
                Start = DateTime.Today.AddDays(14),
                End = DateTime.Today.AddDays(35),
                RowIndex = 3,
                Status = TaskStatus.InProgress,
                Progress = 50,
                Priority = TaskPriority.Normal
            },
            new GanttTask
            {
                Title = "Backend Development",
                Start = DateTime.Today.AddDays(21),
                End = DateTime.Today.AddDays(63),
                RowIndex = 4,
                Status = TaskStatus.NotStarted,
                Progress = 0,
                Priority = TaskPriority.High
            },
            new GanttTask
            {
                Title = "Frontend Development",
                Start = DateTime.Today.AddDays(35),
                End = DateTime.Today.AddDays(70),
                RowIndex = 5,
                Status = TaskStatus.NotStarted,
                Progress = 0,
                Priority = TaskPriority.Normal
            },
            new GanttTask
            {
                Title = "Testing & QA",
                Start = DateTime.Today.AddDays(56),
                End = DateTime.Today.AddDays(84),
                RowIndex = 6,
                Status = TaskStatus.NotStarted,
                Progress = 0,
                Priority = TaskPriority.Critical
            },
            new GanttTask
            {
                Title = "Deployment",
                Start = DateTime.Today.AddDays(77),
                End = DateTime.Today.AddDays(91),
                RowIndex = 7,
                Status = TaskStatus.NotStarted,
                Progress = 0,
                Priority = TaskPriority.High
            }
        };
        
        foreach (var task in tasks)
        {
            _ganttChart.Tasks.Add(new GanttTaskViewModel(task));
        }
    }
}
```

### XAML Implementation

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
    
    <!-- Project Header -->
    <TextBlock Grid.Row="0" Text="{Binding ProjectName}" 
               FontSize="18" FontWeight="Bold" Margin="10"/>
    
    <!-- Controls -->
    <StackPanel Grid.Row="1" Orientation="Horizontal" Margin="10">
        <Button Content="Add Task" Command="{Binding GanttChart.AddTaskCommand}"/>
        <Button Content="Delete Task" Command="{Binding GanttChart.DeleteTaskCommand}"
                CommandParameter="{Binding GanttChart.SelectedTask}"/>
        <ComboBox SelectedValue="{Binding GanttChart.TimeUnit}" Width="100">
            <ComboBoxItem Content="Day" Tag="{x:Static models:TimeUnit.Day}"/>
            <ComboBoxItem Content="Week" Tag="{x:Static models:TimeUnit.Week}"/>
            <ComboBoxItem Content="Month" Tag="{x:Static models:TimeUnit.Month}"/>
        </ComboBox>
    </StackPanel>
    
    <!-- Gantt Chart -->
    <gantt:GanttContainer Grid.Row="2"
                         StartTime="{Binding GanttChart.StartTime}"
                         EndTime="{Binding GanttChart.EndTime}"
                         TimeUnit="{Binding GanttChart.TimeUnit}"
                         Tasks="{Binding GanttChart.TaskModels}"
                         TaskCount="10"
                         ShowGridCells="True"/>
    
    <!-- Status Bar -->
    <StatusBar Grid.Row="3">
        <StatusBarItem Content="{Binding GanttChart.Tasks.Count, StringFormat='Tasks: {0}'}"/>
        <StatusBarItem Content="{Binding GanttChart.TimeUnit, StringFormat='View: {0}'}"/>
    </StatusBar>
</Grid>
```

## Software Development Sprint Planning

Agile sprint planning with story points and team assignments.

```csharp
public class SprintPlanningViewModel : ViewModelBase
{
    private readonly GanttChartViewModel _ganttChart;
    private int _sprintNumber = 1;
    private DateTime _sprintStart = DateTime.Today;
    private TimeSpan _sprintDuration = TimeSpan.FromDays(14);
    
    public SprintPlanningViewModel()
    {
        _ganttChart = new GanttChartViewModel();
        _ganttChart.TimeUnit = TimeUnit.Day;
        ConfigureForSprint();
        CreateSprintBacklog();
    }
    
    public GanttChartViewModel GanttChart => _ganttChart;
    public int SprintNumber
    {
        get => _sprintNumber;
        set => SetProperty(ref _sprintNumber, value);
    }
    
    public DateTime SprintStart
    {
        get => _sprintStart;
        set
        {
            if (SetProperty(ref _sprintStart, value))
            {
                ConfigureForSprint();
            }
        }
    }
    
    private void ConfigureForSprint()
    {
        _ganttChart.StartTime = SprintStart;
        _ganttChart.EndTime = SprintStart.Add(_sprintDuration);
    }
    
    private void CreateSprintBacklog()
    {
        var stories = new[]
        {
            new { Title = "User Authentication API", Points = 8, Developer = "Alice", Days = 3 },
            new { Title = "Product Catalog UI", Points = 5, Developer = "Bob", Days = 2 },
            new { Title = "Shopping Cart Logic", Points = 13, Developer = "Charlie", Days = 5 },
            new { Title = "Payment Integration", Points = 8, Developer = "Alice", Days = 3 },
            new { Title = "Order Management", Points = 5, Developer = "Bob", Days = 2 },
            new { Title = "User Profile Settings", Points = 3, Developer = "Charlie", Days = 1 }
        };
        
        int currentRow = 1;
        DateTime currentStart = SprintStart;
        
        foreach (var story in stories)
        {
            var task = new GanttTask
            {
                Title = $"{story.Title} ({story.Points} pts)",
                Start = currentStart,
                End = currentStart.AddDays(story.Days),
                RowIndex = currentRow++,
                Status = TaskStatus.NotStarted,
                Progress = 0,
                Description = $"Assigned to: {story.Developer}\nStory Points: {story.Points}"
            };
            
            task.AssignedResources.Add(story.Developer);
            _ganttChart.Tasks.Add(new GanttTaskViewModel(task));
            
            // Stagger start times for parallel work
            if (currentRow % 3 == 1)
            {
                currentStart = currentStart.AddDays(1);
            }
        }
    }
    
    public ICommand StartSprintCommand => new RelayCommand(() =>
    {
        foreach (var task in _ganttChart.Tasks.Where(t => t.Status == TaskStatus.NotStarted))
        {
            task.Status = TaskStatus.InProgress;
        }
    });
}
```

## Manufacturing Production Schedule

Production line scheduling with shift patterns and resource constraints.

```csharp
public class ProductionScheduleViewModel : ViewModelBase
{
    private readonly GanttChartViewModel _ganttChart;
    
    public ProductionScheduleViewModel()
    {
        _ganttChart = new GanttChartViewModel();
        _ganttChart.TimeUnit = TimeUnit.Hour;
        _ganttChart.StartTime = DateTime.Today.AddHours(6); // Start at 6 AM
        _ganttChart.EndTime = DateTime.Today.AddDays(3).AddHours(18); // 3 days, end at 6 PM
        
        CreateProductionSchedule();
    }
    
    public GanttChartViewModel GanttChart => _ganttChart;
    
    private void CreateProductionSchedule()
    {
        var productionJobs = new[]
        {
            new { Product = "Widget A", Quantity = 1000, Hours = 8, Line = 1 },
            new { Product = "Widget B", Quantity = 500, Hours = 4, Line = 1 },
            new { Product = "Component X", Quantity = 2000, Hours = 12, Line = 2 },
            new { Product = "Component Y", Quantity = 800, Hours = 6, Line = 2 },
            new { Product = "Assembly Z", Quantity = 300, Hours = 10, Line = 3 }
        };
        
        var lineSchedules = new Dictionary<int, DateTime>
        {
            [1] = _ganttChart.StartTime,
            [2] = _ganttChart.StartTime,
            [3] = _ganttChart.StartTime
        };
        
        foreach (var job in productionJobs)
        {
            var startTime = lineSchedules[job.Line];
            var endTime = startTime.AddHours(job.Hours);
            
            // Skip non-working hours (6 PM to 6 AM)
            if (endTime.Hour > 18)
            {
                var nextDay = startTime.Date.AddDays(1).AddHours(6);
                var remainingHours = job.Hours - (18 - startTime.Hour);
                endTime = nextDay.AddHours(remainingHours);
            }
            
            var task = new GanttTask
            {
                Title = $"{job.Product} ({job.Quantity} units)",
                Start = startTime,
                End = endTime,
                RowIndex = job.Line,
                Status = TaskStatus.NotStarted,
                Progress = 0,
                Description = $"Production Line {job.Line}\nQuantity: {job.Quantity}\nEstimated Time: {job.Hours}h"
            };
            
            task.AssignedResources.Add($"Production Line {job.Line}");
            _ganttChart.Tasks.Add(new GanttTaskViewModel(task));
            
            lineSchedules[job.Line] = endTime;
        }
    }
}
```

## Resource Management

Track resource allocation and identify conflicts.

```csharp
public class ResourceManagementViewModel : ViewModelBase
{
    private readonly GanttChartViewModel _ganttChart;
    private readonly ObservableCollection<string> _availableResources;
    
    public ResourceManagementViewModel()
    {
        _availableResources = new ObservableCollection<string>
        {
            "John Doe", "Jane Smith", "Mike Johnson", "Sarah Wilson", "Tom Brown"
        };
        
        _ganttChart = new GanttChartViewModel();
        CreateResourceSchedule();
    }
    
    public ObservableCollection<string> AvailableResources => _availableResources;
    public GanttChartViewModel GanttChart => _ganttChart;
    
    private void CreateResourceSchedule()
    {
        var assignments = new[]
        {
            new { Resource = "John Doe", Task = "Database Design", Start = 0, Duration = 5 },
            new { Resource = "Jane Smith", Task = "UI Mockups", Start = 0, Duration = 3 },
            new { Resource = "Mike Johnson", Task = "API Development", Start = 2, Duration = 7 },
            new { Resource = "John Doe", Task = "Database Implementation", Start = 5, Duration = 4 },
            new { Resource = "Sarah Wilson", Task = "Testing Framework", Start = 1, Duration = 6 },
            new { Resource = "Jane Smith", Task = "Frontend Development", Start = 3, Duration = 8 }
        };
        
        int rowIndex = 1;
        foreach (var assignment in assignments)
        {
            var task = new GanttTask
            {
                Title = $"{assignment.Task} [{assignment.Resource}]",
                Start = DateTime.Today.AddDays(assignment.Start),
                End = DateTime.Today.AddDays(assignment.Start + assignment.Duration),
                RowIndex = rowIndex++,
                Status = TaskStatus.InProgress,
                Progress = new Random().Next(0, 100)
            };
            
            task.AssignedResources.Add(assignment.Resource);
            _ganttChart.Tasks.Add(new GanttTaskViewModel(task));
        }
    }
    
    public ICommand DetectConflictsCommand => new RelayCommand(DetectResourceConflicts);
    
    private void DetectResourceConflicts()
    {
        var conflicts = new List<string>();
        
        foreach (var resource in _availableResources)
        {
            var resourceTasks = _ganttChart.Tasks
                .Where(t => t.Model.AssignedResources.Contains(resource))
                .OrderBy(t => t.Start)
                .ToList();
            
            for (int i = 0; i < resourceTasks.Count - 1; i++)
            {
                var current = resourceTasks[i];
                var next = resourceTasks[i + 1];
                
                if (current.End > next.Start)
                {
                    conflicts.Add($"{resource}: '{current.Title}' overlaps with '{next.Title}'");
                }
            }
        }
        
        if (conflicts.Any())
        {
            var message = string.Join("\n", conflicts);
            MessageBox.Show($"Resource Conflicts Detected:\n\n{message}", "Conflicts", 
                          MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        else
        {
            MessageBox.Show("No resource conflicts detected.", "Analysis Complete", 
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
```

## Multi-Project Portfolio View

Manage multiple projects in a single view.

```csharp
public class PortfolioViewModel : ViewModelBase
{
    private readonly ObservableCollection<ProjectInfo> _projects;
    private readonly GanttChartViewModel _ganttChart;
    private ProjectInfo? _selectedProject;
    
    public PortfolioViewModel()
    {
        _projects = new ObservableCollection<ProjectInfo>();
        _ganttChart = new GanttChartViewModel();
        
        CreatePortfolio();
        LoadAllProjects();
    }
    
    public ObservableCollection<ProjectInfo> Projects => _projects;
    public GanttChartViewModel GanttChart => _ganttChart;
    
    public ProjectInfo? SelectedProject
    {
        get => _selectedProject;
        set
        {
            if (SetProperty(ref _selectedProject, value))
            {
                LoadSelectedProject();
            }
        }
    }
    
    private void CreatePortfolio()
    {
        _projects.Add(new ProjectInfo
        {
            Id = Guid.NewGuid(),
            Name = "E-Commerce Platform",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(6),
            Status = "In Progress",
            Progress = 35
        });
        
        _projects.Add(new ProjectInfo
        {
            Id = Guid.NewGuid(),
            Name = "Mobile App",
            StartDate = DateTime.Today.AddMonths(1),
            EndDate = DateTime.Today.AddMonths(4),
            Status = "Planning",
            Progress = 10
        });
        
        _projects.Add(new ProjectInfo
        {
            Id = Guid.NewGuid(),
            Name = "Data Migration",
            StartDate = DateTime.Today.AddDays(-30),
            EndDate = DateTime.Today.AddMonths(2),
            Status = "In Progress",
            Progress = 75
        });
    }
    
    private void LoadAllProjects()
    {
        _ganttChart.Tasks.Clear();
        
        int rowIndex = 1;
        foreach (var project in _projects)
        {
            var task = new GanttTask
            {
                Title = project.Name,
                Start = project.StartDate,
                End = project.EndDate,
                RowIndex = rowIndex++,
                Progress = project.Progress,
                Status = project.Status switch
                {
                    "Planning" => TaskStatus.NotStarted,
                    "In Progress" => TaskStatus.InProgress,
                    "Completed" => TaskStatus.Completed,
                    _ => TaskStatus.NotStarted
                }
            };
            
            _ganttChart.Tasks.Add(new GanttTaskViewModel(task));
        }
        
        _ganttChart.TimeUnit = TimeUnit.Month;
        _ganttChart.StartTime = _projects.Min(p => p.StartDate);
        _ganttChart.EndTime = _projects.Max(p => p.EndDate);
    }
    
    private void LoadSelectedProject()
    {
        if (_selectedProject == null) return;
        
        // Load detailed tasks for selected project
        // This would typically come from a service
        _ganttChart.Tasks.Clear();
        // ... load project-specific tasks
    }
}

public class ProjectInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public double Progress { get; set; }
}
```

## Task Dependencies

Implement task dependencies with validation.

```csharp
public class DependencyManagementViewModel : ViewModelBase
{
    private readonly GanttChartViewModel _ganttChart;
    
    public DependencyManagementViewModel()
    {
        _ganttChart = new GanttChartViewModel();
        CreateTasksWithDependencies();
    }
    
    public GanttChartViewModel GanttChart => _ganttChart;
    
    private void CreateTasksWithDependencies()
    {
        // Create tasks
        var taskA = new GanttTask
        {
            Id = Guid.NewGuid(),
            Title = "Requirements Gathering",
            Start = DateTime.Today,
            End = DateTime.Today.AddDays(5),
            RowIndex = 1
        };
        
        var taskB = new GanttTask
        {
            Id = Guid.NewGuid(),
            Title = "System Design",
            Start = DateTime.Today.AddDays(5),
            End = DateTime.Today.AddDays(12),
            RowIndex = 2
        };
        taskB.Dependencies.Add(taskA.Id); // Depends on Requirements
        
        var taskC = new GanttTask
        {
            Id = Guid.NewGuid(),
            Title = "Database Design",
            Start = DateTime.Today.AddDays(5),
            End = DateTime.Today.AddDays(10),
            RowIndex = 3
        };
        taskC.Dependencies.Add(taskA.Id); // Depends on Requirements
        
        var taskD = new GanttTask
        {
            Id = Guid.NewGuid(),
            Title = "Implementation",
            Start = DateTime.Today.AddDays(12),
            End = DateTime.Today.AddDays(25),
            RowIndex = 4
        };
        taskD.Dependencies.AddRange(new[] { taskB.Id, taskC.Id }); // Depends on both Design tasks
        
        foreach (var task in new[] { taskA, taskB, taskC, taskD })
        {
            _ganttChart.Tasks.Add(new GanttTaskViewModel(task));
        }
    }
    
    public ICommand ValidateDependenciesCommand => new RelayCommand(ValidateDependencies);
    
    private void ValidateDependencies()
    {
        var violations = new List<string>();
        
        foreach (var taskVM in _ganttChart.Tasks)
        {
            var task = taskVM.Model;
            foreach (var depId in task.Dependencies)
            {
                var dependency = _ganttChart.Tasks.FirstOrDefault(t => t.Id == depId);
                if (dependency != null && task.Start < dependency.End)
                {
                    violations.Add($"'{task.Title}' starts before dependency '{dependency.Title}' ends");
                }
            }
        }
        
        if (violations.Any())
        {
            MessageBox.Show($"Dependency violations:\n\n{string.Join("\n", violations)}");
        }
        else
        {
            MessageBox.Show("All dependencies are valid.");
        }
    }
    
    public ICommand AutoScheduleCommand => new RelayCommand(AutoScheduleTasks);
    
    private void AutoScheduleTasks()
    {
        var taskLookup = _ganttChart.Tasks.ToDictionary(t => t.Id, t => t);
        var processed = new HashSet<Guid>();
        
        foreach (var taskVM in _ganttChart.Tasks.OrderBy(t => t.Model.Dependencies.Count))
        {
            ScheduleTask(taskVM, taskLookup, processed);
        }
    }
    
    private void ScheduleTask(GanttTaskViewModel taskVM, Dictionary<Guid, GanttTaskViewModel> lookup, HashSet<Guid> processed)
    {
        if (processed.Contains(taskVM.Id)) return;
        
        // Schedule dependencies first
        foreach (var depId in taskVM.Model.Dependencies)
        {
            if (lookup.TryGetValue(depId, out var dependency))
            {
                ScheduleTask(dependency, lookup, processed);
            }
        }
        
        // Find latest end date of dependencies
        DateTime earliestStart = DateTime.Today;
        foreach (var depId in taskVM.Model.Dependencies)
        {
            if (lookup.TryGetValue(depId, out var dependency))
            {
                earliestStart = earliestStart > dependency.End ? earliestStart : dependency.End;
            }
        }
        
        // Update task schedule
        var duration = taskVM.End - taskVM.Start;
        taskVM.Start = earliestStart;
        taskVM.End = earliestStart.Add(duration);
        
        processed.Add(taskVM.Id);
    }
}
```

## Real-time Updates

Implement real-time updates using SignalR or similar technology.

```csharp
public class RealTimeGanttViewModel : ViewModelBase
{
    private readonly GanttChartViewModel _ganttChart;
    private readonly IHubConnection _hubConnection;
    private readonly Timer _updateTimer;
    
    public RealTimeGanttViewModel()
    {
        _ganttChart = new GanttChartViewModel();
        
        // Initialize SignalR connection
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://your-signalr-hub/projectHub")
            .Build();
        
        SetupSignalRHandlers();
        StartConnection();
        
        // Simulate progress updates
        _updateTimer = new Timer(SimulateProgress, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
    }
    
    public GanttChartViewModel GanttChart => _ganttChart;
    
    private void SetupSignalRHandlers()
    {
        _hubConnection.On<Guid, double>("TaskProgressUpdated", (taskId, progress) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var task = _ganttChart.Tasks.FirstOrDefault(t => t.Id == taskId);
                if (task != null)
                {
                    task.Progress = progress;
                    
                    // Update status based on progress
                    task.Status = progress switch
                    {
                        0 => TaskStatus.NotStarted,
                        100 => TaskStatus.Completed,
                        _ => TaskStatus.InProgress
                    };
                }
            });
        });
        
        _hubConnection.On<GanttTask>("TaskAdded", (task) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _ganttChart.Tasks.Add(new GanttTaskViewModel(task));
            });
        });
        
        _hubConnection.On<Guid>("TaskDeleted", (taskId) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var task = _ganttChart.Tasks.FirstOrDefault(t => t.Id == taskId);
                if (task != null)
                {
                    _ganttChart.Tasks.Remove(task);
                }
            });
        });
    }
    
    private async void StartConnection()
    {
        try
        {
            await _hubConnection.StartAsync();
            await _hubConnection.SendAsync("JoinProject", _ganttChart.ProjectId);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Connection failed: {ex.Message}");
        }
    }
    
    private void SimulateProgress(object? state)
    {
        // Simulate random progress updates
        var random = new Random();
        var tasksInProgress = _ganttChart.Tasks.Where(t => t.Status == TaskStatus.InProgress).ToList();
        
        if (tasksInProgress.Any())
        {
            var task = tasksInProgress[random.Next(tasksInProgress.Count)];
            var newProgress = Math.Min(100, task.Progress + random.Next(1, 10));
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                task.Progress = newProgress;
                if (newProgress >= 100)
                {
                    task.Status = TaskStatus.Completed;
                }
            });
            
            // Notify other clients
            _hubConnection?.SendAsync("UpdateTaskProgress", task.Id, newProgress);
        }
    }
    
    public async void Dispose()
    {
        _updateTimer?.Dispose();
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
```

## Custom Styling Example

Advanced styling for different task types and priorities.

```xml
<Window.Resources>
    <!-- Custom styles based on task properties -->
    <Style x:Key="HighPriorityTaskStyle" TargetType="{x:Type gantt:GanttTaskBar}">
        <Setter Property="Background" Value="#FF5722"/>
        <Setter Property="OutlineBrush" Value="#D32F2F"/>
        <Setter Property="OutlineThickness" Value="2"/>
    </Style>
    
    <Style x:Key="CompletedTaskStyle" TargetType="{x:Type gantt:GanttTaskBar}">
        <Setter Property="Background" Value="#4CAF50"/>
        <Setter Property="OutlineBrush" Value="#388E3C"/>
        <Setter Property="Opacity" Value="0.8"/>
    </Style>
    
    <!-- Data template for task bars with conditional styling -->
    <DataTemplate x:Key="ConditionalTaskBarTemplate">
        <gantt:GanttTaskBar>
            <gantt:GanttTaskBar.Style>
                <Style TargetType="{x:Type gantt:GanttTaskBar}">
                    <Setter Property="Background" Value="#2196F3"/>
                    <Style.Triggers>
                        <DataTrigger Binding="{Binding Priority}" Value="Critical">
                            <Setter Property="Background" Value="#FF1744"/>
                        </DataTrigger>
                        <DataTrigger Binding="{Binding Priority}" Value="High">
                            <Setter Property="Background" Value="#FF5722"/>
                        </DataTrigger>
                        <DataTrigger Binding="{Binding Status}" Value="Completed">
                            <Setter Property="Background" Value="#4CAF50"/>
                        </DataTrigger>
                        <DataTrigger Binding="{Binding Status}" Value="OnHold">
                            <Setter Property="Background" Value="#FFC107"/>
                        </DataTrigger>
                    </Style.Triggers>
                </Style>
            </gantt:GanttTaskBar.Style>
        </gantt:GanttTaskBar>
    </DataTemplate>
</Window.Resources>
```

These examples demonstrate the flexibility and power of GPM.Gantt across various domains and use cases. Each example can be adapted and extended based on specific requirements and business needs.