using System;
using System.Collections.Generic;
using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Provides validation services for Gantt chart components and data.
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Validates a Gantt task and returns the validation result.
        /// </summary>
        /// <param name="task">The task to validate.</param>
        /// <returns>Validation result containing success status and error messages.</returns>
        ValidationResult ValidateTask(GanttTask task);
        
        /// <summary>
        /// Validates a time range for consistency and business rules.
        /// </summary>
        /// <param name="start">Start date and time.</param>
        /// <param name="end">End date and time.</param>
        /// <returns>Validation result containing success status and error messages.</returns>
        ValidationResult ValidateTimeRange(DateTime start, DateTime end);
        
        /// <summary>
        /// Validates a collection of tasks for conflicts and dependencies.
        /// </summary>
        /// <param name="tasks">Collection of tasks to validate.</param>
        /// <returns>Validation result containing success status and error messages.</returns>
        ValidationResult ValidateTaskCollection(IEnumerable<GanttTask> tasks);
        
        /// <summary>
        /// Validates task dependencies for circular references and validity.
        /// </summary>
        /// <param name="task">The task to validate dependencies for.</param>
        /// <param name="allTasks">All available tasks for dependency validation.</param>
        /// <returns>Validation result containing success status and error messages.</returns>
        ValidationResult ValidateTaskDependencies(GanttTask task, IEnumerable<GanttTask> allTasks);
    }
}