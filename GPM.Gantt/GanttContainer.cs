using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GPM.Gantt.Configuration;
using GPM.Gantt.Services;
using GPM.Gantt.Models;
using GPM.Gantt.Utilities;
using GPM.Gantt.Interaction;

namespace GPM.Gantt
{
    /// <summary>
    /// Main Gantt chart container that uses Grid layout to display tasks and timeline.
    /// Generates rows and columns based on time range and task count.
    /// </summary>
    public class GanttContainer : Grid
    {
        /// <summary>
        /// Dependency property for the start time of the timeline.
        /// </summary>
        public static readonly DependencyProperty StartTimeProperty = DependencyProperty.Register(
            nameof(StartTime), typeof(DateTime), typeof(GanttContainer), new PropertyMetadata(DateTime.Today, OnLayoutPropertyChanged));

        /// <summary>
        /// Dependency property for the end time of the timeline.
        /// </summary>
        public static readonly DependencyProperty EndTimeProperty = DependencyProperty.Register(
            nameof(EndTime), typeof(DateTime), typeof(GanttContainer), new PropertyMetadata(DateTime.Today.AddDays(6), OnLayoutPropertyChanged));

        /// <summary>
        /// Dependency property for the number of task rows to display.
        /// </summary>
        public static readonly DependencyProperty TaskCountProperty = DependencyProperty.Register(
            nameof(TaskCount), typeof(int), typeof(GanttContainer), new PropertyMetadata(3, OnLayoutPropertyChanged));

        /// <summary>
        /// Dependency property for showing grid cells instead of rows.
        /// </summary>
        public static readonly DependencyProperty ShowGridCellsProperty = DependencyProperty.Register(
            nameof(ShowGridCells), typeof(bool), typeof(GanttContainer), new PropertyMetadata(false, OnLayoutPropertyChanged));
        /// <summary>
        /// Dependency property to control whether tasks outside the visible row range are clamped into it
        /// (true) or skipped (false). Default is false (skip) to avoid overlap when TaskCount decreases.
        /// </summary>
        public static readonly DependencyProperty ClampTasksToVisibleRowsProperty = DependencyProperty.Register(
            nameof(ClampTasksToVisibleRows), typeof(bool), typeof(GanttContainer), new PropertyMetadata(false, OnLayoutPropertyChanged));
        /// <summary>
        /// Dependency property for the time scale unit (Hour, Day, Week, Month, Year).
        /// </summary>
        public static readonly DependencyProperty TimeUnitProperty = DependencyProperty.Register(
            nameof(TimeUnit), typeof(TimeUnit), typeof(GanttContainer), new PropertyMetadata(TimeUnit.Day, OnLayoutPropertyChanged));

        /// <summary>
        /// Dependency property for the header row height.
        /// </summary>
        public static readonly DependencyProperty HeaderRowHeightProperty = DependencyProperty.Register(
            nameof(HeaderRowHeight), typeof(GridLength), typeof(GanttContainer), new PropertyMetadata(GridLength.Auto, OnLayoutPropertyChanged));
        
        /// <summary>
        /// Dependency property for the task row height.
        /// </summary>
        public static readonly DependencyProperty TaskRowHeightProperty = DependencyProperty.Register(
            nameof(TaskRowHeight), typeof(GridLength), typeof(GanttContainer), new PropertyMetadata(GridLength.Auto, OnLayoutPropertyChanged));

        /// <summary>
        /// Dependency property for the collection of tasks to display.
        /// </summary>
        public static readonly DependencyProperty TasksProperty = DependencyProperty.Register(
            nameof(Tasks), typeof(ObservableCollection<GanttTask>), typeof(GanttContainer), new PropertyMetadata(null, OnTasksPropertyChanged));

        /// <summary>
        /// Dependency property for the configuration settings.
        /// </summary>
        public static readonly DependencyProperty ConfigurationProperty = DependencyProperty.Register(
            nameof(Configuration), typeof(GanttConfiguration), typeof(GanttContainer), new PropertyMetadata(GanttConfiguration.Default(), OnLayoutPropertyChanged));

        /// <summary>
        /// Dependency property for the date format string.
        /// </summary>
        public static readonly DependencyProperty DateFormatProperty = DependencyProperty.Register(
            nameof(DateFormat), typeof(string), typeof(GanttContainer), new PropertyMetadata("MMM dd", OnLayoutPropertyChanged));

        /// <summary>
        /// Dependency property for the time format string.
        /// </summary>
        public static readonly DependencyProperty TimeFormatProperty = DependencyProperty.Register(
            nameof(TimeFormat), typeof(string), typeof(GanttContainer), new PropertyMetadata("HH:mm", OnLayoutPropertyChanged));

        /// <summary>
        /// Dependency property for the culture used for time and date formatting.
        /// </summary>
        public static readonly DependencyProperty CultureProperty = DependencyProperty.Register(
            nameof(Culture), typeof(CultureInfo), typeof(GanttContainer), new PropertyMetadata(CultureInfo.CurrentCulture, OnLayoutPropertyChanged));

        /// <summary>
        /// Dependency property for enabling interactive features.
        /// </summary>
        public static readonly DependencyProperty IsInteractionEnabledProperty = DependencyProperty.Register(
            nameof(IsInteractionEnabled), typeof(bool), typeof(GanttContainer), new PropertyMetadata(true, OnInteractionPropertyChanged));

        /// <summary>
        /// Dependency property for enabling drag and drop operations.
        /// </summary>
        public static readonly DependencyProperty IsDragDropEnabledProperty = DependencyProperty.Register(
            nameof(IsDragDropEnabled), typeof(bool), typeof(GanttContainer), new PropertyMetadata(true, OnInteractionPropertyChanged));

        /// <summary>
        /// Dependency property for enabling task resizing operations.
        /// </summary>
        public static readonly DependencyProperty IsResizeEnabledProperty = DependencyProperty.Register(
            nameof(IsResizeEnabled), typeof(bool), typeof(GanttContainer), new PropertyMetadata(true, OnInteractionPropertyChanged));

        /// <summary>
        /// Dependency property for enabling multi-selection.
        /// </summary>
        public static readonly DependencyProperty IsMultiSelectionEnabledProperty = DependencyProperty.Register(
            nameof(IsMultiSelectionEnabled), typeof(bool), typeof(GanttContainer), new PropertyMetadata(true, OnInteractionPropertyChanged));

        /// <summary>
        /// Dependency property for the current theme.
        /// </summary>
        public static readonly DependencyProperty ThemeProperty = DependencyProperty.Register(
            nameof(Theme), typeof(GanttTheme), typeof(GanttContainer), new PropertyMetadata(null, OnThemePropertyChanged));

        /// <summary>
        /// Gets or sets the collection of tasks to be displayed in the Gantt chart.
        /// </summary>
        public ObservableCollection<GanttTask> Tasks
        {
            get => (ObservableCollection<GanttTask>)GetValue(TasksProperty);
            set => SetValue(TasksProperty, value);
        }

        /// <summary>
        /// Gets or sets the start time of the timeline.
        /// </summary>
        public DateTime StartTime
        {
            get => (DateTime)GetValue(StartTimeProperty);
            set => SetValue(StartTimeProperty, value);
        }

        /// <summary>
        /// Gets or sets the end time of the timeline.
        /// </summary>
        public DateTime EndTime
        {
            get => (DateTime)GetValue(EndTimeProperty);
            set => SetValue(EndTimeProperty, value);
        }

        /// <summary>
        /// Gets or sets the number of task rows to display.
        /// </summary>
        public int TaskCount
        {
            get => (int)GetValue(TaskCountProperty);
            set => SetValue(TaskCountProperty, value);
        }

        /// <summary>
        /// Gets or sets whether to show individual grid cells instead of full rows.
        /// </summary>
        public bool ShowGridCells
        {
            get => (bool)GetValue(ShowGridCellsProperty);
            set => SetValue(ShowGridCellsProperty, value);
        }
        /// <summary>
        /// Gets or sets whether tasks beyond the visible row range should be clamped into it (true)
        /// or skipped (false). Default is false.
        /// </summary>
        public bool ClampTasksToVisibleRows
        {
            get => (bool)GetValue(ClampTasksToVisibleRowsProperty);
            set => SetValue(ClampTasksToVisibleRowsProperty, value);
        }
        /// <summary>
        /// Gets or sets the time unit for the timeline scale.
        /// </summary>
        public TimeUnit TimeUnit
        {
            get => (TimeUnit)GetValue(TimeUnitProperty);
            set => SetValue(TimeUnitProperty, value);
        }

        /// <summary>
        /// Gets or sets the height of the header row.
        /// </summary>
        public GridLength HeaderRowHeight
        {
            get => (GridLength)GetValue(HeaderRowHeightProperty);
            set => SetValue(HeaderRowHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the height of task rows.
        /// </summary>
        public GridLength TaskRowHeight
        {
            get => (GridLength)GetValue(TaskRowHeightProperty);
            set => SetValue(TaskRowHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the configuration settings for the Gantt chart.
        /// </summary>
        public GanttConfiguration Configuration
        {
            get => (GanttConfiguration)GetValue(ConfigurationProperty);
            set => SetValue(ConfigurationProperty, value);
        }

        /// <summary>
        /// Gets or sets the date format string used for date display.
        /// </summary>
        public string DateFormat
        {
            get => (string)GetValue(DateFormatProperty);
            set => SetValue(DateFormatProperty, value);
        }

        /// <summary>
        /// Gets or sets the time format string used for time display.
        /// </summary>
        public string TimeFormat
        {
            get => (string)GetValue(TimeFormatProperty);
            set => SetValue(TimeFormatProperty, value);
        }

        /// <summary>
        /// Gets or sets the culture used for time and date formatting.
        /// </summary>
        public CultureInfo Culture
        {
            get => (CultureInfo)GetValue(CultureProperty);
            set => SetValue(CultureProperty, value);
        }

        /// <summary>
        /// Gets or sets whether interactive features are enabled.
        /// </summary>
        public bool IsInteractionEnabled
        {
            get => (bool)GetValue(IsInteractionEnabledProperty);
            set => SetValue(IsInteractionEnabledProperty, value);
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
        /// Gets or sets whether task resizing operations are enabled.
        /// </summary>
        public bool IsResizeEnabled
        {
            get => (bool)GetValue(IsResizeEnabledProperty);
            set => SetValue(IsResizeEnabledProperty, value);
        }

        /// <summary>
        /// Gets or sets whether multi-selection is enabled.
        /// </summary>
        public bool IsMultiSelectionEnabled
        {
            get => (bool)GetValue(IsMultiSelectionEnabledProperty);
            set => SetValue(IsMultiSelectionEnabledProperty, value);
        }

        /// <summary>
        /// Gets or sets the current theme for the Gantt chart.
        /// </summary>
        public GanttTheme? Theme
        {
            get => (GanttTheme?)GetValue(ThemeProperty);
            set => SetValue(ThemeProperty, value);
        }

        // Private fields for performance optimization
        private readonly Dictionary<string, UIElement> _elementCache = new();
        private readonly IValidationService _validationService = new ValidationService();
        private readonly GanttInteractionManager _interactionManager = new();
        private readonly IPerformanceService _performanceService = new PerformanceService();
        private readonly IVirtualizationService _virtualizationService = new VirtualizationService();
        private readonly ElementPool _elementPool = new();
        private bool _isLayoutInvalidated = true;
        private GanttTheme? _currentTheme;
        private List<DateTime>? _cachedTicks;
        private string? _lastTicksCacheKey;
        
        // Viewport tracking for virtualization
        private ScrollViewer? _parentScrollViewer;
        private Rect _currentViewport;
        private readonly List<UIElement> _visibleElements = new();
        private readonly Dictionary<int, double> _rowHeightCache = new();

        #region Events

        /// <summary>
        /// Occurs when a task is selected or deselected.
        /// </summary>
        public event EventHandler<Interaction.SelectionChangedEventArgs>? TaskSelectionChanged;

        /// <summary>
        /// Occurs when a task is moved via drag and drop.
        /// </summary>
        public event EventHandler<TaskMovedEventArgs>? TaskMoved;

        /// <summary>
        /// Occurs when a task is resized.
        /// </summary>
        public event EventHandler<TaskResizedEventArgs>? TaskResized;

        /// <summary>
        /// Occurs when a task edit is requested (double-click).
        /// </summary>
        public event EventHandler<TaskEditRequestedEventArgs>? TaskEditRequested;

        #endregion

        private static void OnThemePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GanttContainer gc)
            {
                gc.ApplyTheme(e.NewValue as GanttTheme);
            }
        }

        private void ApplyTheme(GanttTheme? theme)
        {
            var newTheme = theme ?? ThemeManager.GetCurrentTheme();
            
            // Skip if theme hasn't changed
            if (_currentTheme != null && _currentTheme.Name == newTheme.Name)
            {
                return;
            }
            
            using var measurement = _performanceService.BeginMeasurement("ThemeApplication");
            
            _currentTheme = newTheme;
            
            // Apply theme to the container
            ThemeUtilities.ApplyTheme(this, _currentTheme);
            
            // Debounce layout refresh to avoid excessive rebuilds during theme changes
            var delay = Configuration?.Rendering?.LayoutDebounceDelay ?? 150;
            _performanceService.DebounceOperation(() =>
            {
                if (IsLoaded)
                {
                    _isLayoutInvalidated = true;
                    BuildLayoutImmediate();
                }
            }, delay, "theme-layout");
        }

        public GanttContainer()
        {
            // Initialize default theme if none is set
            if (Theme == null)
            {
                Theme = ThemeManager.GetCurrentTheme();
            }
            
            // Initialize tasks collection immediately with proper change tracking
            var tasksCollection = new ObservableCollection<GanttTask>();
            tasksCollection.CollectionChanged += Tasks_CollectionChanged;
            Tasks = tasksCollection;
            
            // Initialize interaction manager
            SetupInteractionManager();
            
            Loaded += OnGanttContainerLoaded;
            SizeChanged += (s, e) => 
            {
                if (IsLoaded)
                {
                    _isLayoutInvalidated = true; // ensure layout rebuilds on size changes
                    BuildLayout();
                }
            };
            
            // Subscribe to global theme changes
            ThemeManager.ThemeChanged += OnGlobalThemeChanged;
            
            System.Diagnostics.Debug.WriteLine("GanttContainer constructor completed");
        }
        
        private void OnGlobalThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            // Apply the new global theme if no specific theme is set for this container
            if (Theme == null)
            {
                ApplyTheme(e.CurrentTheme);
            }
        }
        
        private void SetupInteractionManager()
        {
            _interactionManager.ValidationService = _validationService;
            
            // Subscribe to interaction events
            _interactionManager.SelectionChanged += (s, e) => TaskSelectionChanged?.Invoke(this, e);
            _interactionManager.TaskMoved += (s, e) => OnTaskMoved(e);
            _interactionManager.TaskResized += (s, e) => OnTaskResized(e);
            _interactionManager.TaskEditRequested += (s, e) => TaskEditRequested?.Invoke(this, e);
            _interactionManager.ValidatingDrag += OnValidatingDrag;
            
            // Configure interaction settings
            UpdateInteractionSettings();
        }
        
        private void OnGanttContainerLoaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("GanttContainer loaded event fired");
            
            // Ensure we have a valid tasks collection
            if (Tasks == null)
            {
                var tasksCollection = new ObservableCollection<GanttTask>();
                tasksCollection.CollectionChanged += Tasks_CollectionChanged;
                Tasks = tasksCollection;
                System.Diagnostics.Debug.WriteLine("Created tasks collection in Loaded event");
            }
            
            // Initialize interaction manager with this container
            _interactionManager.Initialize(this);
            
            // Build layout now that the control is fully loaded
            _isLayoutInvalidated = true;
            BuildLayout();
        }

        private static void OnTasksPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GanttContainer gc)
            {
                System.Diagnostics.Debug.WriteLine($"Tasks property changed: OldValue={e.OldValue?.GetType()}, NewValue={e.NewValue?.GetType()}");
                
                // Unsubscribe from old collection
                if (e.OldValue is ObservableCollection<GanttTask> oldCol)
                {
                    oldCol.CollectionChanged -= gc.Tasks_CollectionChanged;
                }
                
                // Subscribe to new collection
                if (e.NewValue is ObservableCollection<GanttTask> newCol)
                {
                    newCol.CollectionChanged += gc.Tasks_CollectionChanged;
                    System.Diagnostics.Debug.WriteLine($"Wired up collection changed handler for {newCol.Count} tasks");
                }
                
                // Only build layout if the control is loaded
                if (gc.IsLoaded)
                {
                    gc._isLayoutInvalidated = true; // mark invalid before rebuilding
                    gc.BuildLayout();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Control not loaded yet, deferring layout build");
                }
            }
        }

        private void Tasks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Tasks collection changed: Action={e.Action}, NewItems={e.NewItems?.Count ?? 0}");
            
            // Only build layout if the control is loaded
            if (IsLoaded)
            {
                _isLayoutInvalidated = true; // mark invalid before rebuilding
                BuildLayout();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Control not loaded yet, deferring layout build");
            }
        }

        private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GanttContainer gc && gc.IsLoaded)
            {
                gc._isLayoutInvalidated = true; // mark invalid before rebuilding
                gc.BuildLayout();
            }
        }

        private static void OnInteractionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GanttContainer gc)
            {
                gc.UpdateInteractionSettings();
            }
        }

        private void UpdateInteractionSettings()
        {
            _interactionManager.IsInteractionEnabled = IsInteractionEnabled;
            _interactionManager.IsDragDropEnabled = IsDragDropEnabled;
            _interactionManager.IsResizeEnabled = IsResizeEnabled;
            _interactionManager.IsMultiSelectionEnabled = IsMultiSelectionEnabled;
        }

        private void BuildLayout()
        {
            // Only build if needed and loaded
            if (!_isLayoutInvalidated || !IsLoaded)
            {
                return;
            }
            
            // Use debouncing for performance
            var delay = Configuration?.Rendering?.LayoutDebounceDelay ?? 150;
            _performanceService.DebounceOperation(BuildLayoutImmediate, delay, "layout");
        }

        private void BuildLayoutImmediate()
        {
            using var measurement = _performanceService.BeginMeasurement("LayoutBuild");
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"BuildLayoutImmediate called: StartTime={StartTime}, EndTime={EndTime}, TaskCount={TaskCount}");
                
                // Check if virtualization should be enabled
                var renderingConfig = Configuration?.Rendering ?? new RenderingConfiguration();
                var shouldVirtualize = renderingConfig.EnableVirtualization && 
                                      _virtualizationService.ShouldVirtualize(TaskCount, renderingConfig.MaxVisibleTasks);
                
                // Cleanup and recycle existing elements for efficient memory usage
                CleanupAndRecycleElements();
                
                // Clear existing layout
                Children.Clear();
                RowDefinitions.Clear();
                ColumnDefinitions.Clear();
                
                // Use cached ticks when possible
                var ticks = GetCachedTimelineTicks();
                int columns = Math.Max(1, ticks.Count);
                int rows = Math.Max(1, TaskCount + 1); // First row for time headers, subsequent rows for tasks

                System.Diagnostics.Debug.WriteLine($"Creating grid: {rows} rows x {columns} columns (Virtualized: {shouldVirtualize})");

                // Create column definitions
                for (int c = 0; c < columns; c++)
                    ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // Create row definitions
                
                for (int r = 0; r < rows; r++)
                {
                    var height = (r == 0) ? HeaderRowHeight : TaskRowHeight;
                    RowDefinitions.Add(new RowDefinition { Height = height });
                }

                // Build UI elements based on performance level
                if (shouldVirtualize)
                {
                    BuildVirtualizedLayout(ticks, columns, rows, renderingConfig);
                }
                else
                {
                    BuildStandardLayout(ticks, columns, rows);
                }

                _isLayoutInvalidated = false;
                System.Diagnostics.Debug.WriteLine($"Layout complete: {Children.Count} total children added");
                
                // Optimize memory if auto-optimization is enabled
                if (renderingConfig.EnableAutoMemoryOptimization)
                {
                    _performanceService.OptimizeMemoryUsage();
                }
            }
            catch (Exception ex)
            {
                // Add proper error handling
                System.Diagnostics.Debug.WriteLine($"Layout building error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private List<DateTime> GetCachedTimelineTicks()
        {
            var cacheKey = $"{StartTime:yyyy-MM-dd HH:mm:ss}|{EndTime:yyyy-MM-dd HH:mm:ss}|{TimeUnit}|{Culture?.Name ?? "current"}";
            
            if (_lastTicksCacheKey == cacheKey && _cachedTicks != null)
            {
                return _cachedTicks;
            }
            
            _cachedTicks = _performanceService.GetCachedTimelineTicks(StartTime, EndTime, TimeUnit, Culture);
            _lastTicksCacheKey = cacheKey;
            
            return _cachedTicks;
        }

        private void BuildVirtualizedLayout(List<DateTime> ticks, int columns, int rows, RenderingConfiguration renderingConfig)
        {
            var performanceLevel = renderingConfig.PerformanceLevel;
            
            // Initialize viewport tracking
            InitializeViewportTracking();
            
            // Create time headers (always visible)
            BuildTimeHeaders(ticks, columns, performanceLevel);
            
            // Determine visible range based on viewport
            var visibleRange = CalculateVisibleRange(rows);
            
            // Create grid elements only for visible range
            BuildVirtualizedGrid(ticks, columns, visibleRange, performanceLevel);
            
            // Render tasks with viewport culling
            BuildVirtualizedTaskBars(ticks, visibleRange, performanceLevel);
        }

        private void BuildStandardLayout(List<DateTime> ticks, int columns, int rows)
        {
            var performanceLevel = Configuration?.Rendering?.PerformanceLevel ?? PerformanceLevel.Balanced;
            
            // Create time headers
            for (int c = 0; c < columns; c++)
            {
                var dt = ticks[c];
                var timeCell = CreateTimeCell(c, 0, dt, performanceLevel);
                SetRow(timeCell, 0);
                SetColumn(timeCell, c);
                Children.Add(timeCell);
            }

            // Create grid
            BuildStandardGrid(ticks, columns, rows);

            // Render tasks
            BuildTaskBars(ticks, performanceLevel);
        }

        private GanttTimeCell CreateTimeCell(int timeIndex, int rowIndex, DateTime dateTime, PerformanceLevel performanceLevel)
        {
            return new GanttTimeCell
            {
                TimeIndex = timeIndex,
                RowIndex = rowIndex,
                TimeText = TimelineCalculator.FormatTick(dateTime, TimeUnit, DateFormat, TimeFormat, Culture),
                IsWeekend = TimelineCalculator.IsWeekend(dateTime),
                IsToday = TimelineCalculator.IsToday(dateTime)
            };
        }

        private void BuildStandardGrid(List<DateTime> ticks, int columns, int rows)
        {
            // Other rows: grid rows or grid cells
            for (int r = 1; r < rows; r++)
            {
                if (!ShowGridCells)
                {
                    var row = new GanttGridRow { RowIndex = r };
                    SetRow(row, r);
                    SetColumn(row, 0);
                    SetColumnSpan(row, columns);
                    Children.Add(row);
                }
                else
                {
                    for (int c = 0; c < columns; c++)
                    {
                        var dt = ticks[c];
                        var cell = new GanttGridCell
                        {
                            RowIndex = r,
                            TimeIndex = c,
                            IsWeekend = TimelineCalculator.IsWeekend(dt),
                            IsToday = TimelineCalculator.IsToday(dt)
                        };
                        SetRow(cell, r);
                        SetColumn(cell, c);
                        Children.Add(cell);
                    }
                }
            }
        }

        private void BuildTaskBars(List<DateTime> ticks, PerformanceLevel performanceLevel)
        {
            // Ensure minimum viable layout even with no tasks
            if (Tasks == null || Tasks.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No tasks found, creating minimal layout for visual testing");
                
                // Add a simple visual indicator
                var placeholder = new TextBlock
                {
                    Text = "Gantt Chart - No tasks to display",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    Foreground = Brushes.Gray
                };
                SetRow(placeholder, Math.Max(1, RowDefinitions.Count / 2));
                SetColumn(placeholder, 0);
                SetColumnSpan(placeholder, ColumnDefinitions.Count);
                Children.Add(placeholder);
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Processing {Tasks.Count} tasks");
            
            foreach (var task in Tasks)
            {
                if (task == null) continue;

                System.Diagnostics.Debug.WriteLine($"Task: {task.Title}, Start: {task.Start}, End: {task.End}, RowIndex: {task.RowIndex}");

                // Skip tasks completely outside the visible time range
                if (task.End < StartTime || task.Start > EndTime)
                {
                    System.Diagnostics.Debug.WriteLine($"Skipping task {task.Title} - outside time range");
                    continue;
                }
                
                // Decide row index with optional clamping behavior
                int rowIndex;
                if (ClampTasksToVisibleRows)
                {
                    // Clamp into visible range (1..TaskCount) when enabled
                    rowIndex = Math.Max(1, Math.Min(TaskCount, task.RowIndex));
                    System.Diagnostics.Debug.WriteLine($"Clamping task {task.Title} to row {rowIndex} within 1..{TaskCount}");
                }
                else
                {
                    // Skip out-of-range rows to avoid overlapping
                    if (task.RowIndex < 1 || task.RowIndex > TaskCount)
                    {
                        System.Diagnostics.Debug.WriteLine($"Skipping task {task.Title} - row {task.RowIndex} is outside visible range 1..{TaskCount}");
                        continue;
                    }
                    rowIndex = task.RowIndex;
                }

                // Calculate task span using timeline helper
                var (startIndex, columnSpan) = TimelineHelper.CalculateTaskSpan(ticks, task.Start, task.End, TimeUnit);

                System.Diagnostics.Debug.WriteLine($"Task {task.Title}: startIndex={startIndex}, columnSpan={columnSpan}, rowIndex={rowIndex}");

                var taskBar = CreateTaskBarForTask(task, rowIndex, startIndex, columnSpan);

                SetRow(taskBar, rowIndex);
                SetColumn(taskBar, startIndex);
                SetColumnSpan(taskBar, columnSpan);
                Panel.SetZIndex(taskBar, 10); // Place above grid lines
                Children.Add(taskBar);
                
                // Register with interaction manager after adding to visual tree
                if (IsLoaded)
                {
                    _interactionManager.RegisterTaskBar(taskBar);
                }
            }
        }

        /// <summary>
        /// Creates a task bar for the given task with appropriate shape and configuration.
        /// </summary>
        private GanttTaskBar CreateTaskBarForTask(GanttTask task, int rowIndex, int startIndex, int columnSpan)
        {
            GanttTaskBar taskBar;
            
            // Use enhanced task bar if shape rendering is enabled
            if (Configuration?.Rendering?.UseEnhancedShapeRendering == true && task.Shape != TaskBarShape.Rectangle)
            {
                var enhancedTaskBar = new EnhancedGanttTaskBar
                {
                    Shape = task.Shape,
                    ShapeParameters = task.ShapeParameters ?? Configuration.Rendering.DefaultShapeParameters,
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
            taskBar.IsInteractive = IsInteractionEnabled;
            taskBar.Progress = task.Progress;
            taskBar.Priority = task.Priority;
            taskBar.Status = task.Status;
            taskBar.IsDragDropEnabled = IsDragDropEnabled;
            taskBar.IsResizeEnabled = IsResizeEnabled;
            
            return taskBar;
        }
        
        /// <summary>
        /// Configures a task bar (from pool or newly created) with task data and shape settings.
        /// </summary>
        private void ConfigureTaskBar(GanttTaskBar taskBar, GanttTask task, int rowIndex, int startIndex, int columnSpan)
        {
            // Handle enhanced task bar with shape support
            if (taskBar is EnhancedGanttTaskBar enhancedTaskBar && 
                Configuration?.Rendering?.UseEnhancedShapeRendering == true && 
                task.Shape != TaskBarShape.Rectangle)
            {
                enhancedTaskBar.Shape = task.Shape;
                enhancedTaskBar.ShapeParameters = task.ShapeParameters ?? Configuration.Rendering.DefaultShapeParameters;
                enhancedTaskBar.UseLegacyRendering = false;
            }
            
            // Configure common properties
            taskBar.RowIndex = rowIndex;
            taskBar.TimeIndex = startIndex;
            taskBar.CustomText = task.Title;
            taskBar.IsInteractive = IsInteractionEnabled;
            taskBar.Progress = task.Progress;
            taskBar.Priority = task.Priority;
            taskBar.Status = task.Status;
            taskBar.IsDragDropEnabled = IsDragDropEnabled;
            taskBar.IsResizeEnabled = IsResizeEnabled;
        }

        private void OnTaskMoved(TaskMovedEventArgs e)
        {
            // Update the underlying task model
            if (Tasks != null)
            {
                var taskToUpdate = Tasks.FirstOrDefault(t => t.RowIndex == e.OriginalRowIndex);
                if (taskToUpdate != null)
                {
                    taskToUpdate.RowIndex = e.NewRowIndex;
                    taskToUpdate.Start = e.NewStartTime;
                    taskToUpdate.End = e.NewEndTime;
                    
                    // Trigger layout rebuild to reflect changes
                    _isLayoutInvalidated = true;
                    BuildLayout();
                }
            }
            
            TaskMoved?.Invoke(this, e);
        }

        private void OnTaskResized(TaskResizedEventArgs e)
        {
            // Update the underlying task model
            if (Tasks != null)
            {
                var taskToUpdate = Tasks.FirstOrDefault(t => t.Start == e.OriginalStartTime && t.End == e.OriginalEndTime);
                if (taskToUpdate != null)
                {
                    taskToUpdate.Start = e.NewStartTime;
                    taskToUpdate.End = e.NewEndTime;
                    
                    // Trigger layout rebuild to reflect changes
                    _isLayoutInvalidated = true;
                    BuildLayout();
                }
            }
            
            TaskResized?.Invoke(this, e);
        }

        private void OnValidatingDrag(object? sender, DragValidationEventArgs e)
        {
            // Implement custom validation logic here
            // For now, allow all moves within bounds
            e.IsValidDrop = e.NewRowIndex >= 1 && e.NewRowIndex <= TaskCount && e.NewTimeIndex >= 0;
            
            if (!e.IsValidDrop)
            {
                e.ValidationMessage = "Cannot drop task outside valid bounds";
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the currently selected task bars.
        /// </summary>
        /// <returns>Collection of selected task bars.</returns>
        public IEnumerable<GanttTaskBar> GetSelectedTasks()
        {
            return _interactionManager.SelectionManager.SelectedTasks;
        }

        /// <summary>
        /// Selects the specified task bar.
        /// </summary>
        /// <param name="taskBar">The task bar to select.</param>
        /// <param name="addToSelection">Whether to add to current selection.</param>
        public void SelectTask(GanttTaskBar taskBar, bool addToSelection = false)
        {
            _interactionManager.SelectTask(taskBar, addToSelection);
        }

        /// <summary>
        /// Clears all task selections.
        /// </summary>
        public void ClearSelection()
        {
            _interactionManager.ClearSelection();
        }

        /// <summary>
        /// Selects all task bars.
        /// </summary>
        public void SelectAllTasks()
        {
            _interactionManager.SelectAll();
        }

        #endregion
        
        #region UI Virtualization and Optimization
        
        /// <summary>
        /// Initializes viewport tracking for efficient rendering.
        /// </summary>
        private void InitializeViewportTracking()
        {
            if (_parentScrollViewer == null)
            {
                _parentScrollViewer = FindParentScrollViewer(this);
                if (_parentScrollViewer != null)
                {
                    _parentScrollViewer.ScrollChanged += OnParentScrollChanged;
                }
            }
        }
        
        /// <summary>
        /// Finds the parent ScrollViewer for viewport calculations.
        /// </summary>
        private static ScrollViewer? FindParentScrollViewer(DependencyObject child)
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is ScrollViewer scrollViewer)
                    return scrollViewer;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
        
        /// <summary>
        /// Handles scroll changes to update viewport.
        /// </summary>
        private void OnParentScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateViewport();
            
            // Rebuild only if significant viewport change
            if (IsSignificantViewportChange(e))
            {
                _isLayoutInvalidated = true;
                BuildLayout();
            }
        }
        
        /// <summary>
        /// Updates the current viewport rectangle.
        /// </summary>
        private void UpdateViewport()
        {
            if (_parentScrollViewer != null)
            {
                _currentViewport = new Rect(
                    _parentScrollViewer.HorizontalOffset,
                    _parentScrollViewer.VerticalOffset,
                    _parentScrollViewer.ViewportWidth,
                    _parentScrollViewer.ViewportHeight
                );
            }
        }
        
        /// <summary>
        /// Determines if viewport change requires layout rebuild.
        /// </summary>
        private bool IsSignificantViewportChange(ScrollChangedEventArgs e)
        {
            const double threshold = 50.0; // pixels
            return Math.Abs(e.VerticalChange) > threshold || Math.Abs(e.HorizontalChange) > threshold;
        }
        
        /// <summary>
        /// Calculates visible row range based on viewport.
        /// </summary>
        private (int startRow, int endRow) CalculateVisibleRange(int totalRows)
        {
            if (_parentScrollViewer == null)
                return (1, totalRows);
            
            var rowHeight = GetEstimatedRowHeight();
            var startRow = Math.Max(1, (int)(_currentViewport.Y / rowHeight));
            var endRow = Math.Min(totalRows, startRow + (int)(_currentViewport.Height / rowHeight) + 2); // +2 for buffer
            
            return (startRow, endRow);
        }
        
        /// <summary>
        /// Gets estimated row height for viewport calculations.
        /// </summary>
        private double GetEstimatedRowHeight()
        {
            const double defaultHeight = 40.0;
            
            if (TaskRowHeight.GridUnitType == GridUnitType.Pixel)
                return TaskRowHeight.Value;
            
            // Use cached value if available
            if (_rowHeightCache.TryGetValue(0, out var cachedHeight))
                return cachedHeight;
            
            return defaultHeight;
        }
        
        /// <summary>
        /// Builds time headers with performance optimizations.
        /// </summary>
        private void BuildTimeHeaders(List<DateTime> ticks, int columns, PerformanceLevel performanceLevel)
        {
            using var measurement = _performanceService.BeginMeasurement("TimeHeadersBuild");
            
            for (int c = 0; c < columns; c++)
            {
                var dt = ticks[c];
                var timeCell = _elementPool.GetOrCreateTimeCell();
                
                timeCell.TimeIndex = c;
                timeCell.RowIndex = 0;
                timeCell.TimeText = TimelineCalculator.FormatTick(dt, TimeUnit, DateFormat, TimeFormat, Culture);
                timeCell.IsWeekend = TimelineCalculator.IsWeekend(dt);
                timeCell.IsToday = TimelineCalculator.IsToday(dt);
                
                SetRow(timeCell, 0);
                SetColumn(timeCell, c);
                Children.Add(timeCell);
                _visibleElements.Add(timeCell);
            }
        }
        
        /// <summary>
        /// Builds virtualized grid elements for visible range only.
        /// </summary>
        private void BuildVirtualizedGrid(List<DateTime> ticks, int columns, (int startRow, int endRow) visibleRange, PerformanceLevel performanceLevel)
        {
            using var measurement = _performanceService.BeginMeasurement("VirtualizedGridBuild");
            
            var (startRow, endRow) = visibleRange;
            
            for (int r = startRow; r <= endRow; r++)
            {
                if (!ShowGridCells)
                {
                    var row = _elementPool.GetOrCreateGridRow();
                    row.RowIndex = r;
                    SetRow(row, r);
                    SetColumn(row, 0);
                    SetColumnSpan(row, columns);
                    Children.Add(row);
                    _visibleElements.Add(row);
                }
                else
                {
                    for (int c = 0; c < columns; c++)
                    {
                        var dt = ticks[c];
                        var cell = _elementPool.GetOrCreateGridCell();
                        cell.RowIndex = r;
                        cell.TimeIndex = c;
                        cell.IsWeekend = TimelineCalculator.IsWeekend(dt);
                        cell.IsToday = TimelineCalculator.IsToday(dt);
                        
                        SetRow(cell, r);
                        SetColumn(cell, c);
                        Children.Add(cell);
                        _visibleElements.Add(cell);
                    }
                }
            }
        }
        
        /// <summary>
        /// Builds task bars with viewport culling for performance.
        /// </summary>
        private void BuildVirtualizedTaskBars(List<DateTime> ticks, (int startRow, int endRow) visibleRange, PerformanceLevel performanceLevel)
        {
            using var measurement = _performanceService.BeginMeasurement("VirtualizedTaskBarsBuild");
            
            if (Tasks == null || Tasks.Count == 0)
                return;
            
            var (startRow, endRow) = visibleRange;
            
            // Filter tasks within visible range
            var visibleTasks = Tasks.Where(task => 
                task != null &&
                task.RowIndex >= startRow && 
                task.RowIndex <= endRow &&
                task.End >= StartTime && 
                task.Start <= EndTime
            ).ToList();
            
            foreach (var task in visibleTasks)
            {
                var (startIndex, columnSpan) = TimelineHelper.CalculateTaskSpan(ticks, task.Start, task.End, TimeUnit);
                
                var taskBar = _elementPool.GetOrCreateTaskBar();
                
                // Configure task bar using our factory method logic
                ConfigureTaskBar(taskBar, task, task.RowIndex, startIndex, columnSpan);
                
                SetRow(taskBar, task.RowIndex);
                SetColumn(taskBar, startIndex);
                SetColumnSpan(taskBar, columnSpan);
                Panel.SetZIndex(taskBar, 10);
                Children.Add(taskBar);
                _visibleElements.Add(taskBar);
                
                if (IsLoaded)
                {
                    _interactionManager.RegisterTaskBar(taskBar);
                }
            }
        }
        
        /// <summary>
        /// Cleans up and recycles UI elements for efficient memory usage.
        /// </summary>
        private void CleanupAndRecycleElements()
        {
            // Return visible elements to pool
            foreach (var element in _visibleElements)
            {
                if (element is GanttTimeCell timeCell)
                    _elementPool.ReturnTimeCell(timeCell);
                else if (element is GanttGridRow gridRow)
                    _elementPool.ReturnGridRow(gridRow);
                else if (element is GanttGridCell gridCell)
                    _elementPool.ReturnGridCell(gridCell);
                else if (element is GanttTaskBar taskBar)
                    _elementPool.ReturnTaskBar(taskBar);
            }
            
            _visibleElements.Clear();
        }
        
        #endregion
     }
}