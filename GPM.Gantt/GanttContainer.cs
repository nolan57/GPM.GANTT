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
using GPM.Gantt.Models.Templates;
using GPM.Gantt.Utilities;
using GPM.Gantt.Interaction;
using GPM.Gantt.Rendering;
using GPM.Gantt.Models.Calendar;
using GPM.Gantt.Layout;
using GPM.Gantt.TaskManagement;
using GPM.Gantt.Dependency;
using GPM.Gantt.Theme;
using GPM.Gantt.Export;
using GPM.Gantt.Template;

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
        /// Dependency property for showing dependency lines.
        /// </summary>
        public static readonly DependencyProperty ShowDependencyLinesProperty = DependencyProperty.Register(
            nameof(ShowDependencyLines), typeof(bool), typeof(GanttContainer), new PropertyMetadata(true, OnLayoutPropertyChanged));

        /// <summary>
        /// Dependency property for the collection of task dependencies.
        /// </summary>
        public static readonly DependencyProperty DependenciesProperty = DependencyProperty.Register(
            nameof(Dependencies), typeof(ObservableCollection<TaskDependency>), typeof(GanttContainer), new PropertyMetadata(null, OnDependenciesPropertyChanged));

        /// <summary>
        /// Dependency property for highlighting critical path.
        /// </summary>
        public static readonly DependencyProperty HighlightCriticalPathProperty = DependencyProperty.Register(
            nameof(HighlightCriticalPath), typeof(bool), typeof(GanttContainer), new PropertyMetadata(true, OnLayoutPropertyChanged));

        /// <summary>
        /// Dependency property for the calendar service.
        /// </summary>
        public static readonly DependencyProperty CalendarServiceProperty = DependencyProperty.Register(
            nameof(CalendarService), typeof(ICalendarService), typeof(GanttContainer), new PropertyMetadata(null));

        /// <summary>
        /// Dependency property for the dependency service.
        /// </summary>
        public static readonly DependencyProperty DependencyServiceProperty = DependencyProperty.Register(
            nameof(DependencyService), typeof(IDependencyService), typeof(GanttContainer), new PropertyMetadata(null));

        /// <summary>
        /// Dependency property for the export service.
        /// </summary>
        public static readonly DependencyProperty ExportServiceProperty = DependencyProperty.Register(
            nameof(ExportService), typeof(IExportService), typeof(GanttContainer), new PropertyMetadata(null));

        /// <summary>
        /// Dependency property for the template service.
        /// </summary>
        public static readonly DependencyProperty TemplateServiceProperty = DependencyProperty.Register(
            nameof(TemplateService), typeof(ITemplateService), typeof(GanttContainer), new PropertyMetadata(null));

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

        /// <summary>
        /// Gets or sets whether to show dependency lines between tasks.
        /// </summary>
        public bool ShowDependencyLines
        {
            get => (bool)GetValue(ShowDependencyLinesProperty);
            set => SetValue(ShowDependencyLinesProperty, value);
        }

        /// <summary>
        /// Gets or sets the collection of task dependencies.
        /// </summary>
        public ObservableCollection<TaskDependency>? Dependencies
        {
            get => (ObservableCollection<TaskDependency>?)GetValue(DependenciesProperty);
            set => SetValue(DependenciesProperty, value);
        }

        /// <summary>
        /// Gets or sets whether to highlight the critical path.
        /// </summary>
        public bool HighlightCriticalPath
        {
            get => (bool)GetValue(HighlightCriticalPathProperty);
            set => SetValue(HighlightCriticalPathProperty, value);
        }

        /// <summary>
        /// Gets or sets the calendar service for working time calculations.
        /// </summary>
        public ICalendarService? CalendarService
        {
            get => (ICalendarService?)GetValue(CalendarServiceProperty);
            set => SetValue(CalendarServiceProperty, value);
        }

        /// <summary>
        /// Gets or sets the dependency service for managing task relationships.
        /// </summary>
        public IDependencyService? DependencyService
        {
            get => (IDependencyService?)GetValue(DependencyServiceProperty);
            set => SetValue(DependencyServiceProperty, value);
        }

        /// <summary>
        /// Gets or sets the export service for exporting charts.
        /// </summary>
        public IExportService? ExportService
        {
            get => (IExportService?)GetValue(ExportServiceProperty);
            set => SetValue(ExportServiceProperty, value);
        }

        /// <summary>
        /// Gets or sets the template service for managing project templates.
        /// </summary>
        public ITemplateService? TemplateService
        {
            get => (ITemplateService?)GetValue(TemplateServiceProperty);
            set => SetValue(TemplateServiceProperty, value);
        }

        // Private fields for performance optimization
        private readonly Dictionary<string, UIElement> _elementCache = new();
        private readonly IValidationService _validationService = new ValidationService();
        private readonly GanttInteractionManager _interactionManager = new();
        private readonly IPerformanceService _performanceService = new PerformanceService();
        private readonly IVirtualizationService _virtualizationService = new VirtualizationService();
        private readonly ElementPool _elementPool = new();
        private readonly IGpuRenderingService _gpuRenderingService; // 添加GPU渲染服务
        private bool _isLayoutInvalidated = true;
        private GanttTheme? _currentTheme;
        private List<DateTime>? _cachedTicks;
        private string? _lastTicksCacheKey;
        
        // Viewport tracking for virtualization
        private ScrollViewer? _parentScrollViewer;
        private Rect _currentViewport;
        private readonly List<UIElement> _visibleElements = new();
        private readonly Dictionary<int, double> _rowHeightCache = new();

        private void LogState(string source)
        {
            try
            {
                // Ensure a file trace listener exists (lazy init)
                var listenerName = "GanttContainerFile";
                if (System.Diagnostics.Trace.Listeners[listenerName] == null)
                {
                    var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GanttContainer.log");
                    try
                    {
                        var fileListener = new System.Diagnostics.TextWriterTraceListener(logPath, listenerName);
                        System.Diagnostics.Trace.Listeners.Add(fileListener);
                        System.Diagnostics.Trace.AutoFlush = true;
                    }
                    catch { }
                }

                var msg = $"[{DateTime.Now:HH:mm:ss.fff}] {source} | IsLoaded={IsLoaded}, _isLayoutInvalidated={_isLayoutInvalidated}, Children={Children?.Count}, Rows={RowDefinitions?.Count}, Cols={ColumnDefinitions?.Count}";
                System.Diagnostics.Debug.WriteLine(msg);
                System.Diagnostics.Trace.WriteLine(msg);
            }
            catch { }
        }

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
            
            // Apply theme to existing child elements
            ApplyThemeToChildren();
            
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
            
            // 初始化GPU渲染服务
            _gpuRenderingService = InitializeGpuRenderingService();
            
            // Initialize tasks collection immediately with proper change tracking
            var tasksCollection = new ObservableCollection<GanttTask>();
            tasksCollection.CollectionChanged += Tasks_CollectionChanged;
            Tasks = tasksCollection;
            
            // Initialize interaction manager
            SetupInteractionManager();
            
            Loaded += OnGanttContainerLoaded;
            Unloaded += OnGanttContainerUnloaded; // add unloaded for lifecycle logging
            SizeChanged += (s, e) => 
            {
                if (IsLoaded)
                {
                    _isLayoutInvalidated = true; // ensure layout rebuilds on size changes
                    LogState("SizeChanged -> set invalidated");
                    BuildLayout();
                }
            };
            
            // Subscribe to global theme changes
            ThemeManager.ThemeChanged += OnGlobalThemeChanged;
            
            System.Diagnostics.Debug.WriteLine("GanttContainer constructor completed");
            LogState("Constructor completed");
        }
        
        private void OnGanttContainerUnloaded(object sender, RoutedEventArgs e)
        {
            LogState("Unloaded event - begin cleanup");
            try
            {
                if (_parentScrollViewer != null)
                {
                    _parentScrollViewer.ScrollChanged -= OnParentScrollChanged;
                    _parentScrollViewer = null;
                }
            
            // Unsubscribe from global theme changes to avoid memory leaks while unloaded
            ThemeManager.ThemeChanged -= OnGlobalThemeChanged;
            
            // Cleanup pooled/recyclable visuals to reduce retained memory
            CleanupAndRecycleElements();
            }
            catch (Exception ex)
            {
                LogState($"Unloaded cleanup exception: {ex.Message}");
            }
            finally
            {
                LogState("Unloaded event - end cleanup");
            }
        }
        
        private void OnGlobalThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            // Apply the new global theme if no specific theme is set for this container
            if (Theme == null)
            {
                ApplyTheme(e.CurrentTheme);
                LogState("GlobalThemeChanged -> ApplyTheme");
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
            LogState("Loaded event start");
            
            // Ensure ThemeChanged is subscribed after potential Unloaded cleanup
            ThemeManager.ThemeChanged -= OnGlobalThemeChanged;
            ThemeManager.ThemeChanged += OnGlobalThemeChanged;
            
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
            LogState("Loaded -> set invalidated");
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
                    gc.LogState("TasksPropertyChanged -> set invalidated");
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
                LogState("Tasks_CollectionChanged -> set invalidated");
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
                gc.LogState("LayoutPropertyChanged -> set invalidated");
                gc.BuildLayout();
            }
        }

        private static void OnDependenciesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GanttContainer gc)
            {
                // Update dependency tracking
                if (e.OldValue is ObservableCollection<TaskDependency> oldDependencies)
                {
                    oldDependencies.CollectionChanged -= gc.OnDependenciesCollectionChanged;
                }
                
                if (e.NewValue is ObservableCollection<TaskDependency> newDependencies)
                {
                    newDependencies.CollectionChanged += gc.OnDependenciesCollectionChanged;
                }
                
                gc.RefreshDependencyLines();
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
                LogState("BuildLayout skipped");
                return;
            }
            LogState("BuildLayout enqueued");
            
            // Use debouncing for performance
            var delay = Configuration?.Rendering?.LayoutDebounceDelay ?? 150;
            _performanceService.DebounceOperation(BuildLayoutImmediate, delay, "layout");
        }

        private void BuildLayoutImmediate()
        {
            using var measurement = _performanceService.BeginMeasurement("LayoutBuild");
            LogState("BuildLayoutImmediate start");
            
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

                // Create row definitions with proper heights
                for (int r = 0; r < rows; r++)
                {
                    var height = (r == 0) ? HeaderRowHeight : TaskRowHeight;
                    // Ensure we have a minimum height to make elements visible
                    if (height.GridUnitType == GridUnitType.Auto || (height.GridUnitType == GridUnitType.Pixel && height.Value <= 0))
                    {
                        height = new GridLength(30); // Default height of 30 pixels
                    }
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
                LogState("BuildLayoutImmediate end -> cleared invalidated");
                System.Diagnostics.Debug.WriteLine($"Layout complete: {Children.Count} total children added");
                
                // Make sure theme is applied to all children
                if (_currentTheme != null)
                {
                    ApplyThemeToChildren();
                }
                
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
                System.Diagnostics.Trace.WriteLine($"[LayoutError] {ex.GetType().FullName}: {ex.Message}");
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
                LogState("BuildLayoutImmediate exception");
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
            GridLayoutManager.BuildFullGrid(this, ticks, rows, columns, ShowGridCells);
        }

        private void BuildTaskBars(List<DateTime> ticks, PerformanceLevel performanceLevel)
        {
            var renderingConfig = Configuration?.Rendering ?? new RenderingConfiguration();
            TaskBarManager.BuildTaskBars(this, ticks, Tasks, TaskCount, ClampTasksToVisibleRows, performanceLevel, 
                IsInteractionEnabled, IsDragDropEnabled, IsResizeEnabled, _elementPool, _interactionManager, IsLoaded, renderingConfig);
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
            TimeScaleManager.BuildTimeHeaders(this, ticks, columns, performanceLevel, _elementPool, _visibleElements);
        }
        
        /// <summary>
        /// Builds virtualized grid elements for visible range only.
        /// </summary>
        private void BuildVirtualizedGrid(List<DateTime> ticks, int columns, (int startRow, int endRow) visibleRange, PerformanceLevel performanceLevel)
        {
            GridLayoutManager.BuildVirtualizedGrid(this, ticks, columns, visibleRange, ShowGridCells, _elementPool, _visibleElements, _performanceService);
        }
        
        /// <summary>
        /// Builds task bars with viewport culling for performance.
        /// </summary>
        private void BuildVirtualizedTaskBars(List<DateTime> ticks, (int startRow, int endRow) visibleRange, PerformanceLevel performanceLevel)
        {
            var renderingConfig = Configuration?.Rendering ?? new RenderingConfiguration();
            TaskBarManager.BuildVirtualizedTaskBars(this, ticks, Tasks, visibleRange, performanceLevel, 
                IsInteractionEnabled, IsDragDropEnabled, IsResizeEnabled, _elementPool, _interactionManager, IsLoaded, renderingConfig, _performanceService);
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
        
        /// <summary>
        /// Applies the current theme to all existing child elements.
        /// </summary>
        private void ApplyThemeToChildren()
        {
            ThemeApplier.ApplyThemeToChildren(this, _currentTheme);
        }
        
        /// <summary>
        /// Applies theme styling to a task bar.
        /// </summary>
        private void ApplyThemeToTaskBar(GanttTaskBar taskBar)
        {
            try
            {
                // Determine background based on status
                string backgroundResourceKey = taskBar.Status switch
                {
                    Models.TaskStatus.Completed => "GanttTaskCompletedBrush",
                    Models.TaskStatus.InProgress => "GanttTaskInProgressBrush",
                    Models.TaskStatus.Cancelled => "GanttTaskOverdueBrush", // Use overdue color for cancelled
                    Models.TaskStatus.OnHold => "GanttTaskOverdueBrush", // Use overdue color for on hold
                    _ => "GanttTaskDefaultBrush"
                };
                
                taskBar.SetResourceReference(Control.BackgroundProperty, backgroundResourceKey);
                taskBar.SetResourceReference(Border.BorderBrushProperty, "GanttTaskBorderBrush");
                taskBar.SetResourceReference(Border.BorderThicknessProperty, "GanttTaskBorderThickness");
                taskBar.SetResourceReference(Border.CornerRadiusProperty, "GanttTaskCornerRadius");
            }
            catch { /* Silently ignore if resources not available */ }
        }
        
        /// <summary>
        /// Applies theme styling to a time cell.
        /// </summary>
        private void ApplyThemeToTimeCell(GanttTimeCell timeCell)
        {
            try
            {
                timeCell.SetResourceReference(Control.BackgroundProperty, "GanttTimeScaleBackgroundBrush");
                timeCell.SetResourceReference(Border.BorderBrushProperty, "GanttTimeScaleBorderBrush");
                timeCell.SetResourceReference(Border.BorderThicknessProperty, "GanttTimeScaleBorderThickness");
                
                // Also update the text block inside
                if (timeCell.Child is TextBlock textBlock)
                {
                    textBlock.SetResourceReference(TextBlock.ForegroundProperty, "GanttTimeScaleTextBrush");
                    textBlock.SetResourceReference(TextBlock.FontFamilyProperty, "GanttTimeScaleFontFamily");
                    textBlock.SetResourceReference(TextBlock.FontSizeProperty, "GanttTimeScaleFontSize");
                    textBlock.SetResourceReference(TextBlock.FontWeightProperty, "GanttTimeScaleFontWeight");
                }
            }
            catch { /* Silently ignore if resources not available */ }
        }
        
        /// <summary>
        /// Applies theme styling to a grid cell.
        /// </summary>
        private void ApplyThemeToGridCell(GanttGridCell gridCell)
        {
            try
            {
                if (gridCell.IsToday)
                {
                    gridCell.SetResourceReference(Control.BackgroundProperty, "GanttTodayBackgroundBrush");
                }
                else if (gridCell.IsWeekend)
                {
                    gridCell.SetResourceReference(Control.BackgroundProperty, "GanttWeekendBackgroundBrush");
                }
                else
                {
                    gridCell.SetResourceReference(Control.BackgroundProperty, "GanttSecondaryBackgroundBrush");
                }
                
                gridCell.SetResourceReference(Border.BorderBrushProperty, "GanttGridLineBrush");
                gridCell.SetResourceReference(Border.BorderThicknessProperty, "GanttGridLineThickness");
            }
            catch { /* Silently ignore if resources not available */ }
        }
        
        /// <summary>
        /// Applies theme styling to a grid row.
        /// </summary>
        private void ApplyThemeToGridRow(GanttGridRow gridRow)
        {
            try
            {
                gridRow.SetResourceReference(Control.BackgroundProperty, "GanttSecondaryBackgroundBrush");
                gridRow.SetResourceReference(Border.BorderBrushProperty, "GanttGridLineBrush");
                gridRow.SetResourceReference(Border.BorderThicknessProperty, "GanttGridLineThickness");
            }
            catch { /* Silently ignore if resources not available */ }
        }
        
        #endregion
        
        #region Dependency Management
        
        private void OnDependenciesCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshDependencyLines();
        }
        
        /// <summary>
        /// Refreshes the dependency lines display
        /// </summary>
        private void RefreshDependencyLines()
        {
            DependencyManager.RefreshDependencyLines(this, ShowDependencyLines, Dependencies, Tasks, HighlightCriticalPath);
        }
        
        /// <summary>
        /// Calculates screen positions for all visible tasks
        /// </summary>
        private Dictionary<string, Rect> CalculateTaskPositions()
        {
            var positions = new Dictionary<string, Rect>();
            
            if (Tasks == null) return positions;
            
            var ticks = GetCachedTimelineTicks();
            
            foreach (var task in Tasks)
            {
                var rect = CalculateTaskScreenPosition(task, ticks);
                if (rect.HasValue)
                {
                    positions[task.Id.ToString()] = rect.Value;
                }
            }
            
            return positions;
        }
        
        /// <summary>
        /// Calculates screen position for a single task
        /// </summary>
        private Rect? CalculateTaskScreenPosition(GanttTask task, List<DateTime> ticks)
        {
            try
            {
                var startIndex = TimelineCalculator.GetTimeIndex(task.Start, ticks);
                var endIndex = TimelineCalculator.GetTimeIndex(task.End, ticks);
                
                if (startIndex < 0 || endIndex >= ticks.Count || task.RowIndex <= 0)
                    return null;
                
                var columnWidth = ActualWidth / Math.Max(1, ColumnDefinitions.Count);
                var rowHeight = ActualHeight / Math.Max(1, RowDefinitions.Count);
                
                var x = startIndex * columnWidth;
                var y = task.RowIndex * rowHeight;
                var width = Math.Max(1, (endIndex - startIndex + 1) * columnWidth);
                var height = rowHeight * 0.8; // Leave some margin
                
                return new Rect(x, y, width, height);
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Creates dependency line objects for rendering
        /// </summary>
        private List<DependencyLine> CreateDependencyLines(Dictionary<string, Rect> taskPositions)
        {
            var dependencyLines = new List<DependencyLine>();
            
            if (Dependencies == null) return dependencyLines;
            
            foreach (var dependency in Dependencies.Where(d => d.IsActive))
            {
                var line = new DependencyLine
                {
                    Dependency = dependency,
                    LineColor = dependency.IsCritical && HighlightCriticalPath ? 
                        Brushes.Red : Brushes.DarkBlue,
                    LineThickness = dependency.IsCritical && HighlightCriticalPath ? 2.0 : 1.0,
                    IsHighlighted = dependency.IsCritical && HighlightCriticalPath,
                    ShowArrow = true,
                    ShowLagLabel = dependency.Lag != TimeSpan.Zero
                };
                
                dependencyLines.Add(line);
            }
            
            return dependencyLines;
        }
        
        /// <summary>
        /// Exports the Gantt chart using the configured export service
        /// </summary>
        public async System.Threading.Tasks.Task<bool> ExportAsync(string filePath, ExportOptions options)
        {
            return await ExportManager.ExportAsync(ExportService, this, filePath, options);
        }
        
        /// <summary>
        /// Applies a project template using the configured template service
        /// </summary>
        public async System.Threading.Tasks.Task<List<GanttTask>> ApplyTemplateAsync(string templateId, Models.Templates.TemplateApplicationOptions options)
        {
            return await TemplateManager.ApplyTemplateAsync(TemplateService, templateId, options);
        }
        
        /// <summary>
        /// 初始化GPU渲染服务
        /// </summary>
        /// <returns>GPU渲染服务实例</returns>
        private IGpuRenderingService InitializeGpuRenderingService()
        {
            try
            {
                // 从配置中获取渲染技术
                var technology = Configuration?.Rendering?.GpuRenderingTechnology ?? GpuRenderingTechnology.Default;
                var enableGpu = Configuration?.Rendering?.EnableGpuAcceleration ?? false;
                
                if (enableGpu)
                {
                    return GpuRenderingServiceFactory.CreateService(technology);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GPU渲染服务初始化失败: {ex.Message}");
            }
            
            // 回退到默认渲染服务
            return GpuRenderingServiceFactory.CreateService(GpuRenderingTechnology.Default);
        }
        
        #endregion
     }
}