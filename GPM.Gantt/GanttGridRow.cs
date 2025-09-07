using System.Windows.Media;

namespace GPM.Gantt
{
    // Grid row component: Spans across all time columns
    public class GanttGridRow : GanttRectangle
    {
        public GanttGridRow()
        {
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
                BorderThickness = new System.Windows.Thickness(0.5);
            }
        }
    }
}