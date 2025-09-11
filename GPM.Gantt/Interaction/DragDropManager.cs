using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GPM.Gantt.Models;
using GPM.Gantt.Services;

namespace GPM.Gantt.Interaction
{
    /// <summary>
    /// Manages drag and drop operations for task bars in the Gantt chart.
    /// </summary>
    public class DragDropManager
    {
        #region Events

        /// <summary>
        /// Occurs when a drag operation starts.
        /// </summary>
        public event EventHandler<DragStartedEventArgs>? DragStarted;

        /// <summary>
        /// Occurs during dragging to validate drop targets.
        /// </summary>
        public event EventHandler<DragValidationEventArgs>? ValidatingDrop;

        /// <summary>
        /// Occurs when a drag operation completes successfully.
        /// </summary>
        public event EventHandler<DragCompletedEventArgs>? DragCompleted;

        /// <summary>
        /// Occurs when a resize operation completes.
        /// </summary>
        public event EventHandler<ResizeCompletedEventArgs>? ResizeCompleted;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the parent Gantt container.
        /// </summary>
        public GanttContainer? GanttContainer { get; set; }

        /// <summary>
        /// Gets or sets the validation service for drag operations.
        /// </summary>
        public IValidationService? ValidationService { get; set; }

        /// <summary>
        /// Gets or sets whether snap-to-grid is enabled during drag operations.
        /// </summary>
        public bool SnapToGrid { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum task duration in days.
        /// </summary>
        public double MinimumTaskDurationDays { get; set; } = 0.5;

        #endregion

        #region Private Fields

        private readonly VisualFeedbackManager _feedbackManager;
        private DragContext? _currentDragContext;
        private ResizeContext? _currentResizeContext;

        #endregion

        public DragDropManager()
        {
            _feedbackManager = new VisualFeedbackManager();
        }

        #region Drag Operations

        /// <summary>
        /// Starts a drag operation for the specified task bar.
        /// </summary>
        /// <param name="taskBar">The task bar being dragged.</param>
        /// <param name="startPosition">The initial mouse position.</param>
        public void StartDrag(GanttTaskBar taskBar, Point startPosition)
        {
            if (taskBar == null || GanttContainer == null) return;

            _currentDragContext = new DragContext
            {
                TaskBar = taskBar,
                StartPosition = startPosition,
                OriginalRowIndex = taskBar.RowIndex,
                OriginalTimeIndex = taskBar.TimeIndex,
                StartTime = DateTime.Now
            };

            _feedbackManager.ShowDragPreview(taskBar);
            DragStarted?.Invoke(this, new DragStartedEventArgs(taskBar, startPosition));
        }

        /// <summary>
        /// Updates the drag operation with the current mouse position.
        /// </summary>
        /// <param name="currentPosition">The current mouse position.</param>
        public void UpdateDrag(Point currentPosition)
        {
            if (_currentDragContext == null || GanttContainer == null) return;

            var deltaX = currentPosition.X - _currentDragContext.StartPosition.X;
            var deltaY = currentPosition.Y - _currentDragContext.StartPosition.Y;

            // Calculate new position
            var newRowIndex = CalculateRowFromPosition(currentPosition);
            var newTimeIndex = CalculateTimeIndexFromPosition(currentPosition);

            // Snap to grid if enabled
            if (SnapToGrid)
            {
                newRowIndex = Math.Max(1, Math.Min(GanttContainer.TaskCount, newRowIndex));
                newTimeIndex = Math.Max(0, newTimeIndex);
            }

            // Validate the drop position
            var validationArgs = new DragValidationEventArgs(
                _currentDragContext.TaskBar,
                newRowIndex,
                newTimeIndex,
                currentPosition);

            ValidatingDrop?.Invoke(this, validationArgs);

            // Update visual feedback
            _feedbackManager.UpdateDragPreview(
                _currentDragContext.TaskBar,
                currentPosition,
                validationArgs.IsValidDrop);

            _currentDragContext.CurrentRowIndex = newRowIndex;
            _currentDragContext.CurrentTimeIndex = newTimeIndex;
            _currentDragContext.IsValidDrop = validationArgs.IsValidDrop;
        }

        /// <summary>
        /// Completes the drag operation.
        /// </summary>
        /// <param name="endPosition">The final mouse position.</param>
        public void CompleteDrag(Point endPosition)
        {
            if (_currentDragContext == null) return;

            try
            {
                _feedbackManager.HideDragPreview();

                if (_currentDragContext.IsValidDrop &&
                    (_currentDragContext.CurrentRowIndex != _currentDragContext.OriginalRowIndex ||
                     _currentDragContext.CurrentTimeIndex != _currentDragContext.OriginalTimeIndex))
                {
                    // Calculate new dates based on position change
                    var timeIndexDelta = _currentDragContext.CurrentTimeIndex - _currentDragContext.OriginalTimeIndex;

                    // Use precise tick mapping to avoid cumulative +1 span on each drag
                    if (GanttContainer != null)
                    {
                        var ticks = GPM.Gantt.Utilities.TimelineCalculator.GenerateTicks(
                            GanttContainer.StartTime,
                            GanttContainer.EndTime,
                            GanttContainer.TimeUnit,
                            GanttContainer.Culture);

                        int totalColumns = Math.Max(1, ticks.Count);
                        int startIndex = Math.Max(0, Math.Min(_currentDragContext.CurrentTimeIndex, totalColumns - 1));
                        int columnSpan = Math.Max(1, System.Windows.Controls.Grid.GetColumnSpan(_currentDragContext.TaskBar));
                        int endIndex = Math.Max(0, Math.Min(startIndex + columnSpan - 1, totalColumns - 1));

                        var newStartTime = ticks[startIndex];
                        var newEndTime = ticks[endIndex]; // align to start of last included tick

                        var completedArgs = new DragCompletedEventArgs(
                            _currentDragContext.TaskBar,
                            _currentDragContext.OriginalRowIndex,
                            _currentDragContext.OriginalTimeIndex,
                            _currentDragContext.CurrentRowIndex,
                            _currentDragContext.CurrentTimeIndex,
                            newStartTime,
                            newEndTime);

                        DragCompleted?.Invoke(this, completedArgs);
                    }
                    else
                    {
                        var newStartTime = CalculateDateFromTimeIndex(_currentDragContext.CurrentTimeIndex);
                        var taskDuration = CalculateTaskDuration(_currentDragContext.TaskBar);
                        var newEndTime = newStartTime.Add(taskDuration);

                        var completedArgs = new DragCompletedEventArgs(
                            _currentDragContext.TaskBar,
                            _currentDragContext.OriginalRowIndex,
                            _currentDragContext.OriginalTimeIndex,
                            _currentDragContext.CurrentRowIndex,
                            _currentDragContext.CurrentTimeIndex,
                            newStartTime,
                            newEndTime);

                        DragCompleted?.Invoke(this, completedArgs);
                    }
                }
            }
            finally
            {
                _currentDragContext = null;
            }
        }

        #endregion

        #region Resize Operations

        /// <summary>
        /// Starts a resize operation for the specified task bar.
        /// </summary>
        /// <param name="taskBar">The task bar being resized.</param>
        /// <param name="direction">The resize direction.</param>
        /// <param name="startPosition">The initial mouse position.</param>
        public void StartResize(GanttTaskBar taskBar, ResizeDirection direction, Point startPosition)
        {
            if (taskBar == null || GanttContainer == null) return;

            _currentResizeContext = new ResizeContext
            {
                TaskBar = taskBar,
                Direction = direction,
                StartPosition = startPosition,
                OriginalStartTime = CalculateDateFromTimeIndex(taskBar.TimeIndex),
                OriginalEndTime = CalculateDateFromTimeIndex(taskBar.TimeIndex).Add(CalculateTaskDuration(taskBar))
            };

            _feedbackManager.ShowResizePreview(taskBar, direction);
        }

        /// <summary>
        /// Updates the resize operation with the current mouse position.
        /// </summary>
        /// <param name="currentPosition">The current mouse position.</param>
        public void UpdateResize(Point currentPosition)
        {
            if (_currentResizeContext == null || GanttContainer == null) return;

            var deltaX = currentPosition.X - _currentResizeContext.StartPosition.X;
            var timeIndexDelta = CalculateTimeIndexDelta(deltaX);

            DateTime newStartTime, newEndTime;

            if (_currentResizeContext.Direction == ResizeDirection.Left)
            {
                newStartTime = _currentResizeContext.OriginalStartTime.AddDays(timeIndexDelta);
                newEndTime = _currentResizeContext.OriginalEndTime;

                // Ensure minimum duration
                if ((newEndTime - newStartTime).TotalDays < MinimumTaskDurationDays)
                {
                    newStartTime = newEndTime.AddDays(-MinimumTaskDurationDays);
                }
            }
            else // Right
            {
                newStartTime = _currentResizeContext.OriginalStartTime;
                newEndTime = _currentResizeContext.OriginalEndTime.AddDays(timeIndexDelta);

                // Ensure minimum duration
                if ((newEndTime - newStartTime).TotalDays < MinimumTaskDurationDays)
                {
                    newEndTime = newStartTime.AddDays(MinimumTaskDurationDays);
                }
            }

            _currentResizeContext.NewStartTime = newStartTime;
            _currentResizeContext.NewEndTime = newEndTime;

            _feedbackManager.UpdateResizePreview(
                _currentResizeContext.TaskBar,
                currentPosition,
                _currentResizeContext.Direction,
                true); // Assume valid for now
        }

        /// <summary>
        /// Completes the resize operation.
        /// </summary>
        /// <param name="endPosition">The final mouse position.</param>
        public void CompleteResize(Point endPosition)
        {
            if (_currentResizeContext == null) return;

            try
            {
                _feedbackManager.HideResizePreview();

                if (_currentResizeContext.NewStartTime.HasValue && _currentResizeContext.NewEndTime.HasValue)
                {
                    var completedArgs = new ResizeCompletedEventArgs(
                        _currentResizeContext.TaskBar,
                        _currentResizeContext.Direction,
                        _currentResizeContext.OriginalStartTime,
                        _currentResizeContext.OriginalEndTime,
                        _currentResizeContext.NewStartTime.Value,
                        _currentResizeContext.NewEndTime.Value);

                    ResizeCompleted?.Invoke(this, completedArgs);
                }
            }
            finally
            {
                _currentResizeContext = null;
            }
        }

        #endregion

        #region Helper Methods

        private int CalculateRowFromPosition(Point position)
        {
            if (GanttContainer == null) return 1;

            // Calculate row based on Y position
            var headerHeight = GanttContainer.HeaderRowHeight.Value;
            var taskRowHeight = GanttContainer.TaskRowHeight.Value;

            if (position.Y <= headerHeight) return 1;

            var rowIndex = (int)Math.Ceiling((position.Y - headerHeight) / taskRowHeight);
            return Math.Max(1, rowIndex);
        }

        private int CalculateTimeIndexFromPosition(Point position)
        {
            if (GanttContainer == null) return 0;

            // Calculate time index based on X position
            var columnWidth = GanttContainer.ActualWidth / Math.Max(1, GanttContainer.ColumnDefinitions.Count);
            return (int)Math.Floor(position.X / columnWidth);
        }

        private DateTime CalculateDateFromTimeIndex(int timeIndex)
        {
            if (GanttContainer == null) return DateTime.Today;

            var timeRange = GanttContainer.EndTime - GanttContainer.StartTime;
            var totalColumns = Math.Max(1, GanttContainer.ColumnDefinitions.Count);
            var timePerColumn = timeRange.TotalDays / totalColumns;

            return GanttContainer.StartTime.AddDays(timeIndex * timePerColumn);
        }

        private TimeSpan CalculateTaskDuration(GanttTaskBar taskBar)
        {
            // Derive duration from current column span to preserve the task's existing visual length
            if (GanttContainer == null || taskBar == null)
                return TimeSpan.FromDays(1);

            var totalColumns = Math.Max(1, GanttContainer.ColumnDefinitions.Count);
            var timeRange = GanttContainer.EndTime - GanttContainer.StartTime;
            var timePerColumnDays = timeRange.TotalDays / totalColumns;

            // Use the Grid column span of the task bar to compute its duration.
            // Important: TimelineHelper.CalculateTaskSpan treats endIndex as inclusive
            // (columnSpan = endIndex - startIndex + 1). To represent N columns,
            // the taskEnd should align to the start of the last included tick, i.e.,
            // duration = (N - 1) * timePerColumn.
            var columnSpan = Math.Max(1, System.Windows.Controls.Grid.GetColumnSpan(taskBar));
            var durationDays = Math.Max(0, (columnSpan - 1) * timePerColumnDays);
            return TimeSpan.FromDays(durationDays);
        }

        private double CalculateTimeIndexDelta(double deltaX)
        {
            if (GanttContainer == null) return 0;

            var columnWidth = GanttContainer.ActualWidth / Math.Max(1, GanttContainer.ColumnDefinitions.Count);
            var timeRange = GanttContainer.EndTime - GanttContainer.StartTime;
            var totalColumns = GanttContainer.ColumnDefinitions.Count;
            var timePerColumn = timeRange.TotalDays / totalColumns;
            var timePerPixel = timePerColumn / columnWidth;

            return deltaX * timePerPixel;
        }

        #endregion
    }

    #region Context Classes

    internal class DragContext
    {
        public GanttTaskBar? TaskBar { get; set; }
        public Point StartPosition { get; set; }
        public int OriginalRowIndex { get; set; }
        public int OriginalTimeIndex { get; set; }
        public int CurrentRowIndex { get; set; }
        public int CurrentTimeIndex { get; set; }
        public bool IsValidDrop { get; set; } = true;
        public DateTime StartTime { get; set; }
    }

    internal class ResizeContext
    {
        public GanttTaskBar? TaskBar { get; set; }
        public ResizeDirection Direction { get; set; }
        public Point StartPosition { get; set; }
        public DateTime OriginalStartTime { get; set; }
        public DateTime OriginalEndTime { get; set; }
        public DateTime? NewStartTime { get; set; }
        public DateTime? NewEndTime { get; set; }
    }

    #endregion

    #region Event Args

    public class DragStartedEventArgs : EventArgs
    {
        public GanttTaskBar TaskBar { get; }
        public Point StartPosition { get; }

        public DragStartedEventArgs(GanttTaskBar taskBar, Point startPosition)
        {
            TaskBar = taskBar;
            StartPosition = startPosition;
        }
    }

    public class DragValidationEventArgs : EventArgs
    {
        public GanttTaskBar TaskBar { get; }
        public int NewRowIndex { get; }
        public int NewTimeIndex { get; }
        public Point Position { get; }
        public bool IsValidDrop { get; set; } = true;
        public string? ValidationMessage { get; set; }

        public DragValidationEventArgs(GanttTaskBar taskBar, int newRowIndex, int newTimeIndex, Point position)
        {
            TaskBar = taskBar;
            NewRowIndex = newRowIndex;
            NewTimeIndex = newTimeIndex;
            Position = position;
        }
    }

    public class DragCompletedEventArgs : EventArgs
    {
        public GanttTaskBar TaskBar { get; }
        public int OriginalRowIndex { get; }
        public int OriginalTimeIndex { get; }
        public int NewRowIndex { get; }
        public int NewTimeIndex { get; }
        public DateTime NewStartTime { get; }
        public DateTime NewEndTime { get; }

        public DragCompletedEventArgs(GanttTaskBar taskBar, int originalRowIndex, int originalTimeIndex,
            int newRowIndex, int newTimeIndex, DateTime newStartTime, DateTime newEndTime)
        {
            TaskBar = taskBar;
            OriginalRowIndex = originalRowIndex;
            OriginalTimeIndex = originalTimeIndex;
            NewRowIndex = newRowIndex;
            NewTimeIndex = newTimeIndex;
            NewStartTime = newStartTime;
            NewEndTime = newEndTime;
        }
    }

    public class ResizeCompletedEventArgs : EventArgs
    {
        public GanttTaskBar TaskBar { get; }
        public ResizeDirection Direction { get; }
        public DateTime OriginalStartTime { get; }
        public DateTime OriginalEndTime { get; }
        public DateTime NewStartTime { get; }
        public DateTime NewEndTime { get; }

        public ResizeCompletedEventArgs(GanttTaskBar taskBar, ResizeDirection direction,
            DateTime originalStartTime, DateTime originalEndTime,
            DateTime newStartTime, DateTime newEndTime)
        {
            TaskBar = taskBar;
            Direction = direction;
            OriginalStartTime = originalStartTime;
            OriginalEndTime = originalEndTime;
            NewStartTime = newStartTime;
            NewEndTime = newEndTime;
        }
    }

    #endregion
}