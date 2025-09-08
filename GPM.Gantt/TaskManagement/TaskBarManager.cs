using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GPM.Gantt.Configuration;
using GPM.Gantt.Interaction;
using GPM.Gantt.Models;
using GPM.Gantt.Utilities;
using GPM.Gantt.Services;

namespace GPM.Gantt.TaskManagement
{
    /// <summary>
    /// Manages task bar operations for the Gantt chart.
    /// </summary>
    public class TaskBarManager
    {
        /// <summary>
        /// Builds task bars in standard layout.
        /// </summary>
        public static void BuildTaskBars(Grid container, List<DateTime> ticks, ObservableCollection<GanttTask> tasks, 
            int taskCount, bool clampTasksToVisibleRows, PerformanceLevel performanceLevel, 
            bool isInteractionEnabled, bool isDragDropEnabled, bool isResizeEnabled, 
            ElementPool elementPool, GanttInteractionManager interactionManager, bool isLoaded,
            RenderingConfiguration renderingConfig)
        {
            // Ensure minimum viable layout even with no tasks
            if (tasks == null || tasks.Count == 0)
            {
                // Add a simple visual indicator
                var placeholder = new TextBlock
                {
                    Text = "Gantt Chart - No tasks to display",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    Foreground = System.Windows.Media.Brushes.Gray
                };
                Grid.SetRow(placeholder, Math.Max(1, container.RowDefinitions.Count / 2));
                Grid.SetColumn(placeholder, 0);
                Grid.SetColumnSpan(placeholder, container.ColumnDefinitions.Count);
                container.Children.Add(placeholder);
                return;
            }

            foreach (var task in tasks)
            {
                if (task == null) continue;

                // Skip tasks completely outside the visible time range
                // Note: StartTime and EndTime would need to be passed in a real implementation
                // if (task.End < StartTime || task.Start > EndTime)
                // {
                //     continue;
                // }
                
                // Decide row index with optional clamping behavior
                int rowIndex;
                if (clampTasksToVisibleRows)
                {
                    // Clamp into visible range (1..TaskCount) when enabled
                    rowIndex = Math.Max(1, Math.Min(taskCount, task.RowIndex));
                }
                else
                {
                    // Skip out-of-range rows to avoid overlapping
                    if (task.RowIndex < 1 || task.RowIndex > taskCount)
                    {
                        continue;
                    }
                    rowIndex = task.RowIndex;
                }

                // Calculate task span using timeline helper
                var (startIndex, columnSpan) = TimelineHelper.CalculateTaskSpan(ticks, task.Start, task.End, TimeUnit.Day);

                var taskBar = CreateTaskBarForTask(task, rowIndex, startIndex, columnSpan, renderingConfig);

                Grid.SetRow(taskBar, rowIndex);
                Grid.SetColumn(taskBar, startIndex);
                Grid.SetColumnSpan(taskBar, columnSpan);
                Panel.SetZIndex(taskBar, 10); // Place above grid lines
                container.Children.Add(taskBar);
                
                // Make sure the task bar is properly configured with visual properties
                if (container is GanttContainer ganttContainer && ganttContainer.Theme != null)
                {
                    // Apply theme to the specific task bar
                    Theme.ThemeApplier.ApplyThemeToTaskBar(taskBar, ganttContainer.Theme);
                }
                
                // Register with interaction manager after adding to visual tree
                if (isLoaded)
                {
                    interactionManager.RegisterTaskBar(taskBar);
                }
            }
        }
        
        /// <summary>
        /// Builds task bars with viewport culling for performance.
        /// </summary>
        public static void BuildVirtualizedTaskBars(Grid container, List<DateTime> ticks, ObservableCollection<GanttTask> tasks,
            (int startRow, int endRow) visibleRange, PerformanceLevel performanceLevel, 
            bool isInteractionEnabled, bool isDragDropEnabled, bool isResizeEnabled, 
            ElementPool elementPool, GanttInteractionManager interactionManager, bool isLoaded,
            RenderingConfiguration renderingConfig, IPerformanceService performanceService)
        {
            using var measurement = performanceService.BeginMeasurement("VirtualizedTaskBarsBuild");
            
            if (tasks == null || tasks.Count == 0)
                return;
            
            var (startRow, endRow) = visibleRange;
            
            // Filter tasks within visible range
            var visibleTasks = tasks.Where(task => 
                task != null &&
                task.RowIndex >= startRow && 
                task.RowIndex <= endRow
                // && task.End >= StartTime && task.Start <= EndTime  // Would need start/end time parameters
            ).ToList();
            
            foreach (var task in visibleTasks)
            {
                var (startIndex, columnSpan) = TimelineHelper.CalculateTaskSpan(ticks, task.Start, task.End, TimeUnit.Day);
                
                var taskBar = elementPool.GetOrCreateTaskBar();
                
                // Configure task bar using our factory method logic
                ConfigureTaskBar(taskBar, task, task.RowIndex, startIndex, columnSpan, 
                    isInteractionEnabled, isDragDropEnabled, isResizeEnabled, renderingConfig);
                
                Grid.SetRow(taskBar, task.RowIndex);
                Grid.SetColumn(taskBar, startIndex);
                Grid.SetColumnSpan(taskBar, columnSpan);
                Panel.SetZIndex(taskBar, 10);
                container.Children.Add(taskBar);
                // _visibleElements.Add(taskBar); // Would need visibleElements parameter
                
                if (isLoaded)
                {
                    interactionManager.RegisterTaskBar(taskBar);
                }
            }
        }
        
        /// <summary>
        /// Creates a task bar for the given task with appropriate shape and configuration.
        /// </summary>
        private static GanttTaskBar CreateTaskBarForTask(GanttTask task, int rowIndex, int startIndex, int columnSpan, 
            RenderingConfiguration renderingConfig)
        {
            GanttTaskBar taskBar;
            
            // Use enhanced task bar if shape rendering is enabled
            if (renderingConfig?.UseEnhancedShapeRendering == true && task.Shape != TaskBarShape.Rectangle)
            {
                var enhancedTaskBar = new EnhancedGanttTaskBar
                {
                    Shape = task.Shape,
                    ShapeParameters = task.ShapeParameters ?? renderingConfig.DefaultShapeParameters,
                    UseLegacyRendering = false
                };
                taskBar = enhancedTaskBar;
            }
            else
            {
                taskBar = new GanttTaskBar();
            }
            
            // Configure common properties
            taskBar.RowIndex = rowIndex;
            taskBar.TimeIndex = startIndex;
            taskBar.CustomText = task.Title;
            // Other properties would be set in ConfigureTaskBar method
            
            return taskBar;
        }
        
        /// <summary>
        /// Configures a task bar (from pool or newly created) with task data and shape settings.
        /// </summary>
        private static void ConfigureTaskBar(GanttTaskBar taskBar, GanttTask task, int rowIndex, int startIndex, int columnSpan,
            bool isInteractionEnabled, bool isDragDropEnabled, bool isResizeEnabled, RenderingConfiguration renderingConfig)
        {
            // Handle enhanced task bar with shape support
            if (taskBar is EnhancedGanttTaskBar enhancedTaskBar && 
                renderingConfig?.UseEnhancedShapeRendering == true && 
                task.Shape != TaskBarShape.Rectangle)
            {
                enhancedTaskBar.Shape = task.Shape;
                enhancedTaskBar.ShapeParameters = task.ShapeParameters ?? renderingConfig.DefaultShapeParameters;
                enhancedTaskBar.UseLegacyRendering = false;
            }
            
            // Configure common properties
            taskBar.RowIndex = rowIndex;
            taskBar.TimeIndex = startIndex;
            taskBar.CustomText = task.Title;
            taskBar.IsInteractive = isInteractionEnabled;
            taskBar.Progress = task.Progress;
            taskBar.Priority = task.Priority;
            taskBar.Status = task.Status;
            taskBar.IsDragDropEnabled = isDragDropEnabled;
            taskBar.IsResizeEnabled = isResizeEnabled;
            
            // Ensure the task bar is visible by setting proper Z-index
            Panel.SetZIndex(taskBar, 10);
        }
    }
}