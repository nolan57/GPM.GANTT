using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GPM.Gantt
{
    /// <summary>
    /// Grid row component: Spans across all time columns
    /// </summary>
    public class GanttGridRow : GanttShapeBase
    {
        public static readonly DependencyProperty RowIndexProperty = DependencyProperty.Register(
            nameof(RowIndex), typeof(int), typeof(GanttGridRow), new PropertyMetadata(0));

        public int RowIndex
        {
            get => (int)GetValue(RowIndexProperty);
            set => SetValue(RowIndexProperty, value);
        }

        public GanttGridRow()
        {
            // Enable GPU rendering by default
            EnableGpuRendering = true;
            ApplyDefaultTheme();
        }
        
        /// <summary>
        /// Applies default theme styling to the grid row using resource references.
        /// </summary>
        private void ApplyDefaultTheme()
        {
            try
            {
                // Apply theme-aware styling
                this.SetResourceReference(BackgroundProperty, "GanttSecondaryBackgroundBrush");
                this.SetResourceReference(BorderBrushProperty, "GanttGridLineBrush");
                this.SetResourceReference(BorderThicknessProperty, "GanttGridLineThickness");
            }
            catch
            {
                // Fallback to hardcoded values if theme resources aren't available yet
                BorderBrush = Brushes.Silver;
                Background = Brushes.Transparent;
                BorderThickness = new Thickness(0.5);
            }
        }
    }
}