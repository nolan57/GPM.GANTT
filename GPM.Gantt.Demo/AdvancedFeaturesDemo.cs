using System;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GPM.Gantt.Controls;
using GPM.Gantt.Models;
using GPM.Gantt.Services;
using GPM.Gantt.Plugins;

namespace GPM.Gantt.Demo
{
    /// <summary>
    /// Advanced features demo window
    /// </summary>
    public partial class AdvancedFeaturesDemo : Window
    {
        private readonly IPluginService _pluginService;
        private readonly ObservableCollection<IAnnotationConfig> _annotations;
        private Canvas _annotationCanvas;
        private MultiLevelTimeScaleControl _timeScaleControl;

        public AdvancedFeaturesDemo()
        {
            InitializeComponent();
            
            _pluginService = new PluginService();
            _annotations = new ObservableCollection<IAnnotationConfig>();
            
            SetupDemo();
        }

        private void InitializeComponent()
        {
            Title = "Advanced Gantt Features Demo";
            Width = 1200;
            Height = 800;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100, GridUnitType.Pixel) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

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

            // Multi-level time scale
            _timeScaleControl = new MultiLevelTimeScaleControl
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(6),
                Background = Brushes.LightGray,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1)
            };
            Grid.SetRow(_timeScaleControl, 1);
            mainGrid.Children.Add(_timeScaleControl);

            // Main content area with annotation overlay
            var contentContainer = new Grid();
            
            // Background for tasks (placeholder)
            var taskBackground = new Rectangle
            {
                Fill = Brushes.WhiteSmoke,
                Stroke = Brushes.LightGray,
                StrokeThickness = 1
            };
            contentContainer.Children.Add(taskBackground);

            // Annotation canvas
            _annotationCanvas = new Canvas
            {
                Background = Brushes.Transparent
            };
            contentContainer.Children.Add(_annotationCanvas);

            Grid.SetRow(contentContainer, 2);
            mainGrid.Children.Add(contentContainer);

            // Control panel
            var controlPanel = CreateControlPanel();
            Grid.SetRow(controlPanel, 3);
            mainGrid.Children.Add(controlPanel);

            Content = mainGrid;
        }

        private StackPanel CreateControlPanel()
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10),
                Background = Brushes.LightBlue
            };

            // Plugin controls
            var pluginGroup = new GroupBox
            {
                Header = "Annotation Plugins",
                Margin = new Thickness(5)
            };

            var pluginStack = new StackPanel { Orientation = Orientation.Horizontal };

            // Add text annotation button
            var addTextBtn = new Button
            {
                Content = "Add Text",
                Margin = new Thickness(5),
                Padding = new Thickness(10, 5, 10, 5)
            };
            addTextBtn.Click += (s, e) => AddAnnotation(AnnotationType.Text);
            pluginStack.Children.Add(addTextBtn);

            // Add shape annotation button
            var addShapeBtn = new Button
            {
                Content = "Add Shape",
                Margin = new Thickness(5),
                Padding = new Thickness(10, 5, 10, 5)
            };
            addShapeBtn.Click += (s, e) => AddAnnotation(AnnotationType.Shape);
            pluginStack.Children.Add(addShapeBtn);

            // Add line annotation button
            var addLineBtn = new Button
            {
                Content = "Add Line",
                Margin = new Thickness(5),
                Padding = new Thickness(10, 5, 10, 5)
            };
            addLineBtn.Click += (s, e) => AddAnnotation(AnnotationType.Line);
            pluginStack.Children.Add(addLineBtn);

            // Clear annotations button
            var clearBtn = new Button
            {
                Content = "Clear All",
                Margin = new Thickness(5),
                Padding = new Thickness(10, 5, 10, 5)
            };
            clearBtn.Click += (s, e) => ClearAnnotations();
            pluginStack.Children.Add(clearBtn);

            pluginGroup.Content = pluginStack;
            panel.Children.Add(pluginGroup);

            // Time scale controls
            var timeGroup = new GroupBox
            {
                Header = "Time Scale Controls",
                Margin = new Thickness(5)
            };

            var timeStack = new StackPanel { Orientation = Orientation.Horizontal };

            // Zoom in button
            var zoomInBtn = new Button
            {
                Content = "Zoom In",
                Margin = new Thickness(5),
                Padding = new Thickness(10, 5, 10, 5)
            };
            zoomInBtn.Click += (s, e) => _timeScaleControl.ZoomIn();
            timeStack.Children.Add(zoomInBtn);

            // Zoom out button
            var zoomOutBtn = new Button
            {
                Content = "Zoom Out",
                Margin = new Thickness(5),
                Padding = new Thickness(10, 5, 10, 5)
            };
            zoomOutBtn.Click += (s, e) => _timeScaleControl.ZoomOut();
            timeStack.Children.Add(zoomOutBtn);

            // Reset zoom button
            var resetZoomBtn = new Button
            {
                Content = "Reset Zoom",
                Margin = new Thickness(5),
                Padding = new Thickness(10, 5, 10, 5)
            };
            resetZoomBtn.Click += (s, e) => _timeScaleControl.ResetZoom();
            timeStack.Children.Add(resetZoomBtn);

            // Expand week example
            var expandBtn = new Button
            {
                Content = "Expand This Week",
                Margin = new Thickness(5),
                Padding = new Thickness(10, 5, 10, 5)
            };
            expandBtn.Click += (s, e) => ExpandCurrentWeek();
            timeStack.Children.Add(expandBtn);

            timeGroup.Content = timeStack;
            panel.Children.Add(timeGroup);

            return panel;
        }

        private void SetupDemo()
        {
            // Setup time scale events
            _timeScaleControl.TimeSegmentExpanded += OnTimeSegmentExpanded;
            _timeScaleControl.TimeSegmentCollapsed += OnTimeSegmentCollapsed;

            // Add some sample annotations
            AddSampleAnnotations();
        }

        private void AddSampleAnnotations()
        {
            // Add a text annotation
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
            AddAnnotationToCanvas(textConfig);

            // Add a shape annotation
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
            AddAnnotationToCanvas(shapeConfig);

            // Add a line annotation
            var lineConfig = new LineAnnotationConfig
            {
                X = 200,
                Y = 150,
                X2 = 400,
                Y2 = 180,
                StrokeColor = "#FFFF5722",
                StrokeThickness = 3
            };
            AddAnnotationToCanvas(lineConfig);
        }

        private void AddAnnotation(AnnotationType type)
        {
            var plugin = _pluginService.GetPluginByType(type);
            if (plugin == null) return;

            var config = plugin.CreateDefaultConfig();
            
            // Position randomly
            var random = new Random();
            config.X = random.Next(50, (int)(_annotationCanvas.ActualWidth - config.Width - 50));
            config.Y = random.Next(50, (int)(_annotationCanvas.ActualHeight - config.Height - 50));

            // Show configuration dialog
            var configWindow = new AnnotationConfigWindow(plugin, config);
            if (configWindow.ShowDialog() == true)
            {
                AddAnnotationToCanvas(config);
            }
        }

        private void AddAnnotationToCanvas(IAnnotationConfig config)
        {
            var plugin = _pluginService.GetPluginByType(config.Type);
            if (plugin == null) return;

            var element = plugin.CreateAnnotationElement(config);
            _annotationCanvas.Children.Add(element);
            _annotations.Add(config);
        }

        private void ClearAnnotations()
        {
            _annotationCanvas.Children.Clear();
            _annotations.Clear();
        }

        private void ExpandCurrentWeek()
        {
            // Expand the current week to show days
            var monday = GetMondayOfCurrentWeek();
            _timeScaleControl.ExpandTimeSegment(monday, ExtendedTimeUnit.Day);
        }

        private DateTime GetMondayOfCurrentWeek()
        {
            var today = DateTime.Today;
            var dayOfWeek = (int)today.DayOfWeek;
            if (dayOfWeek == 0) dayOfWeek = 7; // Sunday = 7
            return today.AddDays(1 - dayOfWeek);
        }

        private void OnTimeSegmentExpanded(object sender, TimeSegmentExpandEventArgs e)
        {
            MessageBox.Show($"Expanded {e.FromLevel} to {e.ToLevel} starting at {e.SegmentStart:yyyy-MM-dd}",
                "Time Segment Expanded", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnTimeSegmentCollapsed(object sender, TimeSegmentCollapseEventArgs e)
        {
            MessageBox.Show($"Collapsed {e.Level} segment starting at {e.SegmentStart:yyyy-MM-dd}",
                "Time Segment Collapsed", MessageBoxButton.OK, MessageBoxImage.Information);
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
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Title = $"Configure {_plugin.Name}";
            Width = 400;
            Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Configuration control
            var configControl = _plugin.GetConfigurationControl(_config);
            Grid.SetRow(configControl, 0);
            grid.Children.Add(configControl);

            // Buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10)
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 75,
                Height = 25,
                Margin = new Thickness(5, 0, 5, 0),
                IsDefault = true
            };
            okButton.Click += (s, e) => { DialogResult = true; Close(); };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 75,
                Height = 25,
                Margin = new Thickness(5, 0, 5, 0),
                IsCancel = true
            };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            Grid.SetRow(buttonPanel, 1);
            grid.Children.Add(buttonPanel);

            Content = grid;
        }
    }
}