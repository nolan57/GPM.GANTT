using System.Collections.Concurrent;
using System.Windows.Media;
using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Implementation of the theme service for managing Gantt chart themes.
    /// Provides thread-safe theme management with built-in themes and custom theme support.
    /// </summary>
    public class ThemeService : IThemeService
    {
        private readonly ConcurrentDictionary<string, GanttTheme> _themes;
        private GanttTheme _currentTheme;
        private readonly object _currentThemeLock = new();

        /// <summary>
        /// Event raised when the current theme changes.
        /// </summary>
        public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

        /// <summary>
        /// Initializes a new instance of the ThemeService class.
        /// </summary>
        public ThemeService()
        {
            _themes = new ConcurrentDictionary<string, GanttTheme>();
            _currentTheme = GanttThemeFactory.CreateDefault();
            
            // Register built-in themes
            RegisterBuiltInThemes();
        }

        /// <summary>
        /// Gets all available theme names.
        /// </summary>
        /// <returns>A collection of theme names.</returns>
        public IEnumerable<string> GetAvailableThemes()
        {
            return _themes.Keys.OrderBy(name => name);
        }

        /// <summary>
        /// Gets a theme by its name.
        /// </summary>
        /// <param name="themeName">The name of the theme to retrieve.</param>
        /// <returns>The theme instance, or the default theme if not found.</returns>
        public GanttTheme GetTheme(string themeName)
        {
            if (string.IsNullOrWhiteSpace(themeName))
                return GanttThemeFactory.CreateDefault();

            return _themes.TryGetValue(themeName, out var theme) 
                ? theme.Clone() 
                : GanttThemeFactory.CreateDefault();
        }

        /// <summary>
        /// Registers a new theme or updates an existing one.
        /// </summary>
        /// <param name="theme">The theme to register.</param>
        public void RegisterTheme(GanttTheme theme)
        {
            if (theme == null)
                throw new ArgumentNullException(nameof(theme));

            if (string.IsNullOrWhiteSpace(theme.Name))
                throw new ArgumentException("Theme name cannot be null or empty.", nameof(theme));

            _themes.AddOrUpdate(theme.Name, theme.Clone(), (key, existing) => theme.Clone());
        }

        /// <summary>
        /// Removes a theme from the registry.
        /// </summary>
        /// <param name="themeName">The name of the theme to remove.</param>
        /// <returns>True if the theme was removed, false if it didn't exist.</returns>
        public bool RemoveTheme(string themeName)
        {
            if (string.IsNullOrWhiteSpace(themeName))
                return false;

            // Prevent removal of built-in themes
            var builtInThemes = new[] { "Default", "Dark", "Light", "Modern" };
            if (builtInThemes.Contains(themeName))
                return false;

            var removed = _themes.TryRemove(themeName, out _);

            // If the removed theme was the current theme, reset to default
            lock (_currentThemeLock)
            {
                if (removed && _currentTheme.Name == themeName)
                {
                    SetCurrentTheme("Default");
                }
            }

            return removed;
        }

        /// <summary>
        /// Gets the current active theme.
        /// </summary>
        /// <returns>The currently active theme.</returns>
        public GanttTheme GetCurrentTheme()
        {
            lock (_currentThemeLock)
            {
                return _currentTheme.Clone();
            }
        }

        /// <summary>
        /// Sets the current active theme.
        /// </summary>
        /// <param name="themeName">The name of the theme to set as active.</param>
        public void SetCurrentTheme(string themeName)
        {
            if (string.IsNullOrWhiteSpace(themeName))
                return;

            var newTheme = GetTheme(themeName);
            GanttTheme? previousTheme;

            lock (_currentThemeLock)
            {
                if (_currentTheme.Name == newTheme.Name)
                    return; // No change needed

                previousTheme = _currentTheme.Clone();
                _currentTheme = newTheme;
            }

            // Raise the theme changed event
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(previousTheme, newTheme));
        }

        /// <summary>
        /// Creates a custom theme based on a color palette.
        /// </summary>
        /// <param name="name">The name for the custom theme.</param>
        /// <param name="primaryColor">The primary color.</param>
        /// <param name="secondaryColor">The secondary color.</param>
        /// <param name="accentColor">The accent color.</param>
        /// <returns>The created custom theme.</returns>
        public GanttTheme CreateCustomTheme(string name, Color primaryColor, Color secondaryColor, Color accentColor)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Theme name cannot be null or empty.", nameof(name));

            var customTheme = GanttThemeFactory.CreateFromPalette(primaryColor, secondaryColor, accentColor, name);
            RegisterTheme(customTheme);
            return customTheme.Clone();
        }

        /// <summary>
        /// Registers all built-in themes.
        /// </summary>
        private void RegisterBuiltInThemes()
        {
            RegisterTheme(GanttThemeFactory.CreateDefault());
            RegisterTheme(GanttThemeFactory.CreateDark());
            RegisterTheme(GanttThemeFactory.CreateLight());
            RegisterTheme(GanttThemeFactory.CreateModern());
        }
    }
}