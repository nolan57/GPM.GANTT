using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Service interface for managing Gantt chart themes.
    /// Provides functionality for theme registration, retrieval, and application.
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Gets all available theme names.
        /// </summary>
        /// <returns>A collection of theme names.</returns>
        IEnumerable<string> GetAvailableThemes();

        /// <summary>
        /// Gets a theme by its name.
        /// </summary>
        /// <param name="themeName">The name of the theme to retrieve.</param>
        /// <returns>The theme instance, or the default theme if not found.</returns>
        GanttTheme GetTheme(string themeName);

        /// <summary>
        /// Registers a new theme or updates an existing one.
        /// </summary>
        /// <param name="theme">The theme to register.</param>
        void RegisterTheme(GanttTheme theme);

        /// <summary>
        /// Removes a theme from the registry.
        /// </summary>
        /// <param name="themeName">The name of the theme to remove.</param>
        /// <returns>True if the theme was removed, false if it didn't exist.</returns>
        bool RemoveTheme(string themeName);

        /// <summary>
        /// Gets the current active theme.
        /// </summary>
        /// <returns>The currently active theme.</returns>
        GanttTheme GetCurrentTheme();

        /// <summary>
        /// Sets the current active theme.
        /// </summary>
        /// <param name="themeName">The name of the theme to set as active.</param>
        void SetCurrentTheme(string themeName);

        /// <summary>
        /// Creates a custom theme based on a color palette.
        /// </summary>
        /// <param name="name">The name for the custom theme.</param>
        /// <param name="primaryColor">The primary color.</param>
        /// <param name="secondaryColor">The secondary color.</param>
        /// <param name="accentColor">The accent color.</param>
        /// <returns>The created custom theme.</returns>
        GanttTheme CreateCustomTheme(string name, System.Windows.Media.Color primaryColor, 
                                   System.Windows.Media.Color secondaryColor, System.Windows.Media.Color accentColor);

        /// <summary>
        /// Event raised when the current theme changes.
        /// </summary>
        event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
    }

    /// <summary>
    /// Event arguments for theme change events.
    /// </summary>
    public class ThemeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the previous theme.
        /// </summary>
        public GanttTheme? PreviousTheme { get; }

        /// <summary>
        /// Gets the new current theme.
        /// </summary>
        public GanttTheme CurrentTheme { get; }

        /// <summary>
        /// Initializes a new instance of the ThemeChangedEventArgs class.
        /// </summary>
        /// <param name="previousTheme">The previous theme.</param>
        /// <param name="currentTheme">The new current theme.</param>
        public ThemeChangedEventArgs(GanttTheme? previousTheme, GanttTheme currentTheme)
        {
            PreviousTheme = previousTheme;
            CurrentTheme = currentTheme;
        }
    }
}