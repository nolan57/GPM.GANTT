using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Implementation of Gantt chart service for data operations.
    /// </summary>
    public class GanttService : IGanttService
    {
        private readonly IValidationService _validationService;
        private readonly Dictionary<Guid, List<GanttTask>> _projectTasks;
        
        /// <summary>
        /// Initializes a new instance of the GanttService class.
        /// </summary>
        /// <param name="validationService">The validation service.</param>
        public GanttService(IValidationService? validationService = null)
        {
            _validationService = validationService ?? new ValidationService();
            _projectTasks = new Dictionary<Guid, List<GanttTask>>();
        }
        
        /// <summary>
        /// Gets all tasks for the specified project.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns>A collection of tasks.</returns>
        public async Task<IEnumerable<GanttTask>> GetTasksAsync(Guid projectId)
        {
            return await GetTasksAsync(projectId, CancellationToken.None);
        }

        public async Task<IEnumerable<GanttTask>> GetTasksAsync(Guid projectId, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken); // Simulate async operation

            return _projectTasks.TryGetValue(projectId, out var tasks)
                ? tasks.ToList()
                : Enumerable.Empty<GanttTask>();
        }
        
        /// <summary>
        /// Creates a new task.
        /// </summary>
        /// <param name="task">The task to create.</param>
        /// <returns>The created task with updated ID.</returns>
        public async Task<GanttTask> CreateTaskAsync(GanttTask task)
        {
            return await CreateTaskAsync(Guid.Empty, task, CancellationToken.None);
        }

        public async Task<GanttTask> CreateTaskAsync(Guid projectId, GanttTask task, CancellationToken cancellationToken = default)
        {
            await Task.Delay(10, cancellationToken); // Simulate async operation
            
            if (task == null)
                throw new ArgumentNullException(nameof(task));
                
            var validationResult = _validationService.ValidateTask(task);
            if (!validationResult.IsValid)
                throw new InvalidOperationException($"Task validation failed: {string.Join(", ", validationResult.Errors)}");
            
            // Ensure unique ID
            if (task.Id == Guid.Empty)
                task.Id = Guid.NewGuid();
            
            if (!_projectTasks.ContainsKey(projectId))
                _projectTasks[projectId] = new List<GanttTask>();
            
            _projectTasks[projectId].Add(task);
            
            return task;
        }
        
        /// <summary>
        /// Updates an existing task.
        /// </summary>
        /// <param name="task">The task to update.</param>
        /// <returns>The updated task.</returns>
        public async Task<GanttTask> UpdateTaskAsync(GanttTask task)
        {
            await Task.Delay(10); // Simulate async operation
            
            if (task == null)
                throw new ArgumentNullException(nameof(task));
                
            var validationResult = _validationService.ValidateTask(task);
            if (!validationResult.IsValid)
                throw new InvalidOperationException($"Task validation failed: {string.Join(", ", validationResult.Errors)}");
            
            // Find and update the task
            foreach (var projectTasks in _projectTasks.Values)
            {
                var existingTask = projectTasks.FirstOrDefault(t => t.Id == task.Id);
                if (existingTask != null)
                {
                    var index = projectTasks.IndexOf(existingTask);
                    projectTasks[index] = task;
                    return task;
                }
            }
            
            throw new InvalidOperationException($"Task with ID {task.Id} not found");
        }

        public async Task<GanttTask> UpdateTaskAsync(Guid projectId, GanttTask task, CancellationToken cancellationToken = default)
        {
            await Task.Delay(10, cancellationToken); // Simulate async operation

            if (task == null)
                throw new ArgumentNullException(nameof(task));

            var validationResult = _validationService.ValidateTask(task);
            if (!validationResult.IsValid)
                throw new InvalidOperationException($"Task validation failed: {string.Join(", ", validationResult.Errors)}");

            if (_projectTasks.TryGetValue(projectId, out var projectTasks))
            {
                var existingTask = projectTasks.FirstOrDefault(t => t.Id == task.Id);
                if (existingTask != null)
                {
                    var index = projectTasks.IndexOf(existingTask);
                    projectTasks[index] = task;
                    return task;
                }
            }

            // Fallback: search all projects (preserve previous behavior)
            foreach (var tasks in _projectTasks.Values)
            {
                var existingTask = tasks.FirstOrDefault(t => t.Id == task.Id);
                if (existingTask != null)
                {
                    var index = tasks.IndexOf(existingTask);
                    tasks[index] = task;
                    return task;
                }
            }

            throw new InvalidOperationException($"Task with ID {task.Id} not found");
        }
        
        /// <summary>
        /// Deletes a task by ID.
        /// </summary>
        /// <param name="taskId">The task identifier.</param>
        /// <returns>True if deleted successfully, false otherwise.</returns>
        public async Task<bool> DeleteTaskAsync(Guid taskId)
        {
            await Task.Delay(10); // Simulate async operation
            
            foreach (var projectTasks in _projectTasks.Values)
            {
                var task = projectTasks.FirstOrDefault(t => t.Id == taskId);
                if (task != null)
                {
                    projectTasks.Remove(task);
                    return true;
                }
            }
            
            return false;
        }

        public async Task<bool> DeleteTaskAsync(Guid projectId, Guid taskId, CancellationToken cancellationToken = default)
        {
            await Task.Delay(10, cancellationToken); // Simulate async operation

            if (_projectTasks.TryGetValue(projectId, out var projectTasks))
            {
                var task = projectTasks.FirstOrDefault(t => t.Id == taskId);
                if (task != null)
                {
                    projectTasks.Remove(task);
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Validates a collection of tasks.
        /// </summary>
        /// <param name="tasks">The tasks to validate.</param>
        /// <returns>The validation result.</returns>
        public async Task<ValidationResult> ValidateTasksAsync(IEnumerable<GanttTask> tasks)
        {
            await Task.Delay(10); // Simulate async operation
            return _validationService.ValidateTaskCollection(tasks);
        }

        public async Task<ValidationResult> ValidateTasksAsync(IEnumerable<GanttTask> tasks, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken); // Simulate async operation
            return _validationService.ValidateTaskCollection(tasks);
        }
        
        /// <summary>
        /// Calculates the optimal schedule for tasks considering dependencies.
        /// </summary>
        /// <param name="tasks">The tasks to schedule.</param>
        /// <returns>The optimized task collection.</returns>
        public async Task<IEnumerable<GanttTask>> OptimizeScheduleAsync(IEnumerable<GanttTask> tasks)
        {
            await Task.Delay(50); // Simulate complex operation
            
            var taskList = tasks.ToList();
            var optimizedTasks = new List<GanttTask>();
            
            // Simple optimization: sort by dependencies first, then by start time
            var processedIds = new HashSet<Guid>();
            
            while (optimizedTasks.Count < taskList.Count)
            {
                var availableTasks = taskList
                    .Where(t => !processedIds.Contains(t.Id) && 
                               t.Dependencies.All(dep => processedIds.Contains(dep)))
                    .OrderBy(t => t.Start)
                    .ToList();
                
                if (!availableTasks.Any())
                {
                    // Add remaining tasks even if dependencies are not met (circular or missing)
                    var remaining = taskList.Where(t => !processedIds.Contains(t.Id)).ToList();
                    optimizedTasks.AddRange(remaining);
                    break;
                }
                
                foreach (var task in availableTasks)
                {
                    optimizedTasks.Add(task);
                    processedIds.Add(task.Id);
                }
            }
            
            return optimizedTasks;
        }

        public async Task<IEnumerable<GanttTask>> OptimizeScheduleAsync(IEnumerable<GanttTask> tasks, CancellationToken cancellationToken)
        {
            await Task.Delay(50, cancellationToken); // Simulate complex operation

            var taskList = tasks.ToList();
            var optimizedTasks = new List<GanttTask>();

            var processedIds = new HashSet<Guid>();

            while (optimizedTasks.Count < taskList.Count)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var availableTasks = taskList
                    .Where(t => !processedIds.Contains(t.Id) &&
                               t.Dependencies.All(dep => processedIds.Contains(dep)))
                    .OrderBy(t => t.Start)
                    .ToList();

                if (!availableTasks.Any())
                {
                    var remaining = taskList.Where(t => !processedIds.Contains(t.Id)).ToList();
                    optimizedTasks.AddRange(remaining);
                    break;
                }

                foreach (var task in availableTasks)
                {
                    optimizedTasks.Add(task);
                    processedIds.Add(task.Id);
                }
            }

            return optimizedTasks;
        }
    }
}