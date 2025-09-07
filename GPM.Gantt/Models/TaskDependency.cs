using System;

namespace GPM.Gantt.Models
{
    /// <summary>
    /// Defines the type of dependency relationship between tasks
    /// </summary>
    public enum DependencyType
    {
        /// <summary>
        /// Predecessor task must finish before successor task can start
        /// </summary>
        FinishToStart,
        
        /// <summary>
        /// Both tasks must start at the same time
        /// </summary>
        StartToStart,
        
        /// <summary>
        /// Both tasks must finish at the same time
        /// </summary>
        FinishToFinish,
        
        /// <summary>
        /// Successor task must finish before predecessor task can start
        /// </summary>
        StartToFinish
    }

    /// <summary>
    /// Represents a dependency relationship between two tasks
    /// </summary>
    public class TaskDependency
    {
        /// <summary>
        /// Unique identifier for the dependency
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// ID of the predecessor task
        /// </summary>
        public string PredecessorTaskId { get; set; } = string.Empty;
        
        /// <summary>
        /// ID of the successor task
        /// </summary>
        public string SuccessorTaskId { get; set; } = string.Empty;
        
        /// <summary>
        /// Type of dependency relationship
        /// </summary>
        public DependencyType Type { get; set; } = DependencyType.FinishToStart;
        
        /// <summary>
        /// Time lag between tasks (positive for delay, negative for lead time)
        /// </summary>
        public TimeSpan Lag { get; set; } = TimeSpan.Zero;
        
        /// <summary>
        /// Whether this dependency is active
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Description of the dependency
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether this dependency is part of the critical path
        /// </summary>
        public bool IsCritical { get; set; }
        
        /// <summary>
        /// Priority of this dependency (for constraint resolution)
        /// </summary>
        public int Priority { get; set; } = 0;
    }
}