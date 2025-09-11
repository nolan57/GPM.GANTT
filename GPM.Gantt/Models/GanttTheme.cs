using System.Windows;
using System.Windows.Media;

namespace GPM.Gantt.Models
{
    /// <summary>
    /// Represents a comprehensive theme configuration for the Gantt chart component.
    /// Contains all visual styling information including colors, fonts, and layout properties.
    /// </summary>
    public class GanttTheme
    {
        /// <summary>
        /// Gets or sets the unique name of the theme.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the theme.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the background theme configuration.
        /// </summary>
        public BackgroundTheme Background { get; set; } = new();

        /// <summary>
        /// Gets or sets the grid theme configuration.
        /// </summary>
        public GridTheme Grid { get; set; } = new();

        /// <summary>
        /// Gets or sets the task theme configuration.
        /// </summary>
        public TaskTheme Task { get; set; } = new();

        /// <summary>
        /// Gets or sets the time scale theme configuration.
        /// </summary>
        public TimeScaleTheme TimeScale { get; set; } = new();

        /// <summary>
        /// Gets or sets the selection theme configuration.
        /// </summary>
        public SelectionTheme Selection { get; set; } = new();

        /// <summary>
        /// Creates a deep copy of the current theme.
        /// </summary>
        /// <returns>A new GanttTheme instance with copied values.</returns>
        public GanttTheme Clone()
        {
            return new GanttTheme
            {
                Name = Name,
                Description = Description,
                Background = Background.Clone(),
                Grid = Grid.Clone(),
                Task = Task.Clone(),
                TimeScale = TimeScale.Clone(),
                Selection = Selection.Clone()
            };
        }
    }

    /// <summary>
    /// Configuration for background appearance of the Gantt chart.
    /// </summary>
    public class BackgroundTheme
    {
        /// <summary>
        /// Gets or sets the primary background color.
        /// </summary>
        public Color PrimaryColor { get; set; } = Colors.White;

        /// <summary>
        /// Gets or sets the secondary background color for alternate rows.
        /// </summary>
        public Color SecondaryColor { get; set; } = Color.FromRgb(248, 249, 250);

        /// <summary>
        /// Gets or sets the accent background color for special cells.
        /// </summary>
        public Color AccentColor { get; set; } = Color.FromRgb(240, 248, 255);

        /// <summary>
        /// Creates a copy of the current background theme.
        /// </summary>
        public BackgroundTheme Clone()
        {
            return new BackgroundTheme
            {
                PrimaryColor = PrimaryColor,
                SecondaryColor = SecondaryColor,
                AccentColor = AccentColor
            };
        }
    }

    /// <summary>
    /// Configuration for grid appearance in the Gantt chart.
    /// </summary>
    public class GridTheme
    {
        /// <summary>
        /// Gets or sets the color of regular grid lines.
        /// </summary>
        public Color LineColor { get; set; } = Color.FromRgb(224, 224, 224);

        /// <summary>
        /// Gets or sets the color of major grid lines.
        /// </summary>
        public Color MajorLineColor { get; set; } = Color.FromRgb(189, 189, 189);

        /// <summary>
        /// Gets or sets the thickness of grid lines.
        /// </summary>
        public double LineThickness { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the background color for weekend cells.
        /// </summary>
        public Color WeekendBackgroundColor { get; set; } = Color.FromRgb(250, 250, 250);

        /// <summary>
        /// Gets or sets the background color for today's column.
        /// </summary>
        public Color TodayBackgroundColor { get; set; } = Color.FromRgb(255, 248, 220);

        /// <summary>
        /// Creates a copy of the current grid theme.
        /// </summary>
        public GridTheme Clone()
        {
            return new GridTheme
            {
                LineColor = LineColor,
                MajorLineColor = MajorLineColor,
                LineThickness = LineThickness,
                WeekendBackgroundColor = WeekendBackgroundColor,
                TodayBackgroundColor = TodayBackgroundColor
            };
        }
    }

    /// <summary>
    /// Configuration for task bar appearance in the Gantt chart.
    /// </summary>
    public class TaskTheme
    {
        /// <summary>
        /// Gets or sets the default task bar color.
        /// </summary>
        public Color DefaultColor { get; set; } = Color.FromRgb(33, 150, 243);

        /// <summary>
        /// Gets or sets the completed task bar color.
        /// </summary>
        public Color CompletedColor { get; set; } = Color.FromRgb(76, 175, 80);

        /// <summary>
        /// Gets or sets the overdue task bar color.
        /// </summary>
        public Color OverdueColor { get; set; } = Color.FromRgb(244, 67, 54);

        /// <summary>
        /// Gets or sets the in-progress task bar color.
        /// </summary>
        public Color InProgressColor { get; set; } = Color.FromRgb(255, 193, 7);

        /// <summary>
        /// Gets or sets the task bar border color.
        /// </summary>
        public Color BorderColor { get; set; } = Color.FromRgb(25, 118, 210);

        /// <summary>
        /// Gets or sets the task bar corner radius.
        /// </summary>
        public double CornerRadius { get; set; } = 4.0;

        /// <summary>
        /// Gets or sets the task bar border thickness.
        /// </summary>
        public double BorderThickness { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the task text color.
        /// </summary>
        public Color TextColor { get; set; } = Colors.White;

        /// <summary>
        /// Gets or sets the task text font family.
        /// </summary>
        public string FontFamily { get; set; } = "Segoe UI";

        /// <summary>
        /// Gets or sets the task text font size.
        /// </summary>
        public double FontSize { get; set; } = 12.0;

        /// <summary>
        /// Gets or sets whether to enable shadow effects on task bars.
        /// </summary>
        public bool EnableShadow { get; set; } = false;

        /// <summary>
        /// Gets or sets the shadow color for task bars.
        /// </summary>
        public Color ShadowColor { get; set; } = Colors.Black;

        /// <summary>
        /// Gets or sets the shadow blur radius for task bars.
        /// </summary>
        public double ShadowBlurRadius { get; set; } = 8.0;

        /// <summary>
        /// Gets or sets the shadow depth for task bars.
        /// </summary>
        public double ShadowDepth { get; set; } = 2.0;

        /// <summary>
        /// Gets or sets the shadow opacity for task bars.
        /// </summary>
        public double ShadowOpacity { get; set; } = 0.3;

        /// <summary>
        /// Gets or sets the shadow direction for task bars.
        /// </summary>
        public double ShadowDirection { get; set; } = 315.0;

        /// <summary>
        /// Creates a copy of the current task theme.
        /// </summary>
        public TaskTheme Clone()
        {
            return new TaskTheme
            {
                DefaultColor = DefaultColor,
                CompletedColor = CompletedColor,
                OverdueColor = OverdueColor,
                InProgressColor = InProgressColor,
                BorderColor = BorderColor,
                CornerRadius = CornerRadius,
                BorderThickness = BorderThickness,
                TextColor = TextColor,
                FontFamily = FontFamily,
                FontSize = FontSize,
                EnableShadow = EnableShadow,
                ShadowColor = ShadowColor,
                ShadowBlurRadius = ShadowBlurRadius,
                ShadowDepth = ShadowDepth,
                ShadowOpacity = ShadowOpacity,
                ShadowDirection = ShadowDirection
            };
        }
    }

    /// <summary>
    /// Configuration for time scale appearance in the Gantt chart.
    /// </summary>
    public class TimeScaleTheme
    {
        /// <summary>
        /// Gets or sets the background color of the time scale header.
        /// </summary>
        public Color BackgroundColor { get; set; } = Color.FromRgb(248, 249, 250);

        /// <summary>
        /// Gets or sets the text color of the time scale.
        /// </summary>
        public Color TextColor { get; set; } = Color.FromRgb(33, 37, 41);

        /// <summary>
        /// Gets or sets the border color of time scale cells.
        /// </summary>
        public Color BorderColor { get; set; } = Color.FromRgb(222, 226, 230);

        /// <summary>
        /// Gets or sets the accent color for today marker.
        /// </summary>
        public Color TodayMarkerColor { get; set; } = Color.FromRgb(255, 193, 7);

        /// <summary>
        /// Gets or sets the font family for time scale text.
        /// </summary>
        public string FontFamily { get; set; } = "Segoe UI";

        /// <summary>
        /// Gets or sets the font size for time scale text.
        /// </summary>
        public double FontSize { get; set; } = 11.0;

        /// <summary>
        /// Gets or sets the font weight for time scale text.
        /// </summary>
        public FontWeight FontWeight { get; set; } = FontWeights.SemiBold;

        /// <summary>
        /// Gets or sets the border thickness of time scale cells.
        /// </summary>
        public double BorderThickness { get; set; } = 1.0;

        /// <summary>
        /// Creates a copy of the current time scale theme.
        /// </summary>
        public TimeScaleTheme Clone()
        {
            return new TimeScaleTheme
            {
                BackgroundColor = BackgroundColor,
                TextColor = TextColor,
                BorderColor = BorderColor,
                TodayMarkerColor = TodayMarkerColor,
                FontFamily = FontFamily,
                FontSize = FontSize,
                FontWeight = FontWeight,
                BorderThickness = BorderThickness
            };
        }
    }

    /// <summary>
    /// Configuration for selection appearance in the Gantt chart.
    /// </summary>
    public class SelectionTheme
    {
        /// <summary>
        /// Gets or sets the selection border color.
        /// </summary>
        public Color BorderColor { get; set; } = Color.FromRgb(255, 109, 0);

        /// <summary>
        /// Gets or sets the selection border thickness.
        /// </summary>
        public double BorderThickness { get; set; } = 2.0;

        /// <summary>
        /// Gets or sets the selection background overlay color.
        /// </summary>
        public Color OverlayColor { get; set; } = Color.FromArgb(30, 255, 109, 0);

        /// <summary>
        /// Gets or sets the hover effect color.
        /// </summary>
        public Color HoverColor { get; set; } = Color.FromArgb(50, 33, 150, 243);

        /// <summary>
        /// Creates a copy of the current selection theme.
        /// </summary>
        public SelectionTheme Clone()
        {
            return new SelectionTheme
            {
                BorderColor = BorderColor,
                BorderThickness = BorderThickness,
                OverlayColor = OverlayColor,
                HoverColor = HoverColor
            };
        }
    }
}