using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GPM.Gantt.Models
{
    /// <summary>
    /// Represents a project entity for REST API communication.
    /// </summary>
    public class RemoteProject
    {
        /// <summary>
        /// Gets or sets the unique identifier of the project.
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        
        /// <summary>
        /// Gets or sets the project name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the project description.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        
        /// <summary>
        /// Gets or sets the project start date.
        /// </summary>
        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// Gets or sets the project end date.
        /// </summary>
        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }
        
        /// <summary>
        /// Gets or sets the project status.
        /// </summary>
        [JsonPropertyName("status")]
        public ProjectStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets the project manager identifier.
        /// </summary>
        [JsonPropertyName("managerId")]
        public string? ManagerId { get; set; }
        
        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the last modification timestamp.
        /// </summary>
        [JsonPropertyName("lastModified")]
        public DateTime LastModified { get; set; }
        
        /// <summary>
        /// Gets or sets the project version for optimistic concurrency.
        /// </summary>
        [JsonPropertyName("version")]
        public int Version { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of tasks associated with this project.
        /// </summary>
        [JsonPropertyName("tasks")]
        public List<RemoteTask> Tasks { get; set; } = new List<RemoteTask>();
    }
    
    /// <summary>
    /// Represents a task entity for REST API communication.
    /// </summary>
    public class RemoteTask
    {
        /// <summary>
        /// Gets or sets the unique identifier of the task.
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        
        /// <summary>
        /// Gets or sets the project identifier this task belongs to.
        /// </summary>
        [JsonPropertyName("projectId")]
        public Guid ProjectId { get; set; }
        
        /// <summary>
        /// Gets or sets the task title.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the task description.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        
        /// <summary>
        /// Gets or sets the task start date.
        /// </summary>
        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// Gets or sets the task end date.
        /// </summary>
        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }
        
        /// <summary>
        /// Gets or sets the task progress percentage (0-100).
        /// </summary>
        [JsonPropertyName("progress")]
        public double Progress { get; set; }
        
        /// <summary>
        /// Gets or sets the task status.
        /// </summary>
        [JsonPropertyName("status")]
        public TaskStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets the assigned user identifier.
        /// </summary>
        [JsonPropertyName("assignedTo")]
        public string? AssignedTo { get; set; }
        
        /// <summary>
        /// Gets or sets the task priority.
        /// </summary>
        [JsonPropertyName("priority")]
        public TaskPriority Priority { get; set; }
        
        /// <summary>
        /// Gets or sets the estimated duration in hours.
        /// </summary>
        [JsonPropertyName("estimatedHours")]
        public double EstimatedHours { get; set; }
        
        /// <summary>
        /// Gets or sets the actual duration in hours.
        /// </summary>
        [JsonPropertyName("actualHours")]
        public double ActualHours { get; set; }
        
        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the last modification timestamp.
        /// </summary>
        [JsonPropertyName("lastModified")]
        public DateTime LastModified { get; set; }
        
        /// <summary>
        /// Gets or sets the task version for optimistic concurrency.
        /// </summary>
        [JsonPropertyName("version")]
        public int Version { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of task dependencies.
        /// </summary>
        [JsonPropertyName("dependencies")]
        public List<RemoteTaskDependency> Dependencies { get; set; } = new List<RemoteTaskDependency>();
    }
    
    /// <summary>
    /// Represents task dependencies for REST API communication.
    /// </summary>
    public class RemoteTaskDependency
    {
        /// <summary>
        /// Gets or sets the predecessor task identifier.
        /// </summary>
        [JsonPropertyName("predecessorId")]
        public Guid PredecessorId { get; set; }
        
        /// <summary>
        /// Gets or sets the successor task identifier.
        /// </summary>
        [JsonPropertyName("successorId")]
        public Guid SuccessorId { get; set; }
        
        /// <summary>
        /// Gets or sets the dependency type.
        /// </summary>
        [JsonPropertyName("type")]
        public DependencyType Type { get; set; }
        
        /// <summary>
        /// Gets or sets the lag time in hours.
        /// </summary>
        [JsonPropertyName("lag")]
        public double Lag { get; set; }
    }
    
    /// <summary>
    /// Represents the status of a project.
    /// </summary>
    public enum ProjectStatus
    {
        Planning,
        Active,
        OnHold,
        Completed,
        Cancelled
    }
}