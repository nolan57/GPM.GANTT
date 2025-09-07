using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;

namespace GPM.Gantt.Plugins
{
    /// <summary>
    /// Text annotation plugin
    /// </summary>
    public class TextAnnotationPlugin : IAnnotationPlugin
    {
        public string Name => "Text Annotation";
        public string Description => "Add text annotations to the Gantt chart with customizable font, color, and style";
        public AnnotationType Type => AnnotationType.Text;
        public string Version => "1.0.0";
        public string Author => "GPM Development Team";

        public UserControl GetConfigurationControl(IAnnotationConfig? config = null)
        {
            return new TextAnnotationConfigControl(config as TextAnnotationConfig ?? CreateDefaultConfig() as TextAnnotationConfig);
        }

        public UIElement CreateAnnotationElement(IAnnotationConfig config)
        {
            if (!(config is TextAnnotationConfig textConfig))
                throw new ArgumentException("Invalid configuration type", nameof(config));

            var textBlock = new TextBlock
            {
                Text = textConfig.Text,
                FontFamily = new FontFamily(textConfig.FontFamily),
                FontSize = textConfig.FontSize,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(textConfig.Color)),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(textConfig.BackgroundColor)),
                Opacity = textConfig.Opacity,
                HorizontalAlignment = Enum.Parse<HorizontalAlignment>(textConfig.HorizontalAlignment),
                VerticalAlignment = Enum.Parse<VerticalAlignment>(textConfig.VerticalAlignment),
                Width = config.Width,
                Height = config.Height
            };

            Canvas.SetLeft(textBlock, config.X);
            Canvas.SetTop(textBlock, config.Y);

            return textBlock;
        }

        public void ConfigureAnnotation(UIElement element, IAnnotationConfig config)
        {
            if (!(element is TextBlock textBlock) || !(config is TextAnnotationConfig textConfig))
                return;

            textBlock.Text = textConfig.Text;
            textBlock.FontFamily = new FontFamily(textConfig.FontFamily);
            textBlock.FontSize = textConfig.FontSize;
            textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(textConfig.Color));
            textBlock.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(textConfig.BackgroundColor));
            textBlock.Opacity = textConfig.Opacity;
            textBlock.HorizontalAlignment = Enum.Parse<HorizontalAlignment>(textConfig.HorizontalAlignment);
            textBlock.VerticalAlignment = Enum.Parse<VerticalAlignment>(textConfig.VerticalAlignment);
            textBlock.Width = config.Width;
            textBlock.Height = config.Height;

            Canvas.SetLeft(textBlock, config.X);
            Canvas.SetTop(textBlock, config.Y);
        }

        public IAnnotationConfig CreateDefaultConfig()
        {
            return new TextAnnotationConfig();
        }

        public bool ValidateConfig(IAnnotationConfig config)
        {
            if (!(config is TextAnnotationConfig textConfig))
                return false;

            return !string.IsNullOrEmpty(textConfig.Text) &&
                   !string.IsNullOrEmpty(textConfig.FontFamily) &&
                   textConfig.FontSize > 0 &&
                   textConfig.Opacity >= 0 && textConfig.Opacity <= 1;
        }
    }

    /// <summary>
    /// Text annotation configuration control
    /// </summary>
    public class TextAnnotationConfigControl : UserControl
    {
        private readonly TextAnnotationConfig _config;

        public TextAnnotationConfigControl(TextAnnotationConfig config)
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
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Text content
            var textLabel = new Label { Content = "Text Content:" };
            Grid.SetRow(textLabel, 0);
            Grid.SetColumn(textLabel, 0);
            grid.Children.Add(textLabel);

            var textBox = new TextBox();
            textBox.SetBinding(TextBox.TextProperty, new Binding("Text"));
            Grid.SetRow(textBox, 0);
            Grid.SetColumn(textBox, 1);
            grid.Children.Add(textBox);

            // Font size
            var fontSizeLabel = new Label { Content = "Font Size:" };
            Grid.SetRow(fontSizeLabel, 1);
            Grid.SetColumn(fontSizeLabel, 0);
            grid.Children.Add(fontSizeLabel);

            var fontSizeSlider = new Slider
            {
                Minimum = 8,
                Maximum = 72,
                Value = _config.FontSize,
                TickFrequency = 2,
                IsSnapToTickEnabled = true
            };
            fontSizeSlider.SetBinding(Slider.ValueProperty, new Binding("FontSize"));
            Grid.SetRow(fontSizeSlider, 1);
            Grid.SetColumn(fontSizeSlider, 1);
            grid.Children.Add(fontSizeSlider);

            // Color selection
            var colorLabel = new Label { Content = "Text Color:" };
            Grid.SetRow(colorLabel, 2);
            Grid.SetColumn(colorLabel, 0);
            grid.Children.Add(colorLabel);

            var colorPicker = new ComboBox();
            colorPicker.Items.Add("#FF000000"); // Black
            colorPicker.Items.Add("#FFFF0000"); // Red
            colorPicker.Items.Add("#FF0000FF"); // Blue
            colorPicker.Items.Add("#FF008000"); // Green
            colorPicker.SetBinding(ComboBox.SelectedItemProperty, new Binding("Color"));
            Grid.SetRow(colorPicker, 2);
            Grid.SetColumn(colorPicker, 1);
            grid.Children.Add(colorPicker);

            // Opacity
            var opacityLabel = new Label { Content = "Opacity:" };
            Grid.SetRow(opacityLabel, 3);
            Grid.SetColumn(opacityLabel, 0);
            grid.Children.Add(opacityLabel);

            var opacitySlider = new Slider
            {
                Minimum = 0,
                Maximum = 1,
                Value = _config.Opacity,
                TickFrequency = 0.1,
                IsSnapToTickEnabled = true
            };
            opacitySlider.SetBinding(Slider.ValueProperty, new Binding("Opacity"));
            Grid.SetRow(opacitySlider, 3);
            Grid.SetColumn(opacitySlider, 1);
            grid.Children.Add(opacitySlider);

            Content = grid;
        }
    }
}