using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GPM.Gantt.Models;
using GPM.Gantt.Services;
using GPM.Gantt.Plugins;
using GPM.Gantt.Controls;

namespace GPM.Gantt.Demo
{
    /// <summary>
    /// Advanced features demo window
    /// </summary>
    public partial class AdvancedFeaturesDemo : Window
    {
        private readonly IPluginService _pluginService;
        private readonly ObservableCollection<IAnnotationConfig> _annotations;
        private Canvas? _annotationCanvas;
        private MultiLevelTimeScaleControl? _timeScaleControl;

        public AdvancedFeaturesDemo()
        {
            Debug.WriteLine("AdvancedFeaturesDemo: Constructor started");
            try
            {
                InitializeComponent();
                Debug.WriteLine("AdvancedFeaturesDemo: InitializeComponent completed");
                
                _pluginService = new PluginService();
                Debug.WriteLine("AdvancedFeaturesDemo: PluginService created");
                
                _annotations = new ObservableCollection<IAnnotationConfig>();
                Debug.WriteLine("AdvancedFeaturesDemo: Annotations collection created");
                
                SetupDemo();
                Debug.WriteLine("AdvancedFeaturesDemo: SetupDemo completed");
                
                // Add loaded event handler to catch issues during rendering
                Loaded += AdvancedFeaturesDemo_Loaded;
                Debug.WriteLine("AdvancedFeaturesDemo: Loaded event handler attached");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AdvancedFeaturesDemo: Exception in constructor: {ex}");
                throw;
            }
        }

        private void AdvancedFeaturesDemo_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("AdvancedFeaturesDemo: Window loaded event fired");
            try
            {
                // Add any initialization code here that needs to run after the window is loaded
                Debug.WriteLine("AdvancedFeaturesDemo: Window loaded event handler completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AdvancedFeaturesDemo: Exception in Loaded event handler: {ex}");
                // Optionally, show a message box to the user
                // MessageBox.Show($"An error occurred while loading the window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeComponent()
        {
            Debug.WriteLine("AdvancedFeaturesDemo: InitializeComponent method started");
            try
            {
                Title = "Advanced Gantt Features Demo";
                Width = 1200;
                Height = 800;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                Debug.WriteLine("AdvancedFeaturesDemo: Window properties set");

                var mainGrid = new Grid();
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100, GridUnitType.Pixel) });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Debug.WriteLine("AdvancedFeaturesDemo: Main grid created with row definitions");

                // Title
                var titleText = new TextBlock
                {
                    Text = "Advanced Gantt Chart Features Demo",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(10)
                };
                Grid.SetRow(titleText, 0);
                mainGrid.Children.Add(titleText);
                Debug.WriteLine("AdvancedFeaturesDemo: Title text block added");

                // Multi-level time scale
                Debug.WriteLine("AdvancedFeaturesDemo: Creating MultiLevelTimeScaleControl");
                _timeScaleControl = new MultiLevelTimeScaleControl
                {
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddMonths(6),
                    Background = Brushes.LightGray,
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1)
                };
                Debug.WriteLine("AdvancedFeaturesDemo: MultiLevelTimeScaleControl created");
                
                // Add event handlers to catch potential issues with the time scale control
                if (_timeScaleControl != null)
                {
                    Debug.WriteLine("AdvancedFeaturesDemo: Adding event handlers to time scale control");
                    _timeScaleControl.Loaded += TimeScaleControl_Loaded;
                    _timeScaleControl.Unloaded += TimeScaleControl_Unloaded;
                }
                
                Grid.SetRow(_timeScaleControl, 1);
                mainGrid.Children.Add(_timeScaleControl);
                Debug.WriteLine("AdvancedFeaturesDemo: Time scale control added to grid");

                // Main content area with annotation overlay
                var contentContainer = new Grid();
                Debug.WriteLine("AdvancedFeaturesDemo: Content container grid created");
                
                // Background for tasks (placeholder)
                var taskBackground = new Rectangle
                {
                    Fill = Brushes.WhiteSmoke,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1
                };
                contentContainer.Children.Add(taskBackground);
                Debug.WriteLine("AdvancedFeaturesDemo: Task background rectangle added");

                // Annotation canvas
                Debug.WriteLine("AdvancedFeaturesDemo: Creating annotation canvas");
                _annotationCanvas = new Canvas
                {
                    Background = Brushes.Transparent
                };
                Debug.WriteLine("AdvancedFeaturesDemo: Annotation canvas created");
                contentContainer.Children.Add(_annotationCanvas);
                Debug.WriteLine("AdvancedFeaturesDemo: Annotation canvas added to content container");

                Grid.SetRow(contentContainer, 2);
                mainGrid.Children.Add(contentContainer);
                Debug.WriteLine("AdvancedFeaturesDemo: Content container added to main grid");

                // Control panel
                Debug.WriteLine("AdvancedFeaturesDemo: Creating control panel");
                var controlPanel = CreateControlPanel();
                Debug.WriteLine("AdvancedFeaturesDemo: Control panel created");
                Grid.SetRow(controlPanel, 3);
                mainGrid.Children.Add(controlPanel);
                Debug.WriteLine("AdvancedFeaturesDemo: Control panel added to main grid");

                Content = mainGrid;
                Debug.WriteLine("AdvancedFeaturesDemo: Main grid set as window content");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AdvancedFeaturesDemo: Exception in InitializeComponent: {ex}");
                throw;
            }
        }

        
        private void TimeScaleControl_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("AdvancedFeaturesDemo: TimeScaleControl Loaded event fired");
        }

        private void TimeScaleControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("AdvancedFeaturesDemo: TimeScaleControl Unloaded event fired");
        }

        private StackPanel CreateControlPanel()
        {
            Debug.WriteLine("AdvancedFeaturesDemo: CreateControlPanel started");
            try
            {
                var panel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(10),
                    Background = Brushes.LightBlue
                };
                Debug.WriteLine("AdvancedFeaturesDemo: Control panel stack panel created");

                // Plugin controls
                var pluginGroup = new GroupBox
                {
                    Header = "Annotation Plugins",
                    Margin = new Thickness(5)
                };
                Debug.WriteLine("AdvancedFeaturesDemo: Plugin group box created");

                var pluginStack = new StackPanel { Orientation = Orientation.Horizontal };
                Debug.WriteLine("AdvancedFeaturesDemo: Plugin stack panel created");

                // Add text annotation button
                var addTextBtn = new Button
                {
                    Content = "Add Text",
                    Margin = new Thickness(5),
                    Padding = new Thickness(10, 5, 10, 5)
                };
                addTextBtn.Click += (s, e) => AddAnnotation(AnnotationType.Text);
                pluginStack.Children.Add(addTextBtn);
                Debug.WriteLine("AdvancedFeaturesDemo: Add text button created");

                // Add shape annotation button
                var addShapeBtn = new Button
                {
                    Content = "Add Shape",
                    Margin = new Thickness(5),
                    Padding = new Thickness(10, 5, 10, 5)
                };
                addShapeBtn.Click += (s, e) => AddAnnotation(AnnotationType.Shape);
                pluginStack.Children.Add(addShapeBtn);
                Debug.WriteLine("AdvancedFeaturesDemo: Add shape button created");

                // Add line annotation button
                var addLineBtn = new Button
                {
                    Content = "Add Line",
                    Margin = new Thickness(5),
                    Padding = new Thickness(10, 5, 10, 5)
                };
                addLineBtn.Click += (s, e) => AddAnnotation(AnnotationType.Line);
                pluginStack.Children.Add(addLineBtn);
                Debug.WriteLine("AdvancedFeaturesDemo: Add line button created");

                // Clear annotations button
                var clearBtn = new Button
                {
                    Content = "Clear All",
                    Margin = new Thickness(5),
                    Padding = new Thickness(10, 5, 10, 5)
                };
                clearBtn.Click += (s, e) => ClearAnnotations();
                pluginStack.Children.Add(clearBtn);
                Debug.WriteLine("AdvancedFeaturesDemo: Clear button created");

                pluginGroup.Content = pluginStack;
                panel.Children.Add(pluginGroup);
                Debug.WriteLine("AdvancedFeaturesDemo: Plugin group added to panel");

                // Time scale controls
                var timeGroup = new GroupBox
                {
                    Header = "Time Scale Controls",
                    Margin = new Thickness(5)
                };
                Debug.WriteLine("AdvancedFeaturesDemo: Time scale group box created");

                var timeStack = new StackPanel { Orientation = Orientation.Horizontal };

                // Zoom in button
                var zoomInBtn = new Button
                {
                    Content = "Zoom In",
                    Margin = new Thickness(5),
                    Padding = new Thickness(10, 5, 10, 5)
                };
                zoomInBtn.Click += (s, e) => _timeScaleControl?.ZoomIn();
                timeStack.Children.Add(zoomInBtn);
                Debug.WriteLine("AdvancedFeaturesDemo: Zoom in button created");

                // Zoom out button
                var zoomOutBtn = new Button
                {
                    Content = "Zoom Out",
                    Margin = new Thickness(5),
                    Padding = new Thickness(10, 5, 10, 5)
                };
                zoomOutBtn.Click += (s, e) => _timeScaleControl?.ZoomOut();
                timeStack.Children.Add(zoomOutBtn);
                Debug.WriteLine("AdvancedFeaturesDemo: Zoom out button created");

                // Reset zoom button
                var resetZoomBtn = new Button
                {
                    Content = "Reset Zoom",
                    Margin = new Thickness(5),
                    Padding = new Thickness(10, 5, 10, 5)
                };
                resetZoomBtn.Click += (s, e) => _timeScaleControl?.ResetZoom();
                timeStack.Children.Add(resetZoomBtn);
                Debug.WriteLine("AdvancedFeaturesDemo: Reset zoom button created");

                // Expand week example
                var expandBtn = new Button
                {
                    Content = "Expand This Week",
                    Margin = new Thickness(5),
                    Padding = new Thickness(10, 5, 10, 5)
                };
                expandBtn.Click += (s, e) => ExpandCurrentWeek();
                timeStack.Children.Add(expandBtn);
                Debug.WriteLine("AdvancedFeaturesDemo: Expand week button created");

                timeGroup.Content = timeStack;
                panel.Children.Add(timeGroup);
                Debug.WriteLine("AdvancedFeaturesDemo: Time scale group added to panel");

                Debug.WriteLine("AdvancedFeaturesDemo: CreateControlPanel completed");
                return panel;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AdvancedFeaturesDemo: Exception in CreateControlPanel: {ex}");
                throw;
            }
        }

        private void SetupDemo()
        {
            Debug.WriteLine("AdvancedFeaturesDemo: SetupDemo started");
            try
            {
                if (_timeScaleControl != null)
                {
                    Debug.WriteLine("AdvancedFeaturesDemo: Setting up time scale events");
                    _timeScaleControl.TimeSegmentExpanded += OnTimeSegmentExpanded;
                    _timeScaleControl.TimeSegmentCollapsed += OnTimeSegmentCollapsed;
                    Debug.WriteLine("AdvancedFeaturesDemo: Time scale events subscribed");
                }

                // Add some sample annotations
                Debug.WriteLine("AdvancedFeaturesDemo: Adding sample annotations");
                AddSampleAnnotations();
                Debug.WriteLine("AdvancedFeaturesDemo: Sample annotations added");
                Debug.WriteLine("AdvancedFeaturesDemo: SetupDemo completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AdvancedFeaturesDemo: Exception in SetupDemo: {ex}");
                throw;
            }
        }

        private void AddSampleAnnotations()
        {
            Debug.WriteLine("AdvancedFeaturesDemo: AddSampleAnnotations started");
            try
            {
                // Add a text annotation
                Debug.WriteLine("AdvancedFeaturesDemo: Creating text annotation config");
                var textConfig = new TextAnnotationConfig
                {
                    Text = "Project Milestone",
                    X = 100,
                    Y = 50,
                    Width = 150,
                    Height = 30,
                    FontSize = 14,
                    Color = "#FF0000FF"
                };
                Debug.WriteLine("AdvancedFeaturesDemo: Text annotation config created");
                AddAnnotationToCanvas(textConfig);
                Debug.WriteLine("AdvancedFeaturesDemo: Text annotation added to canvas");

                // Add a shape annotation
                Debug.WriteLine("AdvancedFeaturesDemo: Creating shape annotation config");
                var shapeConfig = new ShapeAnnotationConfig
                {
                    ShapeType = "Rectangle",
                    X = 300,
                    Y = 100,
                    Width = 80,
                    Height = 40,
                    FillColor = "#FF4CAF50",
                    StrokeColor = "#FF2196F3"
                };
                Debug.WriteLine("AdvancedFeaturesDemo: Shape annotation config created");
                AddAnnotationToCanvas(shapeConfig);
                Debug.WriteLine("AdvancedFeaturesDemo: Shape annotation added to canvas");

                // Add a line annotation
                Debug.WriteLine("AdvancedFeaturesDemo: Creating line annotation config");
                var lineConfig = new LineAnnotationConfig
                {
                    X = 200,
                    Y = 150,
                    X2 = 400,
                    Y2 = 180,
                    StrokeColor = "#FFFF5722",
                    StrokeThickness = 3
                };
                Debug.WriteLine("AdvancedFeaturesDemo: Line annotation config created");
                AddAnnotationToCanvas(lineConfig);
                Debug.WriteLine("AdvancedFeaturesDemo: Line annotation added to canvas");
                
                Debug.WriteLine("AdvancedFeaturesDemo: AddSampleAnnotations completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AdvancedFeaturesDemo: Exception in AddSampleAnnotations: {ex}");
                throw;
            }
        }

        private void AddAnnotation(AnnotationType type)
        {
            Debug.WriteLine($"AdvancedFeaturesDemo: AddAnnotation started for type {type}");
            try
            {
                var plugin = _pluginService.GetPluginByType(type);
                if (plugin == null || _annotationCanvas == null) 
                {
                    Debug.WriteLine($"AdvancedFeaturesDemo: Plugin or canvas is null for type {type}");
                    return;
                }
                Debug.WriteLine($"AdvancedFeaturesDemo: Plugin retrieved for type {type}");

                var config = plugin.CreateDefaultConfig();
                Debug.WriteLine($"AdvancedFeaturesDemo: Default config created for type {type}");
                
                // Position randomly with bounds checking
                var random = new Random();
                
                // Ensure canvas has valid dimensions
                var canvasWidth = Math.Max(_annotationCanvas.ActualWidth, 100);
                var canvasHeight = Math.Max(_annotationCanvas.ActualHeight, 100);
                
                // Calculate bounds with safety checks
                var minX = 50;
                var maxX = Math.Max(minX, (int)(canvasWidth - config.Width - 50));
                var minY = 50;
                var maxY = Math.Max(minY, (int)(canvasHeight - config.Height - 50));
                
                // Ensure we have valid bounds for random generation
                if (maxX >= minX && maxY >= minY)
                {
                    config.X = random.Next(minX, maxX + 1);
                    config.Y = random.Next(minY, maxY + 1);
                }
                else
                {
                    // Fallback to fixed position if bounds are invalid
                    config.X = 50;
                    config.Y = 50;
                }
                
                Debug.WriteLine($"AdvancedFeaturesDemo: Annotation positioned at ({config.X}, {config.Y})");

                // Show configuration dialog
                Debug.WriteLine($"AdvancedFeaturesDemo: Creating AnnotationConfigWindow for type {type}");
                var configWindow = new AnnotationConfigWindow(plugin, config);
                Debug.WriteLine($"AdvancedFeaturesDemo: AnnotationConfigWindow created for type {type}");
                
                if (configWindow.ShowDialog() == true)
                {
                    Debug.WriteLine($"AdvancedFeaturesDemo: Config window dialog result is true for type {type}");
                    AddAnnotationToCanvas(config);
                    Debug.WriteLine($"AdvancedFeaturesDemo: Annotation added to canvas for type {type}");
                }
                else
                {
                    Debug.WriteLine($"AdvancedFeaturesDemo: Config window dialog result is false for type {type}");
                }
                Debug.WriteLine($"AdvancedFeaturesDemo: AddAnnotation completed for type {type}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AdvancedFeaturesDemo: Exception in AddAnnotation for type {type}: {ex}");
                throw;
            }
        }

        private void AddAnnotationToCanvas(IAnnotationConfig config)
        {
            Debug.WriteLine($"AdvancedFeaturesDemo: AddAnnotationToCanvas started for type {config.Type}");
            try
            {
                var plugin = _pluginService.GetPluginByType(config.Type);
                if (plugin == null || _annotationCanvas == null) 
                {
                    Debug.WriteLine($"AdvancedFeaturesDemo: Plugin or canvas is null for type {config.Type}");
                    return;
                }
                Debug.WriteLine($"AdvancedFeaturesDemo: Plugin retrieved for type {config.Type}");

                var element = plugin.CreateAnnotationElement(config);
                Debug.WriteLine($"AdvancedFeaturesDemo: Annotation element created for type {config.Type}");
                
                _annotationCanvas.Children.Add(element);
                Debug.WriteLine($"AdvancedFeaturesDemo: Annotation element added to canvas for type {config.Type}");
                
                _annotations.Add(config);
                Debug.WriteLine($"AdvancedFeaturesDemo: Annotation config added to collection for type {config.Type}");
                Debug.WriteLine($"AdvancedFeaturesDemo: AddAnnotationToCanvas completed for type {config.Type}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AdvancedFeaturesDemo: Exception in AddAnnotationToCanvas for type {config.Type}: {ex}");
                throw;
            }
        }

        private void ClearAnnotations()
        {
            Debug.WriteLine("AdvancedFeaturesDemo: ClearAnnotations started");
            try
            {
                if (_annotationCanvas == null) 
                {
                    Debug.WriteLine("AdvancedFeaturesDemo: Annotation canvas is null");
                    return;
                }
                _annotationCanvas.Children.Clear();
                Debug.WriteLine("AdvancedFeaturesDemo: Annotation canvas children cleared");
                _annotations.Clear();
                Debug.WriteLine("AdvancedFeaturesDemo: Annotations collection cleared");
                Debug.WriteLine("AdvancedFeaturesDemo: ClearAnnotations completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AdvancedFeaturesDemo: Exception in ClearAnnotations: {ex}");
                throw;
            }
        }

        private void ExpandCurrentWeek()
        {
            Debug.WriteLine("AdvancedFeaturesDemo: ExpandCurrentWeek started");
            try
            {
                if (_timeScaleControl == null) 
                {
                    Debug.WriteLine("AdvancedFeaturesDemo: Time scale control is null");
                    return;
                }
                var monday = GetMondayOfCurrentWeek();
                Debug.WriteLine($"AdvancedFeaturesDemo: Monday of current week is {monday}");
                _timeScaleControl.ExpandTimeSegment(monday, ExtendedTimeUnit.Day);
                Debug.WriteLine("AdvancedFeaturesDemo: ExpandTimeSegment called");
                Debug.WriteLine("AdvancedFeaturesDemo: ExpandCurrentWeek completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AdvancedFeaturesDemo: Exception in ExpandCurrentWeek: {ex}");
                throw;
            }
        }

        private DateTime GetMondayOfCurrentWeek()
        {
            Debug.WriteLine("AdvancedFeaturesDemo: GetMondayOfCurrentWeek started");
            try
            {
                var today = DateTime.Today;
                var dayOfWeek = (int)today.DayOfWeek;
                if (dayOfWeek == 0) dayOfWeek = 7; // Sunday = 7
                var result = today.AddDays(1 - dayOfWeek);
                Debug.WriteLine($"AdvancedFeaturesDemo: GetMondayOfCurrentWeek returning {result}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AdvancedFeaturesDemo: Exception in GetMondayOfCurrentWeek: {ex}");
                throw;
            }
        }

        private void OnTimeSegmentExpanded(object? sender, TimeSegmentExpandEventArgs e)
        {
            Debug.WriteLine($"AdvancedFeaturesDemo: OnTimeSegmentExpanded called with FromLevel={e.FromLevel}, ToLevel={e.ToLevel}, SegmentStart={e.SegmentStart}");
            try
            {
                MessageBox.Show($"Expanded {e.FromLevel} to {e.ToLevel} starting at {e.SegmentStart:yyyy-MM-dd}",
                    "Time Segment Expanded", MessageBoxButton.OK, MessageBoxImage.Information);
                Debug.WriteLine("AdvancedFeaturesDemo: OnTimeSegmentExpanded message box shown");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AdvancedFeaturesDemo: Exception in OnTimeSegmentExpanded: {ex}");
                throw;
            }
        }

        private void OnTimeSegmentCollapsed(object? sender, TimeSegmentCollapseEventArgs e)
        {
            Debug.WriteLine($"AdvancedFeaturesDemo: OnTimeSegmentCollapsed called with Level={e.Level}, SegmentStart={e.SegmentStart}");
            try
            {
                MessageBox.Show($"Collapsed {e.Level} segment starting at {e.SegmentStart:yyyy-MM-dd}",
                    "Time Segment Collapsed", MessageBoxButton.OK, MessageBoxImage.Information);
                Debug.WriteLine("AdvancedFeaturesDemo: OnTimeSegmentCollapsed message box shown");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AdvancedFeaturesDemo: Exception in OnTimeSegmentCollapsed: {ex}");
                throw;
            }
        }
    }

    /// <summary>
    /// Annotation configuration window
    /// </summary>
    public class AnnotationConfigWindow : Window
    {
        private readonly IAnnotationPlugin _plugin;
        private readonly IAnnotationConfig _config;

        public AnnotationConfigWindow(IAnnotationPlugin plugin, IAnnotationConfig config)
        {
            Debug.WriteLine("AnnotationConfigWindow: Constructor started");
            try
            {
                _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
                _config = config ?? throw new ArgumentNullException(nameof(config));
                Debug.WriteLine("AnnotationConfigWindow: Plugin and config assigned");

                InitializeComponent();
                Debug.WriteLine("AnnotationConfigWindow: InitializeComponent completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AnnotationConfigWindow: Exception in constructor: {ex}");
                throw;
            }
        }

        private void InitializeComponent()
        {
            Debug.WriteLine("AnnotationConfigWindow: InitializeComponent started");
            try
            {
                Title = $"Configure {_plugin.Name}";
                Width = 400;
                Height = 300;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                ResizeMode = ResizeMode.NoResize;
                Debug.WriteLine("AnnotationConfigWindow: Window properties set");

                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Debug.WriteLine("AnnotationConfigWindow: Grid created with row definitions");

                // Configuration control
                Debug.WriteLine("AnnotationConfigWindow: Getting configuration control from plugin");
                var configControl = _plugin.GetConfigurationControl(_config);
                Debug.WriteLine("AnnotationConfigWindow: Configuration control retrieved from plugin");
                Grid.SetRow(configControl, 0);
                grid.Children.Add(configControl);
                Debug.WriteLine("AnnotationConfigWindow: Configuration control added to grid");

                // Buttons
                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(10)
                };
                Debug.WriteLine("AnnotationConfigWindow: Button panel created");

                var okButton = new Button
                {
                    Content = "OK",
                    Width = 75,
                    Height = 25,
                    Margin = new Thickness(5, 0, 5, 0),
                    IsDefault = true
                };
                okButton.Click += (s, e) => { DialogResult = true; Close(); };
                Debug.WriteLine("AnnotationConfigWindow: OK button created");

                var cancelButton = new Button
                {
                    Content = "Cancel",
                    Width = 75,
                    Height = 25,
                    Margin = new Thickness(5, 0, 5, 0),
                    IsCancel = true
                };
                cancelButton.Click += (s, e) => { DialogResult = false; Close(); };
                Debug.WriteLine("AnnotationConfigWindow: Cancel button created");

                buttonPanel.Children.Add(okButton);
                buttonPanel.Children.Add(cancelButton);
                Debug.WriteLine("AnnotationConfigWindow: Buttons added to panel");

                Grid.SetRow(buttonPanel, 1);
                grid.Children.Add(buttonPanel);
                Debug.WriteLine("AnnotationConfigWindow: Button panel added to grid");

                Content = grid;
                Debug.WriteLine("AnnotationConfigWindow: Grid set as window content");
                Debug.WriteLine("AnnotationConfigWindow: InitializeComponent completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AnnotationConfigWindow: Exception in InitializeComponent: {ex}");
                throw;
            }
        }
    }
}