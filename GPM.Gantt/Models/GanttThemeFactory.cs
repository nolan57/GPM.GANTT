using System.Windows;
using System.Windows.Media;

namespace GPM.Gantt.Models
{
    /// <summary>
    /// Factory class for creating built-in themes and custom themes for the Gantt chart.
    /// Provides predefined theme configurations and utilities for theme creation.
    /// </summary>
    public static class GanttThemeFactory
    {
        /// <summary>
        /// Creates the default professional theme.
        /// </summary>
        /// <returns>A GanttTheme configured with default professional appearance.</returns>
        public static GanttTheme CreateDefault()
        {
            return new GanttTheme
            {
                Name = "Default",
                Description = "Default professional theme with balanced colors and clear visual hierarchy",
                Background = new BackgroundTheme
                {
                    PrimaryColor = Colors.White,
                    SecondaryColor = Color.FromRgb(248, 249, 250),
                    AccentColor = Color.FromRgb(240, 248, 255)
                },
                Grid = new GridTheme
                {
                    LineColor = Color.FromRgb(224, 224, 224),
                    MajorLineColor = Color.FromRgb(189, 189, 189),
                    LineThickness = 1.0,
                    WeekendBackgroundColor = Color.FromRgb(250, 250, 250),
                    TodayBackgroundColor = Color.FromRgb(255, 248, 220)
                },
                Task = new TaskTheme
                {
                    DefaultColor = Color.FromRgb(33, 150, 243),
                    CompletedColor = Color.FromRgb(76, 175, 80),
                    OverdueColor = Color.FromRgb(244, 67, 54),
                    InProgressColor = Color.FromRgb(255, 193, 7),
                    BorderColor = Color.FromRgb(25, 118, 210),
                    CornerRadius = 4.0,
                    BorderThickness = 1.0,
                    TextColor = Colors.White,
                    FontFamily = "Segoe UI",
                    FontSize = 12.0,
                    EnableShadow = false
                },
                TimeScale = new TimeScaleTheme
                {
                    BackgroundColor = Color.FromRgb(248, 249, 250),
                    TextColor = Color.FromRgb(33, 37, 41),
                    BorderColor = Color.FromRgb(222, 226, 230),
                    TodayMarkerColor = Color.FromRgb(255, 193, 7),
                    FontFamily = "Segoe UI",
                    FontSize = 11.0,
                    FontWeight = FontWeights.SemiBold,
                    BorderThickness = 1.0
                },
                Selection = new SelectionTheme
                {
                    BorderColor = Color.FromRgb(255, 109, 0),
                    BorderThickness = 2.0,
                    OverlayColor = Color.FromArgb(30, 255, 109, 0),
                    HoverColor = Color.FromArgb(50, 33, 150, 243)
                }
            };
        }

        /// <summary>
        /// Creates a dark theme optimized for low-light environments.
        /// </summary>
        /// <returns>A GanttTheme configured with dark appearance.</returns>
        public static GanttTheme CreateDark()
        {
            return new GanttTheme
            {
                Name = "Dark",
                Description = "Dark theme optimized for low-light environments with high contrast",
                Background = new BackgroundTheme
                {
                    PrimaryColor = Color.FromRgb(30, 30, 30),
                    SecondaryColor = Color.FromRgb(45, 45, 45),
                    AccentColor = Color.FromRgb(38, 38, 38)
                },
                Grid = new GridTheme
                {
                    LineColor = Color.FromRgb(60, 60, 60),
                    MajorLineColor = Color.FromRgb(80, 80, 80),
                    LineThickness = 1.0,
                    WeekendBackgroundColor = Color.FromRgb(35, 35, 35),
                    TodayBackgroundColor = Color.FromRgb(60, 50, 30)
                },
                Task = new TaskTheme
                {
                    DefaultColor = Color.FromRgb(100, 181, 246),
                    CompletedColor = Color.FromRgb(129, 199, 132),
                    OverdueColor = Color.FromRgb(239, 154, 154),
                    InProgressColor = Color.FromRgb(255, 204, 128),
                    BorderColor = Color.FromRgb(66, 165, 245),
                    CornerRadius = 4.0,
                    BorderThickness = 1.0,
                    TextColor = Color.FromRgb(240, 240, 240),
                    FontFamily = "Segoe UI",
                    FontSize = 12.0,
                    EnableShadow = true
                },
                TimeScale = new TimeScaleTheme
                {
                    BackgroundColor = Color.FromRgb(40, 40, 40),
                    TextColor = Color.FromRgb(220, 220, 220),
                    BorderColor = Color.FromRgb(70, 70, 70),
                    TodayMarkerColor = Color.FromRgb(255, 193, 7),
                    FontFamily = "Segoe UI",
                    FontSize = 11.0,
                    FontWeight = FontWeights.SemiBold,
                    BorderThickness = 1.0
                },
                Selection = new SelectionTheme
                {
                    BorderColor = Color.FromRgb(255, 183, 77),
                    BorderThickness = 2.0,
                    OverlayColor = Color.FromArgb(40, 255, 183, 77),
                    HoverColor = Color.FromArgb(60, 100, 181, 246)
                }
            };
        }

        /// <summary>
        /// Creates a light theme with high contrast for improved readability.
        /// </summary>
        /// <returns>A GanttTheme configured with light high-contrast appearance.</returns>
        public static GanttTheme CreateLight()
        {
            return new GanttTheme
            {
                Name = "Light",
                Description = "Light theme with high contrast for improved readability",
                Background = new BackgroundTheme
                {
                    PrimaryColor = Color.FromRgb(255, 255, 255),
                    SecondaryColor = Color.FromRgb(250, 250, 250),
                    AccentColor = Color.FromRgb(245, 247, 250)
                },
                Grid = new GridTheme
                {
                    LineColor = Color.FromRgb(200, 200, 200),
                    MajorLineColor = Color.FromRgb(150, 150, 150),
                    LineThickness = 1.0,
                    WeekendBackgroundColor = Color.FromRgb(248, 248, 248),
                    TodayBackgroundColor = Color.FromRgb(255, 252, 232)
                },
                Task = new TaskTheme
                {
                    DefaultColor = Color.FromRgb(13, 71, 161),
                    CompletedColor = Color.FromRgb(46, 125, 50),
                    OverdueColor = Color.FromRgb(211, 47, 47),
                    InProgressColor = Color.FromRgb(245, 124, 0),
                    BorderColor = Color.FromRgb(21, 101, 192),
                    CornerRadius = 2.0,
                    BorderThickness = 2.0,
                    TextColor = Colors.White,
                    FontFamily = "Segoe UI",
                    FontSize = 12.0,
                    EnableShadow = false
                },
                TimeScale = new TimeScaleTheme
                {
                    BackgroundColor = Color.FromRgb(245, 245, 245),
                    TextColor = Color.FromRgb(0, 0, 0),
                    BorderColor = Color.FromRgb(180, 180, 180),
                    TodayMarkerColor = Color.FromRgb(255, 143, 0),
                    FontFamily = "Segoe UI",
                    FontSize = 11.0,
                    FontWeight = FontWeights.Bold,
                    BorderThickness = 1.0
                },
                Selection = new SelectionTheme
                {
                    BorderColor = Color.FromRgb(255, 87, 34),
                    BorderThickness = 3.0,
                    OverlayColor = Color.FromArgb(25, 255, 87, 34),
                    HoverColor = Color.FromArgb(40, 13, 71, 161)
                }
            };
        }

        /// <summary>
        /// Creates a modern theme with flat design and vibrant colors.
        /// </summary>
        /// <returns>A GanttTheme configured with modern flat design appearance.</returns>
        public static GanttTheme CreateModern()
        {
            return new GanttTheme
            {
                Name = "Modern",
                Description = "Modern flat design with vibrant colors and contemporary styling",
                Background = new BackgroundTheme
                {
                    PrimaryColor = Color.FromRgb(250, 251, 252),
                    SecondaryColor = Color.FromRgb(241, 245, 249),
                    AccentColor = Color.FromRgb(236, 242, 249)
                },
                Grid = new GridTheme
                {
                    LineColor = Color.FromRgb(226, 232, 240),
                    MajorLineColor = Color.FromRgb(203, 213, 225),
                    LineThickness = 0.5,
                    WeekendBackgroundColor = Color.FromRgb(248, 250, 252),
                    TodayBackgroundColor = Color.FromRgb(254, 249, 195)
                },
                Task = new TaskTheme
                {
                    DefaultColor = Color.FromRgb(59, 130, 246),
                    CompletedColor = Color.FromRgb(34, 197, 94),
                    OverdueColor = Color.FromRgb(239, 68, 68),
                    InProgressColor = Color.FromRgb(251, 146, 60),
                    BorderColor = Color.FromRgb(37, 99, 235),
                    CornerRadius = 6.0,
                    BorderThickness = 0.5,
                    TextColor = Colors.White,
                    FontFamily = "Segoe UI",
                    FontSize = 12.0,
                    EnableShadow = true
                },
                TimeScale = new TimeScaleTheme
                {
                    BackgroundColor = Color.FromRgb(241, 245, 249),
                    TextColor = Color.FromRgb(51, 65, 85),
                    BorderColor = Color.FromRgb(203, 213, 225),
                    TodayMarkerColor = Color.FromRgb(245, 158, 11),
                    FontFamily = "Segoe UI",
                    FontSize = 11.0,
                    FontWeight = FontWeights.Medium,
                    BorderThickness = 0.5
                },
                Selection = new SelectionTheme
                {
                    BorderColor = Color.FromRgb(245, 101, 101),
                    BorderThickness = 2.0,
                    OverlayColor = Color.FromArgb(35, 245, 101, 101),
                    HoverColor = Color.FromArgb(50, 59, 130, 246)
                }
            };
        }

        /// <summary>
        /// Creates a custom theme based on a primary color palette.
        /// </summary>
        /// <param name="primaryColor">The primary color for the theme.</param>
        /// <param name="secondaryColor">The secondary color for the theme.</param>
        /// <param name="accentColor">The accent color for the theme.</param>
        /// <param name="name">The name of the custom theme.</param>
        /// <returns>A GanttTheme configured with the specified color palette.</returns>
        public static GanttTheme CreateFromPalette(Color primaryColor, Color secondaryColor, Color accentColor, string name = "Custom")
        {
            var theme = CreateDefault();
            theme.Name = name;
            theme.Description = $"Custom theme based on {primaryColor} color palette";

            // Apply primary color to task bars
            theme.Task.DefaultColor = primaryColor;
            theme.Task.BorderColor = DarkenColor(primaryColor, 0.2f);

            // Apply secondary color to completed tasks
            theme.Task.CompletedColor = secondaryColor;

            // Apply accent color to highlights
            theme.TimeScale.TodayMarkerColor = accentColor;
            theme.Selection.BorderColor = accentColor;

            // Generate complementary colors
            theme.Background.AccentColor = LightenColor(primaryColor, 0.9f);
            theme.Grid.TodayBackgroundColor = LightenColor(accentColor, 0.8f);

            return theme;
        }

        /// <summary>
        /// Lightens a color by blending it with white.
        /// </summary>
        /// <param name="color">The color to lighten.</param>
        /// <param name="factor">The lightening factor (0.0 to 1.0).</param>
        /// <returns>The lightened color.</returns>
        private static Color LightenColor(Color color, float factor)
        {
            byte r = (byte)(color.R + (255 - color.R) * factor);
            byte g = (byte)(color.G + (255 - color.G) * factor);
            byte b = (byte)(color.B + (255 - color.B) * factor);
            return Color.FromRgb(r, g, b);
        }

        /// <summary>
        /// Darkens a color by blending it with black.
        /// </summary>
        /// <param name="color">The color to darken.</param>
        /// <param name="factor">The darkening factor (0.0 to 1.0).</param>
        /// <returns>The darkened color.</returns>
        private static Color DarkenColor(Color color, float factor)
        {
            byte r = (byte)(color.R * (1 - factor));
            byte g = (byte)(color.G * (1 - factor));
            byte b = (byte)(color.B * (1 - factor));
            return Color.FromRgb(r, g, b);
        }
    }
}