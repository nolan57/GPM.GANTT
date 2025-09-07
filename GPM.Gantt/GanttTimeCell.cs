using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GPM.Gantt
{
    // Time cell: Used for time display in the first row, inherits from GanttRectangle
    public class GanttTimeCell : GanttRectangle
    {
        public static readonly DependencyProperty TimeTextProperty = DependencyProperty.Register(
            nameof(TimeText), typeof(string), typeof(GanttTimeCell), new FrameworkPropertyMetadata(string.Empty));

        public string TimeText
        {
            get => (string)GetValue(TimeTextProperty);
            set => SetValue(TimeTextProperty, value);
        }

        public GanttTimeCell()
        {
            ApplyDefaultTheme();
            
            // Place text inside the border to display time
            var tb = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            
            // Apply theme-aware text styling
            try
            {
                tb.SetResourceReference(TextBlock.ForegroundProperty, "GanttTimeScaleTextBrush");
                tb.SetResourceReference(TextBlock.FontFamilyProperty, "GanttTimeScaleFontFamily");
                tb.SetResourceReference(TextBlock.FontSizeProperty, "GanttTimeScaleFontSize");
                tb.SetResourceReference(TextBlock.FontWeightProperty, "GanttTimeScaleFontWeight");
            }
            catch
            {
                // Fallback styling
                tb.Foreground = Brushes.Black;
                tb.FontFamily = new FontFamily("Segoe UI");
                tb.FontSize = 11;
            }
            
            tb.SetBinding(TextBlock.TextProperty, new Binding(nameof(TimeText)) { Source = this });
            Child = tb;
        }
        
        /// <summary>
        /// Applies default theme styling to the time cell using resource references.
        /// </summary>
        private void ApplyDefaultTheme()
        {
            try
            {
                // Apply theme-aware styling
                this.SetResourceReference(BackgroundProperty, "GanttTimeScaleBackgroundBrush");
                this.SetResourceReference(BorderBrushProperty, "GanttTimeScaleBorderBrush");
                this.SetResourceReference(BorderThicknessProperty, "GanttTimeScaleBorderThickness");
            }
            catch
            {
                // Fallback to hardcoded values if theme resources aren't available yet
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 250));
                BorderBrush = new SolidColorBrush(Color.FromRgb(222, 226, 230));
                BorderThickness = new Thickness(1);
            }
        }
    }
}