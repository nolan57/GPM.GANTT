using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Service interface for remote project management via REST API.
    /// </summary>
    public interface IRemoteProjectService
    {
        /// <summary>
        /// Gets all projects from the remote API.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A collection of remote projects.</returns>
        Task<IEnumerable<RemoteProject>> GetProjectsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a specific project by ID from the remote API.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The remote project or null if not found.</returns>
        Task<RemoteProject?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates a new project on the remote API.
        /// </summary>
        /// <param name="project">The project to create.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created project with updated server data.</returns>
        Task<RemoteProject> CreateProjectAsync(RemoteProject project, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing project on the remote API.
        /// </summary>
        /// <param name="project">The project to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated project.</returns>
        Task<RemoteProject> UpdateProjectAsync(RemoteProject project, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deletes a project from the remote API.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if deleted successfully, false otherwise.</returns>
        Task<bool> DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all tasks for a specific project from the remote API.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A collection of remote tasks.</returns>
        Task<IEnumerable<RemoteTask>> GetProjectTasksAsync(Guid projectId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Bulk updates tasks for a project on the remote API.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="tasks">The tasks to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated tasks.</returns>
        Task<IEnumerable<RemoteTask>> UpdateProjectTasksAsync(Guid projectId, IEnumerable<RemoteTask> tasks, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Converts a GanttTask to RemoteTask for API communication.
        /// </summary>
        /// <param name="ganttTask">The local Gantt task.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <returns>The converted remote task.</returns>
        RemoteTask ConvertToRemoteTask(GanttTask ganttTask, Guid projectId);
        
        /// <summary>
        /// Converts a RemoteTask to GanttTask for local use.
        /// </summary>
        /// <param name="remoteTask">The remote task.</param>
        /// <returns>The converted Gantt task.</returns>
        GanttTask ConvertToGanttTask(RemoteTask remoteTask);
    }
}