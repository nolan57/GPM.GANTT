using System.Windows;
using System.Windows.Media;
using GPM.Gantt.Models;

namespace GPM.Gantt.Utilities
{
    /// <summary>
    /// Utility class for converting theme models to WPF visual elements.
    /// Provides helper methods for applying theme configurations to UI controls.
    /// </summary>
    public static class ThemeUtilities
    {
        /// <summary>
        /// Converts a Color to a SolidColorBrush.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>A SolidColorBrush with the specified color.</returns>
        public static SolidColorBrush ToBrush(this Color color)
        {
            return new SolidColorBrush(color);
        }

        /// <summary>
        /// Converts a Color to a SolidColorBrush with specified opacity.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <param name="opacity">The opacity value (0.0 to 1.0).</param>
        /// <returns>A SolidColorBrush with the specified color and opacity.</returns>
        public static SolidColorBrush ToBrush(this Color color, double opacity)
        {
            var brush = new SolidColorBrush(color);
            brush.Opacity = opacity;
            return brush;
        }

        /// <summary>
        /// Converts a TaskTheme to a Style for task bars.
        /// </summary>
        /// <param name="taskTheme">The task theme configuration.</param>
        /// <returns>A Style configured for task bars.</returns>
        public static Style CreateTaskBarStyle(TaskTheme taskTheme)
        {
            var style = new Style(typeof(FrameworkElement));
            
            style.Setters.Add(new Setter(FrameworkElement.TagProperty, taskTheme.DefaultColor.ToBrush()));
            
            // Add data triggers for different task statuses if needed
            // This can be extended based on specific task bar control implementation
            
            return style;
        }

        /// <summary>
        /// Converts a GridTheme to a Style for grid cells.
        /// </summary>
        /// <param name="gridTheme">The grid theme configuration.</param>
        /// <returns>A Style configured for grid cells.</returns>
        public static Style CreateGridCellStyle(GridTheme gridTheme)
        {
            var style = new Style(typeof(FrameworkElement));
            
            // Apply grid styling properties
            // This can be extended based on specific grid cell control implementation
            
            return style;
        }

        /// <summary>
        /// Converts a TimeScaleTheme to a Style for time scale cells.
        /// </summary>
        /// <param name="timeScaleTheme">The time scale theme configuration.</param>
        /// <returns>A Style configured for time scale cells.</returns>
        public static Style CreateTimeScaleCellStyle(TimeScaleTheme timeScaleTheme)
        {
            var style = new Style(typeof(FrameworkElement));
            
            // Apply time scale styling properties
            // This can be extended based on specific time scale control implementation
            
            return style;
        }

        /// <summary>
        /// Creates a Thickness object from a uniform value.
        /// </summary>
        /// <param name="uniformThickness">The uniform thickness value.</param>
        /// <returns>A Thickness with all sides set to the specified value.</returns>
        public static Thickness ToThickness(double uniformThickness)
        {
            return new Thickness(uniformThickness);
        }

        /// <summary>
        /// Creates a CornerRadius object from a uniform value.
        /// </summary>
        /// <param name="uniformRadius">The uniform radius value.</param>
        /// <returns>A CornerRadius with all corners set to the specified value.</returns>
        public static CornerRadius ToCornerRadius(double uniformRadius)
        {
            return new CornerRadius(uniformRadius);
        }

        /// <summary>
        /// Gets a resource dictionary containing theme-based brushes.
        /// </summary>
        /// <param name="theme">The theme to convert to resources.</param>
        /// <returns>A ResourceDictionary with theme brushes.</returns>
        public static ResourceDictionary CreateThemeResourceDictionary(GanttTheme theme)
        {
            var resources = new ResourceDictionary();

            // Background brushes
            resources["GanttBackgroundBrush"] = theme.Background.PrimaryColor.ToBrush();
            resources["GanttSecondaryBackgroundBrush"] = theme.Background.SecondaryColor.ToBrush();
            resources["GanttAccentBackgroundBrush"] = theme.Background.AccentColor.ToBrush();

            // Grid brushes
            resources["GanttGridLineBrush"] = theme.Grid.LineColor.ToBrush();
            resources["GanttMajorGridLineBrush"] = theme.Grid.MajorLineColor.ToBrush();
            resources["GanttWeekendBackgroundBrush"] = theme.Grid.WeekendBackgroundColor.ToBrush();
            resources["GanttTodayBackgroundBrush"] = theme.Grid.TodayBackgroundColor.ToBrush();

            // Task brushes
            resources["GanttTaskDefaultBrush"] = theme.Task.DefaultColor.ToBrush();
            resources["GanttTaskCompletedBrush"] = theme.Task.CompletedColor.ToBrush();
            resources["GanttTaskOverdueBrush"] = theme.Task.OverdueColor.ToBrush();
            resources["GanttTaskInProgressBrush"] = theme.Task.InProgressColor.ToBrush();
            resources["GanttTaskBorderBrush"] = theme.Task.BorderColor.ToBrush();
            resources["GanttTaskTextBrush"] = theme.Task.TextColor.ToBrush();

            // Time scale brushes
            resources["GanttTimeScaleBackgroundBrush"] = theme.TimeScale.BackgroundColor.ToBrush();
            resources["GanttTimeScaleTextBrush"] = theme.TimeScale.TextColor.ToBrush();
            resources["GanttTimeScaleBorderBrush"] = theme.TimeScale.BorderColor.ToBrush();
            resources["GanttTodayMarkerBrush"] = theme.TimeScale.TodayMarkerColor.ToBrush();

            // Selection brushes
            resources["GanttSelectionBorderBrush"] = theme.Selection.BorderColor.ToBrush();
            resources["GanttSelectionOverlayBrush"] = theme.Selection.OverlayColor.ToBrush();
            resources["GanttHoverBrush"] = theme.Selection.HoverColor.ToBrush();

            // Thickness values
            resources["GanttGridLineThickness"] = ToThickness(theme.Grid.LineThickness);
            resources["GanttTaskBorderThickness"] = ToThickness(theme.Task.BorderThickness);
            resources["GanttTimeScaleBorderThickness"] = ToThickness(theme.TimeScale.BorderThickness);
            resources["GanttSelectionBorderThickness"] = ToThickness(theme.Selection.BorderThickness);

            // Corner radius
            resources["GanttTaskCornerRadius"] = ToCornerRadius(theme.Task.CornerRadius);

            // Font properties
            resources["GanttTaskFontFamily"] = new FontFamily(theme.Task.FontFamily);
            resources["GanttTaskFontSize"] = theme.Task.FontSize;
            resources["GanttTimeScaleFontFamily"] = new FontFamily(theme.TimeScale.FontFamily);
            resources["GanttTimeScaleFontSize"] = theme.TimeScale.FontSize;
            resources["GanttTimeScaleFontWeight"] = theme.TimeScale.FontWeight;

            return resources;
        }

        /// <summary>
        /// Applies a theme to a FrameworkElement by merging theme resources.
        /// </summary>
        /// <param name="element">The element to apply the theme to.</param>
        /// <param name="theme">The theme to apply.</param>
        public static void ApplyTheme(FrameworkElement element, GanttTheme theme)
        {
            if (element == null || theme == null)
                return;

            var themeResources = CreateThemeResourceDictionary(theme);
            
            // Clear existing theme resources and add new ones
            element.Resources.Clear();
            element.Resources.MergedDictionaries.Add(themeResources);
        }
    }
}