using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GPM.Gantt.Interaction
{
    /// <summary>
    /// Manages task selection state and multi-selection operations in the Gantt chart.
    /// </summary>
    public class TaskSelectionManager
    {
        #region Events

        /// <summary>
        /// Occurs when the selection changes.
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the currently selected task bars.
        /// </summary>
        public ObservableCollection<GanttTaskBar> SelectedTasks { get; } = new();

        /// <summary>
        /// Gets or sets whether multi-selection is enabled.
        /// </summary>
        public bool AllowMultiSelection { get; set; } = true;

        /// <summary>
        /// Gets the primary selected task (first selected or last clicked).
        /// </summary>
        public GanttTaskBar? PrimarySelection => SelectedTasks.FirstOrDefault();

        #endregion

        #region Public Methods

        /// <summary>
        /// Selects a task bar.
        /// </summary>
        /// <param name="taskBar">The task bar to select.</param>
        /// <param name="addToSelection">Whether to add to current selection or replace it.</param>
        public void SelectTask(GanttTaskBar taskBar, bool addToSelection = false)
        {
            if (taskBar == null) return;

            var previousSelection = SelectedTasks.ToList();

            if (!addToSelection || !AllowMultiSelection)
            {
                ClearSelection(false);
            }

            if (!SelectedTasks.Contains(taskBar))
            {
                SelectedTasks.Add(taskBar);
                taskBar.IsSelected = true;
                
                // Move to front of collection to make it primary
                if (SelectedTasks.Count > 1)
                {
                    SelectedTasks.Move(SelectedTasks.Count - 1, 0);
                }
            }

            OnSelectionChanged(previousSelection, SelectedTasks.ToList());
        }

        /// <summary>
        /// Deselects a task bar.
        /// </summary>
        /// <param name="taskBar">The task bar to deselect.</param>
        public void DeselectTask(GanttTaskBar taskBar)
        {
            if (taskBar == null || !SelectedTasks.Contains(taskBar)) return;

            var previousSelection = SelectedTasks.ToList();

            SelectedTasks.Remove(taskBar);
            taskBar.IsSelected = false;

            OnSelectionChanged(previousSelection, SelectedTasks.ToList());
        }

        /// <summary>
        /// Toggles the selection state of a task bar.
        /// </summary>
        /// <param name="taskBar">The task bar to toggle.</param>
        /// <param name="addToSelection">Whether to add to current selection or replace it.</param>
        public void ToggleTaskSelection(GanttTaskBar taskBar, bool addToSelection = false)
        {
            if (taskBar == null) return;

            if (SelectedTasks.Contains(taskBar))
            {
                if (addToSelection && SelectedTasks.Count > 1)
                {
                    DeselectTask(taskBar);
                }
                // Don't deselect if it's the only selected item and not adding to selection
            }
            else
            {
                SelectTask(taskBar, addToSelection);
            }
        }

        /// <summary>
        /// Clears all selections.
        /// </summary>
        /// <param name="raiseEvent">Whether to raise the SelectionChanged event.</param>
        public void ClearSelection(bool raiseEvent = true)
        {
            if (SelectedTasks.Count == 0) return;

            var previousSelection = SelectedTasks.ToList();

            foreach (var task in SelectedTasks)
            {
                task.IsSelected = false;
            }

            SelectedTasks.Clear();

            if (raiseEvent)
            {
                OnSelectionChanged(previousSelection, new List<GanttTaskBar>());
            }
        }

        /// <summary>
        /// Selects all task bars from the provided collection.
        /// </summary>
        /// <param name="taskBars">The task bars to select from.</param>
        public void SelectAll(IEnumerable<GanttTaskBar> taskBars)
        {
            if (!AllowMultiSelection) return;

            var previousSelection = SelectedTasks.ToList();
            var tasksToSelect = taskBars.Where(t => t != null && !SelectedTasks.Contains(t)).ToList();

            foreach (var taskBar in tasksToSelect)
            {
                SelectedTasks.Add(taskBar);
                taskBar.IsSelected = true;
            }

            if (tasksToSelect.Any())
            {
                OnSelectionChanged(previousSelection, SelectedTasks.ToList());
            }
        }

        /// <summary>
        /// Selects task bars within a rectangular region.
        /// </summary>
        /// <param name="taskBars">All available task bars.</param>
        /// <param name="selectionRect">The selection rectangle.</param>
        /// <param name="addToSelection">Whether to add to current selection.</param>
        public void SelectTasksInRect(IEnumerable<GanttTaskBar> taskBars, Rect selectionRect, bool addToSelection = false)
        {
            var previousSelection = SelectedTasks.ToList();

            if (!addToSelection)
            {
                ClearSelection(false);
            }

            var tasksInRect = taskBars.Where(taskBar =>
            {
                if (taskBar?.Parent is FrameworkElement parent)
                {
                    var taskRect = new Rect(
                        taskBar.TranslatePoint(new Point(0, 0), parent),
                        new Size(taskBar.ActualWidth, taskBar.ActualHeight));

                    return selectionRect.IntersectsWith(taskRect);
                }
                return false;
            }).ToList();

            foreach (var taskBar in tasksInRect)
            {
                if (!SelectedTasks.Contains(taskBar))
                {
                    SelectedTasks.Add(taskBar);
                    taskBar.IsSelected = true;
                }
            }

            if (tasksInRect.Any())
            {
                OnSelectionChanged(previousSelection, SelectedTasks.ToList());
            }
        }

        /// <summary>
        /// Handles keyboard input for selection operations.
        /// </summary>
        /// <param name="e">The key event arguments.</param>
        /// <param name="allTaskBars">All available task bars for navigation.</param>
        public void HandleKeyboardInput(KeyEventArgs e, IEnumerable<GanttTaskBar> allTaskBars)
        {
            var taskList = allTaskBars.ToList();
            if (!taskList.Any()) return;

            var currentTask = PrimarySelection;
            var currentIndex = currentTask != null ? taskList.IndexOf(currentTask) : -1;

            switch (e.Key)
            {
                case Key.Up:
                    NavigateVertical(taskList, currentIndex, -1, e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift));
                    e.Handled = true;
                    break;

                case Key.Down:
                    NavigateVertical(taskList, currentIndex, 1, e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift));
                    e.Handled = true;
                    break;

                case Key.Left:
                    NavigateHorizontal(taskList, currentIndex, -1, e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift));
                    e.Handled = true;
                    break;

                case Key.Right:
                    NavigateHorizontal(taskList, currentIndex, 1, e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift));
                    e.Handled = true;
                    break;

                case Key.A when e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control):
                    SelectAll(taskList);
                    e.Handled = true;
                    break;

                case Key.Escape:
                    ClearSelection();
                    e.Handled = true;
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void NavigateVertical(List<GanttTaskBar> taskList, int currentIndex, int direction, bool extendSelection)
        {
            var targetIndex = FindNextTaskInDirection(taskList, currentIndex, direction, true);
            if (targetIndex >= 0 && targetIndex < taskList.Count)
            {
                var targetTask = taskList[targetIndex];
                if (extendSelection && AllowMultiSelection)
                {
                    SelectTask(targetTask, true);
                }
                else
                {
                    SelectTask(targetTask, false);
                }
            }
        }

        private void NavigateHorizontal(List<GanttTaskBar> taskList, int currentIndex, int direction, bool extendSelection)
        {
            var targetIndex = FindNextTaskInDirection(taskList, currentIndex, direction, false);
            if (targetIndex >= 0 && targetIndex < taskList.Count)
            {
                var targetTask = taskList[targetIndex];
                if (extendSelection && AllowMultiSelection)
                {
                    SelectTask(targetTask, true);
                }
                else
                {
                    SelectTask(targetTask, false);
                }
            }
        }

        private int FindNextTaskInDirection(List<GanttTaskBar> taskList, int currentIndex, int direction, bool vertical)
        {
            if (currentIndex < 0 || currentIndex >= taskList.Count)
            {
                return direction > 0 ? 0 : taskList.Count - 1;
            }

            var currentTask = taskList[currentIndex];
            var currentRow = currentTask.RowIndex;
            var currentTime = currentTask.TimeIndex;

            if (vertical)
            {
                // Find task in same time column but different row
                var targetRow = currentRow + direction;
                return taskList.FindIndex(t => t.RowIndex == targetRow);
            }
            else
            {
                // Find task in same row but different time
                var targetTime = currentTime + direction;
                return taskList.FindIndex(t => t.TimeIndex == targetTime && t.RowIndex == currentRow);
            }
        }

        private void OnSelectionChanged(List<GanttTaskBar> previousSelection, List<GanttTaskBar> currentSelection)
        {
            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(previousSelection, currentSelection));
        }

        #endregion
    }

    /// <summary>
    /// Event arguments for selection change events.
    /// </summary>
    public class SelectionChangedEventArgs : EventArgs
    {
        public List<GanttTaskBar> PreviousSelection { get; }
        public List<GanttTaskBar> CurrentSelection { get; }

        public SelectionChangedEventArgs(List<GanttTaskBar> previousSelection, List<GanttTaskBar> currentSelection)
        {
            PreviousSelection = previousSelection ?? new List<GanttTaskBar>();
            CurrentSelection = currentSelection ?? new List<GanttTaskBar>();
        }
    }
}