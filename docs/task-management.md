# Task Management - GPM.Gantt

# Task Management Guide - GPM.Gantt v2.1.0

This guide covers working with tasks, validation, and task-related operations in GPM.Gantt, including the advanced features introduced in version 2.1.0.

## Task Model Overview

The `GanttTask` class is the core data model representing tasks in the Gantt chart. It provides comprehensive properties for project management scenarios.

### Core Properties

```csharp
public class GanttTask
{
    // Identification
    public Guid Id { get; set; }                    // Unique identifier
    public string Title { get; set; }               // Task name
    public string? Description { get; set; }        // Detailed description
    
    // Scheduling
    public DateTime Start { get; set; }             // Start date/time
    public DateTime End { get; set; }               // End date/time
    public int RowIndex { get; set; }               // Display row (1-based)
    
    // Progress and Status
    public double Progress { get; set; }            // 0-100 percentage
    public TaskStatus Status { get; set; }          // Current status
    public TaskPriority Priority { get; set; }      // Priority level
    
    // Relationships
    public Guid? ParentTaskId { get; set; }         // Parent for hierarchy
    public List<Guid> Dependencies { get; set; }    // Task dependencies
    public List<string> AssignedResources { get; set; } // Assigned people/resources
    
    // Calculated
    public TimeSpan Duration => End - Start;        // Computed duration
}
```

## Creating and Managing Tasks

### Basic Task Creation

```csharp
// Simple task
var task = new GanttTask
{
    Title = "Database Design",
    Start = DateTime.Today,
    End = DateTime.Today.AddDays(5),
    RowIndex = 1,
    Status = TaskStatus.NotStarted,
    Priority = TaskPriority.High
};

// Task with additional details
var detailedTask = new GanttTask
{
    Title = "API Development",
    Description = "Develop REST API endpoints for user management",
    Start = DateTime.Today.AddDays(3),
    End = DateTime.Today.AddDays(14),
    RowIndex = 2,
    Status = TaskStatus.InProgress,
    Progress = 25,
    Priority = TaskPriority.Normal
};

// Add assigned resources
detailedTask.AssignedResources.Add("John Doe");
detailedTask.AssignedResources.Add("Jane Smith");
```

### Task Factory Pattern

```csharp
public static class TaskFactory
{
    public static GanttTask CreateDevelopmentTask(string title, DateTime start, int durationDays, 
        string developer, TaskPriority priority = TaskPriority.Normal)
    {
        return new GanttTask
        {
            Title = title,
            Start = start,
            End = start.AddDays(durationDays),
            Status = TaskStatus.NotStarted,
            Priority = priority,
            AssignedResources = { developer }
        };
    }
    
    public static GanttTask CreateMilestone(string title, DateTime date, int rowIndex)
    {
        return new GanttTask
        {
            Title = $"📍 {title}",
            Start = date,
            End = date, // Zero duration for milestones
            RowIndex = rowIndex,
            Status = TaskStatus.NotStarted,
            Priority = TaskPriority.Critical,
            Progress = 0
        };
    }
    
    public static GanttTask CreatePhase(string title, DateTime start, DateTime end, 
        int rowIndex, List<GanttTask> childTasks)
    {
        var phase = new GanttTask
        {
            Title = title,
            Start = start,
            End = end,
            RowIndex = rowIndex,
            Status = TaskStatus.NotStarted,
            Priority = TaskPriority.High
        };
        
        // Set child task relationships
        foreach (var child in childTasks)
        {
            child.ParentTaskId = phase.Id;
        }
        
        return phase;
    }
}
```

### Using with ViewModels

```csharp
// Add task through ViewModel
var taskViewModel = new GanttTaskViewModel(task);
ganttChartViewModel.Tasks.Add(taskViewModel);

// Modify task properties with automatic notifications
taskViewModel.Title = "Updated Task Title";
taskViewModel.Progress = 50;
taskViewModel.Status = TaskStatus.InProgress;

// Access underlying model
var model = taskViewModel.Model;
```

## Task Status Management

### Status Enumeration

```csharp
public enum TaskStatus
{
    NotStarted = 0,    // Task hasn't begun
    InProgress = 1,    // Task is active
    Completed = 2,     // Task is finished
    Cancelled = 3,     // Task was cancelled
    OnHold = 4         // Task is paused
}
```

### Status Workflows

```csharp
public class TaskStatusManager
{
    private static readonly Dictionary<TaskStatus, List<TaskStatus>> ValidTransitions = new()
    {
        [TaskStatus.NotStarted] = new() { TaskStatus.InProgress, TaskStatus.Cancelled },
        [TaskStatus.InProgress] = new() { TaskStatus.Completed, TaskStatus.OnHold, TaskStatus.Cancelled },
        [TaskStatus.OnHold] = new() { TaskStatus.InProgress, TaskStatus.Cancelled },
        [TaskStatus.Completed] = new() { TaskStatus.InProgress }, // Allow reopening
        [TaskStatus.Cancelled] = new() { TaskStatus.NotStarted }  // Allow restart
    };
    
    public static bool CanTransition(TaskStatus from, TaskStatus to)
    {
        return ValidTransitions.ContainsKey(from) && 
               ValidTransitions[from].Contains(to);
    }
    
    public static void TransitionTask(GanttTaskViewModel task, TaskStatus newStatus)
    {
        if (!CanTransition(task.Status, newStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition from {task.Status} to {newStatus}");
        }
        
        task.Status = newStatus;
        
        // Auto-update progress based on status
        switch (newStatus)
        {
            case TaskStatus.NotStarted:
                task.Progress = 0;
                break;
            case TaskStatus.Completed:
                task.Progress = 100;
                break;
            case TaskStatus.Cancelled:
                // Keep existing progress
                break;
        }
    }
}
```

## Task Validation

### Built-in Validation

```csharp
// Check if task is valid
if (!task.IsValid())
{
    var errors = task.GetValidationErrors();
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation Error: {error}");
    }
}

// Using validation service
var validationService = new ValidationService();
var result = validationService.ValidateTask(task);

if (!result.IsValid)
{
    Console.WriteLine($"Validation failed: {string.Join(", ", result.Errors)}");
}
```

### Custom Validation Rules

```csharp
public class CustomTaskValidator : IValidationService
{
    public ValidationResult ValidateTask(GanttTask task)
    {
        var errors = new List<string>();
        
        // Built-in validation
        errors.AddRange(task.GetValidationErrors());
        
        // Custom business rules
        if (task.AssignedResources.Count == 0)
        {
            errors.Add("Task must have at least one assigned resource");
        }
        
        if (task.Priority == TaskPriority.Critical && task.Progress == 0 && 
            task.Start < DateTime.Today)
        {
            errors.Add("Critical tasks cannot remain unstarted past their start date");
        }
        
        if (task.Duration.TotalDays > 30)
        {
            errors.Add("Tasks should not exceed 30 days - consider breaking into smaller tasks");
        }
        
        if (task.End.DayOfWeek == DayOfWeek.Saturday || task.End.DayOfWeek == DayOfWeek.Sunday)
        {
            errors.Add("Tasks should not end on weekends");
        }
        
        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
    
    public ValidationResult ValidateTaskCollection(IEnumerable<GanttTask> tasks)
    {
        var allErrors = new List<string>();
        
        foreach (var task in tasks)
        {
            var taskResult = ValidateTask(task);
            if (!taskResult.IsValid)
            {
                allErrors.AddRange(taskResult.Errors.Select(e => $"{task.Title}: {e}"));
            }
        }
        
        // Collection-level validations
        var taskList = tasks.ToList();
        
        // Check for overlapping tasks in same row
        var rowGroups = taskList.GroupBy(t => t.RowIndex);
        foreach (var group in rowGroups)
        {
            var rowTasks = group.OrderBy(t => t.Start).ToList();
            for (int i = 0; i < rowTasks.Count - 1; i++)
            {
                if (rowTasks[i].End > rowTasks[i + 1].Start)
                {
                    allErrors.Add($"Tasks '{rowTasks[i].Title}' and '{rowTasks[i + 1].Title}' overlap in row {group.Key}");
                }
            }
        }
        
        return new ValidationResult
        {
            IsValid = allErrors.Count == 0,
            Errors = allErrors
        };
    }
}
```

## Task Dependencies

### Setting Up Dependencies

```csharp
// Create dependent tasks
var designTask = new GanttTask
{
    Id = Guid.NewGuid(),
    Title = "System Design",
    Start = DateTime.Today,
    End = DateTime.Today.AddDays(5),
    RowIndex = 1
};

var developmentTask = new GanttTask
{
    Id = Guid.NewGuid(),
    Title = "Development",
    Start = DateTime.Today.AddDays(5), // Starts after design
    End = DateTime.Today.AddDays(15),
    RowIndex = 2
};

// Add dependency
developmentTask.Dependencies.Add(designTask.Id);

var testingTask = new GanttTask
{
    Id = Guid.NewGuid(),
    Title = "Testing",
    Start = DateTime.Today.AddDays(15),
    End = DateTime.Today.AddDays(20),
    RowIndex = 3
};

// Testing depends on development
testingTask.Dependencies.Add(developmentTask.Id);
```

### Dependency Management

```csharp
public class DependencyManager
{
    public static bool HasCircularDependency(IEnumerable<GanttTask> tasks)
    {
        var taskMap = tasks.ToDictionary(t => t.Id, t => t);
        var visited = new HashSet<Guid>();
        var recursionStack = new HashSet<Guid>();
        
        foreach (var task in tasks)
        {
            if (HasCircularDependencyRecursive(task.Id, taskMap, visited, recursionStack))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private static bool HasCircularDependencyRecursive(Guid taskId, 
        Dictionary<Guid, GanttTask> taskMap, 
        HashSet<Guid> visited, 
        HashSet<Guid> recursionStack)
    {
        if (recursionStack.Contains(taskId))
            return true;
            
        if (visited.Contains(taskId))
            return false;
            
        visited.Add(taskId);
        recursionStack.Add(taskId);
        
        if (taskMap.TryGetValue(taskId, out var task))
        {
            foreach (var depId in task.Dependencies)
            {
                if (HasCircularDependencyRecursive(depId, taskMap, visited, recursionStack))
                {
                    return true;
                }
            }
        }
        
        recursionStack.Remove(taskId);
        return false;
    }
    
    public static List<GanttTask> GetTopologicalOrder(IEnumerable<GanttTask> tasks)
    {
        var taskMap = tasks.ToDictionary(t => t.Id, t => t);
        var result = new List<GanttTask>();
        var visited = new HashSet<Guid>();
        
        foreach (var task in tasks)
        {
            if (!visited.Contains(task.Id))
            {
                TopologicalSort(task.Id, taskMap, visited, result);
            }
        }
        
        result.Reverse();
        return result;
    }
    
    private static void TopologicalSort(Guid taskId, 
        Dictionary<Guid, GanttTask> taskMap, 
        HashSet<Guid> visited, 
        List<GanttTask> result)
    {
        visited.Add(taskId);
        
        if (taskMap.TryGetValue(taskId, out var task))
        {
            foreach (var depId in task.Dependencies)
            {
                if (!visited.Contains(depId))
                {
                    TopologicalSort(depId, taskMap, visited, result);
                }
            }
            result.Add(task);
        }
    }
}
```

## Task Hierarchies

### Parent-Child Relationships

```csharp
public class TaskHierarchy
{
    public static void SetupProjectHierarchy(List<GanttTask> tasks)
    {
        // Create project phases
        var phase1 = TaskFactory.CreatePhase("Phase 1: Planning", 
            DateTime.Today, DateTime.Today.AddDays(14), 1, new List<GanttTask>());
            
        var phase2 = TaskFactory.CreatePhase("Phase 2: Development", 
            DateTime.Today.AddDays(14), DateTime.Today.AddDays(60), 5, new List<GanttTask>());
        
        // Create child tasks
        var requirements = new GanttTask
        {
            Title = "Requirements Gathering",
            Start = DateTime.Today,
            End = DateTime.Today.AddDays(7),
            RowIndex = 2,
            ParentTaskId = phase1.Id
        };
        
        var design = new GanttTask
        {
            Title = "System Design",
            Start = DateTime.Today.AddDays(7),
            End = DateTime.Today.AddDays(14),
            RowIndex = 3,
            ParentTaskId = phase1.Id
        };
        
        var coding = new GanttTask
        {
            Title = "Implementation",
            Start = DateTime.Today.AddDays(14),
            End = DateTime.Today.AddDays(45),
            RowIndex = 6,
            ParentTaskId = phase2.Id
        };
        
        var testing = new GanttTask
        {
            Title = "Testing",
            Start = DateTime.Today.AddDays(45),
            End = DateTime.Today.AddDays(60),
            RowIndex = 7,
            ParentTaskId = phase2.Id
        };
        
        tasks.AddRange(new[] { phase1, requirements, design, phase2, coding, testing });
    }
    
    public static List<GanttTask> GetChildren(Guid parentId, IEnumerable<GanttTask> allTasks)
    {
        return allTasks.Where(t => t.ParentTaskId == parentId).ToList();
    }
    
    public static GanttTask? GetParent(Guid childId, IEnumerable<GanttTask> allTasks)
    {
        var child = allTasks.FirstOrDefault(t => t.Id == childId);
        if (child?.ParentTaskId == null) return null;
        
        return allTasks.FirstOrDefault(t => t.Id == child.ParentTaskId);
    }
    
    public static void UpdateParentProgress(GanttTask parent, IEnumerable<GanttTask> allTasks)
    {
        var children = GetChildren(parent.Id, allTasks);
        if (!children.Any()) return;
        
        var totalProgress = children.Sum(c => c.Progress);
        parent.Progress = totalProgress / children.Count;
        
        // Update parent dates to encompass all children
        parent.Start = children.Min(c => c.Start);
        parent.End = children.Max(c => c.End);
    }
}
```

## Resource Management

### Resource Assignment

```csharp
public class ResourceManager
{
    private readonly Dictionary<string, List<GanttTask>> _resourceAssignments = new();
    
    public void AssignResource(GanttTask task, string resourceName)
    {
        if (!task.AssignedResources.Contains(resourceName))
        {
            task.AssignedResources.Add(resourceName);
            
            if (!_resourceAssignments.ContainsKey(resourceName))
            {
                _resourceAssignments[resourceName] = new List<GanttTask>();
            }
            _resourceAssignments[resourceName].Add(task);
        }
    }
    
    public void UnassignResource(GanttTask task, string resourceName)
    {
        task.AssignedResources.Remove(resourceName);
        
        if (_resourceAssignments.ContainsKey(resourceName))
        {
            _resourceAssignments[resourceName].Remove(task);
        }
    }
    
    public List<GanttTask> GetResourceTasks(string resourceName)
    {
        return _resourceAssignments.GetValueOrDefault(resourceName, new List<GanttTask>());
    }
    
    public List<(GanttTask task1, GanttTask task2)> FindResourceConflicts(string resourceName)
    {
        var conflicts = new List<(GanttTask, GanttTask)>();
        var tasks = GetResourceTasks(resourceName).OrderBy(t => t.Start).ToList();
        
        for (int i = 0; i < tasks.Count - 1; i++)
        {
            for (int j = i + 1; j < tasks.Count; j++)
            {
                if (tasks[i].OverlapsWith(tasks[j]))
                {
                    conflicts.Add((tasks[i], tasks[j]));
                }
            }
        }
        
        return conflicts;
    }
    
    public double GetResourceUtilization(string resourceName, DateTime start, DateTime end)
    {
        var tasks = GetResourceTasks(resourceName);
        var totalAssignedTime = TimeSpan.Zero;
        
        foreach (var task in tasks)
        {
            var overlapStart = task.Start > start ? task.Start : start;
            var overlapEnd = task.End < end ? task.End : end;
            
            if (overlapStart < overlapEnd)
            {
                totalAssignedTime = totalAssignedTime.Add(overlapEnd - overlapStart);
            }
        }
        
        var totalAvailableTime = end - start;
        return totalAssignedTime.TotalHours / totalAvailableTime.TotalHours * 100;
    }
}
```

## Progress Tracking

### Progress Calculation

```csharp
public static class ProgressCalculator
{
    public static double CalculateWeightedProgress(IEnumerable<GanttTask> tasks)
    {
        var taskList = tasks.ToList();
        if (!taskList.Any()) return 0;
        
        var totalWeight = taskList.Sum(t => t.Duration.TotalHours);
        var weightedProgress = taskList.Sum(t => t.Progress * t.Duration.TotalHours);
        
        return totalWeight > 0 ? weightedProgress / totalWeight : 0;
    }
    
    public static double CalculateProjectProgress(List<GanttTask> allTasks, Guid projectId)
    {
        var projectTasks = allTasks.Where(t => GetProjectId(t) == projectId).ToList();
        return CalculateWeightedProgress(projectTasks);
    }
    
    public static void UpdateProgressBasedOnStatus(GanttTask task)
    {
        switch (task.Status)
        {
            case TaskStatus.NotStarted:
                task.Progress = 0;
                break;
            case TaskStatus.Completed:
                task.Progress = 100;
                break;
            case TaskStatus.Cancelled:
                // Keep existing progress
                break;
            case TaskStatus.InProgress:
                // Estimate progress based on time elapsed
                if (DateTime.Now >= task.Start)
                {
                    var elapsed = DateTime.Now - task.Start;
                    var total = task.End - task.Start;
                    var estimatedProgress = (elapsed.TotalDays / total.TotalDays) * 100;
                    
                    // Don't exceed 90% for in-progress tasks
                    task.Progress = Math.Min(90, Math.Max(task.Progress, estimatedProgress));
                }
                break;
        }
    }
    
    private static Guid GetProjectId(GanttTask task)
    {
        // Implementation depends on how you associate tasks with projects
        // This could be a property on GanttTask or determined by hierarchy
        return Guid.Empty; // Placeholder
    }
}
```

## Best Practices

### Task Creation

1. **Use meaningful titles** that clearly describe the work
2. **Set realistic durations** based on actual estimates
3. **Assign appropriate priorities** to help with resource allocation
4. **Include detailed descriptions** for complex tasks
5. **Set proper row indices** to organize visual layout

### Validation

1. **Validate early and often** to catch issues before they become problems
2. **Use custom validation rules** for business-specific requirements
3. **Provide clear error messages** to help users fix issues
4. **Validate collections** to catch relationship problems

### Dependencies

1. **Avoid circular dependencies** by validating the dependency graph
2. **Use logical dependencies** that reflect real work relationships
3. **Keep dependency chains simple** to avoid complexity
4. **Document dependency reasons** in task descriptions

### Resource Management

1. **Track resource assignments** to identify conflicts
2. **Monitor resource utilization** to optimize allocation
3. **Plan for resource availability** and time off
4. **Use skill-based assignment** for appropriate task matching

### Performance

1. **Limit the number of tasks** displayed at once for large projects
2. **Use efficient collection operations** when manipulating many tasks
3. **Cache calculated values** like progress and durations
4. **Implement virtualization** for very large task lists

## Advanced Task Features (v2.1.0)

### Annotation Integration

Tasks can be enhanced with rich annotations using the plugin system:

```csharp
// Create a task with associated annotations
var task = new GanttTask
{
    Title = "Critical Development Phase",
    Start = DateTime.Today.AddDays(10),
    End = DateTime.Today.AddDays(20),
    RowIndex = 1,
    Priority = TaskPriority.Critical,
    Status = TaskStatus.InProgress
};

// Add annotation metadata to task
var annotations = new List<TaskAnnotation>
{
    new TaskAnnotation
    {
        Type = AnnotationType.Text,
        Content = "High Risk - Security Review Required",
        Position = new Point(150, 25),
        Priority = AnnotationPriority.High
    },
    new TaskAnnotation
    {
        Type = AnnotationType.Shape,
        Content = "Triangle",
        FillColor = "#FFFF0000",
        Position = new Point(100, 10)
    }
};

task.Metadata["Annotations"] = annotations;
```

### Multi-Level Time Context

Tasks can be displayed with context-aware time scales:

```csharp
// Configure task for multi-level display
var task = new GanttTask
{
    Title = "Project Phase",
    Start = new DateTime(2024, 1, 1),
    End = new DateTime(2024, 12, 31),
    RowIndex = 1
};

// Add time scale context information
var timeContext = new TaskTimeContext
{
    PrimaryUnit = ExtendedTimeUnit.Month,
    SecondaryUnits = new[] { ExtendedTimeUnit.Week, ExtendedTimeUnit.Day },
    GranularityPreference = TimeGranularityPreference.Auto,
    DetailLevel = TaskDetailLevel.Summary
};

task.Metadata["TimeContext"] = timeContext;
```

### Expandable Task Segments

Tasks can be broken down into expandable segments for detailed planning:

```csharp
// Create a task with expandable segments
var task = new GanttTask
{
    Title = "Software Development",
    Start = DateTime.Today,
    End = DateTime.Today.AddDays(90),
    RowIndex = 1
};

// Define expandable segments
var segments = new List<TaskSegment>
{
    new TaskSegment
    {
        Title = "Design Phase",
        Start = DateTime.Today,
        End = DateTime.Today.AddDays(15),
        IsExpandable = true,
        DetailLevel = TaskDetailLevel.Summary
    },
    new TaskSegment
    {
        Title = "Implementation",
        Start = DateTime.Today.AddDays(15),
        End = DateTime.Today.AddDays(60),
        IsExpandable = true,
        DetailLevel = TaskDetailLevel.Summary
    },
    new TaskSegment
    {
        Title = "Testing",
        Start = DateTime.Today.AddDays(60),
        End = DateTime.Today.AddDays(90),
        IsExpandable = false,
        DetailLevel = TaskDetailLevel.Detail
    }
};

task.Metadata["Segments"] = segments;
```

These advanced features enable sophisticated task management scenarios with rich visualization capabilities.