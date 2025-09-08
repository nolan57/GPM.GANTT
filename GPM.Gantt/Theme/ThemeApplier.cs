using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GPM.Gantt.Models;

namespace GPM.Gantt.Theme
{
    /// <summary>
    /// Applies themes to Gantt chart elements.
    /// </summary>
    public static class ThemeApplier
    {
        /// <summary>
        /// Applies the specified theme to all child elements of a Gantt container.
        /// </summary>
        public static void ApplyThemeToChildren(GanttContainer container, GanttTheme currentTheme)
        {
            foreach (UIElement child in container.Children)
            {
                if (child is GanttTimeCell timeCell)
                {
                    ApplyThemeToTimeCell(timeCell, currentTheme);
                }
                else if (child is GanttGridCell gridCell)
                {
                    ApplyThemeToGridCell(gridCell, currentTheme);
                }
                else if (child is GanttGridRow gridRow)
                {
                    ApplyThemeToGridRow(gridRow, currentTheme);
                }
            }
        }
        
        /// <summary>
        /// Applies theme styling to a task bar.
        /// </summary>
        private static void ApplyThemeToTaskBar(GanttTaskBar taskBar, GanttTheme currentTheme)
        {
            try
            {
                // Determine background based on status
                Brush backgroundBrush = taskBar.Status switch
                {
                    GPM.Gantt.Models.TaskStatus.Completed => new SolidColorBrush(currentTheme.Task.CompletedColor),
                    GPM.Gantt.Models.TaskStatus.InProgress => new SolidColorBrush(currentTheme.Task.InProgressColor),
                    GPM.Gantt.Models.TaskStatus.Cancelled => new SolidColorBrush(currentTheme.Task.OverdueColor), // Use overdue color for cancelled
                    GPM.Gantt.Models.TaskStatus.OnHold => new SolidColorBrush(currentTheme.Task.OverdueColor), // Use overdue color for on hold
                    _ => new SolidColorBrush(currentTheme.Task.DefaultColor)
                };
                
                taskBar.Background = backgroundBrush;
                taskBar.BorderBrush = new SolidColorBrush(currentTheme.Task.BorderColor);
                taskBar.BorderThickness = new Thickness(currentTheme.Task.BorderThickness);
                // CornerRadius would need to be handled differently as Border.CornerRadius is not a dependency property
            }
            catch { /* Silently ignore if resources not available */ }
        }
        
        /// <summary>
        /// Applies theme styling to a time cell.
        /// </summary>
        private static void ApplyThemeToTimeCell(GanttTimeCell timeCell, GanttTheme currentTheme)
        {
            try
            {
                timeCell.Background = new SolidColorBrush(currentTheme.TimeScale.BackgroundColor);
                timeCell.BorderBrush = new SolidColorBrush(currentTheme.TimeScale.BorderColor);
                timeCell.BorderThickness = new Thickness(currentTheme.Grid.LineThickness);
                
                // Also update the text block inside
                if (timeCell.Child is TextBlock textBlock)
                {
                    textBlock.Foreground = new SolidColorBrush(currentTheme.TimeScale.TextColor);
                    textBlock.FontFamily = new FontFamily(currentTheme.TimeScale.FontFamily);
                    textBlock.FontSize = currentTheme.TimeScale.FontSize;
                    textBlock.FontWeight = currentTheme.TimeScale.FontWeight;
                }
            }
            catch { /* Silently ignore if resources not available */ }
        }
        
        /// <summary>
        /// Applies theme styling to a grid cell.
        /// </summary>
        private static void ApplyThemeToGridCell(GanttGridCell gridCell, GanttTheme currentTheme)
        {
            try
            {
                if (gridCell.IsToday)
                {
                    gridCell.Background = new SolidColorBrush(currentTheme.Grid.TodayBackgroundColor);
                }
                else if (gridCell.IsWeekend)
                {
                    gridCell.Background = new SolidColorBrush(currentTheme.Grid.WeekendBackgroundColor);
                }
                else
                {
                    gridCell.Background = new SolidColorBrush(currentTheme.Background.SecondaryColor);
                }
                
                gridCell.BorderBrush = new SolidColorBrush(currentTheme.Grid.LineColor);
                gridCell.BorderThickness = new Thickness(currentTheme.Grid.LineThickness);
            }
            catch { /* Silently ignore if resources not available */ }
        }
        
        /// <summary>
        /// Applies theme styling to a grid row.
        /// </summary>
        private static void ApplyThemeToGridRow(GanttGridRow gridRow, GanttTheme currentTheme)
        {
            try
            {
                gridRow.Background = new SolidColorBrush(currentTheme.Background.SecondaryColor);
                gridRow.BorderBrush = new SolidColorBrush(currentTheme.Grid.LineColor);
                gridRow.BorderThickness = new Thickness(currentTheme.Grid.LineThickness);
            }
            catch { /* Silently ignore if resources not available */ }
        }
    }
}