using System;
using System.Windows.Media;
using GPM.Gantt.Models;

namespace GPM.Gantt.Services
{
    /// <summary>
    /// Provides cross-component reusable design tokens (colors, wireframe, shadow, animations) from a centralized source.
    /// Progressive abstraction: initially backed by the current GanttTheme, and can be evolved without changing component code.
    /// </summary>
    public interface IDesignTokenService
    {
        /// <summary>
        /// Raised when tokens may have changed (e.g., theme switched). Components can refresh their visuals accordingly.
        /// </summary>
        event Action? TokensChanged;

        /// <summary>
        /// Gets task bar related tokens for the current theme.
        /// </summary>
        TaskBarTokens GetTaskBarTokens();

        /// <summary>
        /// Gets animation related tokens that can be used across components.
        /// Note: Initially uses conservative defaults; can be made theme- or config-backed progressively.
        /// </summary>
        AnimationTokens GetAnimationTokens();
    }

    #region Token DTOs

    public sealed class TaskBarTokens
    {
        public required TaskColorTokens Colors { get; init; }
        public required WireframeTokens Wireframe { get; init; }
        public required ShadowTokens Shadow { get; init; }
    }

    public sealed class TaskColorTokens
    {
        public required Color Default { get; init; }
        public required Color InProgress { get; init; }
        public required Color Completed { get; init; }
        public required Color Overdue { get; init; }
        public required Color Text { get; init; }
        public required Color Border { get; init; }
    }

    public sealed class WireframeTokens
    {
        public required double BorderThickness { get; init; }
        public required double CornerRadius { get; init; }
    }

    public sealed class ShadowTokens
    {
        public required bool EnableShadow { get; init; }
        public required Color Color { get; init; }
        public required double BlurRadius { get; init; }
        public required double Depth { get; init; }
        public required double Opacity { get; init; }
        public required double Direction { get; init; }
    }

    public sealed class AnimationTokens
    {
        // Durations (milliseconds)
        public int DragPreviewPulseDurationMs { get; init; } = 800;
        public int DragPreviewFadeOutMs { get; init; } = 200;

        // Easing identifiers (can be mapped to actual WPF easing functions in components)
        public string DragPreviewPulseEasing { get; init; } = "CubicEaseInOut";
        public string FadeEasing { get; init; } = "QuadraticEaseOut";
    }

    #endregion
}