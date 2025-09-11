using System.Windows.Media;
using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Static class for managing Gantt chart themes globally.
    /// Provides a simplified API for theme operations and maintains a singleton theme service.
    /// </summary>
    public static class ThemeManager
    {
        private static readonly Lazy<IThemeService> _themeService = new(() => new ThemeService());
        private static readonly Lazy<IDesignTokenService> _designTokenService = new(() => new DesignTokenService(Instance));

        /// <summary>
        /// Gets the singleton theme service instance.
        /// </summary>
        public static IThemeService Instance => _themeService.Value;

        /// <summary>
        /// Gets the singleton design token service instance for cross-component reusable tokens.
        /// </summary>
        public static IDesignTokenService Tokens => _designTokenService.Value;

        /// <summary>
        /// Event raised when the current theme changes.
        /// </summary>
        public static event EventHandler<ThemeChangedEventArgs>? ThemeChanged
        {
            add => Instance.ThemeChanged += value;
            remove => Instance.ThemeChanged -= value;
        }

        /// <summary>
        /// Gets all available theme names.
        /// </summary>
        /// <returns>A collection of theme names.</returns>
        public static IEnumerable<string> GetAvailableThemes() => Instance.GetAvailableThemes();

        /// <summary>
        /// Gets a theme by its name.
        /// </summary>
        /// <param name="themeName">The name of the theme to retrieve.</param>
        /// <returns>The theme instance, or the default theme if not found.</returns>
        public static GanttTheme GetTheme(string themeName) => Instance.GetTheme(themeName);

        /// <summary>
        /// Registers a new theme or updates an existing one.
        /// </summary>
        /// <param name="theme">The theme to register.</param>
        public static void RegisterTheme(GanttTheme theme) => Instance.RegisterTheme(theme);

        /// <summary>
        /// Removes a theme from the registry.
        /// </summary>
        /// <param name="themeName">The name of the theme to remove.</param>
        /// <returns>True if the theme was removed, false if it didn't exist.</returns>
        public static bool RemoveTheme(string themeName) => Instance.RemoveTheme(themeName);

        /// <summary>
        /// Gets the current active theme.
        /// </summary>
        /// <returns>The currently active theme.</returns>
        public static GanttTheme GetCurrentTheme() => Instance.GetCurrentTheme();

        /// <summary>
        /// Sets the current active theme.
        /// </summary>
        /// <param name="themeName">The name of the theme to set as active.</param>
        public static void SetCurrentTheme(string themeName) => Instance.SetCurrentTheme(themeName);

        /// <summary>
        /// Creates a custom theme based on a color palette.
        /// </summary>
        /// <param name="name">The name for the custom theme.</param>
        /// <param name="primaryColor">The primary color.</param>
        /// <param name="secondaryColor">The secondary color.</param>
        /// <param name="accentColor">The accent color.</param>
        /// <returns>The created custom theme.</returns>
        public static GanttTheme CreateCustomTheme(string name, Color primaryColor, Color secondaryColor, Color accentColor)
            => Instance.CreateCustomTheme(name, primaryColor, secondaryColor, accentColor);

        /// <summary>
        /// Creates a custom theme with a configuration action.
        /// </summary>
        /// <param name="name">The name for the custom theme.</param>
        /// <param name="configure">Action to configure the theme.</param>
        /// <returns>The created custom theme.</returns>
        public static GanttTheme CreateCustomTheme(string name, Action<GanttTheme> configure)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Theme name cannot be null or empty.", nameof(name));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var theme = GanttThemeFactory.CreateDefault();
            theme.Name = name;
            theme.Description = $"Custom theme: {name}";
            
            configure(theme);
            
            Instance.RegisterTheme(theme);
            return theme.Clone();
        }

        /// <summary>
        /// Gets built-in theme names.
        /// </summary>
        /// <returns>A collection of built-in theme names.</returns>
        public static IEnumerable<string> GetBuiltInThemes()
        {
            return new[] { "Default", "Dark", "Light", "Modern" };
        }

        /// <summary>
        /// Checks if a theme is a built-in theme.
        /// </summary>
        /// <param name="themeName">The name of the theme to check.</param>
        /// <returns>True if the theme is built-in, false otherwise.</returns>
        public static bool IsBuiltInTheme(string themeName)
        {
            return GetBuiltInThemes().Contains(themeName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Resets all themes to built-in themes only.
        /// </summary>
        public static void ResetToBuiltInThemes()
        {
            // Get all custom themes
            var allThemes = GetAvailableThemes().ToList();
            var builtInThemes = GetBuiltInThemes();
            var customThemes = allThemes.Except(builtInThemes).ToList();

            // Remove all custom themes
            foreach (var customTheme in customThemes)
            {
                RemoveTheme(customTheme);
            }

            // Reset to default theme
            SetCurrentTheme("Default");
        }

        /// <summary>
        /// Exports a theme to a configuration object that can be serialized.
        /// </summary>
        /// <param name="themeName">The name of the theme to export.</param>
        /// <returns>The theme instance ready for serialization.</returns>
        public static GanttTheme ExportTheme(string themeName)
        {
            return GetTheme(themeName);
        }

        /// <summary>
        /// Imports a theme from a configuration object.
        /// </summary>
        /// <param name="theme">The theme to import.</param>
        public static void ImportTheme(GanttTheme theme)
        {
            if (theme == null)
                throw new ArgumentNullException(nameof(theme));

            RegisterTheme(theme);
        }
    }
}