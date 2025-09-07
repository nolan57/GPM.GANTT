using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GPM.Gantt.Rendering;

namespace GPM.Gantt.Models
{
    /// <summary>
    /// Represents a task in the Gantt chart with comprehensive domain modeling and validation.
    /// </summary>
    public class GanttTask
    {
        /// <summary>
        /// Gets or sets the unique identifier for the task.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// Gets or sets the start date and time of the task.
        /// </summary>
        [Required]
        public DateTime Start { get; set; }
        
        /// <summary>
        /// Gets or sets the end date and time of the task.
        /// </summary>
        [Required]
        public DateTime End { get; set; }
        
        /// <summary>
        /// Gets or sets the row index for display positioning (1-based index).
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "RowIndex must be greater than 0")]
        public int RowIndex { get; set; } = 1; // Note: 1-based index, aligned to GanttContainer task rows
        
        /// <summary>
        /// Gets or sets the title of the task.
        /// </summary>
        [Required(ErrorMessage = "Task title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the detailed description of the task.
        /// </summary>
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
        
        /// <summary>
        /// Gets or sets the completion progress as a percentage (0-100).
        /// </summary>
        [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
        public double Progress { get; set; } = 0;
        
        /// <summary>
        /// Gets or sets the priority level of the task.
        /// </summary>
        public TaskPriority Priority { get; set; } = TaskPriority.Normal;
        
        /// <summary>
        /// Gets or sets the ID of the parent task for hierarchical organization.
        /// </summary>
        public Guid? ParentTaskId { get; set; }
        
        /// <summary>
        /// Gets or sets the list of task IDs that this task depends on.
        /// </summary>
        public List<Guid> Dependencies { get; set; } = new();
        
        /// <summary>
        /// Gets or sets whether this task is on the critical path.
        /// </summary>
        public bool IsCritical { get; set; }
        
        /// <summary>
        /// Gets or sets the free float (slack) time for this task.
        /// </summary>
        public TimeSpan FreeFloat { get; set; } = TimeSpan.Zero;
        
        /// <summary>
        /// Gets or sets the total float (slack) time for this task.
        /// </summary>
        public TimeSpan TotalFloat { get; set; } = TimeSpan.Zero;
        
        /// <summary>
        /// Gets or sets the earliest start time based on dependencies.
        /// </summary>
        public DateTime? EarliestStart { get; set; }
        
        /// <summary>
        /// Gets or sets the latest start time based on dependencies.
        /// </summary>
        public DateTime? LatestStart { get; set; }
        
        /// <summary>
        /// Gets or sets the earliest finish time based on dependencies.
        /// </summary>
        public DateTime? EarliestFinish { get; set; }
        
        /// <summary>
        /// Gets or sets the latest finish time based on dependencies.
        /// </summary>
        public DateTime? LatestFinish { get; set; }
        
        /// <summary>
        /// Gets or sets the list of resources assigned to this task.
        /// </summary>
        public List<string> AssignedResources { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the status of the task.
        /// </summary>
        public TaskStatus Status { get; set; } = TaskStatus.NotStarted;
        
        /// <summary>
        /// Gets or sets the shape of the task bar when rendered.
        /// </summary>
        public TaskBarShape Shape { get; set; } = TaskBarShape.Rectangle;
        
        /// <summary>
        /// Gets or sets the custom shape rendering parameters.
        /// </summary>
        public ShapeRenderingParameters? ShapeParameters { get; set; }
        
        /// <summary>
        /// Gets the calculated duration of the task.
        /// </summary>
        public TimeSpan Duration => End - Start;
        
        /// <summary>
        /// Validates the task data and returns validation results.
        /// </summary>
        /// <returns>True if the task is valid, false otherwise.</returns>
        public bool IsValid()
        {
            return Start <= End && 
                   !string.IsNullOrWhiteSpace(Title) && 
                   RowIndex > 0 && 
                   Progress >= 0 && Progress <= 100;
        }
        
        /// <summary>
        /// Gets detailed validation errors for the task.
        /// </summary>
        /// <returns>List of validation error messages.</returns>
        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();
            
            if (Start > End)
                errors.Add("Start date must be before or equal to end date");
                
            if (string.IsNullOrWhiteSpace(Title))
                errors.Add("Task title is required");
                
            if (RowIndex <= 0)
                errors.Add("Row index must be greater than 0");
                
            if (Progress < 0 || Progress > 100)
                errors.Add("Progress must be between 0 and 100");
                
            if (Title?.Length > 200)
                errors.Add("Title cannot exceed 200 characters");
                
            if (Description?.Length > 1000)
                errors.Add("Description cannot exceed 1000 characters");
                
            return errors;
        }
        
        /// <summary>
        /// Checks if this task overlaps with another task in time.
        /// </summary>
        /// <param name="other">The other task to check overlap with.</param>
        /// <returns>True if tasks overlap, false otherwise.</returns>
        public bool OverlapsWith(GanttTask other)
        {
            if (other == null) return false;
            return Start < other.End && End > other.Start;
        }
        
        /// <summary>
        /// Creates a copy of the current task.
        /// </summary>
        /// <returns>A new GanttTask instance with copied values.</returns>
        public GanttTask Clone()
        {
            return new GanttTask
            {
                Id = Guid.NewGuid(), // Generate new ID for the clone
                Start = Start,
                End = End,
                RowIndex = RowIndex,
                Title = Title,
                Description = Description,
                Progress = Progress,
                Priority = Priority,
                ParentTaskId = ParentTaskId,
                Dependencies = new List<Guid>(Dependencies),
                AssignedResources = new List<string>(AssignedResources),
                Status = Status,
                Shape = Shape,
                ShapeParameters = ShapeParameters
            };
        }
    }
    
    /// <summary>
    /// Defines the priority levels for tasks.
    /// </summary>
    public enum TaskPriority
    {
        /// <summary>
        /// Low priority task.
        /// </summary>
        Low = 0,
        
        /// <summary>
        /// Normal priority task.
        /// </summary>
        Normal = 1,
        
        /// <summary>
        /// High priority task.
        /// </summary>
        High = 2,
        
        /// <summary>
        /// Critical priority task.
        /// </summary>
        Critical = 3
    }
    
    /// <summary>
    /// Defines the status of a task.
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// Task has not been started.
        /// </summary>
        NotStarted = 0,
        
        /// <summary>
        /// Task is currently in progress.
        /// </summary>
        InProgress = 1,
        
        /// <summary>
        /// Task has been completed.
        /// </summary>
        Completed = 2,
        
        /// <summary>
        /// Task has been cancelled.
        /// </summary>
        Cancelled = 3,
        
        /// <summary>
        /// Task is on hold.
        /// </summary>
        OnHold = 4
    }
}