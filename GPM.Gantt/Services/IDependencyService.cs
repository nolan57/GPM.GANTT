using System.Collections.Generic;
using System.Threading.Tasks;
using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Service interface for managing task dependencies and critical path analysis
    /// </summary>
    public interface IDependencyService
    {
        /// <summary>
        /// Gets all dependencies for a specific project
        /// </summary>
        /// <param name="projectId">The project identifier</param>
        /// <returns>List of task dependencies</returns>
        Task<List<TaskDependency>> GetDependenciesAsync(string projectId);
        
        /// <summary>
        /// Creates a new task dependency
        /// </summary>
        /// <param name="dependency">The dependency to create</param>
        /// <returns>The created dependency with assigned ID</returns>
        Task<TaskDependency> CreateDependencyAsync(TaskDependency dependency);
        
        /// <summary>
        /// Updates an existing task dependency
        /// </summary>
        /// <param name="dependency">The dependency to update</param>
        /// <returns>True if update was successful</returns>
        Task<bool> UpdateDependencyAsync(TaskDependency dependency);
        
        /// <summary>
        /// Deletes a task dependency
        /// </summary>
        /// <param name="dependencyId">ID of the dependency to delete</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteDependencyAsync(string dependencyId);
        
        /// <summary>
        /// Gets all dependencies for a specific task
        /// </summary>
        /// <param name="taskId">The task identifier</param>
        /// <returns>List of dependencies involving the task</returns>
        Task<List<TaskDependency>> GetTaskDependenciesAsync(string taskId);
        
        /// <summary>
        /// Validates a dependency for circular references and other constraints
        /// </summary>
        /// <param name="dependency">The dependency to validate</param>
        /// <returns>True if dependency is valid</returns>
        Task<bool> ValidateDependencyAsync(TaskDependency dependency);
        
        /// <summary>
        /// Calculates the critical path for a set of tasks
        /// </summary>
        /// <param name="tasks">List of tasks</param>
        /// <param name="dependencies">List of dependencies</param>
        /// <returns>List of task IDs that form the critical path</returns>
        Task<List<string>> GetCriticalPathAsync(List<GanttTask> tasks, List<TaskDependency> dependencies);
        
        /// <summary>
        /// Calculates float times for all tasks
        /// </summary>
        /// <param name="tasks">List of tasks</param>
        /// <param name="dependencies">List of dependencies</param>
        /// <returns>Dictionary mapping task IDs to their float times</returns>
        Task<Dictionary<string, System.TimeSpan>> CalculateFloatTimesAsync(List<GanttTask> tasks, List<TaskDependency> dependencies);
        
        /// <summary>
        /// Checks if adding a dependency would create a circular reference
        /// </summary>
        /// <param name="tasks">List of tasks</param>
        /// <param name="dependencies">Existing dependencies</param>
        /// <param name="newDependency">The new dependency to check</param>
        /// <returns>True if circular dependency would be created</returns>
        bool HasCircularDependency(List<GanttTask> tasks, List<TaskDependency> dependencies, TaskDependency newDependency);
        
        /// <summary>
        /// Calculates early start/finish and late start/finish times for all tasks
        /// </summary>
        /// <param name="tasks">List of tasks</param>
        /// <param name="dependencies">List of dependencies</param>
        /// <returns>Updated tasks with calculated times</returns>
        Task<List<GanttTask>> CalculateScheduleAsync(List<GanttTask> tasks, List<TaskDependency> dependencies);
        
        /// <summary>
        /// Automatically schedules tasks based on dependencies
        /// </summary>
        /// <param name="tasks">List of tasks to schedule</param>
        /// <param name="dependencies">List of dependencies</param>
        /// <param name="projectStart">Project start date</param>
        /// <returns>Updated tasks with calculated dates</returns>
        Task<List<GanttTask>> AutoScheduleTasksAsync(List<GanttTask> tasks, List<TaskDependency> dependencies, System.DateTime projectStart);
    }
}