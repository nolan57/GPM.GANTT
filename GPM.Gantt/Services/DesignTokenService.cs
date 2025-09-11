using System;
using System.Windows.Media;
using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Default implementation that adapts current GanttTheme into reusable design tokens.
    /// Progressive approach: no behavior change for components unless they opt-in to this service.
    /// </summary>
    public sealed class DesignTokenService : IDesignTokenService
    {
        private readonly IThemeService _themeService;

        public event Action? TokensChanged;

        public DesignTokenService(IThemeService themeService)
        {
            _themeService = themeService;
            _themeService.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            TokensChanged?.Invoke();
        }

        public TaskBarTokens GetTaskBarTokens()
        {
            var theme = _themeService.GetCurrentTheme();
            var task = theme.Task;

            return new TaskBarTokens
            {
                Colors = new TaskColorTokens
                {
                    Default = task.DefaultColor,
                    InProgress = task.InProgressColor,
                    Completed = task.CompletedColor,
                    Overdue = task.OverdueColor,
                    Text = task.TextColor,
                    Border = task.BorderColor
                },
                Wireframe = new WireframeTokens
                {
                    BorderThickness = task.BorderThickness,
                    CornerRadius = task.CornerRadius
                },
                Shadow = new ShadowTokens
                {
                    EnableShadow = task.EnableShadow,
                    Color = task.ShadowColor,
                    BlurRadius = task.ShadowBlurRadius,
                    Depth = task.ShadowDepth,
                    Opacity = task.ShadowOpacity,
                    Direction = task.ShadowDirection
                }
            };
        }

        public AnimationTokens GetAnimationTokens()
        {
            // Currently backed by observed defaults / existing timings.
            // Can be extended to read from theme or a separate configuration.
            return new AnimationTokens
            {
                DragPreviewPulseDurationMs = 800,
                DragPreviewFadeOutMs = 200,
                DragPreviewPulseEasing = "CubicEaseInOut",
                FadeEasing = "QuadraticEaseOut"
            };
        }
    }
}