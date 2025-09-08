using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GPM.Gantt.Models;
using GPM.Gantt.Rendering;

namespace GPM.Gantt.Dependency
{
    /// <summary>
    /// Manages task dependencies in the Gantt chart.
    /// </summary>
    public class DependencyManager
    {
        /// <summary>
        /// Refreshes the dependency lines display
        /// </summary>
        public static void RefreshDependencyLines(Grid container, bool showDependencyLines, ObservableCollection<TaskDependency> dependencies, 
            ObservableCollection<GanttTask> tasks, bool highlightCriticalPath)
        {
            if (!showDependencyLines || dependencies == null || !dependencies.Any())
                return;
                
            // Remove existing dependency lines
            var dependencyElements = container.Children.OfType<FrameworkElement>()
                .Where(e => e.Tag?.ToString() == "DependencyLine")
                .ToList();
                
            foreach (var element in dependencyElements)
            {
                container.Children.Remove(element);
            }
            
            // Create task position map
            var taskPositions = CalculateTaskPositions(tasks);
            
            // Create dependency lines
            var dependencyLines = CreateDependencyLines(dependencies, highlightCriticalPath);
            var renderer = new DependencyLineRenderer();
            var lineElements = renderer.RenderDependencyLines(dependencyLines, taskPositions);
            
            // Add dependency line elements to the container
            foreach (var element in lineElements)
            {
                if (element is FrameworkElement fe)
                {
                    fe.Tag = "DependencyLine";
                    fe.IsHitTestVisible = false; // Allow click-through
                }
                container.Children.Add(element);
            }
        }
        
        /// <summary>
        /// Calculates screen positions for all visible tasks
        /// </summary>
        private static Dictionary<string, Rect> CalculateTaskPositions(ObservableCollection<GanttTask> tasks)
        {
            var positions = new Dictionary<string, Rect>();
            
            if (tasks == null) return positions;
            
            // In a real implementation, this would use the actual timeline ticks
            // var ticks = GetCachedTimelineTicks();
            
            foreach (var task in tasks)
            {
                // In a real implementation, this would calculate the actual screen position
                // var rect = CalculateTaskScreenPosition(task, ticks);
                // if (rect.HasValue)
                // {
                //     positions[task.Id.ToString()] = rect.Value;
                // }
            }
            
            return positions;
        }
        
        /// <summary>
        /// Creates dependency line objects for rendering
        /// </summary>
        private static List<DependencyLine> CreateDependencyLines(ObservableCollection<TaskDependency> dependencies, bool highlightCriticalPath)
        {
            var dependencyLines = new List<DependencyLine>();
            
            if (dependencies == null) return dependencyLines;
            
            foreach (var dependency in dependencies.Where(d => d.IsActive))
            {
                var line = new DependencyLine
                {
                    Dependency = dependency,
                    LineColor = dependency.IsCritical && highlightCriticalPath ? 
                        Brushes.Red : Brushes.DarkBlue,
                    LineThickness = dependency.IsCritical && highlightCriticalPath ? 2.0 : 1.0,
                    IsHighlighted = dependency.IsCritical && highlightCriticalPath,
                    ShowArrow = true,
                    ShowLagLabel = dependency.Lag != TimeSpan.Zero
                };
                
                dependencyLines.Add(line);
            }
            
            return dependencyLines;
        }
    }
}