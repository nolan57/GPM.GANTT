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
                switch (child)
                {
                    case GanttTimeCell timeCell:
                        ApplyThemeToTimeCell(timeCell, currentTheme);
                        break;
                    case GanttGridCell gridCell:
                        ApplyThemeToGridCell(gridCell, currentTheme);
                        break;
                    case GanttGridRow gridRow:
                        ApplyThemeToGridRow(gridRow, currentTheme);
                        break;
                    case GanttTaskBar taskBar:
                        ApplyThemeToTaskBar(taskBar, currentTheme);
                        break;
                }
            }
        }
        
        /// <summary>
        /// Applies theme styling to a task bar.
        /// </summary>
        public static void ApplyThemeToTaskBar(GanttTaskBar taskBar, GanttTheme currentTheme)
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
                taskBar.Fill = backgroundBrush; // Make sure Fill is also set
                taskBar.BorderBrush = new SolidColorBrush(currentTheme.Task.BorderColor);
                taskBar.BorderThickness = new Thickness(currentTheme.Task.BorderThickness);
                taskBar.Stroke = new SolidColorBrush(currentTheme.Task.BorderColor); // Make sure Stroke is also set
                taskBar.StrokeThickness = currentTheme.Task.BorderThickness; // Make sure StrokeThickness is also set
                
                // Set corner radius if supported
                // Note: Directly setting CornerRadius on GanttTaskBar may not work as it's a FrameworkElement
                // We might need to handle this differently depending on the actual implementation
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme to task bar: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Applies theme styling to a time cell.
        /// </summary>
        public static void ApplyThemeToTimeCell(GanttTimeCell timeCell, GanttTheme currentTheme)
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme to time cell: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Applies theme styling to a grid cell.
        /// </summary>
        public static void ApplyThemeToGridCell(GanttGridCell gridCell, GanttTheme currentTheme)
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme to grid cell: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Applies theme styling to a grid row.
        /// </summary>
        public static void ApplyThemeToGridRow(GanttGridRow gridRow, GanttTheme currentTheme)
        {
            try
            {
                gridRow.Background = new SolidColorBrush(currentTheme.Background.SecondaryColor);
                gridRow.BorderBrush = new SolidColorBrush(currentTheme.Grid.LineColor);
                gridRow.BorderThickness = new Thickness(currentTheme.Grid.LineThickness);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme to grid row: {ex.Message}");
            }
        }
    }
}