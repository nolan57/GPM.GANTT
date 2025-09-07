using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GPM.Gantt.Configuration;
using GPM.Gantt.Services;
using GPM.Gantt.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GPM.Gantt.ViewModels
{
    /// <summary>
    /// View model for the Gantt chart container, providing data binding and command support.
    /// </summary>
    public class GanttChartViewModel : ViewModelBase
    {
        private readonly IValidationService _validationService;
        private readonly IGanttService _ganttService;
        
        // Backing fields for async commands to access IsRunning/Cancel
        private readonly AsyncRelayCommand _loadTasksAsyncCmd;
        private readonly AsyncRelayCommand _addTaskAsyncCmd;
        private DateTime _startTime = DateTime.Today;
        private DateTime _endTime = DateTime.Today.AddDays(6);
        private Models.TimeUnit _timeUnit = Models.TimeUnit.Day;
        private ObservableCollection<GanttTaskViewModel> _tasks = new();
        private ObservableCollection<GanttTask> _taskModels = new();
        private GanttConfiguration _configuration = GanttConfiguration.Default();
        private GanttTaskViewModel? _selectedTask;
        private string _errorMessage = string.Empty;
        private Guid _projectId = Guid.Empty;
        
        /// <summary>
        /// Initializes a new instance of the GanttChartViewModel class.
        /// </summary>
        /// <param name="validationService">The validation service instance.</param>
        /// <param name="ganttService">The gantt data service instance.</param>
        public GanttChartViewModel(IValidationService? validationService = null, IGanttService? ganttService = null)
        {
            _validationService = validationService ?? new ValidationService();
            _ganttService = ganttService ?? new GanttService(_validationService);
            
            // Initialize commands
            AddTaskCommand = new RelayCommand(AddTask, CanAddTask);
            DeleteTaskCommand = new RelayCommand<GanttTaskViewModel>(DeleteTask, CanDeleteTask);
            UpdateTaskCommand = new RelayCommand<GanttTaskViewModel>(UpdateTask, CanUpdateTask);
            ValidateAllCommand = new RelayCommand(ValidateAll);
            
            // MVVM async commands
            _loadTasksAsyncCmd = new AsyncRelayCommand(ct => LoadTasksAsync(ct), () => ProjectId != Guid.Empty);
            _addTaskAsyncCmd = new AsyncRelayCommand(ct => AddTaskAsync(null, ct), () => ProjectId != Guid.Empty);
            LoadTasksAsyncCommand = _loadTasksAsyncCmd;
            AddTaskAsyncCommand = _addTaskAsyncCmd;
            CancelAsyncCommand = new RelayCommand(() => { _loadTasksAsyncCmd.Cancel(); _addTaskAsyncCmd.Cancel(); }, () => IsBusy);

            // Keep IsBusy and Cancel command state in sync with async command running state
            _loadTasksAsyncCmd.CanExecuteChanged += (_, __) => { OnPropertyChanged(nameof(IsBusy)); CommandManager.InvalidateRequerySuggested(); };
            _addTaskAsyncCmd.CanExecuteChanged += (_, __) => { OnPropertyChanged(nameof(IsBusy)); CommandManager.InvalidateRequerySuggested(); };
            
            // Subscribe to task collection changes and maintain sync with TaskModels
            Tasks.CollectionChanged += (_, e) => {
                OnPropertyChanged(nameof(TaskModels));
                SyncTaskModels();
            };
        }
        
        #region Properties
        
        /// <summary>
        /// Gets or sets the start time of the timeline.
        /// </summary>
        public DateTime StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }
        
        /// <summary>
        /// Gets or sets the end time of the timeline.
        /// </summary>
        public DateTime EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }
        
        /// <summary>
        /// Gets or sets the time unit for the timeline.
        /// </summary>
        public Models.TimeUnit TimeUnit
        {
            get => _timeUnit;
            set => SetProperty(ref _timeUnit, value);
        }
        
        /// <summary>
        /// Gets the collection of task view models.
        /// </summary>
        public ObservableCollection<GanttTaskViewModel> Tasks
        {
            get => _tasks;
            private set => SetProperty(ref _tasks, value);
        }
        
        /// <summary>
        /// Gets the collection of task models for binding to the container.
        /// </summary>
        public ObservableCollection<GanttTask> TaskModels => _taskModels;
        
        /// <summary>
        /// Gets or sets the configuration for the Gantt chart.
        /// </summary>
        public GanttConfiguration Configuration
        {
            get => _configuration;
            set => SetProperty(ref _configuration, value);
        }
        
        /// <summary>
        /// Gets or sets the currently selected task.
        /// </summary>
        public GanttTaskViewModel? SelectedTask
        {
            get => _selectedTask;
            set => SetProperty(ref _selectedTask, value);
        }
        
        /// <summary>
        /// Gets or sets the current error message.
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>
        /// Gets or sets the active ProjectId for service operations.
        /// </summary>
        public Guid ProjectId
        {
            get => _projectId;
            set
            {
                if (SetProperty(ref _projectId, value))
                {
                    _loadTasksAsyncCmd.RaiseCanExecuteChanged();
                    _addTaskAsyncCmd.RaiseCanExecuteChanged();
                }
            }
        }
        
        /// <summary>
        /// Indicates if any async operation is running.
        /// </summary>
        public bool IsBusy => (_loadTasksAsyncCmd?.IsRunning ?? false) || (_addTaskAsyncCmd?.IsRunning ?? false);
        
        #endregion
        
        #region Commands
        
        /// <summary>
        /// Command to add a new task.
        /// </summary>
        public ICommand AddTaskCommand { get; }
        
        /// <summary>
        /// Command to delete a task.
        /// </summary>
        public ICommand DeleteTaskCommand { get; }
        
        /// <summary>
        /// Command to update a task.
        /// </summary>
        public ICommand UpdateTaskCommand { get; }
        
        /// <summary>
        /// Command to validate all tasks.
        /// </summary>
        public ICommand ValidateAllCommand { get; }
        
        // New async MVVM commands (not yet wired in XAML)
        public ICommand LoadTasksAsyncCommand { get; }
        public ICommand AddTaskAsyncCommand { get; }
        
        /// <summary>
        /// Command to cancel the running async operation (if any).
        /// </summary>
        public ICommand CancelAsyncCommand { get; }
        
        #endregion
        
        #region Command Implementations
        
        private void AddTask()
        {
            var newTask = new GanttTask
            {
                Title = "New Task",
                Start = StartTime,
                End = StartTime.AddDays(1),
                RowIndex = Tasks.Count + 1
            };
            
            var taskViewModel = new GanttTaskViewModel(newTask, _validationService);
            Tasks.Add(taskViewModel);
            SelectedTask = taskViewModel;
            
            ClearErrorMessage();
        }
        
        private bool CanAddTask() => true;
        
        private void DeleteTask(GanttTaskViewModel? task)
        {
            if (task != null && Tasks.Contains(task))
            {
                Tasks.Remove(task);
                if (SelectedTask == task)
                {
                    SelectedTask = null;
                }
                ClearErrorMessage();
            }
        }
        
        private bool CanDeleteTask(GanttTaskViewModel? task) => task != null;
        
        private void UpdateTask(GanttTaskViewModel? task)
        {
            if (task != null)
            {
                var validationResult = _validationService.ValidateTask(task.Model);
                if (!validationResult.IsValid)
                {
                    ErrorMessage = string.Join("; ", validationResult.Errors);
                }
                else
                {
                    ClearErrorMessage();
                }
            }
        }
        
        private bool CanUpdateTask(GanttTaskViewModel? task) => task != null;
        
        private void ValidateAll()
        {
            var allTasks = Tasks.Select(vm => vm.Model);
            var validationResult = _validationService.ValidateTaskCollection(allTasks);
            
            if (!validationResult.IsValid)
            {
                ErrorMessage = string.Join("\n", validationResult.Errors);
            }
            else
            {
                ErrorMessage = "All tasks are valid.";
            }
        }
        
        private void ClearErrorMessage()
        {
            ErrorMessage = string.Empty;
        }
        
        #endregion
        
        #region Async Service Operations (Project-aware)

        /// <summary>
        /// Loads tasks from the service for the current ProjectId and replaces the Tasks collection.
        /// </summary>
        public async Task LoadTasksAsync(CancellationToken cancellationToken = default)
        {
            var items = await _ganttService.GetTasksAsync(ProjectId, cancellationToken).ConfigureAwait(false);
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null)
            {
                await dispatcher.InvokeAsync(() =>
                {
                    Tasks.Clear();
                    foreach (var model in items)
                    {
                        Tasks.Add(new GanttTaskViewModel(model, _validationService));
                    }
                    SyncTaskModels();
                    ClearErrorMessage();
                });
            }
            else
            {
                Tasks.Clear();
                foreach (var model in items)
                {
                    Tasks.Add(new GanttTaskViewModel(model, _validationService));
                }
                SyncTaskModels();
                ClearErrorMessage();
            }
        }

        /// <summary>
        /// Creates a task via the service and adds it to the Tasks collection.
        /// </summary>
        public async Task<GanttTaskViewModel> AddTaskAsync(GanttTask? model = null, CancellationToken cancellationToken = default)
        {
            var toCreate = model ?? new GanttTask
            {
                Title = "New Task",
                Start = StartTime,
                End = StartTime.AddDays(1),
                RowIndex = Tasks.Count + 1
            };

            var created = await _ganttService.CreateTaskAsync(ProjectId, toCreate, cancellationToken).ConfigureAwait(false);
            GanttTaskViewModel vm = new(created, _validationService);
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null)
            {
                await dispatcher.InvokeAsync(() =>
                {
                    Tasks.Add(vm);
                    SelectedTask = vm;
                    SyncTaskModels();
                    ClearErrorMessage();
                });
            }
            else
            {
                Tasks.Add(vm);
                SelectedTask = vm;
                SyncTaskModels();
                ClearErrorMessage();
            }

            return vm;
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Synchronizes the TaskModels collection with the Tasks collection.
        /// </summary>
        private void SyncTaskModels()
        {
            _taskModels.Clear();
            foreach (var task in Tasks.Select(vm => vm.Model))
            {
                _taskModels.Add(task);
            }
        }
        
        #endregion
    }
}