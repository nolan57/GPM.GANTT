using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Implementation of remote project service for REST API communication.
    /// </summary>
    public class RemoteProjectService : IRemoteProjectService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the RemoteProjectService class.
        /// </summary>
        /// <param name="httpClient">The HTTP client for API communication.</param>
        public RemoteProjectService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }

        /// <summary>
        /// Gets all projects from the remote API.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A collection of remote projects.</returns>
        public async Task<IEnumerable<RemoteProject>> GetProjectsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("api/projects", cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(cancellationToken);
                    return JsonSerializer.Deserialize<List<RemoteProject>>(json, _jsonOptions) ?? new List<RemoteProject>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"Failed to get projects: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex) when (!(ex is HttpRequestException))
            {
                throw new InvalidOperationException($"Error retrieving projects: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a specific project by ID from the remote API.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The remote project or null if not found.</returns>
        public async Task<RemoteProject?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/projects/{projectId}", cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(cancellationToken);
                    return JsonSerializer.Deserialize<RemoteProject>(json, _jsonOptions);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"Failed to get project {projectId}: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex) when (!(ex is HttpRequestException))
            {
                throw new InvalidOperationException($"Error retrieving project {projectId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a new project on the remote API.
        /// </summary>
        /// <param name="project">The project to create.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created project with updated server data.</returns>
        public async Task<RemoteProject> CreateProjectAsync(RemoteProject project, CancellationToken cancellationToken = default)
        {
            try
            {
                if (project == null)
                    throw new ArgumentNullException(nameof(project));

                var json = JsonSerializer.Serialize(project, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("api/projects", content, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                    return JsonSerializer.Deserialize<RemoteProject>(responseJson, _jsonOptions) 
                           ?? throw new InvalidOperationException("Failed to deserialize created project");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"Failed to create project: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex) when (!(ex is HttpRequestException || ex is ArgumentNullException))
            {
                throw new InvalidOperationException($"Error creating project: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates an existing project on the remote API.
        /// </summary>
        /// <param name="project">The project to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated project.</returns>
        public async Task<RemoteProject> UpdateProjectAsync(RemoteProject project, CancellationToken cancellationToken = default)
        {
            try
            {
                if (project == null)
                    throw new ArgumentNullException(nameof(project));

                var json = JsonSerializer.Serialize(project, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"api/projects/{project.Id}", content, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                    return JsonSerializer.Deserialize<RemoteProject>(responseJson, _jsonOptions) 
                           ?? throw new InvalidOperationException("Failed to deserialize updated project");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"Failed to update project {project.Id}: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex) when (!(ex is HttpRequestException || ex is ArgumentNullException))
            {
                throw new InvalidOperationException($"Error updating project: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes a project from the remote API.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if deleted successfully, false otherwise.</returns>
        public async Task<bool> DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/projects/{projectId}", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets all tasks for a specific project from the remote API.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A collection of remote tasks.</returns>
        public async Task<IEnumerable<RemoteTask>> GetProjectTasksAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/projects/{projectId}/tasks", cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(cancellationToken);
                    return JsonSerializer.Deserialize<List<RemoteTask>>(json, _jsonOptions) ?? new List<RemoteTask>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"Failed to get tasks for project {projectId}: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex) when (!(ex is HttpRequestException))
            {
                throw new InvalidOperationException($"Error retrieving tasks for project {projectId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Bulk updates tasks for a project on the remote API.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="tasks">The tasks to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated tasks.</returns>
        public async Task<IEnumerable<RemoteTask>> UpdateProjectTasksAsync(Guid projectId, IEnumerable<RemoteTask> tasks, CancellationToken cancellationToken = default)
        {
            try
            {
                if (tasks == null)
                    throw new ArgumentNullException(nameof(tasks));

                var tasksList = tasks.ToList();
                var json = JsonSerializer.Serialize(tasksList, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"api/projects/{projectId}/tasks", content, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                    return JsonSerializer.Deserialize<List<RemoteTask>>(responseJson, _jsonOptions) ?? new List<RemoteTask>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"Failed to update tasks for project {projectId}: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex) when (!(ex is HttpRequestException || ex is ArgumentNullException))
            {
                throw new InvalidOperationException($"Error updating tasks for project {projectId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Converts a GanttTask to RemoteTask for API communication.
        /// </summary>
        /// <param name="ganttTask">The local Gantt task.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <returns>The converted remote task.</returns>
        public RemoteTask ConvertToRemoteTask(GanttTask ganttTask, Guid projectId)
        {
            if (ganttTask == null)
                throw new ArgumentNullException(nameof(ganttTask));

            return new RemoteTask
            {
                Id = ganttTask.Id,
                ProjectId = projectId,
                Title = ganttTask.Title,
                Description = ganttTask.Description,
                StartDate = ganttTask.Start,
                EndDate = ganttTask.End,
                Progress = ganttTask.Progress,
                Status = ganttTask.Status, // Direct assignment since both are TaskStatus
                Priority = TaskPriority.Normal, // Default priority since GanttTask doesn't have priority
                EstimatedHours = CalculateEstimatedHours(ganttTask.Start, ganttTask.End),
                ActualHours = ganttTask.Progress / 100.0 * CalculateEstimatedHours(ganttTask.Start, ganttTask.End),
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Version = 1,
                Dependencies = new List<RemoteTaskDependency>()
            };
        }

        /// <summary>
        /// Converts a RemoteTask to GanttTask for local use.
        /// </summary>
        /// <param name="remoteTask">The remote task.</param>
        /// <returns>The converted Gantt task.</returns>
        public GanttTask ConvertToGanttTask(RemoteTask remoteTask)
        {
            if (remoteTask == null)
                throw new ArgumentNullException(nameof(remoteTask));

            return new GanttTask
            {
                Id = remoteTask.Id,
                Title = remoteTask.Title,
                Description = remoteTask.Description,
                Start = remoteTask.StartDate,
                End = remoteTask.EndDate,
                Progress = remoteTask.Progress,
                Status = remoteTask.Status, // Direct assignment since both are TaskStatus
                RowIndex = 0 // Will need to be set based on UI requirements
            };
        }

        /// <summary>
        /// Calculates estimated hours based on start and end dates.
        /// </summary>
        private static double CalculateEstimatedHours(DateTime start, DateTime end)
        {
            var duration = end - start;
            return Math.Max(1.0, duration.TotalHours); // Minimum 1 hour
        }

        /// <summary>
        /// Disposes the resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                // HttpClient is managed externally, so we don't dispose it here
                _disposed = true;
            }
        }
    }
}