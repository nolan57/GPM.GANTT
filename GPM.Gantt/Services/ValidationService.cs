using System;
using System.Collections.Generic;
using System.Linq;
using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Implementation of validation services for Gantt chart components.
    /// </summary>
    public class ValidationService : IValidationService
    {
        /// <summary>
        /// Validates a Gantt task and returns the validation result.
        /// </summary>
        /// <param name="task">The task to validate.</param>
        /// <returns>Validation result containing success status and error messages.</returns>
        public ValidationResult ValidateTask(GanttTask task)
        {
            var result = new ValidationResult();
            
            if (task == null)
            {
                result.AddError("Task cannot be null");
                return result;
            }
            
            // Use the task's built-in validation
            var taskErrors = task.GetValidationErrors();
            foreach (var error in taskErrors)
            {
                result.AddError(error);
            }
            
            // Additional business rule validations
            ValidateTaskBusinessRules(task, result);
            
            return result;
        }
        
        /// <summary>
        /// Validates a time range for consistency and business rules.
        /// </summary>
        /// <param name="start">Start date and time.</param>
        /// <param name="end">End date and time.</param>
        /// <returns>Validation result containing success status and error messages.</returns>
        public ValidationResult ValidateTimeRange(DateTime start, DateTime end)
        {
            var result = new ValidationResult();
            
            if (start > end)
            {
                result.AddError("Start time must be before or equal to end time");
            }
            
            // Check for reasonable time ranges
            var duration = end - start;
            if (duration.TotalDays > 3650) // More than 10 years
            {
                result.AddWarning("Time range exceeds 10 years, which may impact performance");
            }
            
            if (duration.TotalMinutes < 1) // Less than 1 minute
            {
                result.AddWarning("Time range is very short (less than 1 minute)");
            }
            
            return result;
        }
        
        /// <summary>
        /// Validates a collection of tasks for conflicts and dependencies.
        /// </summary>
        /// <param name="tasks">Collection of tasks to validate.</param>
        /// <returns>Validation result containing success status and error messages.</returns>
        public ValidationResult ValidateTaskCollection(IEnumerable<GanttTask> tasks)
        {
            var result = new ValidationResult();
            
            if (tasks == null)
            {
                result.AddError("Task collection cannot be null");
                return result;
            }
            
            var taskList = tasks.ToList();
            
            // Validate individual tasks
            foreach (var task in taskList)
            {
                var taskResult = ValidateTask(task);
                result.Merge(taskResult);
            }
            
            // Check for duplicate IDs
            var duplicateIds = taskList
                .GroupBy(t => t.Id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
                
            foreach (var duplicateId in duplicateIds)
            {
                result.AddError($"Duplicate task ID found: {duplicateId}");
            }
            
            // Check for row conflicts (multiple tasks on same row with overlapping times)
            ValidateRowConflicts(taskList, result);
            
            // Validate all dependencies
            foreach (var task in taskList)
            {
                var dependencyResult = ValidateTaskDependencies(task, taskList);
                result.Merge(dependencyResult);
            }
            
            return result;
        }
        
        /// <summary>
        /// Validates task dependencies for circular references and validity.
        /// </summary>
        /// <param name="task">The task to validate dependencies for.</param>
        /// <param name="allTasks">All available tasks for dependency validation.</param>
        /// <returns>Validation result containing success status and error messages.</returns>
        public ValidationResult ValidateTaskDependencies(GanttTask task, IEnumerable<GanttTask> allTasks)
        {
            var result = new ValidationResult();
            
            if (task == null)
            {
                result.AddError("Task cannot be null for dependency validation");
                return result;
            }
            
            var taskList = allTasks?.ToList() ?? new List<GanttTask>();
            
            foreach (var dependencyId in task.Dependencies)
            {
                // Check if dependency exists
                var dependentTask = taskList.FirstOrDefault(t => t.Id == dependencyId);
                if (dependentTask == null)
                {
                    result.AddError($"Task '{task.Title}' depends on non-existent task ID: {dependencyId}");
                    continue;
                }
                
                // Check for circular dependencies
                if (HasCircularDependency(task, dependentTask, taskList))
                {
                    result.AddError($"Circular dependency detected between '{task.Title}' and '{dependentTask.Title}'");
                }
                
                // Check for logical dependency timing
                if (task.Start < dependentTask.End)
                {
                    result.AddWarning($"Task '{task.Title}' starts before its dependency '{dependentTask.Title}' ends");
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Validates business rules specific to tasks.
        /// </summary>
        /// <param name="task">The task to validate.</param>
        /// <param name="result">The validation result to add errors/warnings to.</param>
        private static void ValidateTaskBusinessRules(GanttTask task, ValidationResult result)
        {
            // Check for weekend-only tasks (might be a warning)
            if (task.Start.DayOfWeek == DayOfWeek.Saturday || task.Start.DayOfWeek == DayOfWeek.Sunday)
            {
                if (task.End.DayOfWeek == DayOfWeek.Saturday || task.End.DayOfWeek == DayOfWeek.Sunday)
                {
                    result.AddWarning($"Task '{task.Title}' is scheduled entirely on weekends");
                }
            }
            
            // Check for very long tasks
            if (task.Duration.TotalDays > 365)
            {
                result.AddWarning($"Task '{task.Title}' has a duration longer than one year");
            }
            
            // Check for tasks in the past
            if (task.End < DateTime.Now && task.Status != Models.TaskStatus.Completed)
            {
                result.AddWarning($"Task '{task.Title}' is overdue and not marked as completed");
            }
            
            // Check for impossible progress
            if (task.Status == Models.TaskStatus.NotStarted && task.Progress > 0)
            {
                result.AddError($"Task '{task.Title}' cannot have progress when status is Not Started");
            }
            
            if (task.Status == Models.TaskStatus.Completed && task.Progress < 100)
            {
                result.AddError($"Task '{task.Title}' cannot be completed with progress less than 100%");
            }
        }
        
        /// <summary>
        /// Validates for row conflicts where multiple tasks overlap on the same row.
        /// </summary>
        /// <param name="tasks">List of tasks to check.</param>
        /// <param name="result">The validation result to add errors to.</param>
        private static void ValidateRowConflicts(List<GanttTask> tasks, ValidationResult result)
        {
            var rowGroups = tasks.GroupBy(t => t.RowIndex);
            
            foreach (var rowGroup in rowGroups)
            {
                var rowTasks = rowGroup.OrderBy(t => t.Start).ToList();
                
                for (int i = 0; i < rowTasks.Count - 1; i++)
                {
                    for (int j = i + 1; j < rowTasks.Count; j++)
                    {
                        if (rowTasks[i].OverlapsWith(rowTasks[j]))
                        {
                            result.AddError($"Tasks '{rowTasks[i].Title}' and '{rowTasks[j].Title}' overlap on row {rowGroup.Key}");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Checks for circular dependencies between tasks.
        /// </summary>
        /// <param name="task">The current task.</param>
        /// <param name="dependentTask">The task being checked for circular dependency.</param>
        /// <param name="allTasks">All available tasks.</param>
        /// <returns>True if circular dependency exists, false otherwise.</returns>
        private static bool HasCircularDependency(GanttTask task, GanttTask dependentTask, List<GanttTask> allTasks)
        {
            var visited = new HashSet<Guid>();
            return HasCircularDependencyRecursive(task.Id, dependentTask, allTasks, visited);
        }
        
        /// <summary>
        /// Recursively checks for circular dependencies.
        /// </summary>
        /// <param name="originalTaskId">The ID of the original task.</param>
        /// <param name="currentTask">The current task in the dependency chain.</param>
        /// <param name="allTasks">All available tasks.</param>
        /// <param name="visited">Set of visited task IDs to prevent infinite loops.</param>
        /// <returns>True if circular dependency exists, false otherwise.</returns>
        private static bool HasCircularDependencyRecursive(Guid originalTaskId, GanttTask currentTask, List<GanttTask> allTasks, HashSet<Guid> visited)
        {
            if (visited.Contains(currentTask.Id))
                return false; // Already processed this path
                
            visited.Add(currentTask.Id);
            
            foreach (var dependencyId in currentTask.Dependencies)
            {
                if (dependencyId == originalTaskId)
                    return true; // Circular dependency found
                    
                var nextTask = allTasks.FirstOrDefault(t => t.Id == dependencyId);
                if (nextTask != null && HasCircularDependencyRecursive(originalTaskId, nextTask, allTasks, visited))
                    return true;
            }
            
            return false;
        }
    }
}