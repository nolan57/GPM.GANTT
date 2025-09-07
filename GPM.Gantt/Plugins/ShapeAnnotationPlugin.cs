using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;

namespace GPM.Gantt.Plugins
{
    /// <summary>
    /// Shape annotation plugin
    /// </summary>
    public class ShapeAnnotationPlugin : IAnnotationPlugin
    {
        public string Name => "Shape Annotation";
        public string Description => "Add various shape annotations to the Gantt chart including rectangles, ellipses, triangles";
        public AnnotationType Type => AnnotationType.Shape;
        public string Version => "1.0.0";
        public string Author => "GPM Development Team";

        public UserControl GetConfigurationControl(IAnnotationConfig? config = null)
        {
            return new ShapeAnnotationConfigControl(config as ShapeAnnotationConfig ?? CreateDefaultConfig() as ShapeAnnotationConfig);
        }

        public UIElement CreateAnnotationElement(IAnnotationConfig config)
        {
            if (!(config is ShapeAnnotationConfig shapeConfig))
                throw new ArgumentException("Invalid configuration type", nameof(config));

            Shape shape = shapeConfig.ShapeType.ToLower() switch
            {
                "rectangle" => new Rectangle(),
                "ellipse" => new Ellipse(),
                "triangle" => CreateTriangle(),
                "diamond" => CreateDiamond(),
                _ => new Rectangle()
            };

            shape.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(shapeConfig.FillColor));
            shape.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(shapeConfig.StrokeColor));
            shape.StrokeThickness = shapeConfig.StrokeThickness;
            shape.Opacity = shapeConfig.Opacity;
            shape.Width = config.Width;
            shape.Height = config.Height;

            if (!string.IsNullOrEmpty(shapeConfig.StrokeDashArray))
            {
                var dashArray = new DoubleCollection();
                var values = shapeConfig.StrokeDashArray.Split(',');
                foreach (var value in values)
                {
                    if (double.TryParse(value.Trim(), out var dashValue))
                        dashArray.Add(dashValue);
                }
                shape.StrokeDashArray = dashArray;
            }

            Canvas.SetLeft(shape, config.X);
            Canvas.SetTop(shape, config.Y);

            return shape;
        }

        public void ConfigureAnnotation(UIElement element, IAnnotationConfig config)
        {
            if (!(element is Shape shape) || !(config is ShapeAnnotationConfig shapeConfig))
                return;

            shape.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(shapeConfig.FillColor));
            shape.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(shapeConfig.StrokeColor));
            shape.StrokeThickness = shapeConfig.StrokeThickness;
            shape.Opacity = shapeConfig.Opacity;
            shape.Width = config.Width;
            shape.Height = config.Height;

            Canvas.SetLeft(shape, config.X);
            Canvas.SetTop(shape, config.Y);
        }

        public IAnnotationConfig CreateDefaultConfig()
        {
            return new ShapeAnnotationConfig();
        }

        public bool ValidateConfig(IAnnotationConfig config)
        {
            if (!(config is ShapeAnnotationConfig shapeConfig))
                return false;

            return !string.IsNullOrEmpty(shapeConfig.ShapeType) &&
                   shapeConfig.StrokeThickness >= 0 &&
                   shapeConfig.Opacity >= 0 && shapeConfig.Opacity <= 1;
        }

        private Polygon CreateTriangle()
        {
            var triangle = new Polygon();
            triangle.Points = new PointCollection
            {
                new Point(0.5, 0),
                new Point(0, 1),
                new Point(1, 1)
            };
            return triangle;
        }

        private Polygon CreateDiamond()
        {
            var diamond = new Polygon();
            diamond.Points = new PointCollection
            {
                new Point(0.5, 0),
                new Point(1, 0.5),
                new Point(0.5, 1),
                new Point(0, 0.5)
            };
            return diamond;
        }
    }

    /// <summary>
    /// Shape annotation configuration control
    /// </summary>
    public class ShapeAnnotationConfigControl : UserControl
    {
        private readonly ShapeAnnotationConfig _config;

        public ShapeAnnotationConfigControl(ShapeAnnotationConfig config)
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

            // Shape type
            var shapeTypeLabel = new Label { Content = "Shape Type:" };
            Grid.SetRow(shapeTypeLabel, 0);
            Grid.SetColumn(shapeTypeLabel, 0);
            grid.Children.Add(shapeTypeLabel);

            var shapeTypeCombo = new ComboBox();
            shapeTypeCombo.Items.Add("Rectangle");
            shapeTypeCombo.Items.Add("Ellipse");
            shapeTypeCombo.Items.Add("Triangle");
            shapeTypeCombo.Items.Add("Diamond");
            shapeTypeCombo.SetBinding(ComboBox.SelectedItemProperty, new Binding("ShapeType"));
            Grid.SetRow(shapeTypeCombo, 0);
            Grid.SetColumn(shapeTypeCombo, 1);
            grid.Children.Add(shapeTypeCombo);

            // Fill color
            var fillColorLabel = new Label { Content = "Fill Color:" };
            Grid.SetRow(fillColorLabel, 1);
            Grid.SetColumn(fillColorLabel, 0);
            grid.Children.Add(fillColorLabel);

            var fillColorPicker = new ComboBox();
            fillColorPicker.Items.Add("#FF4CAF50"); // Green
            fillColorPicker.Items.Add("#FF2196F3"); // Blue
            fillColorPicker.Items.Add("#FFFF5722"); // Orange
            fillColorPicker.Items.Add("#FFF44336"); // Red
            fillColorPicker.SetBinding(ComboBox.SelectedItemProperty, new Binding("FillColor"));
            Grid.SetRow(fillColorPicker, 1);
            Grid.SetColumn(fillColorPicker, 1);
            grid.Children.Add(fillColorPicker);

            // Stroke color
            var strokeColorLabel = new Label { Content = "Stroke Color:" };
            Grid.SetRow(strokeColorLabel, 2);
            Grid.SetColumn(strokeColorLabel, 0);
            grid.Children.Add(strokeColorLabel);

            var strokeColorPicker = new ComboBox();
            strokeColorPicker.Items.Add("#FF2196F3");
            strokeColorPicker.Items.Add("#FF000000");
            strokeColorPicker.Items.Add("#FFFF5722");
            strokeColorPicker.SetBinding(ComboBox.SelectedItemProperty, new Binding("StrokeColor"));
            Grid.SetRow(strokeColorPicker, 2);
            Grid.SetColumn(strokeColorPicker, 1);
            grid.Children.Add(strokeColorPicker);

            // Stroke thickness
            var strokeThicknessLabel = new Label { Content = "Stroke Thickness:" };
            Grid.SetRow(strokeThicknessLabel, 3);
            Grid.SetColumn(strokeThicknessLabel, 0);
            grid.Children.Add(strokeThicknessLabel);

            var strokeThicknessSlider = new Slider
            {
                Minimum = 0,
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