using Xunit;
using GPM.Gantt.Models;
using GPM.Gantt.Services;
using System.Windows.Media;
using System.Linq;

namespace GPM.Gantt.Tests
{
    /// <summary>
    /// Unit tests for theme management functionality.
    /// </summary>
    public class ThemeManagementTests
    {
        private IThemeService _themeService = null!;

        public ThemeManagementTests()
        {
            _themeService = new ThemeService();
        }

        [Fact]
        public void ThemeService_ShouldProvideBuiltInThemes()
        {
            // Arrange & Act
            var availableThemes = _themeService.GetAvailableThemes().ToList();

            // Assert
            Assert.Contains("Default", availableThemes);
            Assert.Contains("Dark", availableThemes);
            Assert.Contains("Light", availableThemes);
            Assert.Contains("Modern", availableThemes);
            Assert.Equal(4, availableThemes.Count);
        }

        [Fact]
        public void ThemeService_ShouldReturnValidDefaultTheme()
        {
            // Arrange & Act
            var defaultTheme = _themeService.GetTheme("Default");

            // Assert
            Assert.NotNull(defaultTheme);
            Assert.Equal("Default", defaultTheme.Name);
            Assert.NotNull(defaultTheme.Background);
            Assert.NotNull(defaultTheme.Grid);
            Assert.NotNull(defaultTheme.Task);
            Assert.NotNull(defaultTheme.TimeScale);
            Assert.NotNull(defaultTheme.Selection);
        }

        [Fact]
        public void ThemeService_ShouldReturnValidDarkTheme()
        {
            // Arrange & Act
            var darkTheme = _themeService.GetTheme("Dark");

            // Assert
            Assert.NotNull(darkTheme);
            Assert.Equal("Dark", darkTheme.Name);
            Assert.Equal(Color.FromRgb(30, 30, 30), darkTheme.Background.PrimaryColor);
            Assert.Equal(Color.FromRgb(40, 40, 40), darkTheme.TimeScale.BackgroundColor);
        }

        [Fact]
        public void ThemeService_ShouldAllowCustomThemeCreation()
        {
            // Arrange
            var customTheme = _themeService.CreateCustomTheme(
                "CustomBlue", 
                Colors.Blue, 
                Colors.Green, 
                Colors.Orange);

            // Act
            var retrievedTheme = _themeService.GetTheme("CustomBlue");

            // Assert
            Assert.NotNull(retrievedTheme);
            Assert.Equal("CustomBlue", retrievedTheme.Name);
        }

        [Fact]
        public void ThemeService_ShouldHandleCurrentThemeChanges()
        {
            // Arrange
            var initialTheme = _themeService.GetCurrentTheme();
            bool themeChangedEventFired = false;
            GanttTheme? newTheme = null;

            _themeService.ThemeChanged += (sender, e) =>
            {
                themeChangedEventFired = true;
                newTheme = e.CurrentTheme;
            };

            // Act
            _themeService.SetCurrentTheme("Dark");
            var currentTheme = _themeService.GetCurrentTheme();

            // Assert
            Assert.True(themeChangedEventFired);
            Assert.NotNull(newTheme);
            Assert.Equal("Dark", currentTheme.Name);
            Assert.Equal("Dark", newTheme.Name);
        }

        [Fact]
        public void ThemeManager_ShouldProvideStaticAccess()
        {
            // Arrange & Act
            var availableThemes = ThemeManager.GetAvailableThemes().ToList();
            var defaultTheme = ThemeManager.GetTheme("Default");
            var currentTheme = ThemeManager.GetCurrentTheme();

            // Assert
            Assert.True(availableThemes.Count >= 4);
            Assert.NotNull(defaultTheme);
            Assert.NotNull(currentTheme);
        }

        [Fact]
        public void ThemeFactory_ShouldCreateAllBuiltInThemes()
        {
            // Arrange & Act
            var defaultTheme = GanttThemeFactory.CreateDefault();
            var darkTheme = GanttThemeFactory.CreateDark();
            var lightTheme = GanttThemeFactory.CreateLight();
            var modernTheme = GanttThemeFactory.CreateModern();

            // Assert
            Assert.Equal("Default", defaultTheme.Name);
            Assert.Equal("Dark", darkTheme.Name);
            Assert.Equal("Light", lightTheme.Name);
            Assert.Equal("Modern", modernTheme.Name);
        }

        [Fact]
        public void ThemeFactory_ShouldCreateCustomPaletteTheme()
        {
            // Arrange
            var primaryColor = Colors.Purple;
            var secondaryColor = Colors.Orange;
            var accentColor = Colors.Yellow;

            // Act
            var customTheme = GanttThemeFactory.CreateFromPalette(
                primaryColor, secondaryColor, accentColor, "CustomPalette");

            // Assert
            Assert.Equal("CustomPalette", customTheme.Name);
            Assert.Equal(primaryColor, customTheme.Task.DefaultColor);
            Assert.Equal(secondaryColor, customTheme.Task.CompletedColor);
        }

        [Fact]
        public void ThemeClone_ShouldCreateIndependentCopy()
        {
            // Arrange
            var originalTheme = GanttThemeFactory.CreateDefault();
            
            // Act
            var clonedTheme = originalTheme.Clone();
            clonedTheme.Name = "ModifiedClone";
            clonedTheme.Task.DefaultColor = Colors.Red;

            // Assert
            Assert.Equal("Default", originalTheme.Name);
            Assert.Equal("ModifiedClone", clonedTheme.Name);
            Assert.NotEqual(originalTheme.Task.DefaultColor, clonedTheme.Task.DefaultColor);
        }
    }
}