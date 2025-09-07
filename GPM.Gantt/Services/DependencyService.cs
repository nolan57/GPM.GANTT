using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Service for managing task dependencies and performing critical path analysis
    /// </summary>
    public class DependencyService : IDependencyService
    {
        private readonly List<TaskDependency> _dependencies = new();

        public async Task<List<TaskDependency>> GetDependenciesAsync(string projectId)
        {
            await Task.Delay(1); // Simulate async operation
            return _dependencies.Where(d => d.IsActive).ToList();
        }

        public async Task<TaskDependency> CreateDependencyAsync(TaskDependency dependency)
        {
            await Task.Delay(1);
            if (string.IsNullOrEmpty(dependency.Id))
                dependency.Id = Guid.NewGuid().ToString();
            
            _dependencies.Add(dependency);
            return dependency;
        }

        public async Task<bool> UpdateDependencyAsync(TaskDependency dependency)
        {
            await Task.Delay(1);
            var existing = _dependencies.FirstOrDefault(d => d.Id == dependency.Id);
            if (existing != null)
            {
                existing.PredecessorTaskId = dependency.PredecessorTaskId;
                existing.SuccessorTaskId = dependency.SuccessorTaskId;
                existing.Type = dependency.Type;
                existing.Lag = dependency.Lag;
                existing.Description = dependency.Description;
                existing.IsCritical = dependency.IsCritical;
                existing.Priority = dependency.Priority;
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteDependencyAsync(string dependencyId)
        {
            await Task.Delay(1);
            var dependency = _dependencies.FirstOrDefault(d => d.Id == dependencyId);
            if (dependency != null)
            {
                dependency.IsActive = false;
                return true;
            }
            return false;
        }

        public async Task<List<TaskDependency>> GetTaskDependenciesAsync(string taskId)
        {
            await Task.Delay(1);
            return _dependencies.Where(d => d.IsActive && 
                (d.PredecessorTaskId == taskId || d.SuccessorTaskId == taskId)).ToList();
        }

        public async Task<bool> ValidateDependencyAsync(TaskDependency dependency)
        {
            await Task.Delay(1);
            
            // Basic validation
            if (string.IsNullOrEmpty(dependency.PredecessorTaskId) || 
                string.IsNullOrEmpty(dependency.SuccessorTaskId))
                return false;
                
            // Cannot depend on itself
            if (dependency.PredecessorTaskId == dependency.SuccessorTaskId)
                return false;
                
            return true;
        }

        public async Task<List<string>> GetCriticalPathAsync(List<GanttTask> tasks, List<TaskDependency> dependencies)
        {
            await Task.Delay(1);
            
            var taskDict = tasks.ToDictionary(t => t.Id.ToString(), t => t);
            var dependencyGraph = BuildDependencyGraph(dependencies);
            
            // Calculate early start/finish times (forward pass)
            var earlyTimes = CalculateEarlyTimes(tasks, dependencies);
            
            // Calculate late start/finish times (backward pass)
            var lateTimes = CalculateLateTimes(tasks, dependencies, earlyTimes);
            
            // Find critical tasks (those with zero float)
            var criticalTasks = new List<string>();
            foreach (var task in tasks)
            {
                var taskId = task.Id.ToString();
                if (earlyTimes.ContainsKey(taskId) && lateTimes.ContainsKey(taskId))
                {
                    var earlyStart = earlyTimes[taskId].EarlyStart;
                    var lateStart = lateTimes[taskId].LateStart;
                    if (Math.Abs((lateStart - earlyStart).TotalDays) < 0.01) // Zero float
                    {
                        criticalTasks.Add(taskId);
                    }
                }
            }
            
            return criticalTasks;
        }

        public async Task<Dictionary<string, TimeSpan>> CalculateFloatTimesAsync(List<GanttTask> tasks, List<TaskDependency> dependencies)
        {
            await Task.Delay(1);
            
            var floatTimes = new Dictionary<string, TimeSpan>();
            var earlyTimes = CalculateEarlyTimes(tasks, dependencies);
            var lateTimes = CalculateLateTimes(tasks, dependencies, earlyTimes);
            
            foreach (var task in tasks)
            {
                var taskId = task.Id.ToString();
                if (earlyTimes.ContainsKey(taskId) && lateTimes.ContainsKey(taskId))
                {
                    var totalFloat = lateTimes[taskId].LateStart - earlyTimes[taskId].EarlyStart;
                    floatTimes[taskId] = totalFloat;
                }
                else
                {
                    floatTimes[taskId] = TimeSpan.Zero;
                }
            }
            
            return floatTimes;
        }

        public bool HasCircularDependency(List<GanttTask> tasks, List<TaskDependency> dependencies, TaskDependency newDependency)
        {
            var allDependencies = dependencies.ToList();
            allDependencies.Add(newDependency);
            
            var graph = BuildDependencyGraph(allDependencies);
            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();
            
            foreach (var task in tasks)
            {
                var taskId = task.Id.ToString();
                if (HasCycle(taskId, graph, visited, recursionStack))
                {
                    return true;
                }
            }
            
            return false;
        }

        public async Task<List<GanttTask>> CalculateScheduleAsync(List<GanttTask> tasks, List<TaskDependency> dependencies)
        {
            await Task.Delay(1);
            
            var earlyTimes = CalculateEarlyTimes(tasks, dependencies);
            var lateTimes = CalculateLateTimes(tasks, dependencies, earlyTimes);
            
            foreach (var task in tasks)
            {
                var taskId = task.Id.ToString();
                if (earlyTimes.ContainsKey(taskId))
                {
                    task.EarliestStart = earlyTimes[taskId].EarlyStart;
                    task.EarliestFinish = earlyTimes[taskId].EarlyFinish;
                }
                
                if (lateTimes.ContainsKey(taskId))
                {
                    task.LatestStart = lateTimes[taskId].LateStart;
                    task.LatestFinish = lateTimes[taskId].LateFinish;
                }
                
                // Calculate float times
                if (task.EarliestStart.HasValue && task.LatestStart.HasValue)
                {
                    task.TotalFloat = task.LatestStart.Value - task.EarliestStart.Value;
                    task.IsCritical = task.TotalFloat.TotalDays < 0.01;
                }
            }
            
            return tasks;
        }

        public async Task<List<GanttTask>> AutoScheduleTasksAsync(List<GanttTask> tasks, List<TaskDependency> dependencies, DateTime projectStart)
        {
            await Task.Delay(1);
            
            var taskDict = tasks.ToDictionary(t => t.Id.ToString(), t => t);
            var dependencyGraph = BuildDependencyGraph(dependencies);
            var visited = new HashSet<string>();
            var scheduled = new Dictionary<string, DateTime>();
            
            // Schedule tasks starting from those with no predecessors
            foreach (var task in tasks)
            {
                var taskId = task.Id.ToString();
                if (!dependencyGraph.Values.Any(successors => successors.Contains(taskId)))
                {
                    ScheduleTask(taskId, taskDict, dependencyGraph, dependencies, projectStart, visited, scheduled);
                }
            }
            
            // Update task dates based on scheduling
            foreach (var task in tasks)
            {
                var taskId = task.Id.ToString();
                if (scheduled.ContainsKey(taskId))
                {
                    var scheduledStart = scheduled[taskId];
                    var duration = task.Duration;
                    task.Start = scheduledStart;
                    task.End = scheduledStart.Add(duration);
                }
            }
            
            return tasks;
        }

        private Dictionary<string, List<string>> BuildDependencyGraph(List<TaskDependency> dependencies)
        {
            var graph = new Dictionary<string, List<string>>();
            
            foreach (var dep in dependencies.Where(d => d.IsActive))
            {
                if (!graph.ContainsKey(dep.PredecessorTaskId))
                    graph[dep.PredecessorTaskId] = new List<string>();
                
                graph[dep.PredecessorTaskId].Add(dep.SuccessorTaskId);
            }
            
            return graph;
        }

        private Dictionary<string, (DateTime EarlyStart, DateTime EarlyFinish)> CalculateEarlyTimes(
            List<GanttTask> tasks, List<TaskDependency> dependencies)
        {
            var earlyTimes = new Dictionary<string, (DateTime EarlyStart, DateTime EarlyFinish)>();
            var taskDict = tasks.ToDictionary(t => t.Id.ToString(), t => t);
            var dependencyGraph = BuildReverseDependencyGraph(dependencies);
            var processed = new HashSet<string>();
            
            // Process tasks in topological order
            var queue = new Queue<string>();
            
            // Start with tasks that have no predecessors
            foreach (var task in tasks)
            {
                var taskId = task.Id.ToString();
                if (!dependencyGraph.ContainsKey(taskId) || !dependencyGraph[taskId].Any())
                {
                    queue.Enqueue(taskId);
                    earlyTimes[taskId] = (task.Start, task.End);
                }
            }
            
            while (queue.Count > 0)
            {
                var taskId = queue.Dequeue();
                if (processed.Contains(taskId)) continue;
                
                processed.Add(taskId);
                var task = taskDict[taskId];
                
                // Find all successors and update their early times
                var successorDeps = dependencies.Where(d => d.PredecessorTaskId == taskId && d.IsActive);
                foreach (var dep in successorDeps)
                {
                    var successorId = dep.SuccessorTaskId;
                    if (!taskDict.ContainsKey(successorId)) continue;
                    
                    var successor = taskDict[successorId];
                    var earliestStart = CalculateEarliestStart(successorId, taskDict, dependencies, earlyTimes);
                    var earliestFinish = earliestStart.Add(successor.Duration);
                    
                    earlyTimes[successorId] = (earliestStart, earliestFinish);
                    queue.Enqueue(successorId);
                }
            }
            
            return earlyTimes;
        }

        private Dictionary<string, (DateTime LateStart, DateTime LateFinish)> CalculateLateTimes(
            List<GanttTask> tasks, List<TaskDependency> dependencies,
            Dictionary<string, (DateTime EarlyStart, DateTime EarlyFinish)> earlyTimes)
        {
            var lateTimes = new Dictionary<string, (DateTime LateStart, DateTime LateFinish)>();
            var taskDict = tasks.ToDictionary(t => t.Id.ToString(), t => t);
            var dependencyGraph = BuildDependencyGraph(dependencies);
            
            // Find project end date
            var projectEnd = earlyTimes.Values.Max(t => t.EarlyFinish);
            
            // Start with tasks that have no successors
            var processed = new HashSet<string>();
            var queue = new Queue<string>();
            
            foreach (var task in tasks)
            {
                var taskId = task.Id.ToString();
                if (!dependencyGraph.ContainsKey(taskId) || !dependencyGraph[taskId].Any())
                {
                    var lateFinish = earlyTimes.ContainsKey(taskId) ? earlyTimes[taskId].EarlyFinish : projectEnd;
                    var lateStart = lateFinish.Subtract(task.Duration);
                    lateTimes[taskId] = (lateStart, lateFinish);
                    queue.Enqueue(taskId);
                }
            }
            
            while (queue.Count > 0)
            {
                var taskId = queue.Dequeue();
                if (processed.Contains(taskId)) continue;
                
                processed.Add(taskId);
                
                // Find all predecessors and update their late times
                var predecessorDeps = dependencies.Where(d => d.SuccessorTaskId == taskId && d.IsActive);
                foreach (var dep in predecessorDeps)
                {
                    var predecessorId = dep.PredecessorTaskId;
                    if (!taskDict.ContainsKey(predecessorId)) continue;
                    
                    var predecessor = taskDict[predecessorId];
                    var latestFinish = CalculateLatestFinish(predecessorId, taskDict, dependencies, lateTimes);
                    var latestStart = latestFinish.Subtract(predecessor.Duration);
                    
                    lateTimes[predecessorId] = (latestStart, latestFinish);
                    queue.Enqueue(predecessorId);
                }
            }
            
            return lateTimes;
        }

        private Dictionary<string, List<string>> BuildReverseDependencyGraph(List<TaskDependency> dependencies)
        {
            var graph = new Dictionary<string, List<string>>();
            
            foreach (var dep in dependencies.Where(d => d.IsActive))
            {
                if (!graph.ContainsKey(dep.SuccessorTaskId))
                    graph[dep.SuccessorTaskId] = new List<string>();
                
                graph[dep.SuccessorTaskId].Add(dep.PredecessorTaskId);
            }
            
            return graph;
        }

        private DateTime CalculateEarliestStart(string taskId, Dictionary<string, GanttTask> taskDict,
            List<TaskDependency> dependencies, Dictionary<string, (DateTime EarlyStart, DateTime EarlyFinish)> earlyTimes)
        {
            var predecessorDeps = dependencies.Where(d => d.SuccessorTaskId == taskId && d.IsActive);
            var earliestStart = taskDict[taskId].Start;
            
            foreach (var dep in predecessorDeps)
            {
                if (earlyTimes.ContainsKey(dep.PredecessorTaskId))
                {
                    var predFinish = earlyTimes[dep.PredecessorTaskId].EarlyFinish;
                    var requiredStart = predFinish.Add(dep.Lag);
                    if (requiredStart > earliestStart)
                        earliestStart = requiredStart;
                }
            }
            
            return earliestStart;
        }

        private DateTime CalculateLatestFinish(string taskId, Dictionary<string, GanttTask> taskDict,
            List<TaskDependency> dependencies, Dictionary<string, (DateTime LateStart, DateTime LateFinish)> lateTimes)
        {
            var successorDeps = dependencies.Where(d => d.PredecessorTaskId == taskId && d.IsActive);
            var latestFinish = taskDict[taskId].End;
            
            foreach (var dep in successorDeps)
            {
                if (lateTimes.ContainsKey(dep.SuccessorTaskId))
                {
                    var succStart = lateTimes[dep.SuccessorTaskId].LateStart;
                    var requiredFinish = succStart.Subtract(dep.Lag);
                    if (requiredFinish < latestFinish)
                        latestFinish = requiredFinish;
                }
            }
            
            return latestFinish;
        }

        private void ScheduleTask(string taskId, Dictionary<string, GanttTask> taskDict,
            Dictionary<string, List<string>> dependencyGraph, List<TaskDependency> dependencies,
            DateTime projectStart, HashSet<string> visited, Dictionary<string, DateTime> scheduled)
        {
            if (visited.Contains(taskId) || scheduled.ContainsKey(taskId))
                return;
            
            visited.Add(taskId);
            var task = taskDict[taskId];
            
            // Calculate earliest start based on predecessors
            var earliestStart = projectStart;
            var predecessorDeps = dependencies.Where(d => d.SuccessorTaskId == taskId && d.IsActive);
            
            foreach (var dep in predecessorDeps)
            {
                // Schedule predecessor first
                ScheduleTask(dep.PredecessorTaskId, taskDict, dependencyGraph, dependencies, projectStart, visited, scheduled);
                
                if (scheduled.ContainsKey(dep.PredecessorTaskId))
                {
                    var predTask = taskDict[dep.PredecessorTaskId];
                    var predEnd = scheduled[dep.PredecessorTaskId].Add(predTask.Duration);
                    var requiredStart = predEnd.Add(dep.Lag);
                    
                    if (requiredStart > earliestStart)
                        earliestStart = requiredStart;
                }
            }
            
            scheduled[taskId] = earliestStart;
        }

        private bool HasCycle(string taskId, Dictionary<string, List<string>> graph, 
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
                    if (HasCycle(successor, graph, visited, recursionStack))
                        return true;
                }
            }
            
            recursionStack.Remove(taskId);
            return false;
        }
    }
}