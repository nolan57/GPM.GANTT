using System;
using System.Collections.Generic;
using System.Linq;
using GPM.Gantt.Services;
using GPM.Gantt.Models;

namespace GPM.Gantt.ViewModels
{
    /// <summary>
    /// View model for individual Gantt tasks, providing data binding and validation.
    /// </summary>
    public class GanttTaskViewModel : ViewModelBase
    {
        private readonly IValidationService _validationService;
        private readonly GanttTask _model;
        
        /// <summary>
        /// Initializes a new instance of the GanttTaskViewModel class.
        /// </summary>
        /// <param name="model">The task model.</param>
        /// <param name="validationService">The validation service.</param>
        public GanttTaskViewModel(GanttTask model, IValidationService? validationService = null)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _validationService = validationService ?? new ValidationService();
        }
        
        /// <summary>
        /// Gets the underlying task model.
        /// </summary>
        public GanttTask Model => _model;
        
        /// <summary>
        /// Gets or sets the task ID.
        /// </summary>
        public Guid Id
        {
            get => _model.Id;
            set
            {
                if (_model.Id != value)
                {
                    _model.Id = value;
                    OnPropertyChanged();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the task title.
        /// </summary>
        public string Title
        {
            get => _model.Title;
            set
            {
                if (_model.Title != value)
                {
                    _model.Title = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the task description.
        /// </summary>
        public string? Description
        {
            get => _model.Description;
            set
            {
                if (_model.Description != value)
                {
                    _model.Description = value;
                    OnPropertyChanged();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        public DateTime Start
        {
            get => _model.Start;
            set
            {
                if (_model.Start != value)
                {
                    _model.Start = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Duration));
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        public DateTime End
        {
            get => _model.End;
            set
            {
                if (_model.End != value)
                {
                    _model.End = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Duration));
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the row index.
        /// </summary>
        public int RowIndex
        {
            get => _model.RowIndex;
            set
            {
                if (_model.RowIndex != value)
                {
                    _model.RowIndex = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the task progress (0-100).
        /// </summary>
        public double Progress
        {
            get => _model.Progress;
            set
            {
                if (Math.Abs(_model.Progress - value) > 0.01)
                {
                    _model.Progress = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the task priority.
        /// </summary>
        public Models.TaskPriority Priority
        {
            get => _model.Priority;
            set
            {
                if (_model.Priority != value)
                {
                    _model.Priority = value;
                    OnPropertyChanged();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the task status.
        /// </summary>
        public Models.TaskStatus Status
        {
            get => _model.Status;
            set
            {
                if (_model.Status != value)
                {
                    _model.Status = value;
                    OnPropertyChanged();
                }
            }
        }
        
        /// <summary>
        /// Gets the calculated duration of the task.
        /// </summary>
        public TimeSpan Duration => _model.Duration;
        
        /// <summary>
        /// Gets a value indicating whether the task is valid.
        /// </summary>
        public bool IsValid => _model.IsValid();
        
        /// <summary>
        /// Gets the validation errors for the task.
        /// </summary>
        public List<string> ValidationErrors => _model.GetValidationErrors();
        
        /// <summary>
        /// Gets a user-friendly validation message.
        /// </summary>
        public string ValidationMessage
        {
            get
            {
                var errors = ValidationErrors;
                return errors.Any() ? string.Join("; ", errors) : "Valid";
            }
        }
        
        /// <summary>
        /// Validates the task using the validation service.
        /// </summary>
        /// <returns>The validation result.</returns>
        public ValidationResult Validate()
        {
            return _validationService.ValidateTask(_model);
        }
        
        /// <summary>
        /// Creates a copy of the current task view model.
        /// </summary>
        /// <returns>A new GanttTaskViewModel with a cloned model.</returns>
        public GanttTaskViewModel Clone()
        {
            return new GanttTaskViewModel(_model.Clone(), _validationService);
        }
    }
}