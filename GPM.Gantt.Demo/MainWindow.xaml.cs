using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using GPM.Gantt.Models;
using GPM.Gantt.ViewModels;
using GPM.Gantt.Services;
using System.Threading;
using System.Threading.Tasks;
using TaskStatus = GPM.Gantt.Models.TaskStatus;

namespace GPM.Gantt.Demo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// Demonstrates the GPM Gantt Chart functionality with comprehensive controls and MVVM pattern.
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    #region Fields
    private readonly IValidationService _validationService;
    private GanttChartViewModel? _viewModel;
    private readonly DispatcherTimer _statusTimer;
    private string _statusMessage = "Ready";

    #endregion

    #region Properties
    /// <summary>
    /// Gets the view model for the Gantt chart.
    /// </summary>
    public GanttChartViewModel? ViewModel
    {
        get => _viewModel;
        private set
        {
            if (_viewModel != value)
            {
                _viewModel = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the status message displayed in the status bar.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
                if (StatusText != null)
                {
                    StatusText.Text = value;
                }
            }
        }
    }
    #endregion

    #region Constructor
    public MainWindow()
    {
        // Initialize status timer EARLY so event handlers invoked during InitializeComponent can use it safely
        _statusTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _statusTimer.Tick += (s, e) =>
        {
            StatusMessage = "Ready";
            _statusTimer.Stop();
        };
    
        InitializeComponent();
        
        // Initialize services
        _validationService = new ValidationService();
        
        // Set up data context
        DataContext = this;
        
        // Wire up the loaded event to ensure proper initialization timing
        Loaded += MainWindow_Loaded;
        
        // Wire up event handlers for better user experience
        SetupEventHandlers();
        
        StatusMessage = "Demo initialized successfully";
        _statusTimer.Start();
    }
    
    #endregion
    
    #region Menu Event Handlers
    
    /// <summary>
    /// Opens the shapes demo window.
    /// </summary>
    private void OpenShapesDemo_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var shapesDemo = new ShapesDemoWindow();
            shapesDemo.Show();
            StatusMessage = "Shapes demo window opened";
            _statusTimer.Start();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening shapes demo: {ex.Message}";
            _statusTimer.Start();
            MessageBox.Show($"Failed to open shapes demo: {ex.Message}", 
                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    #endregion

    #region Initialization Methods
    /// <summary>
    /// Handles the window loaded event to ensure proper initialization timing.
    /// </summary>
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("MainWindow_Loaded called");
            InitializeGanttChart();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in MainWindow_Loaded: {ex.Message}");
            StatusMessage = $"Error during window load: {ex.Message}";
        }
    }

    /// <summary>
    /// Initializes the Gantt chart with comprehensive demo data.
    /// </summary>
    private void InitializeGanttChart()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("InitializeGanttChart called");
            
            // Initialize view model first
            ViewModel = new GanttChartViewModel(_validationService);
            
            // Ensure a ProjectId exists for async operations
            if (ViewModel.ProjectId == Guid.Empty)
            {
                ViewModel.ProjectId = Guid.NewGuid();
            }
            
            // Set initial time range (current week)
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            
            ViewModel.StartTime = startOfWeek;
            ViewModel.EndTime = startOfWeek.AddDays(13); // Two weeks
            
            System.Diagnostics.Debug.WriteLine($"ViewModel created: StartTime={ViewModel.StartTime}, EndTime={ViewModel.EndTime}, ProjectId={ViewModel.ProjectId}");
            
            // Set the GanttContainer properties
            Gantt.StartTime = ViewModel.StartTime;
            Gantt.EndTime = ViewModel.EndTime;
            Gantt.TaskCount = 8;
            
            // Bind the tasks collection from ViewModel
            Gantt.Tasks = ViewModel.TaskModels;
            
            // Create comprehensive sample tasks synchronously (existing demo behavior)
            CreateSampleTasks();
            
            // Force layout update after all properties are set
            Gantt.InvalidateVisual();
            Gantt.UpdateLayout();
            
            StatusMessage = "Gantt chart initialized with sample data";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in InitializeGanttChart: {ex.Message}");
            StatusMessage = $"Error initializing Gantt chart: {ex.Message}";
            MessageBox.Show($"Failed to initialize Gantt chart: {ex.Message}", 
                          "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
    
    /// <summary>
    /// Creates a comprehensive set of sample tasks for demonstration.
    /// </summary>
    private void CreateSampleTasks()
    {
        var baseDate = ViewModel!.StartTime;
        
        var tasks = new List<GanttTaskViewModel>();
        
        // Project Planning Phase
        tasks.Add(new GanttTaskViewModel(new GanttTask
        {
            Id = Guid.NewGuid(),
            Title = "Project Planning",
            Description = "Initial project planning and requirement analysis",
            RowIndex = 1,
            Start = baseDate,
            End = baseDate.AddDays(2),
            Priority = TaskPriority.High,
            Status = TaskStatus.Completed,
            Progress = 100,
            AssignedResources = new List<string> { "Project Manager", "Business Analyst" }
        }, _validationService));
        
        // Design Phase
        tasks.Add(new GanttTaskViewModel(new GanttTask
        {
            Id = Guid.NewGuid(),
            Title = "System Design",
            Description = "Architecture and UI/UX design",
            RowIndex = 2,
            Start = baseDate.AddDays(1),
            End = baseDate.AddDays(4),
            Priority = TaskPriority.High,
            Status = TaskStatus.InProgress,
            Progress = 75,
            AssignedResources = new List<string> { "Architect", "UI Designer" }
        }, _validationService));
        
        // Development Phase
        tasks.Add(new GanttTaskViewModel(new GanttTask
        {
            Id = Guid.NewGuid(),
            Title = "Backend Development",
            Description = "Core API and business logic implementation",
            RowIndex = 3,
            Start = baseDate.AddDays(3),
            End = baseDate.AddDays(8),
            Priority = TaskPriority.Critical,
            Status = TaskStatus.InProgress,
            Progress = 45,
            AssignedResources = new List<string> { "Senior Developer", "Backend Developer" }
        }, _validationService));
        
        tasks.Add(new GanttTaskViewModel(new GanttTask
        {
            Id = Guid.NewGuid(),
            Title = "Frontend Development",
            Description = "User interface and user experience implementation",
            RowIndex = 4,
            Start = baseDate.AddDays(4),
            End = baseDate.AddDays(9),
            Priority = TaskPriority.Normal,
            Status = TaskStatus.NotStarted,
            Progress = 0,
            AssignedResources = new List<string> { "Frontend Developer", "UI Developer" }
        }, _validationService));
        
        // Testing Phase
        tasks.Add(new GanttTaskViewModel(new GanttTask
        {
            Id = Guid.NewGuid(),
            Title = "Unit Testing",
            Description = "Comprehensive unit and integration testing",
            RowIndex = 5,
            Start = baseDate.AddDays(6),
            End = baseDate.AddDays(10),
            Priority = TaskPriority.High,
            Status = TaskStatus.NotStarted,
            Progress = 0,
            AssignedResources = new List<string> { "QA Engineer", "Test Automation" }
        }, _validationService));
        
        tasks.Add(new GanttTaskViewModel(new GanttTask
        {
            Id = Guid.NewGuid(),
            Title = "User Acceptance Testing",
            Description = "End-to-end testing with stakeholders",
            RowIndex = 6,
            Start = baseDate.AddDays(9),
            End = baseDate.AddDays(11),
            Priority = TaskPriority.Normal,
            Status = TaskStatus.NotStarted,
            Progress = 0,
            AssignedResources = new List<string> { "QA Lead", "Business User" }
        }, _validationService));
        
        // Deployment Phase
        tasks.Add(new GanttTaskViewModel(new GanttTask
        {
            Id = Guid.NewGuid(),
            Title = "Deployment Preparation",
            Description = "Production environment setup and deployment scripts",
            RowIndex = 7,
            Start = baseDate.AddDays(10),
            End = baseDate.AddDays(12),
            Priority = TaskPriority.High,
            Status = TaskStatus.NotStarted,
            Progress = 0,
            AssignedResources = new List<string> { "DevOps Engineer", "System Admin" }
        }, _validationService));
        
        tasks.Add(new GanttTaskViewModel(new GanttTask
        {
            Id = Guid.NewGuid(),
            Title = "Go-Live",
            Description = "Production deployment and monitoring",
            RowIndex = 8,
            Start = baseDate.AddDays(12),
            End = baseDate.AddDays(13),
            Priority = TaskPriority.Critical,
            Status = TaskStatus.NotStarted,
            Progress = 0,
            AssignedResources = new List<string> { "Project Manager", "DevOps Engineer", "Support Team" }
        }, _validationService));
        
        // Add tasks to ViewModel which will automatically update TaskModels
        System.Diagnostics.Debug.WriteLine($"Adding {tasks.Count} task view models to ViewModel");
        foreach (var taskViewModel in tasks)
        {
            ViewModel.Tasks.Add(taskViewModel);
        }
        
        System.Diagnostics.Debug.WriteLine($"ViewModel.Tasks now has {ViewModel.Tasks.Count} items");
        System.Diagnostics.Debug.WriteLine($"ViewModel.TaskModels now has {ViewModel.TaskModels.Count} items");
        
        StatusMessage = $"Created {tasks.Count} sample tasks";
        _statusTimer.Start();
    }
    
    /// <summary>
    /// Sets up event handlers for enhanced user experience.
    /// </summary>
    private void SetupEventHandlers()
    {
        // Date picker change handlers
        StartPicker.SelectedDateChanged += OnStartDateChanged;
        EndPicker.SelectedDateChanged += OnEndDateChanged;
        
        // Task count slider change handler
        TaskCountSlider.ValueChanged += OnTaskCountChanged;
        
        // Time unit combo change handler
        TimeUnitCombo.SelectionChanged += OnTimeUnitChanged;
        
        // Row height slider handlers
        HeaderHeightSlider.ValueChanged += OnHeaderHeightChanged;
        TaskHeightSlider.ValueChanged += OnTaskHeightChanged;
        
        // Date/Time format combo handlers
        DateFormatCombo.SelectionChanged += OnDateFormatChanged;
        TimeFormatCombo.SelectionChanged += OnTimeFormatChanged;
    }
    
    private void OnStartDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        StatusMessage = "Start date updated";
        _statusTimer.Start();
    }
    
    private void OnEndDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        StatusMessage = "End date updated";
        _statusTimer.Start();
    }
    
    private void OnTaskCountChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        StatusMessage = $"Task count: {(int)e.NewValue}";
        _statusTimer.Start();
    }
    
    private void OnTimeUnitChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TimeUnitCombo.SelectedItem is ComboBoxItem item)
        {
            StatusMessage = $"Time unit: {item.Content}";
            _statusTimer.Start();
        }
    }
    
    private void OnHeaderHeightChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        StatusMessage = $"Header height: {(int)e.NewValue}px";
        _statusTimer.Start();
    }
    
    private void OnTaskHeightChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        StatusMessage = $"Task height: {(int)e.NewValue}px";
        _statusTimer.Start();
    }
    
    private void OnDateFormatChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DateFormatCombo.SelectedItem is ComboBoxItem item)
        {
            StatusMessage = $"Date format: {item.Content}";
            _statusTimer.Start();
        }
        else if (DateFormatCombo.Text != null)
        {
            StatusMessage = $"Date format: {DateFormatCombo.Text}";
            _statusTimer.Start();
        }
    }
    
    private void OnTimeFormatChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TimeFormatCombo.SelectedItem is ComboBoxItem item)
        {
            StatusMessage = $"Time format: {item.Content}";
            _statusTimer.Start();
        }
        else if (TimeFormatCombo.Text != null)
        {
            StatusMessage = $"Time format: {TimeFormatCombo.Text}";
            _statusTimer.Start();
        }
    }
    #endregion
    
    #region Theme Event Handlers
    /// <summary>
    /// Handles theme selection changes.
    /// </summary>
    private void ThemeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            var themeName = selectedItem.Content.ToString();
            if (!string.IsNullOrEmpty(themeName))
            {
                try
                {
                    // Ensure Gantt control is initialized before applying theme
                    if (Gantt == null)
                    {
                        StatusMessage = "Gantt chart not yet initialized";
                        _statusTimer.Start();
                        return;
                    }
                    
                    // Apply the selected theme
                    var theme = ThemeManager.GetTheme(themeName);
                    if (theme != null)
                    {
                        Gantt.Theme = theme;
                        StatusMessage = $"Applied {themeName} theme";
                    }
                    else
                    {
                        StatusMessage = $"Theme '{themeName}' not found";
                    }
                    _statusTimer.Start();
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error applying theme: {ex.Message}";
                    _statusTimer.Start();
                }
            }
        }
    }
    #endregion
    
    #region Event Handlers
    /// <summary>
    /// Handles window closing to clean up resources.
    /// </summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        _statusTimer?.Stop();
        base.OnClosing(e);
    }
    #endregion
    
    #region INotifyPropertyChanged Implementation
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}