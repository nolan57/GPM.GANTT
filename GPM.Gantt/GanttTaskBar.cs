using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GPM.Gantt.Models;

namespace GPM.Gantt
{
    /// <summary>
    /// Represents a task bar in the Gantt chart with support for progress indication and interactive features.
    /// </summary>
    public class GanttTaskBar : GanttShapeBase
    {
        #region Events

        /// <summary>
        /// Occurs when the task bar is double-clicked.
        /// </summary>
        public event EventHandler<TaskBarEventArgs>? TaskDoubleClicked;

        /// <summary>
        /// Occurs when a drag operation starts.
        /// </summary>
        public event EventHandler<TaskBarDragEventArgs>? DragStarted;

        /// <summary>
        /// Occurs when a drag operation is in progress.
        /// </summary>
        public event EventHandler<TaskBarDragEventArgs>? Dragging;

        /// <summary>
        /// Occurs when a drag operation completes.
        /// </summary>
        public event EventHandler<TaskBarDragEventArgs>? DragCompleted;

        /// <summary>
        /// Occurs when a resize operation starts.
        /// </summary>
        public event EventHandler<TaskBarResizeEventArgs>? ResizeStarted;

        /// <summary>
        /// Occurs when a resize operation is in progress.
        /// </summary>
        public event EventHandler<TaskBarResizeEventArgs>? Resizing;

        /// <summary>
        /// Occurs when a resize operation completes.
        /// </summary>
        public event EventHandler<TaskBarResizeEventArgs>? ResizeCompleted;

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the row index of the task bar.
        /// </summary>
        public static readonly DependencyProperty RowIndexProperty = DependencyProperty.Register(
            nameof(RowIndex), typeof(int), typeof(GanttTaskBar), new PropertyMetadata(0));

        /// <summary>
        /// Gets or sets the time index of the task bar.
        /// </summary>
        public static readonly DependencyProperty TimeIndexProperty = DependencyProperty.Register(
            nameof(TimeIndex), typeof(int), typeof(GanttTaskBar), new PropertyMetadata(0));

        /// <summary>
        /// Gets or sets the custom text to display on the task bar.
        /// </summary>
        public static readonly DependencyProperty CustomTextProperty = DependencyProperty.Register(
            nameof(CustomText), typeof(string), typeof(GanttTaskBar), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets whether the task bar is interactive.
        /// </summary>
        public static readonly DependencyProperty IsInteractiveProperty = DependencyProperty.Register(
            nameof(IsInteractive), typeof(bool), typeof(GanttTaskBar), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets the progress percentage of the task (0-100).
        /// </summary>
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
            nameof(Progress), typeof(double), typeof(GanttTaskBar), new PropertyMetadata(0.0));

        /// <summary>
        /// Gets or sets the priority level of the task.
        /// </summary>
        public static readonly DependencyProperty PriorityProperty = DependencyProperty.Register(
            nameof(Priority), typeof(TaskPriority), typeof(GanttTaskBar), new PropertyMetadata(GPM.Gantt.Models.TaskPriority.Normal));

        /// <summary>
        /// Gets or sets the status of the task.
        /// </summary>
        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            nameof(Status), typeof(GPM.Gantt.Models.TaskStatus), typeof(GanttTaskBar), new PropertyMetadata(GPM.Gantt.Models.TaskStatus.NotStarted));

        /// <summary>
        /// Gets or sets whether drag and drop operations are enabled.
        /// </summary>
        public static readonly DependencyProperty IsDragDropEnabledProperty = DependencyProperty.Register(
            nameof(IsDragDropEnabled), typeof(bool), typeof(GanttTaskBar), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets whether resizing operations are enabled.
        /// </summary>
        public static readonly DependencyProperty IsResizeEnabledProperty = DependencyProperty.Register(
            nameof(IsResizeEnabled), typeof(bool), typeof(GanttTaskBar), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets whether the task bar is selected.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            nameof(IsSelected), typeof(bool), typeof(GanttTaskBar), new PropertyMetadata(false));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the row index of the task bar.
        /// </summary>
        public int RowIndex
        {
            get => (int)GetValue(RowIndexProperty);
            set => SetValue(RowIndexProperty, value);
        }

        /// <summary>
        /// Gets or sets the time index of the task bar.
        /// </summary>
        public int TimeIndex
        {
            get => (int)GetValue(TimeIndexProperty);
            set => SetValue(TimeIndexProperty, value);
        }

        /// <summary>
        /// Gets or sets the custom text to display on the task bar.
        /// </summary>
        public string CustomText
        {
            get => (string)GetValue(CustomTextProperty);
            set => SetValue(CustomTextProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the task bar is interactive.
        /// </summary>
        public bool IsInteractive
        {
            get => (bool)GetValue(IsInteractiveProperty);
            set => SetValue(IsInteractiveProperty, value);
        }

        /// <summary>
        /// Gets or sets the progress percentage of the task (0-100).
        /// </summary>
        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        /// <summary>
        /// Gets or sets the priority level of the task.
        /// </summary>
        public GPM.Gantt.Models.TaskPriority Priority
        {
            get => (GPM.Gantt.Models.TaskPriority)GetValue(PriorityProperty);
            set => SetValue(PriorityProperty, value);
        }

        /// <summary>
        /// Gets or sets the status of the task.
        /// </summary>
        public GPM.Gantt.Models.TaskStatus Status
        {
            get => (GPM.Gantt.Models.TaskStatus)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        /// <summary>
        /// Gets or sets whether drag and drop operations are enabled.
        /// </summary>
        public bool IsDragDropEnabled
        {
            get => (bool)GetValue(IsDragDropEnabledProperty);
            set => SetValue(IsDragDropEnabledProperty, value);
        }

        /// <summary>
        /// Gets or sets whether resizing operations are enabled.
        /// </summary>
        public bool IsResizeEnabled
        {
            get => (bool)GetValue(IsResizeEnabledProperty);
            set => SetValue(IsResizeEnabledProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the task bar is selected.
        /// </summary>
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        #endregion

        #region Private Fields

        private TextBlock? _textBlock;
        private Border? _progressIndicator;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the GanttTaskBar class.
        /// </summary>
        public GanttTaskBar()
        {
            // Enable GPU rendering by default
            EnableGpuRendering = true;
            
            // Create and configure the text block
            _textBlock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            // Create and configure the progress indicator
            _progressIndicator = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // Add children
            if (_progressIndicator != null)
            {
                AddVisualChild(_progressIndicator);
                AddLogicalChild(_progressIndicator);
            }
            
            if (_textBlock != null)
            {
                AddVisualChild(_textBlock);
                AddLogicalChild(_textBlock);
            }
        }

        #endregion

        #region Visual and Logical Children

        /// <summary>
        /// Gets the number of visual child elements within this element.
        /// </summary>
        protected override int VisualChildrenCount => (_textBlock != null ? 1 : 0) + (_progressIndicator != null ? 1 : 0);

        /// <summary>
        /// Overrides System.Windows.Media.Visual.GetVisualChild(System.Int32),
        /// and returns a child at the specified index from a collection of child elements.
        /// </summary>
        protected override Visual GetVisualChild(int index)
        {
            if (index == 0 && _progressIndicator != null)
                return _progressIndicator;
            if (index == 1 && _textBlock != null)
                return _textBlock;
            throw new System.ArgumentOutOfRangeException(nameof(index));
        }

        #endregion

        #region Layout Override

        /// <summary>
        /// Measures the size in layout required for child elements.
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            if (_textBlock != null)
            {
                _textBlock.Text = CustomText;
                _textBlock.Measure(constraint);
            }
            
            if (_progressIndicator != null)
            {
                _progressIndicator.Measure(constraint);
            }
            
            return new Size(0, 0); // Let parent determine size
        }

        /// <summary>
        /// Positions child elements and determines a size for a System.Windows.FrameworkElement.
        /// </summary>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_textBlock != null)
            {
                _textBlock.Text = CustomText;
                _textBlock.Arrange(new Rect(finalSize));
            }
            
            if (_progressIndicator != null)
            {
                // Update progress indicator width based on progress value
                var progressWidth = finalSize.Width * (Progress / 100.0);
                _progressIndicator.Width = progressWidth;
                _progressIndicator.Arrange(new Rect(0, 0, progressWidth, finalSize.Height));
            }
            
            return finalSize;
        }

        #endregion

        #region Rendering Override

        /// <summary>
        /// Render task bar using traditional WPF method
        /// </summary>
        /// <param name="drawingContext">Drawing context</param>
        protected override void RenderWithWpf(DrawingContext drawingContext)
        {
            // Use base class rendering method to render background
            base.RenderWithWpf(drawingContext);
        }

        #endregion

        #region Event Raising Methods

        /// <summary>
        /// Raises the TaskDoubleClicked event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnTaskDoubleClicked(TaskBarEventArgs e)
        {
            TaskDoubleClicked?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the DragStarted event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnDragStarted(TaskBarDragEventArgs e)
        {
            DragStarted?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the Dragging event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnDragging(TaskBarDragEventArgs e)
        {
            Dragging?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the DragCompleted event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnDragCompleted(TaskBarDragEventArgs e)
        {
            DragCompleted?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the ResizeStarted event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnResizeStarted(TaskBarResizeEventArgs e)
        {
            ResizeStarted?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the Resizing event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnResizing(TaskBarResizeEventArgs e)
        {
            Resizing?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the ResizeCompleted event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnResizeCompleted(TaskBarResizeEventArgs e)
        {
            ResizeCompleted?.Invoke(this, e);
        }

        #endregion
    }

    #region Event Args

    /// <summary>
    /// Event arguments for task bar events.
    /// </summary>
    public class TaskBarEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the position of the event.
        /// </summary>
        public Point Position { get; }

        /// <summary>
        /// Initializes a new instance of the TaskBarEventArgs class.
        /// </summary>
        /// <param name="position">The position of the event.</param>
        public TaskBarEventArgs(Point position)
        {
            Position = position;
        }
    }

    /// <summary>
    /// Event arguments for task bar drag events.
    /// </summary>
    public class TaskBarDragEventArgs : TaskBarEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the TaskBarDragEventArgs class.
        /// </summary>
        /// <param name="position">The position of the event.</param>
        public TaskBarDragEventArgs(Point position) : base(position)
        {
        }
    }

    /// <summary>
    /// Event arguments for task bar resize events.
    /// </summary>
    public class TaskBarResizeEventArgs : TaskBarEventArgs
    {
        /// <summary>
        /// Gets the resize direction.
        /// </summary>
        public ResizeDirection Direction { get; }

        /// <summary>
        /// Initializes a new instance of the TaskBarResizeEventArgs class.
        /// </summary>
        /// <param name="position">The position of the event.</param>
        /// <param name="direction">The resize direction.</param>
        public TaskBarResizeEventArgs(Point position, ResizeDirection direction) : base(position)
        {
            Direction = direction;
        }
    }

    #endregion
}