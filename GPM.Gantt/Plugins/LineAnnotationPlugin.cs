using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;

namespace GPM.Gantt.Plugins
{
    /// <summary>
    /// Line annotation plugin
    /// </summary>
    public class LineAnnotationPlugin : IAnnotationPlugin
    {
        public string Name => "Line Annotation";
        public string Description => "Add line annotations to the Gantt chart with customizable caps and styles";
        public AnnotationType Type => AnnotationType.Line;
        public string Version => "1.0.0";
        public string Author => "GPM Development Team";

        public UserControl GetConfigurationControl(IAnnotationConfig? config = null)
        {
            return new LineAnnotationConfigControl(config as LineAnnotationConfig ?? CreateDefaultConfig() as LineAnnotationConfig);
        }

        public UIElement CreateAnnotationElement(IAnnotationConfig config)
        {
            if (!(config is LineAnnotationConfig lineConfig))
                throw new ArgumentException("Invalid configuration type", nameof(config));

            var line = new Line
            {
                X1 = config.X,
                Y1 = config.Y,
                X2 = lineConfig.X2,
                Y2 = lineConfig.Y2,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lineConfig.StrokeColor)),
                StrokeThickness = lineConfig.StrokeThickness,
                Opacity = lineConfig.Opacity
            };

            if (!string.IsNullOrEmpty(lineConfig.StrokeDashArray))
            {
                var dashArray = new DoubleCollection();
                var values = lineConfig.StrokeDashArray.Split(',');
                foreach (var value in values)
                {
                    if (double.TryParse(value.Trim(), out var dashValue))
                        dashArray.Add(dashValue);
                }
                line.StrokeDashArray = dashArray;
            }

            // Set line caps
            if (lineConfig.StartCapType != "None")
            {
                line.StrokeStartLineCap = ParseLineCap(lineConfig.StartCapType);
            }
            if (lineConfig.EndCapType != "None")
            {
                line.StrokeEndLineCap = ParseLineCap(lineConfig.EndCapType);
            }

            return line;
        }

        public void ConfigureAnnotation(UIElement element, IAnnotationConfig config)
        {
            if (!(element is Line line) || !(config is LineAnnotationConfig lineConfig))
                return;

            line.X1 = config.X;
            line.Y1 = config.Y;
            line.X2 = lineConfig.X2;
            line.Y2 = lineConfig.Y2;
            line.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lineConfig.StrokeColor));
            line.StrokeThickness = lineConfig.StrokeThickness;
            line.Opacity = lineConfig.Opacity;

            if (!string.IsNullOrEmpty(lineConfig.StrokeDashArray))
            {
                var dashArray = new DoubleCollection();
                var values = lineConfig.StrokeDashArray.Split(',');
                foreach (var value in values)
                {
                    if (double.TryParse(value.Trim(), out var dashValue))
                        dashArray.Add(dashValue);
                }
                line.StrokeDashArray = dashArray;
            }

            if (lineConfig.StartCapType != "None")
            {
                line.StrokeStartLineCap = ParseLineCap(lineConfig.StartCapType);
            }
            if (lineConfig.EndCapType != "None")
            {
                line.StrokeEndLineCap = ParseLineCap(lineConfig.EndCapType);
            }
        }

        public IAnnotationConfig CreateDefaultConfig()
        {
            return new LineAnnotationConfig();
        }

        public bool ValidateConfig(IAnnotationConfig config)
        {
            if (!(config is LineAnnotationConfig lineConfig))
                return false;

            return lineConfig.StrokeThickness >= 0 &&
                   lineConfig.Opacity >= 0 && lineConfig.Opacity <= 1;
        }

        private PenLineCap ParseLineCap(string capType)
        {
            return capType.ToLower() switch
            {
                "flat" => PenLineCap.Flat,
                "square" => PenLineCap.Square,
                "round" => PenLineCap.Round,
                "triangle" => PenLineCap.Triangle,
                _ => PenLineCap.Flat
            };
        }
    }

    /// <summary>
    /// Line annotation configuration control
    /// </summary>
    public class LineAnnotationConfigControl : UserControl
    {
        private readonly LineAnnotationConfig _config;

        public LineAnnotationConfigControl(LineAnnotationConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            InitializeComponent();
            DataContext = _config;
        }

        private void InitializeComponent()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // End point X2
            var x2Label = new Label { Content = "End Point X:" };
            Grid.SetRow(x2Label, 0);
            Grid.SetColumn(x2Label, 0);
            grid.Children.Add(x2Label);

            var x2TextBox = new TextBox();
            x2TextBox.SetBinding(TextBox.TextProperty, new Binding("X2"));
            Grid.SetRow(x2TextBox, 0);
            Grid.SetColumn(x2TextBox, 1);
            grid.Children.Add(x2TextBox);

            // End point Y2
            var y2Label = new Label { Content = "End Point Y:" };
            Grid.SetRow(y2Label, 1);
            Grid.SetColumn(y2Label, 0);
            grid.Children.Add(y2Label);

            var y2TextBox = new TextBox();
            y2TextBox.SetBinding(TextBox.TextProperty, new Binding("Y2"));
            Grid.SetRow(y2TextBox, 1);
            Grid.SetColumn(y2TextBox, 1);
            grid.Children.Add(y2TextBox);

            // Stroke color
            var strokeColorLabel = new Label { Content = "Line Color:" };
            Grid.SetRow(strokeColorLabel, 2);
            Grid.SetColumn(strokeColorLabel, 0);
            grid.Children.Add(strokeColorLabel);

            var strokeColorPicker = new ComboBox();
            strokeColorPicker.Items.Add("#FFFF5722"); // Orange
            strokeColorPicker.Items.Add("#FF000000"); // Black
            strokeColorPicker.Items.Add("#FF2196F3"); // Blue
            strokeColorPicker.Items.Add("#FFF44336"); // Red
            strokeColorPicker.SetBinding(ComboBox.SelectedItemProperty, new Binding("StrokeColor"));
            Grid.SetRow(strokeColorPicker, 2);
            Grid.SetColumn(strokeColorPicker, 1);
            grid.Children.Add(strokeColorPicker);

            // Stroke thickness
            var strokeThicknessLabel = new Label { Content = "Line Thickness:" };
            Grid.SetRow(strokeThicknessLabel, 3);
            Grid.SetColumn(strokeThicknessLabel, 0);
            grid.Children.Add(strokeThicknessLabel);

            var strokeThicknessSlider = new Slider
            {
                Minimum = 1,
                Maximum = 10,
                Value = _config.StrokeThickness,
                TickFrequency = 0.5,
                IsSnapToTickEnabled = true
            };
            strokeThicknessSlider.SetBinding(Slider.ValueProperty, new Binding("StrokeThickness"));
            Grid.SetRow(strokeThicknessSlider, 3);
            Grid.SetColumn(strokeThicknessSlider, 1);
            grid.Children.Add(strokeThicknessSlider);

            Content = grid;
        }
    }
}