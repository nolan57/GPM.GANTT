using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using GPM.Gantt.Interaction;
using GPM.Gantt.Models;
using GPM.Gantt.Models.Calendar;
using GPM.Gantt.Models.Templates;
using GPM.Gantt.Services;
using GPM.Gantt.Demo;
using WpfSelectionChangedEventArgs = System.Windows.Controls.SelectionChangedEventArgs;
using TaskStatus = GPM.Gantt.Models.TaskStatus;

namespace GPM.Gantt.Demo
{
    public partial class MainWindow : Window
    {
        private GanttTheme _defaultTheme;
        private GanttTheme _darkTheme;
        private GanttTheme _lightTheme;
        private GanttTheme _modernTheme;
        private GanttTheme _customTheme;
        
        // Phase 1 Services
        private IDependencyService _dependencyService;
        private IExportService _exportService;
        private ICalendarService _calendarService;
        private ITemplateService _templateService;
        
        // REST API Services
        private IRestApiService _restApiService;
        private IRemoteProjectService _remoteProjectService;

        public MainWindow()
        {
            InitializeComponent();
            InitializeServices();
            InitializeThemes();
            SetupSampleData();
        }
        
        private void InitializeServices()
        {
            _dependencyService = new DependencyService();
            _exportService = new ExportService();
            _calendarService = new CalendarService();
            _templateService = new TemplateService();
            
            // Initialize REST API services
            _restApiService = new RestApiService("https://api.example.com"); // Replace with actual API URL
            var httpClient = new System.Net.Http.HttpClient();
            httpClient.BaseAddress = new Uri("https://api.example.com");
            _remoteProjectService = new RemoteProjectService(httpClient);
            
            // Initialize Gantt container with services
            Gantt.DependencyService = _dependencyService;
            Gantt.ExportService = _exportService;
            Gantt.CalendarService = _calendarService;
            Gantt.TemplateService = _templateService;
            
            // Initialize with empty dependencies collection
            Gantt.Dependencies = new ObservableCollection<TaskDependency>();
            Gantt.ShowDependencyLines = true;
            Gantt.HighlightCriticalPath = true;
        }

        private void InitializeThemes()
        {
            // Get built-in themes
            _defaultTheme = ThemeManager.GetTheme("Default");
            _darkTheme = ThemeManager.GetTheme("Dark");
            _lightTheme = ThemeManager.GetTheme("Light");
            _modernTheme = ThemeManager.GetTheme("Modern");
            
            // Create custom theme
            _customTheme = ThemeManager.CreateCustomTheme("CustomCorporate", theme =>
            {
                theme.Background.PrimaryColor = Colors.White;
                // Replace Color.Parse with ColorConverter.ConvertFromString
                theme.Background.PrimaryColor = Colors.White;
                theme.Background.SecondaryColor = (Color)ColorConverter.ConvertFromString("#E6F7FF"); // Light blue
                theme.Task.DefaultColor = (Color)ColorConverter.ConvertFromString("#2196F3"); // Blue
                theme.TimeScale.TodayMarkerColor = (Color)ColorConverter.ConvertFromString("#FF9800"); // Orange

            });
            
            // Set default theme
            Gantt.Theme = _defaultTheme;
        }

        private void ThemeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem item)
            {
                string themeName = item.Content.ToString();
                
                switch (themeName)
                {
                    case "Default":
                        Gantt.Theme = _defaultTheme;
                        UpdateColorSelection(_defaultTheme);
                        break;
                    case "Dark":
                        Gantt.Theme = _darkTheme;
                        UpdateColorSelection(_darkTheme);
                        break;
                    case "Light":
                        Gantt.Theme = _lightTheme;
                        UpdateColorSelection(_lightTheme);
                        break;
                    case "Modern":
                        Gantt.Theme = _modernTheme;
                        UpdateColorSelection(_modernTheme);
                        break;
                    case "Custom":
                        Gantt.Theme = _customTheme;
                        UpdateColorSelection(_customTheme);
                        break;
                }
            }
        }

        private void UpdateColorSelection(GanttTheme theme)
        {
            // Update row color selection to match current theme
            var rowColor = theme.Background.SecondaryColor;
            foreach (var item in RowColorComboBox.Items)
            {
                if (item is ComboBoxItem comboBoxItem && comboBoxItem.Tag is string colorTag)
                {
                    if (colorTag == rowColor.ToString())
                    {
                        RowColorComboBox.SelectedItem = comboBoxItem;
                        break;
                    }
                }
            }
            
            // Update primary color selection to match current theme
            var primaryColor = theme.Background.PrimaryColor;
            foreach (var item in PrimaryColorComboBox.Items)
            {
                if (item is ComboBoxItem comboBoxItem && comboBoxItem.Tag is string colorTag)
                {
                    if (colorTag == primaryColor.ToString())
                    {
                        PrimaryColorComboBox.SelectedItem = comboBoxItem;
                        break;
                    }
                }
            }
            
            // Update accent color selection to match current theme
            var accentColor = theme.TimeScale.TodayMarkerColor;
            foreach (var item in AccentColorComboBox.Items)
            {
                if (item is ComboBoxItem comboBoxItem && comboBoxItem.Tag is string colorTag)
                {
                    if (colorTag == accentColor.ToString())
                    {
                        AccentColorComboBox.SelectedItem = comboBoxItem;
                        break;
                    }
                }
            }
        }

        private void RowColorComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateCustomTheme();
        }

        private void PrimaryColorComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateCustomTheme();
        }

        private void AccentColorComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateCustomTheme();
        }

        private void UpdateCustomTheme()
        {
            if (RowColorComboBox.SelectedItem is ComboBoxItem rowItem &&
                PrimaryColorComboBox.SelectedItem is ComboBoxItem primaryItem &&
                AccentColorComboBox.SelectedItem is ComboBoxItem accentItem &&
                rowItem.Tag is string rowColorStr &&
                primaryItem.Tag is string primaryColorStr &&
                accentItem.Tag is string accentColorStr)
            {
                // Replace Color.Parse with ColorConverter.ConvertFromString and cast to Color
                var primaryColor = (Color)ColorConverter.ConvertFromString(primaryColorStr);
                var rowColor = (Color)ColorConverter.ConvertFromString(rowColorStr);
                var accentColor = (Color)ColorConverter.ConvertFromString(accentColorStr);
                
                _customTheme.Background.PrimaryColor = primaryColor;
                _customTheme.Background.SecondaryColor = rowColor;
                _customTheme.TimeScale.TodayMarkerColor = accentColor;
                _customTheme.Task.DefaultColor = primaryColor;
                
                // Apply updated theme
                Gantt.Theme = _customTheme;
            }
        }

        private void SetupSampleData()
        {
            // Set sample date range
            var today = DateTime.Today;
            var endDate = today.AddDays(30);
            
            // Create sample tasks
            var tasks = new[]
            {
                new GanttTask { Title = "Project Planning", Start = today, End = today.AddDays(5), RowIndex = 1 },
                new GanttTask { Title = "Requirement Analysis", Start = today.AddDays(5), End = today.AddDays(10), RowIndex = 2 },
                new GanttTask { Title = "Design Phase", Start = today.AddDays(10), End = today.AddDays(15), RowIndex = 3 },
                new GanttTask { Title = "Implementation", Start = today.AddDays(15), End = today.AddDays(25), RowIndex = 4 },
                new GanttTask { Title = "Testing", Start = today.AddDays(25), End = today.AddDays(28), RowIndex = 5 },
                new GanttTask { Title = "Deployment", Start = today.AddDays(28), End = today.AddDays(30), RowIndex = 6 }
            };
            
            Gantt.Tasks = new ObservableCollection<GanttTask>(tasks);
            Gantt.StartTime = today;
            Gantt.EndTime = endDate;
            
            // Ensure the Gantt chart is properly configured
            Gantt.TaskCount = Math.Max(Gantt.TaskCount, tasks.Length);
            Gantt.TimeUnit = TimeUnit.Day;
            Gantt.ShowGridCells = true;
            
            // Force a layout update
            Gantt.InvalidateVisual();
            Gantt.UpdateLayout();
        }

        private void RunAlternatingRowDemo(object sender, RoutedEventArgs e)
        {
            // Create and show the alternating row demo window
            var demoWindow = new AlternatingRowDemo();
            demoWindow.Show();
        }

        private void ExitApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void RunCustomShapesDemo(object sender, RoutedEventArgs e)
        {
            // Create and show the custom shapes demo window
            var demoWindow = new ShapesDemoWindow();
            demoWindow.Show();
        }

        private void RunTimeScaleDemo(object sender, RoutedEventArgs e)
        {
            // TODO: Implement time scale demo
            MessageBox.Show("Time Scale Demo - Coming Soon!", "Demo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RunInteractiveDemo(object sender, RoutedEventArgs e)
        {
            // Open the advanced features demo window
            var advancedDemo = new AdvancedFeaturesDemo();
            advancedDemo.Show();
        }

        #region Phase 1 Event Handlers
        
        private void RunDependencyDemo(object sender, RoutedEventArgs e)
        {
            try
            {
                // Add sample dependencies
                AddSampleDependencies(sender, e);
                
                // Calculate and highlight critical path
                CalculateCriticalPath(sender, e);
                
                MessageBox.Show("Dependency demo completed! Dependencies added and critical path calculated.", "Demo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running dependency demo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void RunExportDemo(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PNG Files (*.png)|*.png|PDF Files (*.pdf)|*.pdf|JPEG Files (*.jpg)|*.jpg",
                    DefaultExt = "png",
                    FileName = "GanttChart"
                };
                
                if (saveFileDialog.ShowDialog() == true)
                {
                    var extension = Path.GetExtension(saveFileDialog.FileName).ToLowerInvariant();
                    var format = extension switch
                    {
                        ".png" => ExportFormat.PNG,
                        ".pdf" => ExportFormat.PDF,
                        ".jpg" or ".jpeg" => ExportFormat.JPEG,
                        _ => ExportFormat.PNG
                    };
                    
                    var options = new ExportOptions
                    {
                        Format = format,
                        Width = (int)Gantt.ActualWidth,
                        Height = (int)Gantt.ActualHeight,
                        Quality = 90,
                        IncludeBackground = true,
                        IncludeDependencies = true
                    };
                    
                    Task.Run(async () =>
                    {
                        await _exportService.ExportAsync(Gantt, saveFileDialog.FileName, options);
                        
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Chart exported successfully to {saveFileDialog.FileName}", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during export: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void RunCalendarDemo(object sender, RoutedEventArgs e)
        {
            try
            {
                var calendar = new WorkingCalendar
                {
                    Name = "Demo Calendar",
                    Type = CalendarType.Standard
                };
                
                // Create the calendar first to get an ID
                var createdCalendar = await _calendarService.CreateCalendarAsync(calendar);
                
                var startDate = DateTime.Today;
                var workingDays = await _calendarService.CountWorkingDaysAsync(startDate, startDate.AddDays(30), createdCalendar.Id);
                var nextWorkingDay = await _calendarService.GetNextWorkingDayAsync(startDate, createdCalendar.Id);
                
                // Get working times from the working days
                var workingTimes = createdCalendar.WorkingDays.Where(wd => wd.IsWorkingDay).SelectMany(wd => wd.WorkingTimes);
                var totalWorkingHours = workingTimes.Sum(wt => (wt.EndTime - wt.StartTime).TotalHours);
                
                var message = $"Calendar Demo Results:\n" +
                             $"Working days in next 30 days: {workingDays}\n" +
                             $"Next working day: {nextWorkingDay:yyyy-MM-dd}\n" +
                             $"Working hours per day: {totalWorkingHours} hours";
                
                MessageBox.Show(message, "Calendar Demo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running calendar demo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void RunTemplateDemo(object sender, RoutedEventArgs e)
        {
            try
            {
                var templates = await _templateService.GetBuiltInTemplatesAsync();
                var templateNames = string.Join(", ", templates.Select(t => t.Name));
                
                MessageBox.Show($"Available templates: {templateNames}\n\nSelect a template from the dropdown and click 'Apply Template' to see it in action.", "Template Demo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running template demo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void ExportAsPng(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PNG Files (*.png)|*.png",
                    DefaultExt = "png",
                    FileName = "GanttChart.png"
                };
                
                if (saveFileDialog.ShowDialog() == true)
                {
                    var options = new ExportOptions
                    {
                        Format = ExportFormat.PNG,
                        Width = (int)Gantt.ActualWidth,
                        Height = (int)Gantt.ActualHeight,
                        IncludeBackground = true,
                        IncludeDependencies = true
                    };
                    
                    await _exportService.ExportAsync(Gantt, saveFileDialog.FileName, options);
                    MessageBox.Show($"Chart exported as PNG to {saveFileDialog.FileName}", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting as PNG: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void ExportAsPdf(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    DefaultExt = "pdf",
                    FileName = "GanttChart.pdf"
                };
                
                if (saveFileDialog.ShowDialog() == true)
                {
                    var options = new ExportOptions
                    {
                        Format = ExportFormat.PDF,
                        Width = (int)Gantt.ActualWidth,
                        Height = (int)Gantt.ActualHeight,
                        IncludeBackground = true,
                        IncludeDependencies = true,
                        Title = "Gantt Chart",
                        Author = "GPM.Gantt Demo",
                        Subject = "Project Timeline"
                    };
                    
                    await _exportService.ExportAsync(Gantt, saveFileDialog.FileName, options);
                    MessageBox.Show($"Chart exported as PDF to {saveFileDialog.FileName}", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting as PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void ExportAsJpeg(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JPEG Files (*.jpg)|*.jpg",
                    DefaultExt = "jpg",
                    FileName = "GanttChart.jpg"
                };
                
                if (saveFileDialog.ShowDialog() == true)
                {
                    var options = new ExportOptions
                    {
                        Format = ExportFormat.JPEG,
                        Width = (int)Gantt.ActualWidth,
                        Height = (int)Gantt.ActualHeight,
                        Quality = 90,
                        IncludeBackground = true,
                        IncludeDependencies = true
                    };
                    
                    await _exportService.ExportAsync(Gantt, saveFileDialog.FileName, options);
                    MessageBox.Show($"Chart exported as JPEG to {saveFileDialog.FileName}", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting as JPEG: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ShowDependenciesCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && Gantt != null)
            {
                Gantt.ShowDependencyLines = checkBox.IsChecked == true;
            }
        }
        
        private void HighlightCriticalPathCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && Gantt != null)
            {
                Gantt.HighlightCriticalPath = checkBox.IsChecked == true;
            }
        }
        
        private void AddSampleDependencies(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Gantt.Tasks?.Count >= 6)
                {
                    var tasks = Gantt.Tasks.ToList();
                    var dependencies = new List<TaskDependency>
                    {
                        new TaskDependency
                        {
                            PredecessorTaskId = tasks[0].Id.ToString(),
                            SuccessorTaskId = tasks[1].Id.ToString(),
                            Type = DependencyType.FinishToStart
                        },
                        new TaskDependency
                        {
                            PredecessorTaskId = tasks[1].Id.ToString(),
                            SuccessorTaskId = tasks[2].Id.ToString(),
                            Type = DependencyType.FinishToStart
                        },
                        new TaskDependency
                        {
                            PredecessorTaskId = tasks[2].Id.ToString(),
                            SuccessorTaskId = tasks[3].Id.ToString(),
                            Type = DependencyType.FinishToStart
                        },
                        new TaskDependency
                        {
                            PredecessorTaskId = tasks[3].Id.ToString(),
                            SuccessorTaskId = tasks[4].Id.ToString(),
                            Type = DependencyType.FinishToStart
                        },
                        new TaskDependency
                        {
                            PredecessorTaskId = tasks[4].Id.ToString(),
                            SuccessorTaskId = tasks[5].Id.ToString(),
                            Type = DependencyType.FinishToStart
                        }
                    };
                    
                    Gantt.Dependencies.Clear();
                    foreach (var dependency in dependencies)
                    {
                        Gantt.Dependencies.Add(dependency);
                    }
                    
                    MessageBox.Show($"Added {dependencies.Count} sample dependencies between tasks.", "Dependencies", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Need at least 6 tasks to create sample dependencies.", "Dependencies", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding dependencies: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void CalculateCriticalPath(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Gantt.Tasks?.Any() == true && Gantt.Dependencies?.Any() == true)
                {
                    var tasks = Gantt.Tasks.ToList();
                    var dependencies = Gantt.Dependencies.ToList();
                    
                    var criticalPath = await _dependencyService.GetCriticalPathAsync(tasks, dependencies);
                    var floatTimes = await _dependencyService.CalculateFloatTimesAsync(tasks, dependencies);
                    
                    // Update task critical path flags
                    foreach (var task in tasks)
                    {
                        task.IsCritical = criticalPath.Contains(task.Id.ToString());
                        if (floatTimes.TryGetValue(task.Id.ToString(), out var floatTime))
                        {
                            task.TotalFloat = floatTime;
                        }
                    }
                    
                    var criticalTaskTitles = tasks.Where(t => t.IsCritical).Select(t => t.Title);
                    MessageBox.Show($"Critical Path calculated!\n\nCritical tasks: {string.Join(", ", criticalTaskTitles)}", "Critical Path", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Need tasks and dependencies to calculate critical path.", "Critical Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating critical path: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void TemplateComboBox_SelectionChanged(object sender, WpfSelectionChangedEventArgs e)
        {
            // Template selection changed - enable apply button if needed
            if (ApplyTemplateButton != null)
            {
                ApplyTemplateButton.IsEnabled = TemplateComboBox.SelectedItem != null;
            }
        }
        
        private async void ApplyTemplate(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TemplateComboBox.SelectedItem is ComboBoxItem item && item.Tag is string templateType)
                {
                    var templates = await _templateService.GetBuiltInTemplatesAsync();
                    var template = templates.FirstOrDefault(t => t.Name.ToLowerInvariant().Contains(templateType));
                    
                    if (template != null)
                    {
                        var result = MessageBox.Show("This will replace current tasks. Continue?", "Apply Template", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            var startDate = DateTime.Today;
                            var options = new Models.Templates.TemplateApplicationOptions
                            {
                                ProjectStartDate = startDate,
                                IncludeDependencies = true,
                                AutoSchedule = true,
                                DurationScale = 1.0
                            };
                            
                            var ganttTasks = await _templateService.ApplyTemplateAsync(template.Id, options);
                            
                            if (ganttTasks.Any())
                            {
                                Gantt.Tasks = new ObservableCollection<GanttTask>(ganttTasks);
                                Gantt.StartTime = ganttTasks.Min(t => t.Start);
                                Gantt.EndTime = ganttTasks.Max(t => t.End);
                                
                                // Apply template dependencies if they exist
                                var dependencies = new List<TaskDependency>();
                                var taskIdMapping = ganttTasks.ToDictionary(t => t.Id.ToString(), t => t);
                                
                                foreach (var depTemplate in template.DependencyTemplates)
                                {
                                    var predTask = ganttTasks.FirstOrDefault(t => t.Title.Contains(template.TaskTemplates.FirstOrDefault(tt => tt.Id == depTemplate.PredecessorTaskId)?.Name ?? ""));
                                    var succTask = ganttTasks.FirstOrDefault(t => t.Title.Contains(template.TaskTemplates.FirstOrDefault(tt => tt.Id == depTemplate.SuccessorTaskId)?.Name ?? ""));
                                    
                                    if (predTask != null && succTask != null)
                                    {
                                        dependencies.Add(new TaskDependency
                                        {
                                            PredecessorTaskId = predTask.Id.ToString(),
                                            SuccessorTaskId = succTask.Id.ToString(),
                                            Type = depTemplate.Type,
                                            Lag = depTemplate.Lag
                                        });
                                    }
                                }
                                
                                Gantt.Dependencies = new ObservableCollection<TaskDependency>(dependencies);
                            }
                            
                            MessageBox.Show($"Template '{template.Name}' applied successfully!", "Template", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Selected template not found.", "Template", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying template: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void CalendarTypeComboBox_SelectionChanged(object sender, WpfSelectionChangedEventArgs e)
        {
            // Calendar type selection changed - update working calendar if needed
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem item && item.Tag is string calendarType)
            {
                // Update calendar service with selected type
                // This could be used to change the default calendar used for calculations
            }
        }
        
        private async void ShowWorkingDays(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedCalendarType = CalendarTypeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag
                    ? Enum.Parse<CalendarType>(tag)
                    : CalendarType.Standard;
                
                var calendar = new WorkingCalendar
                {
                    Name = $"{selectedCalendarType} Calendar",
                    Type = selectedCalendarType
                };
                
                // Configure the calendar based on the selected type
                calendar.WorkingDays.Clear(); // Clear default working days
                
                var workingDaysList = selectedCalendarType switch
                {
                    CalendarType.Standard => new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                    CalendarType.NightShift => new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                    CalendarType.TwentyFourHour => new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday },
                    _ => new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }
                };
                
                var workingTimesList = selectedCalendarType switch
                {
                    CalendarType.Standard => new List<WorkingTime> { new WorkingTime { StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0) } },
                    CalendarType.NightShift => new List<WorkingTime> { new WorkingTime { StartTime = new TimeSpan(22, 0, 0), EndTime = new TimeSpan(6, 0, 0) } },
                    CalendarType.TwentyFourHour => new List<WorkingTime> { new WorkingTime { StartTime = new TimeSpan(0, 0, 0), EndTime = new TimeSpan(23, 59, 59) } },
                    _ => new List<WorkingTime> { new WorkingTime { StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0) } }
                };
                
                // Create working days with proper structure
                foreach (var dayOfWeek in workingDaysList)
                {
                    calendar.WorkingDays.Add(new WorkingDay
                    {
                        DayOfWeek = dayOfWeek,
                        IsWorkingDay = true,
                        WorkingTimes = new List<WorkingTime>(workingTimesList),
                        Description = $"{selectedCalendarType} working day"
                    });
                }
                
                // Add non-working days
                var allDays = Enum.GetValues<DayOfWeek>();
                foreach (var day in allDays)
                {
                    if (!workingDaysList.Contains(day))
                    {
                        calendar.WorkingDays.Add(new WorkingDay
                        {
                            DayOfWeek = day,
                            IsWorkingDay = false,
                            WorkingTimes = new List<WorkingTime>(),
                            Description = "Non-working day"
                        });
                    }
                }
                
                // Create the calendar first to get an ID, then use it for calculations
                var createdCalendar = await _calendarService.CreateCalendarAsync(calendar);
                
                var startDate = DateTime.Today;
                var endDate = startDate.AddDays(30);
                var workingDays = await _calendarService.CountWorkingDaysAsync(startDate, endDate, createdCalendar.Id);
                var allWorkingTimes = createdCalendar.WorkingDays.Where(wd => wd.IsWorkingDay).SelectMany(wd => wd.WorkingTimes);
                var totalHours = workingDays * allWorkingTimes.Sum(wt => (wt.EndTime - wt.StartTime).TotalHours);
                
                var workingDayNames = createdCalendar.WorkingDays.Where(wd => wd.IsWorkingDay).Select(wd => wd.DayOfWeek.ToString());
                var workingTimesDisplay = allWorkingTimes.Select(wt => $"{wt.StartTime:hh\\:mm} - {wt.EndTime:hh\\:mm}");
                
                var message = $"Calendar: {createdCalendar.Name}\n" +
                             $"Working Days: {string.Join(", ", workingDayNames)}\n" +
                             $"Working Times: {string.Join(", ", workingTimesDisplay)}\n" +
                             $"Working days in next 30 days: {workingDays}\n" +
                             $"Total working hours: {totalHours:F1}";
                
                MessageBox.Show(message, "Working Days", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing working days: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        #endregion
        
        #region REST API Integration Methods
        
        private async void RunRestApiDemo(object sender, RoutedEventArgs e)
        {
            try
            {
                // Test API connection
                var isConnected = await _restApiService.TestConnectionAsync();
                if (!isConnected)
                {
                    MessageBox.Show("Unable to connect to REST API. Please check the API URL and try again.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Create a sample remote project
                var remoteProject = new RemoteProject
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample API Project",
                    Description = "A project created via REST API integration",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(30),
                    Status = ProjectStatus.Active,
                    ManagerId = "demo-user",
                    CreatedAt = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    Version = 1
                };
                
                // Add sample tasks to the project
                remoteProject.Tasks.AddRange(CreateSampleRemoteTasks(remoteProject.Id));
                
                // Create project via API (simulate)
                MessageBox.Show($"Created remote project: {remoteProject.Name}\nTasks: {remoteProject.Tasks.Count}\nAPI Connection: {(isConnected ? "✓" : "✗")}", "REST API Demo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running REST API demo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void SynchronizeWithApi(object sender, RoutedEventArgs e)
        {
            try
            {
                var syncOptions = new SyncOptions
                {
                    Bidirectional = true,
                    ConflictResolution = ConflictResolution.ServerWins,
                    Scope = SyncScope.All,
                    LastSyncTime = DateTime.UtcNow.AddDays(-1), // Sync changes from last day
                    BatchSize = 50
                };
                
                var result = await _restApiService.SynchronizeAsync(syncOptions);
                
                var message = $"Synchronization completed:\n" +
                             $"Success: {(result.Success ? "✓" : "✗")}\n" +
                             $"Records Downloaded: {result.RecordsDownloaded}\n" +
                             $"Records Uploaded: {result.RecordsUploaded}\n" +
                             $"Conflicts: {result.ConflictsCount}\n" +
                             $"Duration: {result.Duration.TotalSeconds:F1}s";
                
                if (result.Errors.Any())
                {
                    message += $"\n\nErrors:\n{string.Join("\n", result.Errors.Take(3))}";
                }
                
                MessageBox.Show(message, "Synchronization Result", MessageBoxButton.OK, result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during synchronization: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void UploadCurrentProject(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Gantt.Tasks?.Any() != true)
                {
                    MessageBox.Show("No tasks to upload. Please create some tasks first.", "Upload", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                // Convert current Gantt tasks to remote project
                var remoteProject = new RemoteProject
                {
                    Id = Guid.NewGuid(),
                    Name = "Uploaded Project",
                    Description = "Project uploaded from GPM Gantt Demo",
                    StartDate = Gantt.StartTime,
                    EndDate = Gantt.EndTime,
                    Status = ProjectStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    Version = 1
                };
                
                // Convert Gantt tasks to remote tasks
                foreach (var ganttTask in Gantt.Tasks)
                {
                    var remoteTask = _remoteProjectService.ConvertToRemoteTask(ganttTask, remoteProject.Id);
                    remoteProject.Tasks.Add(remoteTask);
                }
                
                // Simulate upload
                var changes = new List<DataChange>
                {
                    new DataChange
                    {
                        EntityId = remoteProject.Id,
                        EntityType = EntityType.Project,
                        Operation = ChangeOperation.Create,
                        Timestamp = DateTime.UtcNow,
                        Data = System.Text.Json.JsonSerializer.Serialize(remoteProject),
                        CurrentVersion = 1
                    }
                };
                
                var uploadResult = await _restApiService.UploadChangesAsync(changes);
                
                var message = $"Upload completed:\n" +
                             $"Success: {(uploadResult.Success ? "✓" : "✗")}\n" +
                             $"Records Uploaded: {uploadResult.SuccessCount}\n" +
                             $"Failures: {uploadResult.FailureCount}";
                
                if (uploadResult.Errors.Any())
                {
                    message += $"\n\nErrors:\n{string.Join("\n", uploadResult.Errors.Take(3))}";
                }
                
                MessageBox.Show(message, "Upload Result", MessageBoxButton.OK, uploadResult.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during upload: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void TestApiConnection(object sender, RoutedEventArgs e)
        {
            try
            {
                var isConnected = await _restApiService.TestConnectionAsync();
                var message = isConnected 
                    ? "✓ Successfully connected to REST API" 
                    : "✗ Failed to connect to REST API";
                
                MessageBox.Show(message, "Connection Test", MessageBoxButton.OK, 
                    isConnected ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing connection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void SetApiCredentials(object sender, RoutedEventArgs e)
        {
            try
            {
                // Simple demo - in a real application, you'd use a proper authentication dialog
                var token = "demo-token-12345"; // Hardcoded for demo purposes
                
                _restApiService.SetAuthentication(token);
                MessageBox.Show($"API credentials set successfully.\nToken: {token}", "Authentication", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting credentials: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private List<RemoteTask> CreateSampleRemoteTasks(Guid projectId)
        {
            var today = DateTime.Today;
            return new List<RemoteTask>
            {
                new RemoteTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Title = "API Planning",
                    Description = "Plan the API integration",
                    StartDate = today,
                    EndDate = today.AddDays(3),
                    Progress = 100,
                    Status = TaskStatus.Completed,
                    Priority = TaskPriority.High,
                    EstimatedHours = 24,
                    ActualHours = 24,
                    CreatedAt = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    Version = 1
                },
                new RemoteTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Title = "API Implementation",
                    Description = "Implement the REST API endpoints",
                    StartDate = today.AddDays(3),
                    EndDate = today.AddDays(10),
                    Progress = 60,
                    Status = TaskStatus.InProgress,
                    Priority = TaskPriority.High,
                    EstimatedHours = 56,
                    ActualHours = 34,
                    CreatedAt = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    Version = 1
                },
                new RemoteTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Title = "API Testing",
                    Description = "Test the API integration",
                    StartDate = today.AddDays(10),
                    EndDate = today.AddDays(15),
                    Progress = 0,
                    Status = TaskStatus.NotStarted,
                    Priority = TaskPriority.Normal,
                    EstimatedHours = 40,
                    ActualHours = 0,
                    CreatedAt = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    Version = 1
                }
            };
        }
    }
    #endregion
}