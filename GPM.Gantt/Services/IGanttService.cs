using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GPM.Gantt.Models;
using System.Threading;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Service interface for Gantt chart operations and data management.
    /// </summary>
    public interface IGanttService
    {
        /// <summary>
        /// Gets all tasks for the specified project.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns>A collection of tasks.</returns>
        Task<IEnumerable<GanttTask>> GetTasksAsync(Guid projectId);
        
        /// <summary>
        /// Creates a new task.
        /// </summary>
        /// <param name="task">The task to create.</param>
        /// <returns>The created task with updated ID.</returns>
        Task<GanttTask> CreateTaskAsync(GanttTask task);
        
        /// <summary>
        /// Updates an existing task.
        /// </summary>
        /// <param name="task">The task to update.</param>
        /// <returns>The updated task.</returns>
        Task<GanttTask> UpdateTaskAsync(GanttTask task);
        
        /// <summary>
        /// Deletes a task by ID.
        /// </summary>
        /// <param name="taskId">The task identifier.</param>
        /// <returns>True if deleted successfully, false otherwise.</returns>
        Task<bool> DeleteTaskAsync(Guid taskId);
        
        /// <summary>
        /// Validates a collection of tasks.
        /// </summary>
        /// <param name="tasks">The tasks to validate.</param>
        /// <returns>The validation result.</returns>
        Task<ValidationResult> ValidateTasksAsync(IEnumerable<GanttTask> tasks);
        
        /// <summary>
        /// Calculates the optimal schedule for tasks considering dependencies.
        /// </summary>
        /// <param name="tasks">The tasks to schedule.</param>
        /// <returns>The optimized task collection.</returns>
        Task<IEnumerable<GanttTask>> OptimizeScheduleAsync(IEnumerable<GanttTask> tasks);

        // New overloads with CancellationToken and project scoping
        Task<IEnumerable<GanttTask>> GetTasksAsync(Guid projectId, CancellationToken cancellationToken);

        Task<GanttTask> CreateTaskAsync(Guid projectId, GanttTask task, CancellationToken cancellationToken = default);

        Task<GanttTask> UpdateTaskAsync(Guid projectId, GanttTask task, CancellationToken cancellationToken = default);

        Task<bool> DeleteTaskAsync(Guid projectId, Guid taskId, CancellationToken cancellationToken = default);

        Task<ValidationResult> ValidateTasksAsync(IEnumerable<GanttTask> tasks, CancellationToken cancellationToken);

        Task<IEnumerable<GanttTask>> OptimizeScheduleAsync(IEnumerable<GanttTask> tasks, CancellationToken cancellationToken);
    }
}