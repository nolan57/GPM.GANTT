using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPM.Gantt.Models;
using GPM.Gantt.Services;
using System.Windows.Media;

namespace GPM.Gantt.Tests
{
    /// <summary>
    /// Unit tests for theme management functionality.
    /// </summary>
    [TestClass]
    public class ThemeManagementTests
    {
        private IThemeService _themeService = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _themeService = new ThemeService();
        }

        [TestMethod]
        public void ThemeService_ShouldProvideBuiltInThemes()
        {
            // Arrange & Act
            var availableThemes = _themeService.GetAvailableThemes().ToList();

            // Assert
            Assert.IsTrue(availableThemes.Contains("Default"));
            Assert.IsTrue(availableThemes.Contains("Dark"));
            Assert.IsTrue(availableThemes.Contains("Light"));
            Assert.IsTrue(availableThemes.Contains("Modern"));
            Assert.AreEqual(4, availableThemes.Count);
        }

        [TestMethod]
        public void ThemeService_ShouldReturnValidDefaultTheme()
        {
            // Arrange & Act
            var defaultTheme = _themeService.GetTheme("Default");

            // Assert
            Assert.IsNotNull(defaultTheme);
            Assert.AreEqual("Default", defaultTheme.Name);
            Assert.IsNotNull(defaultTheme.Background);
            Assert.IsNotNull(defaultTheme.Grid);
            Assert.IsNotNull(defaultTheme.Task);
            Assert.IsNotNull(defaultTheme.TimeScale);
            Assert.IsNotNull(defaultTheme.Selection);
        }

        [TestMethod]
        public void ThemeService_ShouldReturnValidDarkTheme()
        {
            // Arrange & Act
            var darkTheme = _themeService.GetTheme("Dark");

            // Assert
            Assert.IsNotNull(darkTheme);
            Assert.AreEqual("Dark", darkTheme.Name);
            Assert.AreEqual(Color.FromRgb(30, 30, 30), darkTheme.Background.PrimaryColor);
            Assert.AreEqual(Color.FromRgb(40, 40, 40), darkTheme.TimeScale.BackgroundColor);
        }

        [TestMethod]
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
            Assert.IsNotNull(retrievedTheme);
            Assert.AreEqual("CustomBlue", retrievedTheme.Name);
        }

        [TestMethod]
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
            Assert.IsTrue(themeChangedEventFired);
            Assert.IsNotNull(newTheme);
            Assert.AreEqual("Dark", currentTheme.Name);
            Assert.AreEqual("Dark", newTheme.Name);
        }

        [TestMethod]
        public void ThemeManager_ShouldProvideStaticAccess()
        {
            // Arrange & Act
            var availableThemes = ThemeManager.GetAvailableThemes().ToList();
            var defaultTheme = ThemeManager.GetTheme("Default");
            var currentTheme = ThemeManager.GetCurrentTheme();

            // Assert
            Assert.IsTrue(availableThemes.Count >= 4);
            Assert.IsNotNull(defaultTheme);
            Assert.IsNotNull(currentTheme);
        }

        [TestMethod]
        public void ThemeFactory_ShouldCreateAllBuiltInThemes()
        {
            // Arrange & Act
            var defaultTheme = GanttThemeFactory.CreateDefault();
            var darkTheme = GanttThemeFactory.CreateDark();
            var lightTheme = GanttThemeFactory.CreateLight();
            var modernTheme = GanttThemeFactory.CreateModern();

            // Assert
            Assert.AreEqual("Default", defaultTheme.Name);
            Assert.AreEqual("Dark", darkTheme.Name);
            Assert.AreEqual("Light", lightTheme.Name);
            Assert.AreEqual("Modern", modernTheme.Name);
        }

        [TestMethod]
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
            Assert.AreEqual("CustomPalette", customTheme.Name);
            Assert.AreEqual(primaryColor, customTheme.Task.DefaultColor);
            Assert.AreEqual(secondaryColor, customTheme.Task.CompletedColor);
        }

        [TestMethod]
        public void ThemeClone_ShouldCreateIndependentCopy()
        {
            // Arrange
            var originalTheme = GanttThemeFactory.CreateDefault();
            
            // Act
            var clonedTheme = originalTheme.Clone();
            clonedTheme.Name = "ModifiedClone";
            clonedTheme.Task.DefaultColor = Colors.Red;

            // Assert
            Assert.AreEqual("Default", originalTheme.Name);
            Assert.AreEqual("ModifiedClone", clonedTheme.Name);
            Assert.AreNotEqual(originalTheme.Task.DefaultColor, clonedTheme.Task.DefaultColor);
        }
    }
}