using System;
using System.Collections.Generic;
using GPM.Gantt.Models.Calendar;

namespace GPM.Gantt.Models.Templates
{
    /// <summary>
    /// Category of project template
    /// </summary>
    public enum TemplateCategory
    {
        SoftwareDevelopment,
        Construction,
        Marketing,
        Research,
        Manufacturing,
        Event,
        General,
        Custom
    }

    /// <summary>
    /// Priority level for template tasks
    /// </summary>
    public enum TemplatePriority
    {
        Low,
        Normal, 
        High,
        Critical
    }

    /// <summary>
    /// Represents a task template within a project template
    /// </summary>
    public class TaskTemplate
    {
        /// <summary>
        /// Unique identifier for the task template
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Name of the task
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the task
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Estimated duration for the task
        /// </summary>
        public TimeSpan EstimatedDuration { get; set; }
        
        /// <summary>
        /// Task priority
        /// </summary>
        public TemplatePriority Priority { get; set; } = TemplatePriority.Normal;
        
        /// <summary>
        /// Shape for the task bar
        /// </summary>
        public TaskBarShape Shape { get; set; } = TaskBarShape.Rectangle;
        
        /// <summary>
        /// Order/sequence of this task in the template
        /// </summary>
        public int Order { get; set; }
        
        /// <summary>
        /// ID of parent task (for hierarchical tasks)
        /// </summary>
        public string? ParentTaskId { get; set; }
        
        /// <summary>
        /// List of predecessor task IDs
        /// </summary>
        public List<string> PredecessorTaskIds { get; set; } = new();
        
        /// <summary>
        /// Dependency type for predecessors
        /// </summary>
        public DependencyType DependencyType { get; set; } = DependencyType.FinishToStart;
        
        /// <summary>
        /// Lag time for dependencies
        /// </summary>
        public TimeSpan DependencyLag { get; set; } = TimeSpan.Zero;
        
        /// <summary>
        /// Required skills or resources for this task
        /// </summary>
        public List<string> RequiredSkills { get; set; } = new();
        
        /// <summary>
        /// Estimated effort in person-hours
        /// </summary>
        public double EstimatedEffort { get; set; }
        
        /// <summary>
        /// Whether this task is a milestone
        /// </summary>
        public bool IsMilestone { get; set; }
        
        /// <summary>
        /// Template-specific notes or instructions
        /// </summary>
        public string Notes { get; set; } = string.Empty;
        
        /// <summary>
        /// Custom attributes for the task
        /// </summary>
        public Dictionary<string, object> CustomAttributes { get; set; } = new();
    }

    /// <summary>
    /// Represents a dependency template between task templates
    /// </summary>
    public class DependencyTemplate
    {
        /// <summary>
        /// Unique identifier for the dependency template
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// ID of predecessor task template
        /// </summary>
        public string PredecessorTaskId { get; set; } = string.Empty;
        
        /// <summary>
        /// ID of successor task template
        /// </summary>
        public string SuccessorTaskId { get; set; } = string.Empty;
        
        /// <summary>
        /// Type of dependency
        /// </summary>
        public DependencyType Type { get; set; } = DependencyType.FinishToStart;
        
        /// <summary>
        /// Lag time for the dependency
        /// </summary>
        public TimeSpan Lag { get; set; } = TimeSpan.Zero;
        
        /// <summary>
        /// Description of the dependency relationship
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a complete project template
    /// </summary>
    public class ProjectTemplate
    {
        /// <summary>
        /// Unique identifier for the template
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Name of the template
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the template
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Category of the template
        /// </summary>
        public TemplateCategory Category { get; set; } = TemplateCategory.General;
        
        /// <summary>
        /// Version of the template
        /// </summary>
        public string Version { get; set; } = "1.0";
        
        /// <summary>
        /// Author/creator of the template
        /// </summary>
        public string Author { get; set; } = string.Empty;
        
        /// <summary>
        /// Date when template was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Date when template was last modified
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// List of task templates
        /// </summary>
        public List<TaskTemplate> TaskTemplates { get; set; } = new();
        
        /// <summary>
        /// List of dependency templates
        /// </summary>
        public List<DependencyTemplate> DependencyTemplates { get; set; } = new();
        
        /// <summary>
        /// Estimated total duration for the project
        /// </summary>
        public TimeSpan EstimatedDuration { get; set; }
        
        /// <summary>
        /// Calendar template to use with this project
        /// </summary>
        public string? CalendarTemplateId { get; set; }
        
        /// <summary>
        /// Tags for categorizing and searching templates
        /// </summary>
        public List<string> Tags { get; set; } = new();
        
        /// <summary>
        /// Whether this template is publicly available
        /// </summary>
        public bool IsPublic { get; set; } = true;
        
        /// <summary>
        /// Whether this template is a built-in system template
        /// </summary>
        public bool IsBuiltIn { get; set; } = false;
        
        /// <summary>
        /// Usage count for this template
        /// </summary>
        public int UsageCount { get; set; } = 0;
        
        /// <summary>
        /// Rating for this template (1-5)
        /// </summary>
        public double Rating { get; set; } = 0.0;
        
        /// <summary>
        /// Number of ratings
        /// </summary>
        public int RatingCount { get; set; } = 0;
        
        /// <summary>
        /// Prerequisites or requirements for using this template
        /// </summary>
        public List<string> Prerequisites { get; set; } = new();
        
        /// <summary>
        /// Deliverables expected from this project template
        /// </summary>
        public List<string> Deliverables { get; set; } = new();
        
        /// <summary>
        /// Custom properties for the template
        /// </summary>
        public Dictionary<string, object> CustomProperties { get; set; } = new();
        
        /// <summary>
        /// Instructions for using this template
        /// </summary>
        public string Instructions { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets the total estimated effort in person-hours
        /// </summary>
        public double TotalEstimatedEffort => TaskTemplates.Sum(t => t.EstimatedEffort);
        
        /// <summary>
        /// Gets the number of milestones in the template
        /// </summary>
        public int MilestoneCount => TaskTemplates.Count(t => t.IsMilestone);
        
        /// <summary>
        /// Validates the template for consistency
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("Template name is required");
            
            if (!TaskTemplates.Any())
                errors.Add("Template must have at least one task");
            
            // Check for circular dependencies
            if (HasCircularDependencies())
                errors.Add("Template contains circular dependencies");
            
            // Check for invalid dependencies
            foreach (var dep in DependencyTemplates)
            {
                if (!TaskTemplates.Any(t => t.Id == dep.PredecessorTaskId))
                    errors.Add($"Dependency references non-existent predecessor task: {dep.PredecessorTaskId}");
                
                if (!TaskTemplates.Any(t => t.Id == dep.SuccessorTaskId))
                    errors.Add($"Dependency references non-existent successor task: {dep.SuccessorTaskId}");
            }
            
            return errors;
        }
        
        private bool HasCircularDependencies()
        {
            var graph = new Dictionary<string, List<string>>();
            
            // Build dependency graph
            foreach (var dep in DependencyTemplates)
            {
                if (!graph.ContainsKey(dep.PredecessorTaskId))
                    graph[dep.PredecessorTaskId] = new List<string>();
                
                graph[dep.PredecessorTaskId].Add(dep.SuccessorTaskId);
            }
            
            // Check for cycles using DFS
            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();
            
            foreach (var taskId in TaskTemplates.Select(t => t.Id))
            {
                if (HasCycleDFS(taskId, graph, visited, recursionStack))
                    return true;
            }
            
            return false;
        }
        
        private bool HasCycleDFS(string taskId, Dictionary<string, List<string>> graph, 
            HashSet<string> visited, HashSet<string> recursionStack)
        {
            if (recursionStack.Contains(taskId))
                return true;
            
            if (visited.Contains(taskId))
                return false;
            
            visited.Add(taskId);
            recursionStack.Add(taskId);
            
            if (graph.ContainsKey(taskId))
            {
                foreach (var successor in graph[taskId])
                {
                    if (HasCycleDFS(successor, graph, visited, recursionStack))
                        return true;
                }
            }
            
            recursionStack.Remove(taskId);
            return false;
        }
    }

    /// <summary>
    /// Options for applying a project template
    /// </summary>
    public class TemplateApplicationOptions
    {
        /// <summary>
        /// Project start date
        /// </summary>
        public DateTime ProjectStartDate { get; set; } = DateTime.Today;
        
        /// <summary>
        /// Calendar to use for scheduling
        /// </summary>
        public string? CalendarId { get; set; }
        
        /// <summary>
        /// Whether to include dependencies from template
        /// </summary>
        public bool IncludeDependencies { get; set; } = true;
        
        /// <summary>
        /// Whether to auto-schedule tasks based on dependencies
        /// </summary>
        public bool AutoSchedule { get; set; } = true;
        
        /// <summary>
        /// Resource assignments mapping (template skill -> actual resource)
        /// </summary>
        public Dictionary<string, string> ResourceMappings { get; set; } = new();
        
        /// <summary>
        /// Scale factor for durations (1.0 = no scaling)
        /// </summary>
        public double DurationScale { get; set; } = 1.0;
        
        /// <summary>
        /// Custom property values to apply
        /// </summary>
        public Dictionary<string, object> CustomPropertyValues { get; set; } = new();
        
        /// <summary>
        /// Prefix to add to task names
        /// </summary>
        public string TaskNamePrefix { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether to create parent containers for task groups
        /// </summary>
        public bool CreateTaskGroups { get; set; } = true;
    }
}