using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GPM.Gantt.Models;
using GPM.Gantt.Services;

namespace GPM.Gantt.Interaction
{
    /// <summary>
    /// Coordinates all interactive features for the Gantt chart including selection, drag-drop, and keyboard navigation.
    /// </summary>
    public class GanttInteractionManager
    {
        #region Events

        /// <summary>
        /// Occurs when a task is selected or deselected.
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

        /// <summary>
        /// Occurs when a task is moved via drag and drop.
        /// </summary>
        public event EventHandler<TaskMovedEventArgs>? TaskMoved;

        /// <summary>
        /// Occurs when a task is resized.
        /// </summary>
        public event EventHandler<TaskResizedEventArgs>? TaskResized;

        /// <summary>
        /// Occurs when a task is double-clicked for editing.
        /// </summary>
        public event EventHandler<TaskEditRequestedEventArgs>? TaskEditRequested;

        /// <summary>
        /// Occurs when validation is needed for a drag operation.
        /// </summary>
        public event EventHandler<DragValidationEventArgs>? ValidatingDrag;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the parent Gantt container.
        /// </summary>
        public GanttContainer? GanttContainer { get; set; }

        /// <summary>
        /// Gets or sets the validation service.
        /// </summary>
        public IValidationService? ValidationService { get; set; }

        /// <summary>
        /// Gets the selection manager.
        /// </summary>
        public TaskSelectionManager SelectionManager { get; }

        /// <summary>
        /// Gets the drag and drop manager.
        /// </summary>
        public DragDropManager DragDropManager { get; }

        /// <summary>
        /// Gets or sets whether interactive features are enabled.
        /// </summary>
        public bool IsInteractionEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether drag and drop is enabled.
        /// </summary>
        public bool IsDragDropEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether task resizing is enabled.
        /// </summary>
        public bool IsResizeEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether multi-selection is enabled.
        /// </summary>
        public bool IsMultiSelectionEnabled
        {
            get => SelectionManager.AllowMultiSelection;
            set => SelectionManager.AllowMultiSelection = value;
        }

        #endregion

        #region Private Fields

        private readonly Dictionary<GanttTaskBar, TaskBarEventHandlers> _taskBarHandlers = new();
        private bool _isInitialized;

        #endregion

        public GanttInteractionManager()
        {
            SelectionManager = new TaskSelectionManager();
            DragDropManager = new DragDropManager();

            SetupEventHandlers();
        }

        #region Initialization

        /// <summary>
        /// Initializes the interaction manager with the specified Gantt container.
        /// </summary>
        /// <param name="ganttContainer">The Gantt container to manage.</param>
        public void Initialize(GanttContainer ganttContainer)
        {
            if (_isInitialized)
            {
                Cleanup();
            }

            GanttContainer = ganttContainer ?? throw new ArgumentNullException(nameof(ganttContainer));
            DragDropManager.GanttContainer = ganttContainer;
            DragDropManager.ValidationService = ValidationService;

            // Subscribe to container events
            ganttContainer.Loaded += OnContainerLoaded;
            ganttContainer.KeyDown += OnContainerKeyDown;
            ganttContainer.MouseDown += OnContainerMouseDown;

            _isInitialized = true;
        }

        private void SetupEventHandlers()
        {
            SelectionManager.SelectionChanged += (s, e) => SelectionChanged?.Invoke(this, e);

            DragDropManager.DragCompleted += OnDragCompleted;
            DragDropManager.ResizeCompleted += OnResizeCompleted;
            DragDropManager.ValidatingDrop += OnValidatingDrop;
        }

        #endregion

        #region Task Bar Management

        /// <summary>
        /// Registers a task bar for interaction handling.
        /// </summary>
        /// <param name="taskBar">The task bar to register.</param>
        public void RegisterTaskBar(GanttTaskBar taskBar)
        {
            if (taskBar == null || _taskBarHandlers.ContainsKey(taskBar)) return;

            var handlers = new TaskBarEventHandlers();

            // Set up event handlers
            handlers.TaskDoubleClickedHandler = (s, e) => OnTaskDoubleClicked(taskBar, e);
            handlers.DragStartedHandler = (s, e) => OnTaskDragStarted(taskBar, e);
            handlers.DraggingHandler = (s, e) => OnTaskDragging(taskBar, e);
            handlers.DragCompletedHandler = (s, e) => OnTaskDragCompleted(taskBar, e);
            handlers.ResizeStartedHandler = (s, e) => OnTaskResizeStarted(taskBar, e);
            handlers.ResizingHandler = (s, e) => OnTaskResizing(taskBar, e);
            handlers.ResizeCompletedHandler = (s, e) => OnTaskResizeCompleted(taskBar, e);

            // Subscribe to events
            taskBar.TaskDoubleClicked += handlers.TaskDoubleClickedHandler;
            taskBar.DragStarted += handlers.DragStartedHandler;
            taskBar.Dragging += handlers.DraggingHandler;
            taskBar.DragCompleted += handlers.DragCompletedHandler;
            taskBar.ResizeStarted += handlers.ResizeStartedHandler;
            taskBar.Resizing += handlers.ResizingHandler;
            taskBar.ResizeCompleted += handlers.ResizeCompletedHandler;

            // Configure interactive properties
            taskBar.IsDragDropEnabled = IsDragDropEnabled;
            taskBar.IsResizeEnabled = IsResizeEnabled;
            taskBar.IsInteractive = IsInteractionEnabled;

            _taskBarHandlers[taskBar] = handlers;
        }

        /// <summary>
        /// Unregisters a task bar from interaction handling.
        /// </summary>
        /// <param name="taskBar">The task bar to unregister.</param>
        public void UnregisterTaskBar(GanttTaskBar taskBar)
        {
            if (taskBar == null || !_taskBarHandlers.TryGetValue(taskBar, out var handlers)) return;

            // Unsubscribe from events
            taskBar.TaskDoubleClicked -= handlers.TaskDoubleClickedHandler;
            taskBar.DragStarted -= handlers.DragStartedHandler;
            taskBar.Dragging -= handlers.DraggingHandler;
            taskBar.DragCompleted -= handlers.DragCompletedHandler;
            taskBar.ResizeStarted -= handlers.ResizeStartedHandler;
            taskBar.Resizing -= handlers.ResizingHandler;
            taskBar.ResizeCompleted -= handlers.ResizeCompletedHandler;

            _taskBarHandlers.Remove(taskBar);
        }

        /// <summary>
        /// Gets all registered task bars.
        /// </summary>
        /// <returns>Collection of registered task bars.</returns>
        public IEnumerable<GanttTaskBar> GetRegisteredTaskBars()
        {
            return _taskBarHandlers.Keys.ToList();
        }

        #endregion

        #region Selection Methods

        /// <summary>
        /// Selects the specified task bar.
        /// </summary>
        /// <param name="taskBar">The task bar to select.</param>
        /// <param name="addToSelection">Whether to add to current selection.</param>
        public void SelectTask(GanttTaskBar taskBar, bool addToSelection = false)
        {
            if (!IsInteractionEnabled) return;
            SelectionManager.SelectTask(taskBar, addToSelection);
        }

        /// <summary>
        /// Clears all selections.
        /// </summary>
        public void ClearSelection()
        {
            SelectionManager.ClearSelection();
        }

        /// <summary>
        /// Selects all task bars.
        /// </summary>
        public void SelectAll()
        {
            if (!IsInteractionEnabled || !IsMultiSelectionEnabled) return;
            SelectionManager.SelectAll(_taskBarHandlers.Keys);
        }

        #endregion

        #region Event Handlers

        private void OnContainerLoaded(object sender, RoutedEventArgs e)
        {
            // Container is loaded, can now access visual tree
            RefreshTaskBarRegistrations();
        }

        private void OnContainerKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsInteractionEnabled) return;

            // Handle keyboard shortcuts
            switch (e.Key)
            {
                case Key.A when e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control):
                    SelectAll();
                    e.Handled = true;
                    break;

                case Key.Escape:
                    ClearSelection();
                    e.Handled = true;
                    break;

                case Key.Delete:
                    DeleteSelectedTasks();
                    e.Handled = true;
                    break;

                default:
                    SelectionManager.HandleKeyboardInput(e, _taskBarHandlers.Keys);
                    break;
            }
        }

        private void OnContainerMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsInteractionEnabled) return;

            // If clicking on empty space, clear selection unless Ctrl is held
            if (e.OriginalSource == GanttContainer)
            {
                if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    ClearSelection();
                }
            }
        }

        private void OnTaskDoubleClicked(GanttTaskBar taskBar, TaskBarEventArgs e)
        {
            TaskEditRequested?.Invoke(this, new TaskEditRequestedEventArgs(taskBar));
        }

        private void OnTaskDragStarted(GanttTaskBar taskBar, TaskBarDragEventArgs e)
        {
            if (!IsDragDropEnabled) return;
            DragDropManager.StartDrag(taskBar, e.Position);
        }

        private void OnTaskDragging(GanttTaskBar taskBar, TaskBarDragEventArgs e)
        {
            if (!IsDragDropEnabled) return;
            DragDropManager.UpdateDrag(e.Position);
        }

        private void OnTaskDragCompleted(GanttTaskBar taskBar, TaskBarDragEventArgs e)
        {
            if (!IsDragDropEnabled) return;
            DragDropManager.CompleteDrag(e.Position);
        }

        private void OnTaskResizeStarted(GanttTaskBar taskBar, TaskBarResizeEventArgs e)
        {
            if (!IsResizeEnabled) return;
            DragDropManager.StartResize(taskBar, e.Direction, e.Position);
        }

        private void OnTaskResizing(GanttTaskBar taskBar, TaskBarResizeEventArgs e)
        {
            if (!IsResizeEnabled) return;
            DragDropManager.UpdateResize(e.Position);
        }

        private void OnTaskResizeCompleted(GanttTaskBar taskBar, TaskBarResizeEventArgs e)
        {
            if (!IsResizeEnabled) return;
            DragDropManager.CompleteResize(e.Position);
        }

        private void OnDragCompleted(object? sender, DragCompletedEventArgs e)
        {
            TaskMoved?.Invoke(this, new TaskMovedEventArgs(
                e.TaskBar,
                e.OriginalRowIndex, e.OriginalTimeIndex,
                e.NewRowIndex, e.NewTimeIndex,
                e.NewStartTime, e.NewEndTime));
        }

        private void OnResizeCompleted(object? sender, ResizeCompletedEventArgs e)
        {
            TaskResized?.Invoke(this, new TaskResizedEventArgs(
                e.TaskBar,
                e.Direction,
                e.OriginalStartTime, e.OriginalEndTime,
                e.NewStartTime, e.NewEndTime));
        }

        private void OnValidatingDrop(object? sender, DragValidationEventArgs e)
        {
            ValidatingDrag?.Invoke(this, e);
        }

        #endregion

        #region Helper Methods

        private void RefreshTaskBarRegistrations()
        {
            if (GanttContainer == null) return;

            // Find all GanttTaskBar instances in the visual tree
            var taskBars = FindVisualChildren<GanttTaskBar>(GanttContainer).ToList();

            // Register new task bars
            foreach (var taskBar in taskBars)
            {
                if (!_taskBarHandlers.ContainsKey(taskBar))
                {
                    RegisterTaskBar(taskBar);
                }
            }

            // Unregister removed task bars
            var removedTaskBars = _taskBarHandlers.Keys.Except(taskBars).ToList();
            foreach (var taskBar in removedTaskBars)
            {
                UnregisterTaskBar(taskBar);
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T tChild)
                {
                    yield return tChild;
                }

                foreach (var childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }

        private void DeleteSelectedTasks()
        {
            if (SelectionManager.SelectedTasks.Count == 0) return;

            var tasksToDelete = SelectionManager.SelectedTasks.ToList();
            var args = new TaskDeletionRequestedEventArgs(tasksToDelete);
            
            // Raise event for parent to handle
            TaskEditRequested?.Invoke(this, new TaskEditRequestedEventArgs(tasksToDelete.First()) { Action = "Delete", AffectedTasks = tasksToDelete });
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Cleans up the interaction manager and releases resources.
        /// </summary>
        public void Cleanup()
        {
            // Unregister all task bars
            var taskBars = _taskBarHandlers.Keys.ToList();
            foreach (var taskBar in taskBars)
            {
                UnregisterTaskBar(taskBar);
            }

            // Unsubscribe from container events
            if (GanttContainer != null)
            {
                GanttContainer.Loaded -= OnContainerLoaded;
                GanttContainer.KeyDown -= OnContainerKeyDown;
                GanttContainer.MouseDown -= OnContainerMouseDown;
            }

            SelectionManager.ClearSelection();
            _isInitialized = false;
        }

        #endregion
    }

    #region Helper Classes

    internal class TaskBarEventHandlers
    {
        public EventHandler<TaskBarEventArgs>? TaskDoubleClickedHandler { get; set; }
        public EventHandler<TaskBarDragEventArgs>? DragStartedHandler { get; set; }
        public EventHandler<TaskBarDragEventArgs>? DraggingHandler { get; set; }
        public EventHandler<TaskBarDragEventArgs>? DragCompletedHandler { get; set; }
        public EventHandler<TaskBarResizeEventArgs>? ResizeStartedHandler { get; set; }
        public EventHandler<TaskBarResizeEventArgs>? ResizingHandler { get; set; }
        public EventHandler<TaskBarResizeEventArgs>? ResizeCompletedHandler { get; set; }
    }

    #endregion

    #region Event Args

    public class TaskMovedEventArgs : EventArgs
    {
        public GanttTaskBar TaskBar { get; }
        public int OriginalRowIndex { get; }
        public int OriginalTimeIndex { get; }
        public int NewRowIndex { get; }
        public int NewTimeIndex { get; }
        public DateTime NewStartTime { get; }
        public DateTime NewEndTime { get; }

        public TaskMovedEventArgs(GanttTaskBar taskBar, int originalRowIndex, int originalTimeIndex,
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

    public class TaskResizedEventArgs : EventArgs
    {
        public GanttTaskBar TaskBar { get; }
        public ResizeDirection Direction { get; }
        public DateTime OriginalStartTime { get; }
        public DateTime OriginalEndTime { get; }
        public DateTime NewStartTime { get; }
        public DateTime NewEndTime { get; }

        public TaskResizedEventArgs(GanttTaskBar taskBar, ResizeDirection direction,
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

    public class TaskEditRequestedEventArgs : EventArgs
    {
        public GanttTaskBar TaskBar { get; }
        public string Action { get; set; } = "Edit";
        public List<GanttTaskBar>? AffectedTasks { get; set; }

        public TaskEditRequestedEventArgs(GanttTaskBar taskBar)
        {
            TaskBar = taskBar;
        }
    }

    public class TaskDeletionRequestedEventArgs : EventArgs
    {
        public List<GanttTaskBar> TasksToDelete { get; }

        public TaskDeletionRequestedEventArgs(List<GanttTaskBar> tasksToDelete)
        {
            TasksToDelete = tasksToDelete;
        }
    }

    #endregion
}