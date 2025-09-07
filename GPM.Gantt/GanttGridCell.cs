using System.ComponentModel;

namespace GPM.Gantt
{
    // Grid cell component: Located above grid rows and aligned with time columns
    public class GanttGridCell : GanttRectangle
    {
        public GanttGridCell()
        {
            // Set up property change notifications
            DependencyPropertyDescriptor.FromProperty(IsWeekendProperty, typeof(GanttGridCell))
                ?.AddValueChanged(this, (s, e) => UpdateCellStyling());
            DependencyPropertyDescriptor.FromProperty(IsTodayProperty, typeof(GanttGridCell))
                ?.AddValueChanged(this, (s, e) => UpdateCellStyling());
                
            ApplyDefaultTheme();
        }

        /// <summary>
        /// Applies default theme styling to the grid cell using resource references.
        /// </summary>
        private void ApplyDefaultTheme()
        {
            try
            {
                // Apply theme-aware styling
                this.SetResourceReference(BackgroundProperty, "GanttSecondaryBackgroundBrush");
                this.SetResourceReference(BorderBrushProperty, "GanttGridLineBrush");
                this.SetResourceReference(BorderThicknessProperty, "GanttGridLineThickness");
                
                // Apply weekend/today styling based on properties
                UpdateCellStyling();
            }
            catch
            {
                // Fallback to hardcoded values if theme resources aren't available yet
                Background = System.Windows.Media.Brushes.White;
                BorderBrush = System.Windows.Media.Brushes.LightGray;
                BorderThickness = new System.Windows.Thickness(0.5);
            }
        }
        
        /// <summary>
        /// Updates cell styling based on IsWeekend and IsToday properties.
        /// </summary>
        private void UpdateCellStyling()
        {
            try
            {
                if (IsToday)
                {
                    this.SetResourceReference(BackgroundProperty, "GanttTodayBackgroundBrush");
                }
                else if (IsWeekend)
                {
                    this.SetResourceReference(BackgroundProperty, "GanttWeekendBackgroundBrush");
                }
                else
                {
                    this.SetResourceReference(BackgroundProperty, "GanttSecondaryBackgroundBrush");
                }
            }
            catch
            {
                // Fallback styling
                Background = IsToday ? System.Windows.Media.Brushes.LightYellow :
                            IsWeekend ? System.Windows.Media.Brushes.LightGray :
                            System.Windows.Media.Brushes.White;
            }
        }
    }
}