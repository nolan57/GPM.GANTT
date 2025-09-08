using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GPM.Gantt.Interaction;
using GPM.Gantt.Models;
using GPM.Gantt.Services;

namespace GPM.Gantt.Interaction
{
    /// <summary>
    /// Manages user interactions with the Gantt chart.
    /// </summary>
    public class InteractionManager
    {
        private readonly GanttInteractionManager _interactionManager;
        private readonly IValidationService _validationService;
        
        public InteractionManager(GanttInteractionManager interactionManager, IValidationService validationService)
        {
            _interactionManager = interactionManager;
            _validationService = validationService;
        }
        
        /// <summary>
        /// Updates interaction settings
        /// </summary>
        public void UpdateInteractionSettings(bool isInteractionEnabled, bool isDragDropEnabled, bool isResizeEnabled, bool isMultiSelectionEnabled)
        {
            _interactionManager.IsInteractionEnabled = isInteractionEnabled;
            _interactionManager.IsDragDropEnabled = isDragDropEnabled;
            _interactionManager.IsResizeEnabled = isResizeEnabled;
            _interactionManager.IsMultiSelectionEnabled = isMultiSelectionEnabled;
        }
        
        /// <summary>
        /// Handles task moved event
        /// </summary>
        public void OnTaskMoved(TaskMovedEventArgs e, ObservableCollection<GanttTask> tasks)
        {
            // Update the underlying task model
            if (tasks != null)
            {
                var taskToUpdate = tasks.FirstOrDefault(t => t.RowIndex == e.OriginalRowIndex);
                if (taskToUpdate != null)
                {
                    taskToUpdate.RowIndex = e.NewRowIndex;
                    taskToUpdate.Start = e.NewStartTime;
                    taskToUpdate.End = e.NewEndTime;
                }
            }
        }
        
        /// <summary>
        /// Handles task resized event
        /// </summary>
        public void OnTaskResized(TaskResizedEventArgs e, ObservableCollection<GanttTask> tasks)
        {
            // Update the underlying task model
            if (tasks != null)
            {
                var taskToUpdate = tasks.FirstOrDefault(t => t.Start == e.OriginalStartTime && t.End == e.OriginalEndTime);
                if (taskToUpdate != null)
                {
                    taskToUpdate.Start = e.NewStartTime;
                    taskToUpdate.End = e.NewEndTime;
                }
            }
        }
        
        /// <summary>
        /// Handles drag validation
        /// </summary>
        public void OnValidatingDrag(DragValidationEventArgs e, int taskCount)
        {
            // Implement custom validation logic here
            // For now, allow all moves within bounds
            e.IsValidDrop = e.NewRowIndex >= 1 && e.NewRowIndex <= taskCount && e.NewTimeIndex >= 0;
            
            if (!e.IsValidDrop)
            {
                e.ValidationMessage = "Cannot drop task outside valid bounds";
            }
        }
    }
}