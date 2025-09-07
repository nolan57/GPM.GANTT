using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.ComponentModel;
using GPM.Gantt.Models;

namespace GPM.Gantt
{
    /// <summary>
    /// Enhanced task bar with progress visualization, drag-and-drop support, and context menu functionality.
    /// </summary>
    public class GanttTaskBar : GanttRectangle
    {
        #region Dependency Properties
        
        /// <summary>
        /// Gets or sets the custom text displayed on the task bar.
        /// </summary>
        public static readonly DependencyProperty CustomTextProperty = DependencyProperty.Register(
            nameof(CustomText), typeof(string), typeof(GanttTaskBar), new FrameworkPropertyMetadata(string.Empty));

        public string CustomText
        {
            get => (string)GetValue(CustomTextProperty);
            set => SetValue(CustomTextProperty, value);
        }

        /// <summary>
        /// Gets or sets the progress percentage (0-100) for the task.
        /// </summary>
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
            nameof(Progress), typeof(double), typeof(GanttTaskBar), 
            new FrameworkPropertyMetadata(0.0, OnProgressChanged));

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, Math.Max(0, Math.Min(100, value)));
        }

        /// <summary>
        /// Gets or sets the priority of the task.
        /// </summary>
        public static readonly DependencyProperty PriorityProperty = DependencyProperty.Register(
            nameof(Priority), typeof(TaskPriority), typeof(GanttTaskBar), 
            new FrameworkPropertyMetadata(TaskPriority.Normal));

        public TaskPriority Priority
        {
            get => (TaskPriority)GetValue(PriorityProperty);
            set => SetValue(PriorityProperty, value);
        }

        /// <summary>
        /// Gets or sets the status of the task.
        /// </summary>
        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            nameof(Status), typeof(Models.TaskStatus), typeof(GanttTaskBar), 
            new FrameworkPropertyMetadata(Models.TaskStatus.NotStarted));

        public Models.TaskStatus Status
        {
            get => (Models.TaskStatus)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the task bar supports drag and drop operations.
        /// </summary>
        public static readonly DependencyProperty IsDragDropEnabledProperty = DependencyProperty.Register(
            nameof(IsDragDropEnabled), typeof(bool), typeof(GanttTaskBar), 
            new FrameworkPropertyMetadata(true));

        public bool IsDragDropEnabled
        {
            get => (bool)GetValue(IsDragDropEnabledProperty);
            set => SetValue(IsDragDropEnabledProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the task bar supports resizing operations.
        /// </summary>
        public static readonly DependencyProperty IsResizeEnabledProperty = DependencyProperty.Register(
            nameof(IsResizeEnabled), typeof(bool), typeof(GanttTaskBar), 
            new FrameworkPropertyMetadata(true));

        public bool IsResizeEnabled
        {
            get => (bool)GetValue(IsResizeEnabledProperty);
            set => SetValue(IsResizeEnabledProperty, value);
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the task bar is double-clicked.
        /// </summary>
        public event EventHandler<TaskBarEventArgs>? TaskDoubleClicked;

        /// <summary>
        /// Occurs when the task bar drag operation starts.
        /// </summary>
        public event EventHandler<TaskBarDragEventArgs>? DragStarted;

        /// <summary>
        /// Occurs when the task bar is being dragged.
        /// </summary>
        public event EventHandler<TaskBarDragEventArgs>? Dragging;

        /// <summary>
        /// Occurs when the task bar drag operation completes.
        /// </summary>
        public event EventHandler<TaskBarDragEventArgs>? DragCompleted;

        /// <summary>
        /// Occurs when the task bar resize operation starts.
        /// </summary>
        public event EventHandler<TaskBarResizeEventArgs>? ResizeStarted;

        /// <summary>
        /// Occurs when the task bar is being resized.
        /// </summary>
        public event EventHandler<TaskBarResizeEventArgs>? Resizing;

        /// <summary>
        /// Occurs when the task bar resize operation completes.
        /// </summary>
        public event EventHandler<TaskBarResizeEventArgs>? ResizeCompleted;

        #endregion

        #region Private Fields

        private Grid? _mainGrid;
        private Border? _progressBar;
        private TextBlock? _textBlock;
        private ContextMenu? _contextMenu;
        private bool _isDragging;
        private bool _isResizing;
        private Point _lastMousePosition;
        private ResizeDirection _resizeDirection = ResizeDirection.None;
        private const double ResizeHandleWidth = 8.0;

        #endregion

        public GanttTaskBar()
        {
            CornerRadius = new CornerRadius(4);
            ContentPadding = new Thickness(4, 2, 4, 2);
            ZIndex = 10; // Ensure task bars appear above grid elements
            
            InitializeTemplate();
            InitializeContextMenu();
            SetupEventHandlers();
        }

        private void InitializeTemplate()
        {
            // Create main grid container
            _mainGrid = new Grid();
            
            // Create progress bar background
            _progressBar = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(80, 76, 175, 80)), // Semi-transparent green
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch,
                CornerRadius = new CornerRadius(2)
            };
            
            // Create text block
            _textBlock = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(4, 0, 4, 0),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Medium
            };
            
            // Bind text to CustomText property
            _textBlock.SetBinding(TextBlock.TextProperty, new Binding(nameof(CustomText)) { Source = this });
            
            // Add elements to grid
            _mainGrid.Children.Add(_progressBar);
            _mainGrid.Children.Add(_textBlock);
            
            Child = _mainGrid;
            
            UpdateProgressDisplay();
        }

        private void InitializeContextMenu()
        {
            _contextMenu = new ContextMenu();
            
            var editItem = new MenuItem { Header = "Edit Task" };
            editItem.Click += (s, e) => OnTaskDoubleClicked();
            
            var deleteItem = new MenuItem { Header = "Delete Task" };
            deleteItem.Click += (s, e) => 
            {
                // Raise event for parent to handle
                TaskDoubleClicked?.Invoke(this, new TaskBarEventArgs(this, "Delete"));
            };
            
            _contextMenu.Items.Add(editItem);
            _contextMenu.Items.Add(new Separator());
            _contextMenu.Items.Add(deleteItem);
            
            ContextMenu = _contextMenu;
        }

        private void SetupEventHandlers()
        {
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;

            // Use PreviewMouseLeftButtonDown for double-click detection
            PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        }

        private DateTime _lastClickTime;
        private const int DoubleClickThreshold = 300; // milliseconds

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var currentTime = DateTime.Now;
            if ((currentTime - _lastClickTime).TotalMilliseconds < DoubleClickThreshold)
            {
                OnTaskDoubleClicked();
                e.Handled = true;
            }
            _lastClickTime = currentTime;
        }

        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GanttTaskBar taskBar)
            {
                taskBar.UpdateProgressDisplay();
            }
        }

        private void UpdateProgressDisplay()
        {
            if (_progressBar != null && _mainGrid != null)
            {
                var progressWidth = Math.Max(0, Math.Min(100, Progress)) / 100.0;
                _progressBar.Width = ActualWidth > 0 ? ActualWidth * progressWidth : 0;
                
                // Update progress bar color based on status
                var color = Status switch
                {
                    Models.TaskStatus.Completed => Color.FromArgb(80, 76, 175, 80), // Green
                    Models.TaskStatus.InProgress => Color.FromArgb(80, 33, 150, 243), // Blue
                    Models.TaskStatus.OnHold => Color.FromArgb(80, 255, 193, 7), // Amber
                    Models.TaskStatus.Cancelled => Color.FromArgb(80, 244, 67, 54), // Red
                    _ => Color.FromArgb(80, 158, 158, 158) // Gray
                };
                
                _progressBar.Background = new SolidColorBrush(color);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateProgressDisplay();
        }

        #region Mouse Event Handlers

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsInteractive) return;
            
            _lastMousePosition = e.GetPosition(this);
            
            // Check if we're near an edge for resizing
            if (IsResizeEnabled)
            {
                _resizeDirection = GetResizeDirection(_lastMousePosition);
                if (_resizeDirection != ResizeDirection.None)
                {
                    _isResizing = true;
                    CaptureMouse();
                    ResizeStarted?.Invoke(this, new TaskBarResizeEventArgs(this, _resizeDirection));
                    e.Handled = true;
                    return;
                }
            }
            
            // Start drag operation
            if (IsDragDropEnabled)
            {
                _isDragging = true;
                CaptureMouse();
                DragStarted?.Invoke(this, new TaskBarDragEventArgs(this, _lastMousePosition));
            }
            
            // Handle selection
            if (!IsSelected)
            {
                IsSelected = true;
            }
            
            e.Handled = true;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
                DragCompleted?.Invoke(this, new TaskBarDragEventArgs(this, e.GetPosition(this)));
            }
            
            if (_isResizing)
            {
                _isResizing = false;
                ReleaseMouseCapture();
                ResizeCompleted?.Invoke(this, new TaskBarResizeEventArgs(this, _resizeDirection));
                _resizeDirection = ResizeDirection.None;
            }
            
            e.Handled = true;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var currentPosition = e.GetPosition(this);
            
            if (_isResizing)
            {
                Resizing?.Invoke(this, new TaskBarResizeEventArgs(this, _resizeDirection, currentPosition));
                e.Handled = true;
                return;
            }
            
            if (_isDragging)
            {
                Dragging?.Invoke(this, new TaskBarDragEventArgs(this, currentPosition));
                e.Handled = true;
                return;
            }
            
            // Update cursor based on position
            if (IsResizeEnabled)
            {
                var resizeDir = GetResizeDirection(currentPosition);
                Cursor = resizeDir switch
                {
                    ResizeDirection.Left or ResizeDirection.Right => Cursors.SizeWE,
                    ResizeDirection.None => IsDragDropEnabled ? Cursors.Hand : Cursors.Arrow,
                    _ => Cursors.Arrow
                };
            }
            else if (IsDragDropEnabled)
            {
                Cursor = Cursors.Hand;
            }
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            IsHovered = true;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            IsHovered = false;
            if (!_isDragging && !_isResizing)
            {
                Cursor = Cursors.Arrow;
            }
        }

        #endregion

        #region Helper Methods

        private ResizeDirection GetResizeDirection(Point position)
        {
            if (position.X <= ResizeHandleWidth)
                return ResizeDirection.Left;
            if (position.X >= ActualWidth - ResizeHandleWidth)
                return ResizeDirection.Right;
            return ResizeDirection.None;
        }

        private void OnTaskDoubleClicked()
        {
            TaskDoubleClicked?.Invoke(this, new TaskBarEventArgs(this, "Edit"));
        }

        #endregion
    }

    #region Event Args Classes

    /// <summary>
    /// Event arguments for task bar events.
    /// </summary>
    public class TaskBarEventArgs : EventArgs
    {
        public GanttTaskBar TaskBar { get; }
        public string Action { get; }

        public TaskBarEventArgs(GanttTaskBar taskBar, string action)
        {
            TaskBar = taskBar;
            Action = action;
        }
    }

    /// <summary>
    /// Event arguments for task bar drag events.
    /// </summary>
    public class TaskBarDragEventArgs : EventArgs
    {
        public GanttTaskBar TaskBar { get; }
        public Point Position { get; }

        public TaskBarDragEventArgs(GanttTaskBar taskBar, Point position)
        {
            TaskBar = taskBar;
            Position = position;
        }
    }

    /// <summary>
    /// Event arguments for task bar resize events.
    /// </summary>
    public class TaskBarResizeEventArgs : EventArgs
    {
        public GanttTaskBar TaskBar { get; }
        public ResizeDirection Direction { get; }
        public Point Position { get; }

        public TaskBarResizeEventArgs(GanttTaskBar taskBar, ResizeDirection direction, Point position = default)
        {
            TaskBar = taskBar;
            Direction = direction;
            Position = position;
        }
    }

    /// <summary>
    /// Enumeration for resize directions.
    /// </summary>
    public enum ResizeDirection
    {
        None,
        Left,
        Right
    }

    #endregion
}